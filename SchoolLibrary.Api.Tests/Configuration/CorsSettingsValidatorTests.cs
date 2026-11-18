using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SchoolLibrary.Api.Configuration;
using Xunit;

namespace SchoolLibrary.Api.Tests.Configuration;

public class CorsSettingsValidatorTests
{
    [Theory]
    [InlineData("Production", "https://library.example", true)]
    [InlineData("Development", "http://localhost:4200", true)]
    [InlineData("Production", "http://library.example", false)]
    [InlineData("Production", "https://localhost:4200", false)]
    [InlineData("Production", "*", false)]
    [InlineData("Production", "https://library.example/", false)]
    [InlineData("Production", "https://library.example/path", false)]
    [InlineData("Production", "https://user:pass@library.example", false)]
    public void Validate_Origin_EnforcesEnvironmentRules(string environment, string origin, bool valid)
    {
        var validator = new CorsSettingsValidator(new TestEnvironment(environment));
        Assert.Equal(valid, validator.Validate(null, new CorsSettings { AllowedOrigins = [origin] }).Succeeded);
    }

    [Fact]
    public void Validate_EmptyOrigins_Fails()
    {
        Assert.True(new CorsSettingsValidator(new TestEnvironment("Production"))
            .Validate(null, new CorsSettings()).Failed);
    }

    private sealed class TestEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
