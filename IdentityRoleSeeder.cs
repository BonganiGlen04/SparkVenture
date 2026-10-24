using Microsoft.AspNetCore.Identity;
using SparkVenture.Data;
using SparkVenture.Data.Models;

namespace SparkVenture
{
    public static class IdentityRoleSeeder
    {
        public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "Customer", "Reviewer" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(
                        new ApplicationRole { Name = roleName });

                    if (!result.Succeeded)
                    {
                        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                        throw new InvalidOperationException(
                            $"Could not create Identity role '{roleName}': {errors}");
                    }
                }
            }

            // Optional: create or promote a trusted bootstrap administrator.
            // Configure BootstrapAdmin__Email and BootstrapAdmin__Password as
            // environment variables or deployment secrets. Do not commit credentials.
            var adminEmail = configuration["BootstrapAdmin:Email"];
            var adminPassword = configuration["BootstrapAdmin:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(admin, adminPassword);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException(
                        $"Could not create bootstrap administrator: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                var roleResult = await userManager.AddToRoleAsync(admin, "Admin");

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException(
                        $"Could not assign the Admin role: {errors}");
                }
            }
        }
    }
}
