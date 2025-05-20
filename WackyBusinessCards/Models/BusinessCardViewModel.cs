using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WackyBusinessCards.Models;

public class BusinessCardViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Company is required")]
    [StringLength(100, ErrorMessage = "Company cannot be longer than 100 characters")]
    public string Company { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Phone is required")]
    [StringLength(20, ErrorMessage = "Phone cannot be longer than 20 characters")]
    public string Phone { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "Website cannot be longer than 100 characters")]
    public string Website { get; set; } = string.Empty;
    
    [StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters")]
    public string Address { get; set; } = string.Empty;
    
    // Wacky styling properties
    [Required(ErrorMessage = "Background color is required")]
    [RegularExpression("^#([A-Fa-f0-9]{6})$", ErrorMessage = "Background color must be a valid hex color code (e.g., #FF5733)")]
    public string BackgroundColor { get; set; } = "#ffffff";
    
    [Required(ErrorMessage = "Text color is required")]
    [RegularExpression("^#([A-Fa-f0-9]{6})$", ErrorMessage = "Text color must be a valid hex color code (e.g., #000000)")]
    public string TextColor { get; set; } = "#000000";
    
    [Required(ErrorMessage = "Font family is required")]
    public string FontFamily { get; set; } = "Arial, sans-serif";
    
    [Required(ErrorMessage = "Border style is required")]
    public string BorderStyle { get; set; } = "solid";
    
    [Required(ErrorMessage = "Border color is required")]
    [RegularExpression("^#([A-Fa-f0-9]{6})$", ErrorMessage = "Border color must be a valid hex color code (e.g., #000000)")]
    public string BorderColor { get; set; } = "#000000";
    
    [Required(ErrorMessage = "Border width is required")]
    [Range(0, 20, ErrorMessage = "Border width must be between 0 and 20 pixels")]
    public int BorderWidth { get; set; } = 1;
    
    [Required(ErrorMessage = "Border radius is required")]
    [Range(0, 50, ErrorMessage = "Border radius must be between 0 and 50 pixels")]
    public int BorderRadius { get; set; } = 5;
    
    [StringLength(200, ErrorMessage = "Image URL cannot be longer than 200 characters")]
    public string ImageUrl { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Special effect is required")]
    public string SpecialEffect { get; set; } = "none";
    
    // Dropdown options for the form
    public List<SelectListItem>? FontFamilyOptions { get; set; }
    public List<SelectListItem>? BorderStyleOptions { get; set; }
    public List<SelectListItem>? SpecialEffectOptions { get; set; }
    
    // Convert from ViewModel to Model
    public BusinessCard ToBusinessCard()
    {
        return new BusinessCard
        {
            Id = Id,
            Name = Name,
            Title = Title,
            Company = Company,
            Email = Email,
            Phone = Phone,
            Website = Website,
            Address = Address,
            BackgroundColor = BackgroundColor,
            TextColor = TextColor,
            FontFamily = FontFamily,
            BorderStyle = BorderStyle,
            BorderColor = BorderColor,
            BorderWidth = BorderWidth,
            BorderRadius = BorderRadius,
            ImageUrl = ImageUrl,
            SpecialEffect = SpecialEffect
        };
    }
    
    // Convert from Model to ViewModel
    public static BusinessCardViewModel FromBusinessCard(BusinessCard businessCard)
    {
        return new BusinessCardViewModel
        {
            Id = businessCard.Id,
            Name = businessCard.Name,
            Title = businessCard.Title,
            Company = businessCard.Company,
            Email = businessCard.Email,
            Phone = businessCard.Phone,
            Website = businessCard.Website,
            Address = businessCard.Address,
            BackgroundColor = businessCard.BackgroundColor,
            TextColor = businessCard.TextColor,
            FontFamily = businessCard.FontFamily,
            BorderStyle = businessCard.BorderStyle,
            BorderColor = businessCard.BorderColor,
            BorderWidth = businessCard.BorderWidth,
            BorderRadius = businessCard.BorderRadius,
            ImageUrl = businessCard.ImageUrl,
            SpecialEffect = businessCard.SpecialEffect
        };
    }
}
