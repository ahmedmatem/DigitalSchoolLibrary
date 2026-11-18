using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolLibrary.Api.Configuration;
using SchoolLibrary.Infrastructure.Email;
using SchoolLibrary.Api.Tests.Infrastructure;
using Xunit;

namespace SchoolLibrary.Api.Tests.Configuration;

public class EnvironmentStartupTests
{
    [Theory]
    [InlineData(true, "Cors:AllowedOrigins")]
    [InlineData(false, "Email:ApiToken")]
    public void MissingRequiredConfiguration_PreventsStartup(bool cors, string setting)
    {
        using var factory = new LibraryApiFactory();
        using var host = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            if (cors)
                services.PostConfigure<CorsSettings>(options => options.AllowedOrigins = []);
            else
                services.PostConfigure<EmailOptions>(options => options.ApiToken = "");
        }));

        var exception = Assert.ThrowsAny<Exception>(() => host.CreateClient());

        Assert.Contains(setting, exception.ToString());
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task ConfiguredHost_StartsWithIsolatedServices(string environment)
    {
        using var factory = new LibraryApiFactory();
        using var host = factory.WithWebHostBuilder(builder => builder.UseEnvironment(environment));
        using var client = host.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });

        Assert.Equal(environment, host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName);
        using var response = await client.PostAsJsonAsync("/api/auth/forgot-password", new { Email = "unknown@example.com" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
