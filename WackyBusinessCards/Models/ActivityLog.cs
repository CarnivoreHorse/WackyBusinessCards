using System;
using System.ComponentModel.DataAnnotations;

namespace WackyBusinessCards.Models;

public class ActivityLog
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string Action { get; set; } = string.Empty;
    
    [Required]
    public string EntityType { get; set; } = string.Empty;
    
    public string? EntityId { get; set; }
    
    public string? Details { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.Now;
    
    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}
