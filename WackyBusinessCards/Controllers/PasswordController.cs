using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WackyBusinessCards.Models;
using WackyBusinessCards.Services;
using WackyBusinessCards.ViewModels;

namespace WackyBusinessCards.Controllers;

public class PasswordController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailService _emailService;
    private readonly ILogger<PasswordController> _logger;
    private readonly IConfiguration _configuration;

    public PasswordController(
        UserManager<ApplicationUser> userManager,
        EmailService emailService,
        ILogger<PasswordController> logger,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    // GET: /Password/ForgotPassword
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // POST: /Password/ForgotPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // Generate the reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Create reset link
            var baseUrl = _configuration["ApplicationUrl"];
            var callbackUrl = $"{baseUrl}/Password/ResetPassword?userId={HttpUtility.UrlEncode(user.Id)}&token={HttpUtility.UrlEncode(token)}";
            
            // Send email
            await _emailService.SendPasswordResetEmailAsync(model.Email, callbackUrl);
            
            _logger.LogInformation("Password reset email sent to {Email}", model.Email);
            
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        return View(model);
    }

    // GET: /Password/ForgotPasswordConfirmation
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    // GET: /Password/ResetPassword
    public IActionResult ResetPassword(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return BadRequest("Invalid password reset link");
        }

        var model = new ResetPasswordViewModel
        {
            UserId = userId,
            Token = token
        };

        return View(model);
    }

    // POST: /Password/ResetPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (result.Succeeded)
        {
            _logger.LogInformation("Password reset successful for user {Email}", user.Email);
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        
        return View(model);
    }

    // GET: /Password/ResetPasswordConfirmation
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }
}
