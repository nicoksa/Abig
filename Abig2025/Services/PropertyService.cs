// Services/PropertyService.cs
using Abig2025.Data;
using Abig2025.Models.DTO;
using Abig2025.Models.Properties;
using Abig2025.Models.Properties.Enums;
using Abig2025.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Abig2025.Helpers;

namespace Abig2025.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PropertyService> _logger;

        public PropertyService(AppDbContext context, ILogger<PropertyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> CreatePropertyAsync(Property property)
        {
            try
            {
                _context.Properties.Add(property);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Propiedad creada con ID: {PropertyId}", property.PropertyId);
                return property.PropertyId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear propiedad");
                throw;
            }
        }

        public async Task<Property?> GetPropertyByIdAsync(int propertyId)
        {
            try
            {
                return await _context.Properties
                    .Include(p => p.Location)
                    .Include(p => p.Status)
                    .Include(p => p.Images)
                    .Include(p => p.Features)
                        .ThenInclude(f => f.FeatureDefinition)
                    .Include(p => p.Owner)
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener propiedad con ID: {PropertyId}", propertyId);
                return null;
            }
        }

        public async Task<bool> UpdatePropertyAsync(Property property)
        {
            try
            {

                _context.Properties.Update(property);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Propiedad actualizada con ID: {PropertyId}", property.PropertyId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar propiedad con ID: {PropertyId}", property.PropertyId);
                return false;
            }
        }

        public async Task<bool> DeletePropertyAsync(int propertyId)
        {
            try
            {
                var property = await GetPropertyByIdAsync(propertyId);
                if (property == null)
                    return false;

                // Cambiar estado a inactivo en lugar de eliminar
                property.IsActive = false;
                await UpdatePropertyAsync(property);

                _logger.LogInformation("Propiedad desactivada con ID: {PropertyId}", propertyId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar propiedad con ID: {PropertyId}", propertyId);
                return false;
            }
        }

        public async Task<List<Property>> GetUserPropertiesAsync(int userId)
        {
            try
            {
                return await _context.Properties
                    .Include(p => p.Location)
                    .Include(p => p.Images)
                    .Where(p => p.OwnerId == userId && p.IsActive)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener propiedades del usuario {UserId}", userId);
                return new List<Property>();
            }
        }

        public async Task<bool> PublishPropertyAsync(int propertyId)
        {
            try
            {
                var property = await GetPropertyByIdAsync(propertyId);
                if (property == null)
                    return false;

                // Crear o actualizar el estado
                var status = property.Status ?? new PropertyStatus { PropertyId = propertyId };
                status.State = PropertyState.Publicado;
                status.UpdatedAt = HoraArgentina.Now;

                if (property.Status == null)
                {
                    _context.PropertyStatuses.Add(status);
                }
                else
                {
                    _context.PropertyStatuses.Update(status);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Propiedad publicada con ID: {PropertyId}", propertyId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al publicar propiedad con ID: {PropertyId}", propertyId);
                return false;
            }
        }

        public async Task<bool> UnpublishPropertyAsync(int propertyId)
        {
            try
            {
                var property = await GetPropertyByIdAsync(propertyId);
                if (property == null || property.Status == null)
                    return false;

                property.Status.State = PropertyState.Pausado;
                property.Status.UpdatedAt = HoraArgentina.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Propiedad despublicada con ID: {PropertyId}", propertyId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al despublicar propiedad con ID: {PropertyId}", propertyId);
                return false;
            }
        }

        public async Task<int> CreatePropertyFromDraftAsync(PropertyTempData draftData, int userId)
        {
            try
            {
                // Crear propiedad principal
                var property = new Property
                {
                    OwnerId = userId,
                    Title = draftData.Title,
                    Description = draftData.Description,
                    Price = draftData.Price,
                    Currency = draftData.Currency,
                    OperationType = draftData.OperationType,
                    PropertyType = draftData.PropertyType,
                    CreatedAt = HoraArgentina.Now,
                    IsActive = true
                };

                // Crear ubicación
                var location = new PropertyLocation
                {
                    Street = draftData.Street,
                    Number = draftData.Number,
                    PostalCode = draftData.PostalCode,
                    Latitude = (double?)draftData.Latitude,
                    Longitude = (double?)draftData.Longitude,
                    CityName = draftData.City,
                    ProvinceName = draftData.Province
                };

                // Si tenemos IDs, los asignamos
                if (draftData.ProvinceId.HasValue)
                    location.ProvinceId = draftData.ProvinceId;
                if (draftData.CityId.HasValue)
                    location.CityId = draftData.CityId;
                if (draftData.NeighborhoodId.HasValue)
                    location.NeighborhoodId = draftData.NeighborhoodId;

                property.Location = location;

                // Crear características principales (si existen en draftData)
                if (draftData.MainRooms.HasValue ||
                    draftData.Bedrooms.HasValue ||
                    draftData.Bathrooms.HasValue ||
                    draftData.ParkingSpaces.HasValue ||
                    draftData.CoveredArea.HasValue ||
                    draftData.TotalArea.HasValue)
                {
                    // Necesitarías una entidad PropertyMainFeatures si la tienes
                    // Por ahora, estos datos pueden ir en campos adicionales
                }

                // Agregar propiedad al contexto
                _context.Properties.Add(property);
                await _context.SaveChangesAsync();

                // Crear características (features)
                if (draftData.Features?.Count > 0)
                {
                    var propertyFeatures = draftData.Features
                        .Where(f => f.Value == "true")
                        .Select(f => new PropertyFeature
                        {
                            PropertyId = property.PropertyId,
                            FeatureDefinitionId = f.FeatureDefinitionId,
                            Value = f.Value
                        }).ToList();

                    if (propertyFeatures.Any())
                    {
                        _context.PropertyFeatures.AddRange(propertyFeatures);
                    }
                }

                // Crear estado inicial
                var status = new PropertyStatus
                {
                    PropertyId = property.PropertyId,
                    State = PropertyState.Publicado,
                    UpdatedAt = HoraArgentina.Now,
                    Notes = "Publicado desde borrador"
                };

                _context.PropertyStatuses.Add(status);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Propiedad creada desde borrador con ID: {PropertyId}", property.PropertyId);
                return property.PropertyId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear propiedad desde borrador");
                throw;
            }
        }
    }
}