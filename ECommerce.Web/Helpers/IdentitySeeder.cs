using ECommerce.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Web.Helpers;

public static class IdentitySeeder
{
    private const string AdminRoleName = "Admin";
    private const string CustomerRoleName = "Customer";
    private const string DefaultAdminFullName = "System Administrator";
    private const string DefaultAdminUserName = "admin";
    private const string DefaultAdminEmail = "admin@ecommerce.local";
    private const string DefaultAdminPassword = "Admin@12345";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await EnsureRoleExistsAsync(roleManager, AdminRoleName);
        await EnsureRoleExistsAsync(roleManager, CustomerRoleName);

        var adminUser = await userManager.FindByNameAsync(DefaultAdminUserName)
            ?? await userManager.FindByEmailAsync(DefaultAdminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                FullName = DefaultAdminFullName,
                UserName = DefaultAdminUserName,
                Email = DefaultAdminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, DefaultAdminPassword);
            EnsureSucceeded(createResult, "Failed to create the default admin account.");
        }

        if (!await userManager.IsInRoleAsync(adminUser, AdminRoleName))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(adminUser, AdminRoleName);
            EnsureSucceeded(addToRoleResult, "Failed to assign the Admin role to the default admin account.");
        }
    }

    private static async Task EnsureRoleExistsAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var createRoleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
        EnsureSucceeded(createRoleResult, $"Failed to seed the '{roleName}' role.");
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{message} {errors}");
    }
}
