using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WackyBusinessCards.Constants;
using WackyBusinessCards.Data;
using WackyBusinessCards.Models;
using WackyBusinessCards.Services;
using WackyBusinessCards.ViewModels;

namespace WackyBusinessCards.Controllers;

[Authorize(Policy = "RequireAdminRole")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly BusinessCardService _businessCardService;
    private readonly ActivityLogService _activityLogService;
    private readonly UserManagementService _userManagementService;
    private readonly StatisticsService _statisticsService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        BusinessCardService businessCardService,
        ActivityLogService activityLogService,
        UserManagementService userManagementService,
        StatisticsService statisticsService,
        ILogger<AdminController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _businessCardService = businessCardService;
        _activityLogService = activityLogService;
        _userManagementService = userManagementService;
        _statisticsService = statisticsService;
        _logger = logger;
    }

    // GET: Admin
    public async Task<IActionResult> Index()
    {
        var stats = await _statisticsService.GetDashboardStatisticsAsync();
        return View(stats);
    }

    // GET: Admin/Users
    public async Task<IActionResult> Users()
    {
        var users = await _userManagementService.GetAllUsersAsync();
        return View(users);
    }

    // GET: Admin/UserDetails/5
    public async Task<IActionResult> UserDetails(string id)
    {
        var model = await _userManagementService.GetUserDetailsAsync(id);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    // POST: Admin/ToggleRole
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRole(string userId, string role)
    {
        var adminId = _userManager.GetUserId(User);
        var result = await _userManagementService.ToggleUserRoleAsync(userId, role, adminId);

        if (!result.Success)
        {
            foreach (var error in result.Errors)
            {
                TempData["ErrorMessage"] = error;
            }
        }

        return RedirectToAction(nameof(UserDetails), new { id = userId });
    }

    // GET: Admin/BusinessCards
    public async Task<IActionResult> BusinessCards()
    {
        var cards = await _context.BusinessCards
            .Include(bc => bc.User)
            .OrderByDescending(bc => bc.Id)
            .ToListAsync();

        return View(cards);
    }

    // GET: Admin/CreateUser
    public async Task<IActionResult> CreateUser()
    {
        var model = new CreateUserViewModel
        {
            AvailableRoles = await _userManagementService.GetAvailableRolesAsync()
        };
        return View(model);
    }

    // POST: Admin/CreateUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        // Debug information
        _logger.LogInformation("Creating user with IsAdmin={IsAdmin}", model.IsAdmin);

        if (ModelState.IsValid)
        {
            var adminId = _userManager.GetUserId(User);

            // Ensure IsAdmin is properly set
            _logger.LogInformation("Creating user {Email} with IsAdmin={IsAdmin}", model.Email, model.IsAdmin);

            var result = await _userManagementService.CreateUserAsync(model, adminId);

            if (result.Success)
            {
                var roleMessage = model.IsAdmin ? "with Admin privileges" : "with standard User privileges";
                TempData["SuccessMessage"] = $"User {model.Email} created successfully {roleMessage}.";

                // Log this important action
                await _activityLogService.LogActivityAsync(
                    adminId,
                    "CreateUser",
                    "User",
                    null,
                    $"Created new user {model.Email} {roleMessage}"
                );

                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        // If we got this far, something failed, redisplay form with available roles
        model.AvailableRoles = await _userManagementService.GetAvailableRolesAsync();
        return View(model);
    }

    // GET: Admin/ActivityLogs
    public async Task<IActionResult> ActivityLogs()
    {
        var logs = await _activityLogService.GetRecentActivityLogsAsync(100);
        return View(logs);
    }

    // POST: Admin/MakeAdmin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeAdmin(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "User ID is required.";
            return RedirectToAction(nameof(Users));
        }

        var adminId = _userManager.GetUserId(User);
        var result = await _userManagementService.ToggleUserRoleAsync(userId, Roles.Admin, adminId);

        if (result.Success)
        {
            var user = await _userManager.FindByIdAsync(userId);
            TempData["SuccessMessage"] = $"User {user?.Email} has been made an admin successfully.";

            // Log this important action
            await _activityLogService.LogActivityAsync(
                adminId,
                "MakeAdmin",
                "User",
                userId,
                $"User {user?.Email} was granted admin privileges"
            );
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(", ", result.Errors);
        }

        return RedirectToAction(nameof(Users));
    }

    // POST: Admin/DeleteUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "User ID is required.";
            return RedirectToAction(nameof(Users));
        }

        // Don't allow admins to delete themselves
        var currentUserId = _userManager.GetUserId(User);
        if (userId == currentUserId)
        {
            TempData["ErrorMessage"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Users));
        }

        _logger.LogInformation("Attempting to delete user {UserId} by admin {AdminId}", userId, currentUserId);

        var result = await _userManagementService.DeleteUserAsync(userId, currentUserId);

        if (result.Success)
        {
            TempData["SuccessMessage"] = "User deleted successfully.";
            _logger.LogInformation("User {UserId} deleted successfully by admin {AdminId}", userId, currentUserId);
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(", ", result.Errors);
            _logger.LogWarning("Failed to delete user {UserId}: {Errors}", userId, string.Join(", ", result.Errors));
        }

        return RedirectToAction(nameof(Users));
    }
}
