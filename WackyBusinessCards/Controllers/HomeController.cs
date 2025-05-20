using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WackyBusinessCards.Models;
using WackyBusinessCards.Services;

namespace WackyBusinessCards.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly BusinessCardService _businessCardService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AdminInitializationService _adminInitService;

    public HomeController(
        ILogger<HomeController> logger,
        BusinessCardService businessCardService,
        UserManager<ApplicationUser> userManager,
        AdminInitializationService adminInitService)
    {
        _logger = logger;
        _businessCardService = businessCardService;
        _userManager = userManager;
        _adminInitService = adminInitService;
    }

    public async Task<IActionResult> Index()
    {
        List<BusinessCard> businessCards;

        if (User.Identity?.IsAuthenticated == true)
        {
            // Get only the user's business cards
            var userId = _userManager.GetUserId(User);
            businessCards = await _businessCardService.GetUserBusinessCardsAsync(userId);
        }
        else
        {
            // For anonymous users, show all cards
            businessCards = await _businessCardService.GetAllBusinessCardsAsync();
        }

        return View(businessCards);
    }

    public async Task<IActionResult> Details(int id)
    {
        BusinessCard? businessCard;

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            businessCard = await _businessCardService.GetUserBusinessCardByIdAsync(id, userId);

            // If the user doesn't own this card, try to get it from the general collection
            if (businessCard == null)
            {
                businessCard = await _businessCardService.GetBusinessCardByIdAsync(id);
                // Set ViewData to indicate this is not the user's card
                ViewData["IsUserCard"] = false;
            }
            else
            {
                ViewData["IsUserCard"] = true;
            }
        }
        else
        {
            businessCard = await _businessCardService.GetBusinessCardByIdAsync(id);
            ViewData["IsUserCard"] = false;
        }

        if (businessCard == null)
        {
            return NotFound();
        }

        return View(businessCard);
    }

    // GET: Home/Create
    [Authorize]
    public IActionResult Create()
    {
        var viewModel = new BusinessCardViewModel();
        PopulateDropdownLists(viewModel);
        return View(viewModel);
    }

    // POST: Home/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(BusinessCardViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var businessCard = viewModel.ToBusinessCard();

            // Set the user ID
            businessCard.UserId = _userManager.GetUserId(User);

            await _businessCardService.CreateBusinessCardAsync(businessCard);
            return RedirectToAction(nameof(Index));
        }

        PopulateDropdownLists(viewModel);
        return View(viewModel);
    }

    // GET: Home/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _userManager.GetUserId(User);
        var businessCard = await _businessCardService.GetUserBusinessCardByIdAsync(id, userId);

        if (businessCard == null)
        {
            return NotFound();
        }

        var viewModel = BusinessCardViewModel.FromBusinessCard(businessCard);
        PopulateDropdownLists(viewModel);
        return View(viewModel);
    }

    // POST: Home/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int id, BusinessCardViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        // Check if the user owns this card
        var userId = _userManager.GetUserId(User);
        if (!await _businessCardService.UserOwnsBusinessCardAsync(id, userId))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            var businessCard = viewModel.ToBusinessCard();

            // Ensure the user ID is preserved
            businessCard.UserId = userId;

            var result = await _businessCardService.UpdateBusinessCardAsync(businessCard);

            if (result == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        PopulateDropdownLists(viewModel);
        return View(viewModel);
    }

    // GET: Home/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User);
        var businessCard = await _businessCardService.GetUserBusinessCardByIdAsync(id, userId);

        if (businessCard == null)
        {
            return NotFound();
        }

        return View(businessCard);
    }

    // POST: Home/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Check if the user owns this card
        var userId = _userManager.GetUserId(User);
        if (!await _businessCardService.UserOwnsBusinessCardAsync(id, userId))
        {
            return Forbid();
        }

        var result = await _businessCardService.DeleteBusinessCardAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Special action to initialize admin user
    public async Task<IActionResult> InitializeAdmin()
    {
        try
        {
            await _adminInitService.InitializeAsync();
            return Content("Admin user has been initialized successfully. You can now log in with admin@example.com / Admin@123");
        }
        catch (Exception ex)
        {
            return Content($"Error initializing admin user: {ex.Message}");
        }
    }

    // Helper method to populate dropdown lists
    private void PopulateDropdownLists(BusinessCardViewModel viewModel)
    {
        viewModel.FontFamilyOptions = _businessCardService.GetFontFamilies()
            .Select(f => new SelectListItem { Text = f, Value = f })
            .ToList();

        viewModel.BorderStyleOptions = _businessCardService.GetBorderStyles()
            .Select(b => new SelectListItem { Text = b, Value = b })
            .ToList();

        viewModel.SpecialEffectOptions = _businessCardService.GetSpecialEffects()
            .Select(e => new SelectListItem { Text = e, Value = e })
            .ToList();
    }
}
