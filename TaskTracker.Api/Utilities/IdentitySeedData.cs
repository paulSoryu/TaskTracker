namespace TaskTracker.Api.Utilities;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.DataAccess.Entities;

public static class IdentitySeedData
{
    public static async Task EnsureAtLeastOneAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<UserEntity>>();

        const string adminRole = "Admin";
        const string defaultAdminEmail = "admin@admin";

        if (!await roleManager.RoleExistsAsync(adminRole))
            await roleManager.CreateAsync(new IdentityRole<Guid>(adminRole));

        var admins = await userManager.GetUsersInRoleAsync(adminRole);

        if (admins.Count == 0)
        {
            // Check if a user with this email already exists
            var defaultAdmin = await userManager.FindByEmailAsync(defaultAdminEmail);

            if (defaultAdmin == null)
            {
                defaultAdmin = new UserEntity
                {
                    UserName = defaultAdminEmail,
                    Email = defaultAdminEmail,
                    EmailConfirmed = true
                };

                // It's better to take password from Configuration/Secrets
                var createResult = await userManager.CreateAsync(defaultAdmin, "adminA1!");
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create a default admin: {errors}");
                }
            }

            await userManager.AddToRoleAsync(defaultAdmin, adminRole);
        }
    }
}