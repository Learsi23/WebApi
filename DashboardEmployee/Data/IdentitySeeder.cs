using DashboardEmployee.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace DashboardEmployee.Data;

/// <summary>
/// Creates the roles and the two demo users if they do not exist yet.
/// Passwords come from configuration (user-secrets in development), never from source code.
/// </summary>
/// 
public static class IdentitySeeder
{
    public const string AdminEmail = "admin@company.se";
    public const string ViewerEmail = "viewer@company.se";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await EnsureUserAsync(userManager, AdminEmail, configuration["SeedUser:AdminPassword"]!, Roles.Admin);
        await EnsureUserAsync(userManager, ViewerEmail, configuration["SeedUser:ViewerPassword"]!, Roles.Viewer);


    }
    private static async Task EnsureUserAsync(UserManager<IdentityUser> userManager, string email, string? password, string role)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException($"No seed password for {email}. Set it with 'dotnet user - secrets'.");

        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            throw new InvalidOperationException($"Could not create {email}: {string.Join(" ",
            result.Errors.Select(e => e.Description))}");

        await userManager.AddToRoleAsync(user, role);
    }
}
