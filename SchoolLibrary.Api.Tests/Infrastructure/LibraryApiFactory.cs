using SchoolLibrary.Api;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SchoolLibrary.Application.Common.Interfaces;
using SchoolLibrary.Infrastructure.Data;
using SchoolLibrary.Infrastructure.Identity;
using SchoolLibrary.Infrastructure.Storage;

namespace SchoolLibrary.Api.Tests.Infrastructure;

public sealed class LibraryApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=unused;Database=unused");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName));
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
            services.PostConfigure<R2StorageOptions>(options =>
            {
                options.AccountId = "test";
                options.BucketName = "test";
                options.AccessKeyId = "test";
                options.SecretAccessKey = "test";
            });
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender, TestEmailSender>();
        });
    }

    public HttpClient CreateHttpsClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false
    });

    public async Task<Guid> CreateUserAsync(bool mustChangePassword = false)
    {
        using var scope = Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), UserName = "student@example.com", Email = "student@example.com",
            FirstName = "Test", FatherName = "Test", LastName = "Student",
            MustChangePassword = mustChangePassword
        };
        var result = await users.CreateAsync(user, "current123");
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        return user.Id;
    }

    private sealed class TestEmailSender : IEmailSender
    {
        public Task SendPasswordResetAsync(string recipientEmail, string resetUrl, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
