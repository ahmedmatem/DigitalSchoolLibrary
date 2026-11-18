using System.ComponentModel.DataAnnotations;
using SchoolLibrary.Application.DTOs.AuthDtos;
using Xunit;

namespace SchoolLibrary.Application.Tests.DTOs.AuthDtos;

public class ResetPasswordDtoTests
{
    [Theory]
    [InlineData(nameof(ResetPasswordDto.Email), null)]
    [InlineData(nameof(ResetPasswordDto.Email), "")]
    [InlineData(nameof(ResetPasswordDto.Email), "   ")]
    [InlineData(nameof(ResetPasswordDto.Token), null)]
    [InlineData(nameof(ResetPasswordDto.Token), "")]
    [InlineData(nameof(ResetPasswordDto.Token), "   ")]
    [InlineData(nameof(ResetPasswordDto.Email), "invalid-email")]
    [InlineData(nameof(ResetPasswordDto.NewPassword), null)]
    [InlineData(nameof(ResetPasswordDto.NewPassword), "")]
    [InlineData(nameof(ResetPasswordDto.NewPassword), "   ")]
    [InlineData(nameof(ResetPasswordDto.ConfirmNewPassword), null)]
    [InlineData(nameof(ResetPasswordDto.ConfirmNewPassword), "")]
    [InlineData(nameof(ResetPasswordDto.ConfirmNewPassword), "   ")]
    [InlineData(nameof(ResetPasswordDto.NewPassword), "abc12")]
    [InlineData(nameof(ResetPasswordDto.ConfirmNewPassword), "different123")]
    public void Validate_InvalidRequest_ReturnsErrorForMember(string member, string? value)
    {
        var model = CreateValidRequest();
        switch (member)
        {
            case nameof(model.Email): model.Email = value!; break;
            case nameof(model.Token): model.Token = value!; break;
            case nameof(model.NewPassword): model.NewPassword = value!; break;
            case nameof(model.ConfirmNewPassword): model.ConfirmNewPassword = value!; break;
            default: throw new ArgumentOutOfRangeException(nameof(member));
        }
        var errors = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), errors, true);

        Assert.False(isValid);
        Assert.Contains(errors, error => error.MemberNames.Contains(member));
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("new-password123")]
    public void Validate_ValidRequest_ReturnsNoErrors(string password)
    {
        var model = CreateValidRequest();
        model.NewPassword = password;
        model.ConfirmNewPassword = password;
        var errors = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), errors, true);

        Assert.True(isValid);
        Assert.Empty(errors);
    }

    private static ResetPasswordDto CreateValidRequest()
    {
        return new ResetPasswordDto
        {
            Email = "student@example.com",
            Token = "reset-token",
            NewPassword = "new-password123",
            ConfirmNewPassword = "new-password123"
        };
    }
}
