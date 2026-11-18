using Microsoft.Extensions.Hosting;
using Moq;
using SchoolLibrary.Infrastructure.Email;
using Xunit;

namespace SchoolLibrary.Infrastructure.Tests.Configuration;

public class EmailOptionsValidatorTests
{
    [Theory]
    [InlineData("Production", "https://library.example", false, true)]
    [InlineData("Development", "http://localhost:4200", true, true)]
    [InlineData("Production", "http://library.example", false, false)]
    [InlineData("Production", "https://localhost", false, false)]
    [InlineData("Production", "https://library.example", true, false)]
    [InlineData("Production", "https://library.example/?token=secret", false, false)]
    [InlineData("Production", "https://user:pass@library.example", false, false)]
    public void Validate_EnvironmentAndDeliverySettings_EnforcesSafeUrls(
        string environment, string url, bool testRecipient, bool valid)
    {
        var options = ValidOptions();
        options.FrontendBaseUrl = url;
        options.UseTestRecipient = testRecipient;
        options.TestRecipient = "test@example.com";

        var result = CreateValidator(environment).Validate(null, options);

        Assert.Equal(valid, result.Succeeded);
    }

    [Theory]
    [InlineData("token")]
    [InlineData("sender")]
    [InlineData("url")]
    [InlineData("test-recipient")]
    public void Validate_MissingRequiredSetting_ReturnsFailureWithoutLeakingValues(string setting)
    {
        var options = ValidOptions();
        switch (setting)
        {
            case "token": options.ApiToken = ""; break;
            case "sender": options.FromAddress = "not-an-email"; break;
            case "url": options.FrontendBaseUrl = ""; break;
            case "test-recipient": options.UseTestRecipient = true; options.TestRecipient = ""; break;
        }

        var result = CreateValidator("Development").Validate(null, options);

        Assert.True(result.Failed);
        Assert.DoesNotContain("test-api-token", result.FailureMessage);
    }

    [Fact]
    public void Validate_ProductionSandboxSender_ReturnsFailure()
    {
        var options = ValidOptions();
        options.FromAddress = "onboarding@resend.dev";
        Assert.True(CreateValidator("Production").Validate(null, options).Failed);
    }

    private static EmailOptions ValidOptions() => new()
    {
        ApiToken = "test-api-token", FromAddress = "library@example.com",
        FrontendBaseUrl = "https://library.example", UseTestRecipient = false
    };

    private static EmailOptionsValidator CreateValidator(string name)
    {
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(item => item.EnvironmentName).Returns(name);
        return new EmailOptionsValidator(environment.Object);
    }
}
