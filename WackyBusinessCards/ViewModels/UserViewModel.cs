using System;
using System.Collections.Generic;

namespace WackyBusinessCards.ViewModels;

public class UserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int BusinessCardCount { get; set; }
    public string? ProfilePicturePath { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();

    // Helper method to get the profile picture URL
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
