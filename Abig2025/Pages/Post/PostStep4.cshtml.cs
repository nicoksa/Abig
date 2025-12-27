using Abig2025.Data;
using Abig2025.Helpers;
using Abig2025.Models.DTO;
using Abig2025.Models.Properties;
using Abig2025.Models.Subscriptions;
using Abig2025.Services;
using Abig2025.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Abig2025.Pages.Post
{
    public class PostStep4Model : PostPageBase
    {
        private readonly IDraftService _draftService;
        private readonly IPropertyService _propertyService;
        private readonly ITempFileService _tempFileService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly AppDbContext _context;
        private readonly ILogger<PostStep4Model> _logger;

        [BindProperty(SupportsGet = true)]
        public Guid? DraftId { get; set; }

        public PropertyTempData DraftData { get; set; } = new();
        public UserSubscription? CurrentSubscription { get; set; }
        public int RemainingPublications { get; set; }
        public bool CanPublish { get; set; }

        public PostStep4Model(
            IDraftService draftService,
            IPropertyService propertyService,
            ITempFileService tempFileService,
            ISubscriptionService subscriptionService,
            AppDbContext context,
            ILogger<PostStep4Model> logger)
        {
            _draftService = draftService;
            _propertyService = propertyService;
            _tempFileService = tempFileService;
            _subscriptionService = subscriptionService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            try
            {
                // 1. Validar draft
                if (!DraftId.HasValue || DraftId == Guid.Empty)
                    return RedirectToPage("/Post/Post");

                var (error, draft) = await GetAndValidateDraftAsync(DraftId, _draftService, _logger);
                if (error != null) return error;

                if (draft == null)
                    return RedirectToPage("/Post/Post");

                // 2. Cargar datos del draft
                DraftData = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

                // 3. Obtener usuario
                var userId = GetAuthenticatedUserIdOrThrow();

                // 4. Validar suscripción y publicaciones
                CurrentSubscription = await _subscriptionService.GetActiveUserSubscriptionAsync(userId);
                RemainingPublications = await _subscriptionService.GetRemainingPublicationsAsync(userId);
                CanPublish = await _subscriptionService.CanUserPublishAsync(userId);

                // 5. VALIDACIÓN CRÍTICA: Redirigir si no puede publicar
                if (!CanPublish || RemainingPublications <= 0)
                {
                    _logger.LogWarning(
                        "Usuario {UserId} sin publicaciones disponibles. Redirigiendo a /Post/Plans",
                        userId);

                    TempData["NoPublications"] = true;
                    TempData["RemainingPublications"] = RemainingPublications;
                    TempData["CurrentPlan"] = CurrentSubscription?.Plan?.Name ?? "Sin plan";

                    return RedirectToPage("/Post/Plans");
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar Step 4");
                ModelState.AddModelError(string.Empty, "Error al cargar los datos");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostPublishAsync()
        {
            try
            {
                // 1. Validaciones iniciales
                if (!DraftId.HasValue)
                    return RedirectToPage("/Post/Post");

                var draft = await _draftService.GetDraftAsync(DraftId.Value);
                if (draft == null)
                    return RedirectToPage("/Post/Post");

                var userId = GetAuthenticatedUserIdOrThrow();

                // 2. VERIFICACIÓN CRÍTICA: ¿Puede publicar?
                var canPublish = await _subscriptionService.CanUserPublishAsync(userId);
                if (!canPublish)
                {
                    TempData["ErrorMessage"] = "No tienes publicaciones disponibles en tu plan actual";
                    TempData["NoPublications"] = true;
                    return RedirectToPage("/Post/Plans");
                }

                // 3. Obtener suscripción actual
                var subscription = await _subscriptionService.GetActiveUserSubscriptionAsync(userId);
                if (subscription == null)
                {
                    TempData["ErrorMessage"] = "No tienes un plan activo";
                    return RedirectToPage("/Post/Plans");
                }

                // 4. Deserializar draft
                var propertyData = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

                // 5. Crear propiedad
                var propertyId = await _propertyService.CreatePropertyFromDraftAsync(propertyData, userId);

                if (propertyId == 0)
                {
                    ModelState.AddModelError(string.Empty, "Error al crear la propiedad");
                    await LoadPageData();
                    return Page();
                }

                // 6. Mover imágenes temporales
                await MoveTempImagesToPermanentAsync(propertyData.TempImages, propertyId);

                // 7. Registrar publicación
                await RecordPublicationAsync(userId, propertyId, subscription.PlanId);

                // 8. Eliminar draft
                await _draftService.DeleteDraftAsync(DraftId.Value);

                // 9. Mensaje de éxito
                TempData["SuccessMessage"] = "¡Propiedad publicada exitosamente!";

                return RedirectToPage("/Post/PublicationSuccess", new { propertyId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al publicar propiedad");
                ModelState.AddModelError(string.Empty,
                    "Ocurrió un error al publicar. Intenta nuevamente.");
                await LoadPageData();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostBackAsync()
        {
            if (!DraftId.HasValue)
                return RedirectToPage("/Post/Post");

            return RedirectToPage("/Post/PostStep3", new { draftId = DraftId.Value });
        }

        // ========== MÉTODOS AUXILIARES ==========

        private async Task LoadPageData()
        {
            if (DraftId.HasValue)
            {
                var draft = await _draftService.GetDraftAsync(DraftId.Value);
                if (draft != null)
                {
                    DraftData = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;
                }
            }

            var userId = GetAuthenticatedUserIdOrThrow();
            CurrentSubscription = await _subscriptionService.GetActiveUserSubscriptionAsync(userId);
            RemainingPublications = await _subscriptionService.GetRemainingPublicationsAsync(userId);
            CanPublish = await _subscriptionService.CanUserPublishAsync(userId);
        }

        private async Task MoveTempImagesToPermanentAsync(List<TempImageInfo> tempImages, int propertyId)
        {
            if (tempImages?.Count > 0)
            {
                foreach (var tempImage in tempImages)
                {
                    try
                    {
                        var permanentPath = await _tempFileService.MoveToPermanentAsync(
                            tempImage.FileName,
                            propertyId);

                        var propertyImage = new PropertyImage
                        {
                            PropertyId = propertyId,
                            FileName = Path.GetFileName(permanentPath),
                            ImageUrl = permanentPath,
                            FileSize = tempImage.Size,
                            UploadedAt = tempImage.UploadedAt,
                            IsMain = false,
                            DisplayOrder = 0
                        };

                        _context.PropertyImages.Add(propertyImage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error moviendo imagen {tempImage.FileName}");
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task RecordPublicationAsync(int userId, int propertyId, int planId)
        {
            var plan = await _subscriptionService.GetPlanByIdAsync(planId);
            if (plan == null)
                throw new ArgumentException($"Plan {planId} no encontrado");

            var publication = new PropertyPublication
            {
                PropertyId = propertyId,
                UserId = userId,
                PlanId = planId,
                PublishedAt = HoraArgentina.Now,
                ExpiresAt = plan.DurationDays > 0
                    ? HoraArgentina.Now.AddDays(plan.DurationDays)
                    : null,
                IsActive = true,
                Notes = $"Publicación desde borrador {DraftId}"
            };

            _context.PropertyPublications.Add(publication);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Publicación registrada: Property={PropertyId}, User={UserId}, Plan={PlanId}",
                propertyId, userId, planId);
        }
    }
}