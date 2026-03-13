using FishCash.Models;

namespace FishCash.Services;

/// <summary>
/// Interface for partner (supplier/buyer) management
/// </summary>
public interface IPartnerService
{
    Task<List<Partner>> GetPartnersAsync(PartnerType? filterType = null);
    Task<Partner?> GetPartnerByIdAsync(int id);
    Task<Partner> AddPartnerAsync(Partner partner);
    Task UpdatePartnerAsync(Partner partner);
    Task DeletePartnerAsync(int id);
}
