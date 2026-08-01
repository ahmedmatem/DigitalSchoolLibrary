using Amazon.Runtime;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SchoolLibrary.Application.Interfaces;
using SchoolLibrary.Infrastructure.Data;
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

            return services;
        }
    }
}
