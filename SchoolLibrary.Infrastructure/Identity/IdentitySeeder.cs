using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SchoolLibrary.Domain.Constants;

namespace SchoolLibrary.Infrastructure.Identity
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            string[] roles =
            [
                RoleConstants.Student,
                RoleConstants.Teacher,
                RoleConstants.Admin
            ];

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole<Guid>
                        {
                            Id = Guid.NewGuid(),
                            Name = roleName,
                            NormalizedName =
                                roleName.ToUpperInvariant()
                        });
                }
            }
        }
    }
}
