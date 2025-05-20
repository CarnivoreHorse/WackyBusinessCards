namespace WackyBusinessCards.Models;

public class BusinessCard
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    // Wacky styling properties
    public string BackgroundColor { get; set; } = "#ffffff";
    public string TextColor { get; set; } = "#000000";
    public string FontFamily { get; set; } = "Arial, sans-serif";
    public string BorderStyle { get; set; } = "solid";
    public string BorderColor { get; set; } = "#000000";
    public int BorderWidth { get; set; } = 1;
    public int BorderRadius { get; set; } = 5;
    public string ImageUrl { get; set; } = string.Empty;
    public string SpecialEffect { get; set; } = "none"; // Options: none, rotate, shadow, etc.

    // User relationship
    public string? UserId { get; set; }
    public virtual ApplicationUser? User { get; set; }
}
