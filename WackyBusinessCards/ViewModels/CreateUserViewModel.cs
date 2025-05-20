using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WackyBusinessCards.ViewModels;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Bio")]
    [StringLength(500, ErrorMessage = "Bio cannot be longer than 500 characters")]
    public string? Bio { get; set; }

    [Display(Name = "Job Title")]
    [StringLength(100)]
    public string? JobTitle { get; set; }

    [Display(Name = "Company")]
    [StringLength(100)]
    public string? Company { get; set; }

    [Display(Name = "Profile Picture")]
    public IFormFile? ProfilePicture { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    // Roles and settings
    [Display(Name = "Admin privileges")]
    public bool IsAdmin { get; set; } = false;

    [Display(Name = "Email Confirmed")]
    public bool EmailConfirmed { get; set; } = true;

    [Display(Name = "Send Welcome Email")]
    public bool SendWelcomeEmail { get; set; } = true;

    // For future use with additional roles
    public List<string> SelectedRoles { get; set; } = new List<string>();

    // For displaying available roles in the UI
    public List<RoleViewModel> AvailableRoles { get; set; } = new List<RoleViewModel>();
}
