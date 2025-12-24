using Abig2025.Models.DTO;
using Abig2025.Models.Properties;

namespace Abig2025.Services.Interfaces
{
    public interface IPropertyService
    {
        Task<int> CreatePropertyAsync(Property property);
        Task<Property?> GetPropertyByIdAsync(int propertyId);
        Task<bool> UpdatePropertyAsync(Property property);
        Task<bool> DeletePropertyAsync(int propertyId);
        Task<List<Property>> GetUserPropertiesAsync(int userId);
        Task<bool> PublishPropertyAsync(int propertyId);
        Task<bool> UnpublishPropertyAsync(int propertyId);
        Task<int> CreatePropertyFromDraftAsync(PropertyTempData draftData, int userId);
    }
}