using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WackyBusinessCards.Constants;
using WackyBusinessCards.Data;
using WackyBusinessCards.Models;
using WackyBusinessCards.ViewModels;

namespace WackyBusinessCards.Services;

public class UserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly ActivityLogService _activityLogService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<UserManagementService> _logger;
    private readonly EmailService _emailService;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        ActivityLogService activityLogService,
        IWebHostEnvironment webHostEnvironment,
        ILogger<UserManagementService> logger,
        EmailService emailService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _activityLogService = activityLogService;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<List<UserViewModel>> GetAllUsersAsync()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                UserName = u.UserName ?? string.Empty,
                FullName = $"{u.FirstName} {u.LastName}".Trim(),
                CreatedAt = u.CreatedAt ?? DateTime.Now,
                ProfilePicturePath = u.ProfilePicturePath
            })
            .ToListAsync();

        foreach (var user in users)
        {
            var userEntity = await _userManager.FindByIdAsync(user.Id);
            if (userEntity != null)
            {
                user.Roles = await _userManager.GetRolesAsync(userEntity);
                user.BusinessCardCount = await _context.BusinessCards.CountAsync(bc => bc.UserId == user.Id);
            }
        }

        return users;
    }

    public async Task<UserDetailsViewModel?> GetUserDetailsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        var model = new UserDetailsViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            ProfilePicturePath = user.ProfilePicturePath,
            CreatedAt = user.CreatedAt ?? DateTime.Now,
            Roles = await _userManager.GetRolesAsync(user),
            BusinessCards = await _context.BusinessCards
                .Where(bc => bc.UserId == user.Id)
                .ToListAsync(),
            RecentActivity = await _activityLogService.GetUserActivityLogsAsync(userId, 10)
        };

        return model;
    }

    public async Task<(bool Success, string[] Errors)> CreateUserAsync(CreateUserViewModel model, string creatorId)
    {
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            EmailConfirmed = model.EmailConfirmed,
            CreatedAt = DateTime.Now
        };

        // Handle profile picture upload if provided
        if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
        {
            try
            {
                // Create a unique filename
                var fileName = $"{Guid.NewGuid()}_{model.ProfilePicture.FileName}";
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");

                // Ensure directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(fileStream);
                }

                // Set the profile picture path
                user.ProfilePicturePath = $"/uploads/profiles/{fileName}";
            }
            catch (Exception ex)
            {
                // Log the error but continue with user creation
                _logger.LogError(ex, "Error uploading profile picture for user {Email}", model.Email);
            }
        }

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        // Assign roles
        _logger.LogInformation("Assigning roles for user {Email}, IsAdmin={IsAdmin}", user.Email, model.IsAdmin);

        if (model.IsAdmin)
        {
            _logger.LogInformation("Adding user {Email} to Admin role", user.Email);
            var adminRoleResult = await _userManager.AddToRoleAsync(user, Roles.Admin);
            if (adminRoleResult.Succeeded)
            {
                _logger.LogInformation("Successfully added user {Email} to Admin role", user.Email);
            }
            else
            {
                _logger.LogWarning("Failed to add user {Email} to Admin role: {Errors}",
                    user.Email, string.Join(", ", adminRoleResult.Errors.Select(e => e.Description)));
            }
        }

        _logger.LogInformation("Adding user {Email} to User role", user.Email);
        var userRoleResult = await _userManager.AddToRoleAsync(user, Roles.User);
        if (userRoleResult.Succeeded)
        {
            _logger.LogInformation("Successfully added user {Email} to User role", user.Email);
        }
        else
        {
            _logger.LogWarning("Failed to add user {Email} to User role: {Errors}",
                user.Email, string.Join(", ", userRoleResult.Errors.Select(e => e.Description)));
        }

        // Add any additional selected roles
        if (model.SelectedRoles != null && model.SelectedRoles.Any())
        {
            foreach (var role in model.SelectedRoles)
            {
                // Skip if already added or if it's the default User role
                if ((role == Roles.Admin && model.IsAdmin) || role == Roles.User)
                {
                    continue;
                }

                if (await _roleManager.RoleExistsAsync(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
        }

        // Send welcome email if requested
        if (model.SendWelcomeEmail)
        {
            try
            {
                var emailResult = await _emailService.SendWelcomeEmailAsync(
                    model.Email,
                    model.FirstName,
                    model.LastName,
                    model.Password
                );

                if (emailResult)
                {
                    _logger.LogInformation("Welcome email sent successfully to {Email}", model.Email);
                }
                else
                {
                    _logger.LogWarning("Failed to send welcome email to {Email}", model.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to {Email}", model.Email);
                // Continue with user creation even if email fails
            }
        }

        // Log the activity and send notification
        await _activityLogService.LogActivityAsync(
            creatorId,
            "CreateUser",
            "User",
            user.Id,
            $"Created user {user.Email} with roles: {string.Join(", ", model.IsAdmin ? new[] { Roles.Admin, Roles.User } : new[] { Roles.User })}",
            sendNotification: true
        );

        return (true, Array.Empty<string>());
    }

    public async Task<(bool Success, string[] Errors)> ToggleUserRoleAsync(string userId, string role, string adminId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, new[] { "User not found" });
        }

        // Don't allow removing the Admin role from the last admin
        if (role == Roles.Admin && await _userManager.IsInRoleAsync(user, Roles.Admin))
        {
            var adminCount = (await _userManager.GetUsersInRoleAsync(Roles.Admin)).Count;
            if (adminCount <= 1)
            {
                return (false, new[] { "Cannot remove the last admin" });
            }
        }

        IdentityResult result;
        string action;

        if (await _userManager.IsInRoleAsync(user, role))
        {
            result = await _userManager.RemoveFromRoleAsync(user, role);
            action = "Removed";
        }
        else
        {
            result = await _userManager.AddToRoleAsync(user, role);
            action = "Added";
        }

        if (result.Succeeded)
        {
            // Log the activity and send notification
            await _activityLogService.LogActivityAsync(
                adminId,
                $"{action}Role",
                "User",
                userId,
                $"{action} role {role} for user {user.Email}",
                sendNotification: true
            );

            return (true, Array.Empty<string>());
        }

        return (false, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<List<RoleViewModel>> GetAvailableRolesAsync()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return roles.Select(r => new RoleViewModel
        {
            Id = r.Id,
            Name = r.Name ?? string.Empty,
            Description = GetRoleDescription(r.Name ?? string.Empty),
            IsSelected = false
        }).ToList();
    }

    private string GetRoleDescription(string roleName)
    {
        return roleName switch
        {
            Roles.Admin => "Full access to all system features including user management",
            Roles.User => "Standard user with access to create and manage business cards",
            _ => $"Role: {roleName}"
        };
    }

    public async Task<(bool Success, string[] Errors)> DeleteUserAsync(string userId, string adminId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, new[] { "User not found" });
        }

        // Check if this is the last admin
        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
        {
            var adminCount = (await _userManager.GetUsersInRoleAsync(Roles.Admin)).Count;
            if (adminCount <= 1)
            {
                return (false, new[] { "Cannot delete the last admin user" });
            }
        }

        // Get user's business cards
        var businessCards = await _context.BusinessCards
            .Where(bc => bc.UserId == userId)
            .ToListAsync();

        // Delete business cards
        if (businessCards.Any())
        {
            _context.BusinessCards.RemoveRange(businessCards);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} business cards for user {UserId}",
                businessCards.Count, userId);
        }

        // Delete user's activity logs
        var activityLogs = await _context.ActivityLogs
            .Where(al => al.UserId == userId)
            .ToListAsync();

        if (activityLogs.Any())
        {
            _context.ActivityLogs.RemoveRange(activityLogs);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} activity logs for user {UserId}",
                activityLogs.Count, userId);
        }

        // Delete the user
        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            // Log the activity
            await _activityLogService.LogActivityAsync(
                adminId,
                "DeleteUser",
                "User",
                userId,
                $"Deleted user {user.Email}",
                sendNotification: true
            );

            _logger.LogInformation("User {Email} (ID: {UserId}) was deleted by admin {AdminId}",
                user.Email, userId, adminId);

            return (true, Array.Empty<string>());
        }

        return (false, result.Errors.Select(e => e.Description).ToArray());
    }
}
