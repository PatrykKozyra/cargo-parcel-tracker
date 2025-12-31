using Microsoft.AspNetCore.Identity;
using CargoParcelTracker.Models;

namespace CargoParcelTracker.Data;

public static class IdentitySeeder
{
    public static async Task SeedRolesAndUsersAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Seed Roles
        await SeedRolesAsync(roleManager);

        // Seed Users
        await SeedUsersAsync(userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "Trader" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Admin User
        await CreateUserIfNotExistsAsync(
            userManager,
            username: "admin",
            email: "admin@shell.com",
            fullName: "Administrator",
            password: "admin123",
            role: "Admin"
        );

        // Trader User
        await CreateUserIfNotExistsAsync(
            userManager,
            username: "patryk",
            email: "patryk@shell.com",
            fullName: "Patryk Trader",
            password: "patryk123",
            role: "Trader"
        );
    }

    private static async Task CreateUserIfNotExistsAsync(
        UserManager<ApplicationUser> userManager,
        string username,
        string email,
        string fullName,
        string password,
        string role)
    {
        var existingUser = await userManager.FindByNameAsync(username);
        if (existingUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
