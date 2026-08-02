using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SchoolLibrary.Application.Interfaces;
using SchoolLibrary.Infrastructure.Data;
using SchoolLibrary.Infrastructure.Identity;
using SchoolLibrary.Infrastructure.Services;
using SchoolLibrary.Infrastructure.Storage;

namespace SchoolLibrary.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration
                .GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' was not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = false;

                    options.User.RequireUniqueEmail = true;
                })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            services
                .AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddIdentityCookies();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "SchoolLibrary.Auth";

                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                options.Cookie.SameSite = SameSiteMode.None;

                options.ExpireTimeSpan = TimeSpan.FromHours(8);

                options.SlidingExpiration = true;

                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;

                    return Task.CompletedTask;
                };
            });

            services.AddAuthorization();

            services
                .AddOptions<R2StorageOptions>()
                .Bind(configuration.GetSection(R2StorageOptions.SectionName))
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.AccountId),
                    "R2 AccountId is required.")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.BucketName),
                    "R2 BucketName is required.")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.AccessKeyId),
                    "R2 AccessKeyId is required.")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.SecretAccessKey),
                    "R2 SecretAccessKey is required.")
                .ValidateOnStart();

            services.AddSingleton<IAmazonS3>(serviceProvider =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<R2StorageOptions>>()
                    .Value;

                var credentials = new BasicAWSCredentials(
                    options.AccessKeyId,
                    options.SecretAccessKey);

                var configuration = new AmazonS3Config
                {
                    ServiceURL = $"https://{options.AccountId}.r2.cloudflarestorage.com"
                };

                return new AmazonS3Client(
                    credentials,
                    configuration);
            });

            services.AddScoped<IFileStorageService, R2FileStorageService>();
            services.AddScoped<ILookupService, LookupService>();
            services.AddScoped<IResourceService, ResourceService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
