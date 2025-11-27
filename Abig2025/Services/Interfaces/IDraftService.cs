using Abig2025.Models.DTO;
using Abig2025.Models.Properties;

namespace Abig2025.Services
{
    public interface IDraftService
    {
        Task<PropertyDraft?> GetDraftAsync(Guid draftId);
        Task<PropertyDraft> CreateDraftAsync(int userId, PropertyTempData data);
        Task UpdateDraftAsync(Guid draftId, PropertyTempData data, int nextStep);
        Task DeleteDraftAsync(Guid draftId);
    }
}

