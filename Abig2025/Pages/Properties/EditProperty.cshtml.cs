using Abig2025.Data;
using Abig2025.Helpers;
using Abig2025.Models.DTO;
using Abig2025.Models.Properties;
using Abig2025.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;


namespace Abig2025.Pages.Properties
{
    [Authorize]
    public class EditPropertyModel : PageModel
    {
        private readonly AppDbContext _context;

        private readonly IFeatureService _featureService;
        private readonly ILogger<EditPropertyModel> _logger;

        [BindProperty(SupportsGet = true)]
        public int PropertyId { get; set; }

        [BindProperty]
        public PropertyTempData Data { get; set; } = new();

        public Property? ExistingProperty { get; set; }
        public List<FeatureDefinition> FeatureDefinitions { get; set; } = new();

        public EditPropertyModel(
            AppDbContext context,
            IFeatureService featureService,

            ILogger<EditPropertyModel> logger)
        {
            _context = context;
            _featureService = featureService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            try
            {
                // 1. Verificar que el usuario está autenticado
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return RedirectToPage("/Account/Login");

                // 2. Obtener la propiedad
                ExistingProperty = await _context.Properties
                    .Include(p => p.Location)
                        .ThenInclude(l => l.Province)
                    .Include(p => p.Location)
                        .ThenInclude(l => l.City)
                    .Include(p => p.Location)
                        .ThenInclude(l => l.Neighborhood)
                    .Include(p => p.Images)
                    .Include(p => p.Features)
                        .ThenInclude(f => f.FeatureDefinition)
                    .Include(p => p.Status)
                    .FirstOrDefaultAsync(p => p.PropertyId == PropertyId && p.OwnerId == userId.Value);

                if (ExistingProperty == null)
                {
                    TempData["ErrorMessage"] = "Propiedad no encontrada o no tienes permisos para editarla.";
                    return RedirectToPage("/Profile/MyProperties");
                }

                // 3. Mapear la propiedad existente a PropertyTempData
                MapPropertyToTempData(ExistingProperty);

                // 4. Cargar definiciones de características
                FeatureDefinitions = await _featureService
                    .GetFeaturesForProperty(Data.PropertyType);

                // 5. Sincronizar características seleccionadas
                SyncFeaturesWithDefinitions();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la propiedad para edición");
                TempData["ErrorMessage"] = "Error al cargar la propiedad para edición.";
                return RedirectToPage("/Profile/MyProperties");
            }
        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                // 1. Verificar que el usuario está autenticado
                var userId = User.GetUserId();
                if (!userId.HasValue)
                    return RedirectToPage("/Account/Login");

                // 2. Validar modelo
                if (!ModelState.IsValid)
                {
                    // Recargar datos necesarios
                    ExistingProperty = await _context.Properties
                        .Include(p => p.Location)
                        .FirstOrDefaultAsync(p => p.PropertyId == PropertyId && p.OwnerId == userId.Value);

                    if (ExistingProperty == null)
                    {
                        TempData["ErrorMessage"] = "Propiedad no encontrada.";
                        return RedirectToPage("/Profile/MyProperties");
                    }

                    FeatureDefinitions = await _featureService
                        .GetFeaturesForProperty(Data.PropertyType);

                    return Page();
                }

                // 3. Obtener la propiedad existente
                ExistingProperty = await _context.Properties
                    .Include(p => p.Location)
                    .Include(p => p.Images)
                    .Include(p => p.Features)
                    .Include(p => p.Status)
                    .FirstOrDefaultAsync(p => p.PropertyId == PropertyId && p.OwnerId == userId.Value);

                if (ExistingProperty == null)
                {
                    TempData["ErrorMessage"] = "Propiedad no encontrada o no tienes permisos para editarla.";
                    return RedirectToPage("/Profile/MyProperties");
                }

                // 4. Actualizar la propiedad
                await UpdateExistingProperty();

                // 5. Mensaje de éxito
                TempData["SuccessMessage"] = "¡Propiedad actualizada exitosamente!";

                // 6. Redirigir a la vista de la propiedad o lista de propiedades
                return RedirectToPage("/Profile/MyProperties");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la propiedad");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar la propiedad. Intenta nuevamente.");

                // Recargar datos para mostrar la página
                var userId = User.GetUserId();
                if (userId.HasValue)
                {
                    ExistingProperty = await _context.Properties
                        .Include(p => p.Location)
                        .FirstOrDefaultAsync(p => p.PropertyId == PropertyId && p.OwnerId == userId.Value);

                    FeatureDefinitions = await _featureService
                        .GetFeaturesForProperty(Data.PropertyType);
                }

                return Page();
            }
        }


        private void MapPropertyToTempData(Property property)
        {
            Data = new PropertyTempData
            {
                // Información básica
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                Currency = property.Currency,
                OperationType = property.OperationType,
                PropertyType = property.PropertyType,
                Subtype = property.Subtype,

                // Ubicación
                Street = property.Location?.Street,
                Number = property.Location?.Number,
                PostalCode = property.Location?.PostalCode,
                Latitude = (decimal?)property.Location?.Latitude,
                Longitude = (decimal?)property.Location?.Longitude,

                Province = property.Location?.Province?.Name ?? property.Location?.ProvinceName,
                City = property.Location?.City?.Name ?? property.Location?.CityName,
                Neighborhood = property.Location?.Neighborhood?.Name,

                ProvinceId = property.Location?.ProvinceId,
                CityId = property.Location?.CityId,
                NeighborhoodId = property.Location?.NeighborhoodId,

                // Características principales
                MainRooms = property.MainRooms,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                ParkingSpaces = property.ParkingSpaces,
                CoveredArea = property.CoveredArea,
                TotalArea = property.TotalArea,
                Age = property.Age,
                IsNew = property.IsNew,
                IsUnderConstruction = property.IsUnderConstruction,

                // Video
                VideoUrl = property.VideoUrl,

                // Expensas
                Expenses = property.Expenses,
                ExpensesCurrency = property.ExpensesCurrency,

                // Características (features)
                Features = property.Features.Select(f => new PropertyFeatureTemp
                {
                    FeatureDefinitionId = f.FeatureDefinitionId,
                    Value = f.Value ?? "false"
                }).ToList(),

                // Imágenes (convertir de PropertyImage a TempImageInfo)
                TempImages = property.Images.Select(img => new TempImageInfo
                {
                    FileName = img.FileName ?? Path.GetFileName(img.ImageUrl),
                    OriginalName = img.FileName ?? "Imagen de propiedad",
                    Size = img.FileSize,
                    UploadedAt = img.UploadedAt,
                    IsMain = img.IsMain
                }).ToList()
            };
        }

        private async Task UpdateExistingProperty()
        {
            // Actualizar propiedades básicas
            ExistingProperty.Title = Data.Title;
            ExistingProperty.Description = Data.Description;
            ExistingProperty.Price = Data.Price;
            ExistingProperty.Currency = Data.Currency;
            ExistingProperty.OperationType = Data.OperationType;
            ExistingProperty.PropertyType = Data.PropertyType;
            ExistingProperty.Subtype = Data.Subtype;

            // Actualizar características principales
            ExistingProperty.MainRooms = Data.MainRooms;
            ExistingProperty.Bedrooms = Data.Bedrooms;
            ExistingProperty.Bathrooms = Data.Bathrooms;
            ExistingProperty.ParkingSpaces = Data.ParkingSpaces;
            ExistingProperty.CoveredArea = Data.CoveredArea;
            ExistingProperty.TotalArea = Data.TotalArea;
            ExistingProperty.Age = Data.Age;
            ExistingProperty.IsNew = Data.IsNew;
            ExistingProperty.IsUnderConstruction = Data.IsUnderConstruction;

            // Actualizar video
            ExistingProperty.VideoUrl = Data.VideoUrl;

            // Actualizar expensas
            ExistingProperty.Expenses = Data.Expenses;
            ExistingProperty.ExpensesCurrency = Data.ExpensesCurrency;

            // Actualizar ubicación
            if (ExistingProperty.Location == null)
            {
                ExistingProperty.Location = new PropertyLocation();
            }

            ExistingProperty.Location.Street = Data.Street;
            ExistingProperty.Location.Number = Data.Number;
            ExistingProperty.Location.PostalCode = Data.PostalCode;
            ExistingProperty.Location.Latitude = (double?)Data.Latitude;
            ExistingProperty.Location.Longitude = (double?)Data.Longitude;

            // Actualizar IDs de ubicación si están disponibles
            if (Data.ProvinceId.HasValue)
                ExistingProperty.Location.ProvinceId = Data.ProvinceId;
            if (Data.CityId.HasValue)
                ExistingProperty.Location.CityId = Data.CityId;
            if (Data.NeighborhoodId.HasValue)
                ExistingProperty.Location.NeighborhoodId = Data.NeighborhoodId;

            // Mantener nombres como respaldo
            ExistingProperty.Location.ProvinceName = Data.Province;
            ExistingProperty.Location.CityName = Data.City;

            // Actualizar características (features)
            await UpdatePropertyFeatures();

            // Guardar cambios
            await _context.SaveChangesAsync();
        }

        private async Task UpdatePropertyFeatures()
        {
            // 1. Obtener definiciones de características para el tipo de propiedad
            var definitions = await _featureService.GetFeaturesForProperty(Data.PropertyType);

            // 2. Obtener características actuales de la propiedad
            var existingFeatures = _context.PropertyFeatures
                .Where(pf => pf.PropertyId == PropertyId)
                .ToList();

            // 3. Si no hay características seleccionadas en el Data, eliminar todas las existentes
            if (Data.Features == null || !Data.Features.Any(f => f.Value == "true"))
            {
                if (existingFeatures.Any())
                {
                    _context.PropertyFeatures.RemoveRange(existingFeatures);
                }
                return;
            }

            // 4. Filtrar características seleccionadas (solo las que están en "true")
            var selectedFeatureIds = Data.Features
                .Where(f => f.Value == "true")
                .Select(f => f.FeatureDefinitionId)
                .ToList();

            // 5. Eliminar características que ya no están seleccionadas
            var featuresToRemove = existingFeatures
                .Where(ef => !selectedFeatureIds.Contains(ef.FeatureDefinitionId))
                .ToList();

            if (featuresToRemove.Any())
            {
                _context.PropertyFeatures.RemoveRange(featuresToRemove);
            }

            // 6. Agregar nuevas características seleccionadas que no existían antes
            var existingFeatureIds = existingFeatures.Select(ef => ef.FeatureDefinitionId).ToList();
            var newFeaturesToAdd = selectedFeatureIds
                .Where(featureId => !existingFeatureIds.Contains(featureId))
                .Select(featureId => new PropertyFeature
                {
                    PropertyId = PropertyId,
                    FeatureDefinitionId = featureId,
                    Value = "true"
                })
                .ToList();

            if (newFeaturesToAdd.Any())
            {
                await _context.PropertyFeatures.AddRangeAsync(newFeaturesToAdd);
            }

            // 7. Asegurarse de que todas las características agregadas existen en las definiciones
            var validFeatureIds = definitions.Select(d => d.FeatureDefinitionId).ToList();
            foreach (var feature in newFeaturesToAdd)
            {
                if (!validFeatureIds.Contains(feature.FeatureDefinitionId))
                {
                    _context.PropertyFeatures.Remove(feature);
                }
            }
        }

        private void SyncFeaturesWithDefinitions()
        {
            // Asegurar que todas las características definidas estén en el Data
            foreach (var def in FeatureDefinitions)
            {
                var existingFeature = Data.Features
                    .FirstOrDefault(f => f.FeatureDefinitionId == def.FeatureDefinitionId);

                if (existingFeature == null)
                {
                    Data.Features.Add(new PropertyFeatureTemp
                    {
                        FeatureDefinitionId = def.FeatureDefinitionId,
                        Value = "false"
                    });
                }
            }

            // Eliminar características que ya no existen en las definiciones
            var featureIdsToRemove = Data.Features
                .Where(f => !FeatureDefinitions.Any(d => d.FeatureDefinitionId == f.FeatureDefinitionId))
                .Select(f => f.FeatureDefinitionId)
                .ToList();

            foreach (var id in featureIdsToRemove)
            {
                Data.Features.RemoveAll(f => f.FeatureDefinitionId == id);
            }
        }
    }
}
