using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WackyBusinessCards.Data;
using WackyBusinessCards.Models;

namespace WackyBusinessCards.Services;

public class ActivityLogService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ActivityLogService> _logger;
    private readonly EmailService _emailService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ActivityLogService(
        ApplicationDbContext context,
        ILogger<ActivityLogService> logger,
        EmailService emailService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
        _userManager = userManager;
    }

    public async Task LogActivityAsync(string userId, string action, string entityType, string? entityId = null, string? details = null, bool sendNotification = false)
    {
        var activityLog = new ActivityLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            Timestamp = DateTime.Now
        };

        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Activity logged: {Action} by user {UserId} on {EntityType} {EntityId}",
            action, userId, entityType, entityId ?? "N/A");

        // Send email notification if requested
        if (sendNotification)
        {
            await SendActivityNotificationAsync(userId, action, details);
        }
    }

    private async Task SendActivityNotificationAsync(string userId, string action, string? details)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _emailService.SendAccountActivityNotificationAsync(
                    user.Email,
                    action,
                    details ?? "No additional details available"
                );

                _logger.LogInformation("Activity notification email sent to {Email} for {Action}",
                    user.Email, action);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send activity notification email to user {UserId} for {Action}",
                userId, action);
        }
    }

    public async Task<List<ActivityLog>> GetRecentActivityLogsAsync(int count = 20)
    {
        return await _context.ActivityLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<ActivityLog>> GetUserActivityLogsAsync(string userId, int count = 20)
    {
        return await _context.ActivityLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<ActivityLog>> GetActivityLogsByEntityAsync(string entityType, string entityId, int count = 20)
    {
        return await _context.ActivityLogs
            .Include(a => a.User)
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetActivityStatisticsAsync(int days = 7)
    {
        var cutoffDate = DateTime.Now.AddDays(-days);

        var activityCounts = await _context.ActivityLogs
            .Where(a => a.Timestamp >= cutoffDate)
            .GroupBy(a => a.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Action, x => x.Count);

        return activityCounts;
    }
}
