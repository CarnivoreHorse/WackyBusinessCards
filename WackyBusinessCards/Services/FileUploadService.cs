using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WackyBusinessCards.Services;

public class FileUploadService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public FileUploadService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<string> UploadProfilePictureAsync(IFormFile file, string userId)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("No file was uploaded.");
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
        {
            throw new ArgumentException("Invalid file type. Only JPG, PNG, and GIF files are allowed.");
        }

        // Validate file size (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
        {
            throw new ArgumentException("File size exceeds the limit of 5MB.");
        }

        // Create directory if it doesn't exist
        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile-pictures");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Generate a unique filename
        var uniqueFileName = $"{userId}_{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Save the file
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        // Return the relative path to the file
        return $"/uploads/profile-pictures/{uniqueFileName}";
    }

    public void DeleteProfilePicture(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        // Get the absolute path
        var fileName = Path.GetFileName(filePath);
        var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile-pictures", fileName);

        // Delete the file if it exists
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
