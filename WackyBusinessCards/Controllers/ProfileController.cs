using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WackyBusinessCards.Data;
using WackyBusinessCards.Models;
using WackyBusinessCards.Services;

namespace WackyBusinessCards.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly BusinessCardService _businessCardService;
    private readonly FileUploadService _fileUploadService;
    private readonly ApplicationDbContext _context;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        BusinessCardService businessCardService,
        FileUploadService fileUploadService,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _businessCardService = businessCardService;
        _fileUploadService = fileUploadService;
        _context = context;
    }

    // GET: Profile
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var userId = user.Id;
        var cards = await _businessCardService.GetUserBusinessCardsAsync(userId);

        var viewModel = new UserProfileViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            UserName = user.UserName ?? string.Empty,
            ProfilePicturePath = user.ProfilePicturePath,
            BusinessCardCount = cards.Count,
            MemberSince = user.CreatedAt ?? DateTime.Now
        };

        return View(viewModel);
    }

    // GET: Profile/Edit
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var viewModel = new UserProfileViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            UserName = user.UserName ?? string.Empty,
            ProfilePicturePath = user.ProfilePicturePath
        };

        return View(viewModel);
    }

    // POST: Profile/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserProfileViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Update user properties
        user.FirstName = viewModel.FirstName;
        user.LastName = viewModel.LastName;
        user.PhoneNumber = viewModel.PhoneNumber;

        // Handle profile picture upload
        if (viewModel.ProfilePictureFile != null && viewModel.ProfilePictureFile.Length > 0)
        {
            try
            {
                // Delete the old profile picture if it exists
                if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                {
                    _fileUploadService.DeleteProfilePicture(user.ProfilePicturePath);
                }

                // Upload the new profile picture
                var profilePicturePath = await _fileUploadService.UploadProfilePictureAsync(
                    viewModel.ProfilePictureFile,
                    user.Id);

                // Update the user's profile picture path
                user.ProfilePicturePath = profilePicturePath;
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("ProfilePictureFile", ex.Message);
                return View(viewModel);
            }
            catch (Exception)
            {
                ModelState.AddModelError("ProfilePictureFile", "An error occurred while uploading the profile picture.");
                return View(viewModel);
            }
        }

        // Email change requires additional verification, so we're not updating it here
        // If you want to implement email change, you'd need to add email confirmation

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(viewModel);
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Profile/DeleteProfilePicture
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProfilePicture()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(user.ProfilePicturePath))
        {
            // Delete the profile picture file
            _fileUploadService.DeleteProfilePicture(user.ProfilePicturePath);

            // Update the user's profile picture path
            user.ProfilePicturePath = null;
            await _userManager.UpdateAsync(user);
        }

        return RedirectToAction(nameof(Edit));
    }

    // GET: Profile/Cards
    public async Task<IActionResult> Cards()
    {
        var userId = _userManager.GetUserId(User);
        var cards = await _businessCardService.GetUserBusinessCardsAsync(userId);
        return View(cards);
    }
}
