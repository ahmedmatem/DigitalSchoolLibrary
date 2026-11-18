using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SchoolLibrary.Api.Tests.Infrastructure;
using SchoolLibrary.Application.DTOs.AuthDtos;
using SchoolLibrary.Infrastructure.Identity;
using Xunit;

namespace SchoolLibrary.Api.Tests.Controllers;

public class AuthControllerTests : IDisposable
{
    private readonly LibraryApiFactory factory = new();

    [Theory]
    [InlineData("GET", "/api/auth/me")]
    [InlineData("POST", "/api/auth/change-password")]
    [InlineData("POST", "/api/auth/logout")]
    public async Task ProtectedEndpoint_Anonymous_Returns401WithoutRedirect(string method, string path)
    {
        using var client = factory.CreateHttpsClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        request.Content = JsonContent.Create(new { });

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Theory]
    [InlineData("/api/auth/forgot-password", "Email")]
    [InlineData("/api/auth/reset-password", "Token")]
    public async Task RecoveryEndpoint_InvalidRequest_ReturnsValidationProblem(string path, string member)
    {
        using var client = factory.CreateHttpsClient();

        using var response = await client.PostAsJsonAsync(path, new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.True(problem.Errors.ContainsKey(member));
        Assert.NotEmpty(problem.Errors[member]);
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_ReturnsGenericSuccess()
    {
        using var client = factory.CreateHttpsClient();

        using var response = await client.PostAsJsonAsync("/api/auth/forgot-password",
            new ForgotPasswordDto { Email = "unknown@example.com" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(body);
        Assert.Equal("Ако съществува активен профил с този имейл, ще бъде изпратен линк за възстановяване.", body["message"]);
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_ReturnsApplicationValidationProblem()
    {
        await factory.CreateUserAsync();
        using var client = factory.CreateHttpsClient();

        using var response = await client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordDto
        {
            Email = "student@example.com", Token = "invalid-token",
            NewPassword = "new123", ConfirmNewPassword = "new123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("/api/auth/reset-password", problem.Instance);
        Assert.Equal("Линкът за възстановяване е невалиден или е изтекъл.", problem.Detail);
        var error = Assert.Single(problem.Errors);
        Assert.Equal("general", error.Key);
        Assert.Equal(problem.Detail, Assert.Single(error.Value));
    }

    [Fact]
    public async Task ResetPassword_ValidToken_Returns204AndNewPasswordWorks()
    {
        await factory.CreateUserAsync(mustChangePassword: true);
        string token;
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByEmailAsync("student@example.com");
            Assert.NotNull(user);
            token = await users.GeneratePasswordResetTokenAsync(user);
        }
        using var client = factory.CreateHttpsClient();

        using var response = await client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordDto
        {
            Email = "student@example.com", Token = token, NewPassword = "new123", ConfirmNewPassword = "new123"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
        await LoginAsync(client, "new123");
        var me = await client.GetFromJsonAsync<MeDto>("/api/auth/me");
        Assert.NotNull(me);
        Assert.False(me.MustChangePassword);
        using var oldPasswordClient = factory.CreateHttpsClient();
        using var oldLogin = await oldPasswordClient.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "student@example.com", Password = "current123"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_InvalidDto_ReturnsValidationProblem()
    {
        await factory.CreateUserAsync();
        using var client = factory.CreateHttpsClient();
        await LoginAsync(client);

        using var response = await client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordDto
        {
            CurrentPassword = "current123", NewPassword = "short", ConfirmNewPassword = "different"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("NewPassword"));
        Assert.True(problem.Errors.ContainsKey("ConfirmNewPassword"));
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_Returns400AndPreservesFlag()
    {
        await factory.CreateUserAsync(mustChangePassword: true);
        using var client = factory.CreateHttpsClient();
        await LoginAsync(client);

        using var response = await client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordDto
        {
            CurrentPassword = "wrong123", NewPassword = "new123", ConfirmNewPassword = "new123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("/api/auth/change-password", problem.Instance);
        Assert.Equal("Новата парола не отговаря на изискванията.", problem.Detail);
        var error = Assert.Single(problem.Errors);
        Assert.Equal("PasswordMismatch", error.Key);
        Assert.False(string.IsNullOrWhiteSpace(Assert.Single(error.Value)));
        var me = await client.GetFromJsonAsync<MeDto>("/api/auth/me");
        Assert.NotNull(me);
        Assert.True(me.MustChangePassword);
    }

    [Fact]
    public async Task ChangePassword_Success_RefreshesCookieAndMeReturnsClearedFlag()
    {
        var id = await factory.CreateUserAsync(mustChangePassword: true);
        using var client = factory.CreateHttpsClient();
        await LoginAsync(client);

        using var response = await client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordDto
        {
            CurrentPassword = "current123", NewPassword = "new123", ConfirmNewPassword = "new123"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), cookie => cookie.StartsWith("SchoolLibrary.Auth="));
        var me = await client.GetFromJsonAsync<MeDto>("/api/auth/me");
        Assert.NotNull(me);
        Assert.Equal(id, me.Id);
        Assert.False(me.MustChangePassword);
        using var newClient = factory.CreateHttpsClient();
        await LoginAsync(newClient, "new123");
    }

    [Fact]
    public async Task Logout_Authenticated_Returns204AndRemovesAccess()
    {
        await factory.CreateUserAsync();
        using var client = factory.CreateHttpsClient();
        await LoginAsync(client);

        using var response = await client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var me = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }

    [Fact]
    public async Task PasswordRecovery_SixthRequest_Returns429()
    {
        using var client = factory.CreateHttpsClient();
        for (var index = 0; index < 5; index++)
        {
            using var allowed = await client.PostAsJsonAsync("/api/auth/forgot-password",
                new ForgotPasswordDto { Email = "unknown@example.com" });
            Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        }

        using var response = await client.PostAsJsonAsync("/api/auth/forgot-password",
            new ForgotPasswordDto { Email = "unknown@example.com" });

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Theory]
    [InlineData("https://library.example", true)]
    [InlineData("https://untrusted.example", false)]
    public async Task CorsPreflight_UsesConfiguredOrigins(string origin, bool allowed)
    {
        using var client = factory.CreateHttpsClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/auth/me");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        if (allowed)
        {
            Assert.Equal(origin, Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
            Assert.Equal("true", Assert.Single(response.Headers.GetValues("Access-Control-Allow-Credentials")));
        }
        else
        {
            Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        }
    }

    private static async Task LoginAsync(HttpClient client, string password = "current123")
    {
        using var response = await client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "student@example.com", Password = password
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public void Dispose() => factory.Dispose();
}
