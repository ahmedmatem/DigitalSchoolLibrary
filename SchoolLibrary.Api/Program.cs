using SchoolLibrary.Api.ExceptionHandling;
using SchoolLibrary.Infrastructure;
using SchoolLibrary.Infrastructure.Identity;

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

            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            builder.Services.AddInfrastructure(builder.Configuration);

            var app = builder.Build();

            app.UseExceptionHandler();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            await IdentitySeeder.SeedRolesAsync(app.Services);

            app.Run();
        }
    }
}
