using FishCash.Data;
using FishCash.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FishCash.Services;

/// <summary>
/// Service for managing trading partners (suppliers and buyers)
/// </summary>
public class PartnerService : IPartnerService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PartnerService> _logger;

    public PartnerService(AppDbContext context, ILogger<PartnerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Partner>> GetPartnersAsync(PartnerType? filterType = null)
    {
        try
        {
            var query = _context.Partners.Where(p => p.IsActive);
            if (filterType.HasValue)
                query = query.Where(p => p.PartnerType == filterType.Value);

            return await query.OrderBy(p => p.Name).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading partners");
            throw;
        }
    }

    public async Task<Partner?> GetPartnerByIdAsync(int id)
    {
        try
        {
            return await _context.Partners.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading partner {Id}", id);
            throw;
        }
    }

    public async Task<Partner> AddPartnerAsync(Partner partner)
    {
        try
        {
            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Partner added: {Name} ({Type})", partner.Name, partner.PartnerType);
            return partner;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding partner {Name}", partner.Name);
            throw;
        }
    }

    public async Task UpdatePartnerAsync(Partner partner)
    {
        try
        {
            _context.Partners.Update(partner);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Partner updated: {Name}", partner.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating partner {Id}", partner.Id);
            throw;
        }
    }

    public async Task DeletePartnerAsync(int id)
    {
        try
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner != null)
            {
                partner.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
                _logger.LogInformation("Partner soft-deleted: {Name}", partner.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting partner {Id}", id);
            throw;
        }
    }
}
