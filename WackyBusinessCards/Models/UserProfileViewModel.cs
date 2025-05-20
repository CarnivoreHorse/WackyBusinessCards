using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WackyBusinessCards.Models;

public class UserProfileViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    // Profile picture
    [Display(Name = "Profile Picture")]
    public string? ProfilePicturePath { get; set; }

    [Display(Name = "Upload New Picture")]
    public IFormFile? ProfilePictureFile { get; set; }

    // Statistics
    [Display(Name = "Number of Business Cards")]
    public int BusinessCardCount { get; set; }

    [Display(Name = "Member Since")]
    [DataType(DataType.Date)]
    public DateTime MemberSince { get; set; }

    // Helper method to get the profile picture URL
    public string GetProfilePictureUrl()
    {
        if (string.IsNullOrEmpty(ProfilePicturePath))
        {
            // Return a URL for a generated avatar if no profile picture is set
            return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString($"{FirstName} {LastName}".Trim())}&background=random&size=256";
        }

        return ProfilePicturePath;
    }
}
