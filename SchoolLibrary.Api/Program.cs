using SchoolLibrary.Api.ExceptionHandling;
using SchoolLibrary.Api.Configuration;
using Microsoft.Extensions.Options;
using SchoolLibrary.Infrastructure;
using SchoolLibrary.Infrastructure.Identity;
using System.Threading.RateLimiting;

namespace SchoolLibrary.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("password-recovery", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(15),
                            QueueLimit = 0
                        }));
            });

            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

            builder.Services.AddSingleton<IValidateOptions<CorsSettings>, CorsSettingsValidator>();
            builder.Services.AddOptions<CorsSettings>()
                .Bind(builder.Configuration.GetSection(CorsSettings.SectionName))
                .ValidateOnStart();
            builder.Services.AddCors();
            builder.Services.AddOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>()
                .Configure<IOptions<CorsSettings>>((options, settings) =>
                    options.AddPolicy("AngularClient", policy => policy
                        .WithOrigins(settings.Value.AllowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()));

            var app = builder.Build();

            app.UseExceptionHandler();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseRateLimiter();

            app.UseCors("AngularClient");

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            // Validate external-service settings before startup touches the database.
            app.Services.GetRequiredService<IStartupValidator>().Validate();
            await IdentitySeeder.SeedRolesAsync(app.Services);

            app.Run();
        }
    }
}
