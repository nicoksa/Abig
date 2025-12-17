using Abig2025.Models.DTO;
using Abig2025.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Abig2025.Pages.Post
{
    public class PostStep3Model : PageModel
    {
        private readonly IDraftService _draftService;

        public PostStep3Model(IDraftService draftService)
        {
            _draftService = draftService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid? DraftId { get; set; }

        [BindProperty]
        public PropertyTempData Data { get; set; } = new();

        public async Task<IActionResult> OnGet()
        {
            if (!DraftId.HasValue)
            {
                return RedirectToPage("/Post/Post");
            }

            var draft = await _draftService.GetDraftAsync(DraftId.Value);
            if (draft == null)
            {
                return RedirectToPage("/Post/Post");
            }

            // Cargar datos existentes del draft
            Data = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

            // Inicializar características si no existen
            EnsureFeaturesExist();

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!DraftId.HasValue)
            {
                return RedirectToPage("/Post/Post");
            }

            var draft = await _draftService.GetDraftAsync(DraftId.Value);
            if (draft == null)
            {
                return RedirectToPage("/Post/Post");
            }

            // Cargar los datos existentes del draft
            var existingData = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

            // Actualizar características del paso 3
            UpdateFeaturesFromForm(existingData.Features);
            UpdateFieldFeaturesFromForm(existingData.FieldFeatures);

            // Actualizar el draft con paso 3 completado
            await _draftService.UpdateDraftAsync(DraftId.Value, existingData, nextStep: 3);

            return RedirectToPage("/Post/PostStep4", new { draftId = DraftId });
        }


        private void EnsureFeaturesExist()
        {
            // Lista de características predefinidas para el paso 3
            var predefinedFeatures = new List<string>
            {
                "PermiteMascotas",
                "AccesoDiscapacitados",
                "Parrilla",
                "AptoProfesional",
                "UsoComercial",
                "Gimnasio",
                "Hidromasaje",
                "Solarium",
                "PistaDeportiva",
                "SalaJuegos",
                "AireAcondicionado",
                "CocinaEquipada",
                "Amueblado",
                "Alarma",
                "Quincho",
                "Sauna",
                "Caldera",
                "Lavavajillas",
                "Termotanque",
                "DormitorioSuite",
                "Balcon",
                "Cocina",
                "Sotano",
                "Patio",
                "Escritorio",
                "Jardin",
                "Comedor",
                "Baulera",
                "Terraza",
                "Toilette",
                "Lavadero",
                "InternetWifi",
                "Ascensor",
                "Vigilancia",
                "Limpieza",
                "Agua",
                "Electricidad"
            };

            // Características específicas para campos
            var fieldFeatures = new List<string>
            {
                "Campo_Molino",
                "Campo_Manga",
                "Campo_Cargador",
                "Campo_Alambrado",
                "Campo_Galpon",
                "Campo_SistemaRiego",
                "Campo_Agricola",
                "Campo_Ganadero",
                "Campo_Mixto"
            };

            foreach (var featureName in predefinedFeatures)
            {
                // Si no existe la característica en la lista, la agregamos
                if (!Data.Features.Any(f => f.Name == featureName))
                {
                    Data.Features.Add(new PropertyFeatureTemp
                    {
                        Name = featureName,
                        Value = "false" // Por defecto desactivado
                    });
                }
            }

            // Inicializar características de campo si no existen
            foreach (var fieldFeatureName in fieldFeatures)
            {
                if (!Data.FieldFeatures.Any(f => f.Name == fieldFeatureName))
                {
                    Data.FieldFeatures.Add(new FieldFeatureTemp
                    {
                        Name = fieldFeatureName,
                        Value = "false"
                    });
                }
            }
        }

        private void UpdateFeaturesFromForm(List<PropertyFeatureTemp> features)
        {
            // Obtener todos los checkboxes del formulario
            var form = Request.Form;

            // Para cada característica en la lista, actualizar su valor basado en el formulario
            foreach (var feature in features)
            {
                // Verificar si el checkbox está marcado en el formulario
                var isChecked = form.ContainsKey(feature.Name);
                feature.Value = isChecked.ToString().ToLower();
            }
        }

        private void UpdateFieldFeaturesFromForm(List<FieldFeatureTemp> fieldFeatures)
        {
            var form = Request.Form;

            foreach (var fieldFeature in fieldFeatures)
            {
                var isChecked = form.ContainsKey(fieldFeature.Name);
                fieldFeature.Value = isChecked.ToString().ToLower();
            }
        }
    }
}