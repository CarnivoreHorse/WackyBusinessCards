using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace WackyBusinessCards.Models;

public class ApplicationUser : IdentityUser
{
    // Add custom user properties here
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Track when the user was created
    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    // Profile picture
    public string? ProfilePicturePath { get; set; }

    // Navigation property for user's business cards
    public virtual ICollection<BusinessCard> BusinessCards { get; set; } = new List<BusinessCard>();

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
