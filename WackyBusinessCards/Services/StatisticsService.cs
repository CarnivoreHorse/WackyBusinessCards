using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WackyBusinessCards.Constants;
using WackyBusinessCards.Data;
using WackyBusinessCards.Models;
using WackyBusinessCards.ViewModels;

namespace WackyBusinessCards.Services;

public class StatisticsService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ActivityLogService _activityLogService;

    public StatisticsService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ActivityLogService activityLogService)
    {
        _context = context;
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    public async Task<AdminDashboardViewModel> GetDashboardStatisticsAsync()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var totalBusinessCards = await _context.BusinessCards.CountAsync();
        var totalAdmins = (await _userManager.GetUsersInRoleAsync(Roles.Admin)).Count;
        
        var recentUsers = await _userManager.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
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

        var recentActivity = await _activityLogService.GetRecentActivityLogsAsync(10);
        
        var userGrowth = await GetUserGrowthDataAsync(30);
        var cardGrowth = await GetBusinessCardGrowthDataAsync(30);
        var activityStats = await _activityLogService.GetActivityStatisticsAsync(7);
        
        var popularCardStyles = await GetPopularCardStylesAsync();
        
        return new AdminDashboardViewModel
        {
            TotalUsers = totalUsers,
            TotalBusinessCards = totalBusinessCards,
            TotalAdmins = totalAdmins,
            RecentUsers = recentUsers,
            RecentActivity = recentActivity,
            UserGrowthData = userGrowth,
            CardGrowthData = cardGrowth,
            ActivityStatistics = activityStats,
            PopularCardStyles = popularCardStyles
        };
    }

    private async Task<Dictionary<string, int>> GetUserGrowthDataAsync(int days)
    {
        var result = new Dictionary<string, int>();
        var startDate = DateTime.Now.AddDays(-days).Date;
        
        var usersByDate = await _userManager.Users
            .Where(u => u.CreatedAt >= startDate)
            .GroupBy(u => u.CreatedAt!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);
            
        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var dateString = date.ToString("MM/dd");
            result[dateString] = usersByDate.ContainsKey(date) ? usersByDate[date] : 0;
        }
        
        return result;
    }
    
    private async Task<Dictionary<string, int>> GetBusinessCardGrowthDataAsync(int days)
    {
        var result = new Dictionary<string, int>();
        var startDate = DateTime.Now.AddDays(-days).Date;
        
        // For simplicity, we'll use the database ID as a proxy for creation date
        // In a real app, you'd have a CreatedAt field on the BusinessCard model
        var cardCounts = await _context.BusinessCards
            .GroupBy(b => b.Id % days) // This is just a placeholder for demo purposes
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Day, x => x.Count);
            
        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var dateString = date.ToString("MM/dd");
            result[dateString] = cardCounts.ContainsKey(i) ? cardCounts[i] : 0;
        }
        
        return result;
    }
    
    private async Task<Dictionary<string, int>> GetPopularCardStylesAsync()
    {
        var backgroundColors = await _context.BusinessCards
            .GroupBy(b => b.BackgroundColor)
            .Select(g => new { Color = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToDictionaryAsync(x => x.Color, x => x.Count);
            
        return backgroundColors;
    }
}
