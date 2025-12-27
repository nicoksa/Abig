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
        private readonly ISubscriptionService _subscriptionService;

        public PropertyService(AppDbContext context, ILogger<PropertyService> logger, ISubscriptionService subscriptionService)
        {
            _context = context;
            _logger = logger;
            _subscriptionService = subscriptionService;
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

        // Services/PropertyService.cs - VERSIÓN COMPLETA CORREGIDA
        public async Task<int> CreatePropertyFromDraftAsync(PropertyTempData draftData, int userId)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de propiedad para usuario {UserId}", userId);

                // 1. Verificar usuario (solo verificación, NO cargar)
                var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
                if (!userExists)
                {
                    throw new InvalidOperationException($"Usuario con ID {userId} no encontrado");
                }
                _logger.LogInformation("Usuario {UserId} verificado", userId);

                // 2. Obtener suscripción activa
                var subscription = await _subscriptionService.GetActiveUserSubscriptionAsync(userId);
                if (subscription == null)
                {
                    throw new InvalidOperationException("Usuario no tiene suscripción activa");
                }

                // 3. Verificar si puede publicar
                var canPublish = await _subscriptionService.CanUserPublishAsync(userId);
                if (!canPublish)
                {
                    throw new InvalidOperationException("No tiene publicaciones disponibles en su plan actual");
                }

                // ========== PASO 1: Crear propiedad PRIMERO ==========
                _logger.LogInformation("Creando propiedad...");
                var property = new Property
                {
                    OwnerId = userId, // SOLO el ID
                    Title = draftData.Title,
                    Description = draftData.Description,
                    Price = draftData.Price,
                    Currency = draftData.Currency,
                    OperationType = draftData.OperationType,
                    PropertyType = draftData.PropertyType,
                    CreatedAt = HoraArgentina.Now,
                    IsActive = true,
                    Location = null, // No asignar aquí
                    Status = null,   // No asignar aquí
                    Images = null,   // No asignar aquí
                    Features = null, // No asignar aquí
                    Favorites = null // No asignar aquí
                };

                // Agregar y guardar SOLO la propiedad
                _context.Properties.Add(property);
                await _context.SaveChangesAsync(); // PRIMER SaveChanges
                _logger.LogInformation("Propiedad creada con ID: {PropertyId}", property.PropertyId);

                // ========== PASO 2: Crear ubicación ==========
                _logger.LogInformation("Creando ubicación para propiedad {PropertyId}...", property.PropertyId);
                var location = new PropertyLocation
                {
                    PropertyId = property.PropertyId,
                    Street = draftData.Street,
                    Number = draftData.Number,
                    PostalCode = draftData.PostalCode,
                    Latitude = (double?)draftData.Latitude,
                    Longitude = (double?)draftData.Longitude,
                    CityName = draftData.City,
                    ProvinceName = draftData.Province,
                    ProvinceId = draftData.ProvinceId,
                    CityId = draftData.CityId,
                    NeighborhoodId = draftData.NeighborhoodId
                };

                _context.PropertyLocations.Add(location);

                // ========== PASO 3: Crear características ==========
                if (draftData.Features?.Count > 0)
                {
                    _logger.LogInformation("Creando {Count} características...", draftData.Features.Count);
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

                // ========== PASO 4: Crear estado ==========
                _logger.LogInformation("Creando estado de propiedad...");
                var status = new PropertyStatus
                {
                    PropertyId = property.PropertyId,
                    State = PropertyState.Publicado,
                    UpdatedAt = HoraArgentina.Now,
                    Notes = "Publicado desde borrador"
                };

                _context.PropertyStatuses.Add(status);

                // ========== GUARDAR TODO (ubicación, características, estado) ==========
                await _context.SaveChangesAsync(); // SEGUNDO SaveChanges
                _logger.LogInformation("Detalles de propiedad guardados");

                // ========== PASO 5: Registrar publicación ==========
                _logger.LogInformation("Registrando publicación...");
                var publicationRegistered = await _subscriptionService.RegisterPublicationAsync(
                    userId,
                    property.PropertyId,
                    subscription.PlanId);

                if (!publicationRegistered)
                {
                    _logger.LogWarning("Registro de publicación falló, cambiando estado a borrador");
                    status.State = PropertyState.Borrador;
                    status.Notes = "Publicación falló en registro";
                    await _context.SaveChangesAsync(); // TERCER SaveChanges (solo actualizar estado)

                    // Aún retornamos el ID para que se pueda manejar
                    return property.PropertyId;
                }

                _logger.LogInformation(
                    "Propiedad {PropertyId} publicada exitosamente para usuario {UserId}",
                    property.PropertyId, userId);

                return property.PropertyId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR CRÍTICO al crear propiedad desde borrador para el usuario {UserId}", userId);
                throw;
            }
        }
    }
    
}