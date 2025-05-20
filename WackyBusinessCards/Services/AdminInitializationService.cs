using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WackyBusinessCards.Constants;
using WackyBusinessCards.Models;

namespace WackyBusinessCards.Services;

public class AdminInitializationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminInitializationService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitializeAsync()
    {
        // Create roles if they don't exist
        foreach (var roleName in Roles.AllRoles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create admin user if it doesn't exist
        var adminEmail = "admin@example.com";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin@123");

            if (result.Succeeded)
            {
                // Add admin to Admin role
                await _userManager.AddToRoleAsync(adminUser, Roles.Admin);
                
                // Also add to User role
                await _userManager.AddToRoleAsync(adminUser, Roles.User);
            }
        }
        else
        {
            // Ensure the admin user is in the Admin role
            if (!await _userManager.IsInRoleAsync(adminUser, Roles.Admin))
            {
                await _userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
            
            // Also ensure admin is in User role
            if (!await _userManager.IsInRoleAsync(adminUser, Roles.User))
            {
                await _userManager.AddToRoleAsync(adminUser, Roles.User);
            }
        }
    }
}
