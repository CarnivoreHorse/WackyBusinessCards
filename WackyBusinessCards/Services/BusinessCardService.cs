using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WackyBusinessCards.Data;
using WackyBusinessCards.Models;

namespace WackyBusinessCards.Services;

public class BusinessCardService
{
    private readonly ApplicationDbContext _context;

    public BusinessCardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BusinessCard>> GetAllBusinessCardsAsync()
    {
        return await _context.BusinessCards.ToListAsync();
    }

    public async Task<List<BusinessCard>> GetUserBusinessCardsAsync(string userId)
    {
        return await _context.BusinessCards
            .Where(bc => bc.UserId == userId)
            .ToListAsync();
    }

    public async Task<BusinessCard?> GetBusinessCardByIdAsync(int id)
    {
        return await _context.BusinessCards.FindAsync(id);
    }

    public async Task<BusinessCard?> GetUserBusinessCardByIdAsync(int id, string userId)
    {
        return await _context.BusinessCards
            .FirstOrDefaultAsync(bc => bc.Id == id && bc.UserId == userId);
    }

    public async Task<BusinessCard> CreateBusinessCardAsync(BusinessCard businessCard)
    {
        _context.BusinessCards.Add(businessCard);
        await _context.SaveChangesAsync();

        return businessCard;
    }

    public async Task<BusinessCard?> UpdateBusinessCardAsync(BusinessCard businessCard)
    {
        var existingCard = await _context.BusinessCards.FindAsync(businessCard.Id);
        if (existingCard == null)
        {
            return null;
        }

        // Update the existing card properties
        _context.Entry(existingCard).CurrentValues.SetValues(businessCard);

        // Save changes to the database
        await _context.SaveChangesAsync();

        return existingCard;
    }

    public async Task<bool> DeleteBusinessCardAsync(int id)
    {
        var businessCard = await _context.BusinessCards.FindAsync(id);
        if (businessCard == null)
        {
            return false;
        }

        _context.BusinessCards.Remove(businessCard);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UserOwnsBusinessCardAsync(int id, string userId)
    {
        var card = await _context.BusinessCards.FindAsync(id);
        return card != null && card.UserId == userId;
    }

    // Helper method to get available font families
    public List<string> GetFontFamilies()
    {
        return new List<string>
        {
            "Arial, sans-serif",
            "'Comic Sans MS', cursive",
            "'Indie Flower', cursive",
            "'Pirata One', cursive",
            "Georgia, serif",
            "Verdana, sans-serif",
            "Courier New, monospace"
        };
    }

    // Helper method to get available border styles
    public List<string> GetBorderStyles()
    {
        return new List<string>
        {
            "solid",
            "dashed",
            "dotted",
            "double",
            "groove",
            "ridge",
            "inset",
            "outset"
        };
    }

    // Helper method to get available special effects
    public List<string> GetSpecialEffects()
    {
        return new List<string>
        {
            "none",
            "rotate",
            "shadow",
            "sparkle"
        };
    }
}
