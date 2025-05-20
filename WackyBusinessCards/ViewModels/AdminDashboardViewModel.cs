using System.Collections.Generic;
using WackyBusinessCards.Models;

namespace WackyBusinessCards.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalBusinessCards { get; set; }
    public int TotalAdmins { get; set; }

    public List<UserViewModel> RecentUsers { get; set; } = new List<UserViewModel>();
    public List<ActivityLog> RecentActivity { get; set; } = new List<ActivityLog>();

    // Chart data
    public Dictionary<string, int> UserGrowthData { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> CardGrowthData { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> ActivityStatistics { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> PopularCardStyles { get; set; } = new Dictionary<string, int>();
}
