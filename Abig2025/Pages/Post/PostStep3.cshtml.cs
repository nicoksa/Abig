
using Abig2025.Models.DTO;
using Abig2025.Models.Properties;
using Abig2025.Services;
using Abig2025.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Abig2025.Pages.Post
{
    public class PostStep3Model : PostPageBase
    {
        private readonly IDraftService _draftService;
        private readonly IFeatureService _featureService;
        private readonly ILogger<PostStep2Model> _logger;

        public PostStep3Model(IDraftService draftService, IFeatureService featureService, ILogger<PostStep2Model> logger)
        {
            _draftService = draftService;
            _featureService = featureService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid DraftId { get; set; }

        [BindProperty]
        public PropertyTempData Data { get; set; } = new();

        public List<FeatureDefinition> FeatureDefinitions { get; set; } = new();


        public async Task<IActionResult> OnGet()
        {
            var (error, draft) = await GetAndValidateDraftAsync(DraftId, _draftService, _logger);
            if (error != null) return error;


            if (DraftId == Guid.Empty)
                return RedirectToPage("/Post/Post");

            //var draft = await _draftService.GetDraftAsync(DraftId);
            if (draft == null)
                return RedirectToPage("/Post/Post");

            Data = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

            FeatureDefinitions = await _featureService
                .GetFeaturesForProperty(Data.PropertyType);

            SyncDraftFeatures();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (DraftId == Guid.Empty)
                {
                    return RedirectToPage("/Post/Post");
                }

                var draft = await _draftService.GetDraftAsync(DraftId);
                if (draft == null)
                {
                    return RedirectToPage("/Post/Post");
                }

                // Cargar datos existentes del draft
                var existingData = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

                // Obtener definiciones de características
                FeatureDefinitions = await _featureService
                    .GetFeaturesForProperty(existingData.PropertyType);

                // Actualizar características desde el formulario
                UpdateFeaturesFromForm(existingData);

                // Actualizar el draft con los cambios
                await _draftService.UpdateDraftAsync(
                    DraftId,
                    existingData,
                    nextStep: 3);

                // Redirigir al siguiente paso
                return RedirectToPage("/Post/PostStep4", new { draftId = DraftId });
            }
            catch (Exception ex)
            {
                // Log error y mostrar mensaje
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");

                // Recargar los datos para mostrar la página con el error
                return await OnGet();
            }
        }

        // Método para volver al paso 2
        public async Task<IActionResult> OnPostBackAsync()
        {
            try
            {
                if (DraftId == Guid.Empty)
                {
                    return RedirectToPage("/Post/Post");
                }

                var draft = await _draftService.GetDraftAsync(DraftId);
                if (draft == null)
                {
                    return RedirectToPage("/Post/Post");
                }

                // Cargar datos existentes
                var existingData = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

                // Obtener definiciones de características
                FeatureDefinitions = await _featureService
                    .GetFeaturesForProperty(existingData.PropertyType);

                // Actualizar características desde el formulario antes de retroceder
                UpdateFeaturesFromForm(existingData);

                // Guardar los cambios en el draft (mantener el step 2)
                await _draftService.UpdateDraftAsync(
                    DraftId,
                    existingData,
                    nextStep: 2);

                // Redirigir al paso 2
                return RedirectToPage("/Post/PostStep2", new { draftId = DraftId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return await OnGet();
            }
        }

        private void SyncDraftFeatures()
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
                        Value = "false" // Valor por defecto
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

        private void UpdateFeaturesFromForm(PropertyTempData data)
        {
            foreach (var def in FeatureDefinitions)
            {
                var key = $"feature_{def.FeatureDefinitionId}";
                var isChecked = Request.Form.ContainsKey(key);

                // Buscar o crear la característica
                var feature = data.Features
                    .FirstOrDefault(f => f.FeatureDefinitionId == def.FeatureDefinitionId);

                if (feature == null)
                {
                    feature = new PropertyFeatureTemp
                    {
                        FeatureDefinitionId = def.FeatureDefinitionId
                    };
                    data.Features.Add(feature);
                }

                feature.Value = isChecked ? "true" : "false";
            }
        }
    }
}
