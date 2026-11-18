using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SchoolLibrary.Application.Common.Exceptions;
using SchoolLibrary.Application.Common.Interfaces;
using SchoolLibrary.Application.DTOs.AuthDtos;
using SchoolLibrary.Infrastructure.Data;
using SchoolLibrary.Infrastructure.Email;
using SchoolLibrary.Infrastructure.Identity;
using SchoolLibrary.Infrastructure.Services;
using Xunit;

namespace SchoolLibrary.Infrastructure.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly Mock<UserManager<ApplicationUser>> users;
    private readonly Mock<SignInManager<ApplicationUser>> signIn;
    private readonly ApplicationDbContext db;
    private readonly AuthService service;
    private readonly ApplicationUser user = new()
    {
        Id = Guid.NewGuid(),
        Email = "student@example.com",
        MustChangePassword = true
    };

    public AuthServiceTests()
    {
        users = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(), Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(), new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(), Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());
        signIn = new Mock<SignInManager<ApplicationUser>>(
            users.Object, new HttpContextAccessor(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Options.Create(new IdentityOptions()), Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(), Mock.Of<IUserConfirmation<ApplicationUser>>());
        db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        service = new AuthService(users.Object, signIn.Object, db,
            Mock.Of<IEmailSender>(), Options.Create(new EmailOptions()));

        users.Setup(manager => manager.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        users.Setup(manager => manager.FindByEmailAsync("student@example.com")).ReturnsAsync(user);
        users.Setup(manager => manager.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        users.Setup(manager => manager.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);
        signIn.Setup(manager => manager.RefreshSignInAsync(user)).Returns(Task.CompletedTask);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChangePasswordAsync_IdentityFailure_PreservesFlagAndDoesNotRefreshSession(bool initialFlag)
    {
        user.MustChangePassword = initialFlag;
        users.Setup(manager => manager.ChangePasswordAsync(user, "current123", "new123"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordMismatch", Description = "Incorrect current password."
            }));

        var exception = await Assert.ThrowsAsync<ValidationException>(() => ChangePasswordAsync());

        Assert.Equal(initialFlag, user.MustChangePassword);
        Assert.Equal("Incorrect current password.", Assert.Single(exception.Errors["PasswordMismatch"]));
        VerifyNoPostPasswordUpdates();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChangePasswordAsync_Success_ClearsAndPersistsFlagThenRefreshesSession(bool initialFlag)
    {
        user.MustChangePassword = initialFlag;
        users.Setup(manager => manager.ChangePasswordAsync(user, "current123", "new123"))
            .Callback(() => Assert.Equal(initialFlag, user.MustChangePassword))
            .ReturnsAsync(IdentityResult.Success);
        users.Setup(manager => manager.UpdateAsync(user))
            .Callback(() => Assert.False(user.MustChangePassword))
            .ReturnsAsync(IdentityResult.Success);

        await ChangePasswordAsync();

        Assert.False(user.MustChangePassword);
        users.Verify(manager => manager.ChangePasswordAsync(user, "current123", "new123"), Times.Once);
        users.Verify(manager => manager.UpdateAsync(user), Times.Once);
        users.Verify(manager => manager.UpdateSecurityStampAsync(user), Times.Once);
        signIn.Verify(manager => manager.RefreshSignInAsync(user), Times.Once);
    }

    [Theory]
    [InlineData("InvalidToken")]
    [InlineData("PasswordTooShort")]
    public async Task ResetPasswordAsync_IdentityFailure_PreservesFlagAndDoesNotUpdateUser(string errorCode)
    {
        users.Setup(manager => manager.ResetPasswordAsync(user, "reset-token", "new123"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = errorCode, Description = "Identity rejected the request."
            }));

        var exception = await Assert.ThrowsAsync<ValidationException>(() => ResetPasswordAsync());

        Assert.True(user.MustChangePassword);
        if (errorCode == "InvalidToken")
        {
            Assert.Equal("Линкът за възстановяване е невалиден или е изтекъл.", exception.Message);
            Assert.Empty(exception.Errors);
        }
        else
        {
            Assert.Equal("Identity rejected the request.", Assert.Single(exception.Errors[errorCode]));
        }
        VerifyNoPostPasswordUpdates();
    }

    [Fact]
    public async Task ResetPasswordAsync_Success_ClearsAndPersistsFlagAndInvalidatesSessions()
    {
        users.Setup(manager => manager.ResetPasswordAsync(user, "reset-token", "new123"))
            .Callback(() => Assert.True(user.MustChangePassword))
            .ReturnsAsync(IdentityResult.Success);
        users.Setup(manager => manager.UpdateAsync(user))
            .Callback(() => Assert.False(user.MustChangePassword))
            .ReturnsAsync(IdentityResult.Success);

        await ResetPasswordAsync();

        Assert.False(user.MustChangePassword);
        users.Verify(manager => manager.ResetPasswordAsync(user, "reset-token", "new123"), Times.Once);
        users.Verify(manager => manager.UpdateAsync(user), Times.Once);
        users.Verify(manager => manager.UpdateSecurityStampAsync(user), Times.Once);
        signIn.Verify(manager => manager.RefreshSignInAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task PasswordOperation_UserUpdateFails_RestoresFlagWithoutRefreshingSession(bool reset, bool initialFlag)
    {
        user.MustChangePassword = initialFlag;
        users.Setup(manager => manager.ChangePasswordAsync(user, "current123", "new123"))
            .ReturnsAsync(IdentityResult.Success);
        users.Setup(manager => manager.ResetPasswordAsync(user, "reset-token", "new123"))
            .ReturnsAsync(IdentityResult.Success);
        users.Setup(manager => manager.UpdateAsync(user)).ReturnsAsync(IdentityResult.Failed(
            new IdentityError { Code = "UpdateFailed", Description = "Could not save user." }));

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => reset ? ResetPasswordAsync() : ChangePasswordAsync());

        Assert.Equal(initialFlag, user.MustChangePassword);
        Assert.Equal("Could not save user.", Assert.Single(exception.Errors["UpdateFailed"]));
        users.Verify(manager => manager.UpdateSecurityStampAsync(It.IsAny<ApplicationUser>()), Times.Never);
        signIn.Verify(manager => manager.RefreshSignInAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task PasswordOperation_UserUpdateThrows_RestoresFlagAndRethrows(bool reset, bool initialFlag)
    {
        user.MustChangePassword = initialFlag;
        users.Setup(manager => manager.ChangePasswordAsync(user, "current123", "new123"))
            .ReturnsAsync(IdentityResult.Success);
        users.Setup(manager => manager.ResetPasswordAsync(user, "reset-token", "new123"))
            .ReturnsAsync(IdentityResult.Success);
        var failure = new InvalidOperationException("Could not save user.");
        users.Setup(manager => manager.UpdateAsync(user)).ThrowsAsync(failure);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => reset ? ResetPasswordAsync() : ChangePasswordAsync());

        Assert.Same(failure, exception);
        Assert.Equal(initialFlag, user.MustChangePassword);
        users.Verify(manager => manager.UpdateSecurityStampAsync(It.IsAny<ApplicationUser>()), Times.Never);
        signIn.Verify(manager => manager.RefreshSignInAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task PasswordOperation_MissingOrInactiveUser_RejectsBeforeChangingPassword(bool reset, bool missing)
    {
        if (missing)
        {
            users.Setup(manager => manager.FindByIdAsync(user.Id.ToString())).ReturnsAsync((ApplicationUser?)null);
            users.Setup(manager => manager.FindByEmailAsync("student@example.com")).ReturnsAsync((ApplicationUser?)null);
        }
        else
        {
            user.IsActive = false;
        }

        await Assert.ThrowsAsync<ValidationException>(() => reset ? ResetPasswordAsync() : ChangePasswordAsync());

        Assert.True(user.MustChangePassword);
        users.Verify(manager => manager.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        users.Verify(manager => manager.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        VerifyNoPostPasswordUpdates();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetMeAsync_ActiveUser_ReturnsStoredFlag(bool mustChangePassword)
    {
        user.MustChangePassword = mustChangePassword;
        db.Users.Add(user);
        await db.SaveChangesAsync();
        users.Setup(manager => manager.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Student" });

        var result = await service.GetMeAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(mustChangePassword, result.MustChangePassword);
        Assert.Equal("Student", Assert.Single(result.Roles));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetMeAsync_MissingOrInactiveUser_ReturnsNull(bool missing)
    {
        if (missing)
            users.Setup(manager => manager.FindByIdAsync(user.Id.ToString())).ReturnsAsync((ApplicationUser?)null);
        else
            user.IsActive = false;

        var result = await service.GetMeAsync(user.Id);

        Assert.Null(result);
        users.Verify(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    private Task ChangePasswordAsync() => service.ChangePasswordAsync(user.Id, new ChangePasswordDto
    {
        CurrentPassword = "current123", NewPassword = "new123", ConfirmNewPassword = "new123"
    });

    private Task ResetPasswordAsync() => service.ResetPasswordAsync(new ResetPasswordDto
    {
        Email = " student@example.com ", Token = "reset-token", NewPassword = "new123", ConfirmNewPassword = "new123"
    });

    private void VerifyNoPostPasswordUpdates()
    {
        users.Verify(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        users.Verify(manager => manager.UpdateSecurityStampAsync(It.IsAny<ApplicationUser>()), Times.Never);
        signIn.Verify(manager => manager.RefreshSignInAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    public void Dispose()
    {
        db.Dispose();
        users.Object.Dispose();
    }
}
