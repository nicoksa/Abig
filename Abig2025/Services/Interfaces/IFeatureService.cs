using Abig2025.Models.Properties;
using Abig2025.Models.Properties.Enums;

namespace Abig2025.Services.Interfaces
{
    public interface IFeatureService
    {
        Task<List<FeatureDefinition>> GetFeaturesForProperty(PropertyType propertyType);
    }
}
