using System;
using System.Collections.Generic;
using WackyBusinessCards.Models;

namespace WackyBusinessCards.ViewModels;

public class UserDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePicturePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public List<BusinessCard> BusinessCards { get; set; } = new List<BusinessCard>();
    public List<ActivityLog> RecentActivity { get; set; } = new List<ActivityLog>();

    // Helper property to get the full name
    public string FullName => $"{FirstName} {LastName}".Trim();

    // Helper property to get the profile picture URL
    public string GetProfilePictureUrl()
    {
        if (string.IsNullOrEmpty(ProfilePicturePath))
        {
            // Return a URL for a generated avatar if no profile picture is set
            return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(FullName)}&background=random&size=256";
        }

        return ProfilePicturePath;
    }
}
