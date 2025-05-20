using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WackyBusinessCards.Constants;
using WackyBusinessCards.Models;

namespace WackyBusinessCards.Controllers;

public class SetupController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public SetupController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: /Setup/CreateAdmin
    public async Task<IActionResult> CreateAdmin()
    {
        try
        {
            // Create roles if they don't exist
            if (!await _roleManager.RoleExistsAsync(Roles.Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            }

            if (!await _roleManager.RoleExistsAsync(Roles.User))
            {
                await _roleManager.CreateAsync(new IdentityRole(Roles.User));
            }

            // Check if admin user exists
            var adminEmail = "admin@example.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Create a new admin user
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Administrator",
                    CreatedAt = DateTime.Now
                };

                // Use a simple password for testing
                var result = await _userManager.CreateAsync(adminUser, "Password123!");

                if (result.Succeeded)
                {
                    // Add to roles
                    await _userManager.AddToRoleAsync(adminUser, Roles.Admin);
                    await _userManager.AddToRoleAsync(adminUser, Roles.User);

                    return Content($"Admin user created successfully! Email: {adminEmail}, Password: Password123!");
                }
                else
                {
                    return Content($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Reset the admin password
                var token = await _userManager.GeneratePasswordResetTokenAsync(adminUser);
                var resetResult = await _userManager.ResetPasswordAsync(adminUser, token, "Password123!");

                if (resetResult.Succeeded)
                {
                    // Ensure admin is in roles
                    if (!await _userManager.IsInRoleAsync(adminUser, Roles.Admin))
                    {
                        await _userManager.AddToRoleAsync(adminUser, Roles.Admin);
                    }

                    if (!await _userManager.IsInRoleAsync(adminUser, Roles.User))
                    {
                        await _userManager.AddToRoleAsync(adminUser, Roles.User);
                    }

                    return Content($"Admin user password reset successfully! Email: {adminEmail}, Password: Password123!");
                }
                else
                {
                    return Content($"Failed to reset admin password: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
                }
            }
        }
        catch (Exception ex)
        {
            return Content($"Error: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // GET: /Setup/ListUsers
    public async Task<IActionResult> ListUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var result = new System.Text.StringBuilder();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.AppendLine($"User: {user.Email}, Roles: {string.Join(", ", roles)}");
        }

        return Content(result.ToString());
    }

    // GET: /Setup/ListRoles
    public async Task<IActionResult> ListRoles()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        var result = new System.Text.StringBuilder();

        foreach (var role in roles)
        {
            result.AppendLine($"Role: {role.Name}");
        }

        return Content(result.ToString());
    }
}
