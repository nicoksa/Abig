using Abig2025.Data;
using Abig2025.Models.Properties;
using Abig2025.Models.Properties.Enums;
using Abig2025.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class FeatureService : IFeatureService
{
    private readonly AppDbContext _context;

    public FeatureService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<FeatureDefinition>> GetFeaturesForProperty(PropertyType propertyType)
    {
    
        return await _context.FeatureDefinitions
            .Where(f => f.IsActive &&
                       (f.Scope == FeatureScope.All ||
                        (f.Scope == FeatureScope.Urban && propertyType != PropertyType.Campo) ||
                        (f.Scope == FeatureScope.Field && propertyType == PropertyType.Campo)))
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();
   
    }
}
