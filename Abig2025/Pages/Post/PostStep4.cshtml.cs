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
using System.Security.Claims;
using System.Text.Json;

namespace Abig2025.Pages.Post
{
    public class PostStep4Model : PageModel
    {
        private readonly IDraftService _draftService;
        private readonly IPropertyService _propertyService;
        private readonly ITempFileService _tempFileService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly AppDbContext _context;
        private readonly ILogger<PostStep4Model> _logger;

        [BindProperty(SupportsGet = true)]
        public Guid? DraftId { get; set; }

        [BindProperty]
        public string? SelectedPlan { get; set; } = "PlanActual"; // "PlanActual" o ID de plan

        [BindProperty]
        public bool AcceptTerms { get; set; }

        public PropertyTempData DraftData { get; set; } = new();
        public UserSubscription? CurrentSubscription { get; set; }
        public List<SubscriptionPlan> AvailablePlans { get; set; } = new();
        public int RemainingPublications { get; set; }
        public SubscriptionPlan? FreePlan { get; set; }

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
                if (!DraftId.HasValue || DraftId == Guid.Empty)
                {
                    return RedirectToPage("/Post/Post");
                }

                var draft = await _draftService.GetDraftAsync(DraftId.Value);
                if (draft == null)
                {
                    return RedirectToPage("/Post/Post");
                }

                // Cargar datos del draft
                DraftData = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

                // Obtener el usuario actual
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return RedirectToPage("/Account/Login");
                }

                // Obtener suscripción actual del usuario
                CurrentSubscription = await _subscriptionService.GetActiveUserSubscriptionAsync(userId);

                // Obtener planes disponibles (excluyendo el gratuito si ya tiene)
                AvailablePlans = await _subscriptionService.GetAvailablePlansAsync();

                // Obtener plan gratuito (siempre disponible)
                FreePlan = await _context.SubscriptionPlans
                    .FirstOrDefaultAsync(p => p.PlanId == 1); // Plan gratuito

                // Obtener publicaciones restantes
                RemainingPublications = await _subscriptionService.GetRemainingPublicationsAsync(userId);

                /* Si no tiene publicaciones disponibles, redirigir
                if (RemainingPublications <= 0 && CurrentSubscription?.Plan.MaxPublications != -1)
                {
                    TempData["ErrorMessage"] = "No tienes publicaciones disponibles en tu plan actual.";
                    return RedirectToPage("/Subscriptions/Upgrade");
                }
                */
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el paso 4");
                ModelState.AddModelError(string.Empty, "Error al cargar los datos. Intenta nuevamente.");
                return Page();
            }
        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                if (!AcceptTerms)
                {
                    ModelState.AddModelError(string.Empty, "Debes aceptar los términos y condiciones.");
                    await LoadPageData();
                    return Page();
                }

                if (!DraftId.HasValue)
                {
                    return RedirectToPage("/Post/Post");
                }

                // 1. Obtener el draft
                var draft = await _draftService.GetDraftAsync(DraftId.Value);
                if (draft == null)
                {
                    return RedirectToPage("/Post/Post");
                }

                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return RedirectToPage("/Account/Login");
                }

                // 2. Verificar si puede publicar
                var canPublish = await _subscriptionService.CanUserPublishAsync(userId);
                if (!canPublish)
                {
                    TempData["ErrorMessage"] = "No tienes publicaciones disponibles en tu plan actual.";
                    await LoadPageData();
                    return Page();
                }

                // 3. Deserializar datos del draft
                var propertyData = JsonSerializer.Deserialize<PropertyTempData>(draft.JsonData)!;

                // 4. Determinar el plan a usar
                int planId;
                if (SelectedPlan == "PlanActual" && CurrentSubscription != null)
                {
                    planId = CurrentSubscription.PlanId;
                }
                else if (SelectedPlan == "PlanActual" && CurrentSubscription == null)
                {
                    // Usar plan gratuito
                    planId = 1; // ID del plan gratuito
                }
                else if (int.TryParse(SelectedPlan, out int selectedPlanId))
                {
                    planId = selectedPlanId;

                    // Verificar si el usuario necesita suscribirse a este plan
                    var selectedPlan = await _subscriptionService.GetPlanByIdAsync(planId);
                    if (selectedPlan != null && selectedPlan.Price > 0)
                    {
                        // Redirigir a página de pago/suscripción
                        return RedirectToPage("/Subscriptions/Subscribe", new { planId = planId, draftId = DraftId.Value });
                    }
                }
                else
                {
                    // Fallback al plan gratuito
                    planId = 1;
                }

                // 5. Crear la propiedad en la base de datos
                var propertyId = await CreatePropertyFromDraftAsync(propertyData, userId);

                if (propertyId == 0)
                {
                    ModelState.AddModelError(string.Empty, "Error al crear la propiedad.");
                    await LoadPageData();
                    return Page();
                }

                // 6. Mover imágenes temporales a ubicación permanente
                await MoveTempImagesToPermanentAsync(propertyData.TempImages, propertyId);

                // 7. Registrar la publicación
                await RecordPublicationAsync(userId, propertyId, planId);

                // 8. Eliminar el draft
                await _draftService.DeleteDraftAsync(DraftId.Value);

                // 9. Redirigir a la página de éxito
                return RedirectToPage("/Post/PublicationSuccess", new { propertyId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al publicar la propiedad");
                ModelState.AddModelError(string.Empty,
                    "Ocurrió un error al publicar la propiedad. Por favor, intenta nuevamente.");
                await LoadPageData();
                return Page();
            }
        }

        // Método auxiliar para cargar datos de la página
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

            var userId = GetCurrentUserId();
            CurrentSubscription = await _subscriptionService.GetActiveUserSubscriptionAsync(userId);
            AvailablePlans = await _subscriptionService.GetAvailablePlansAsync();
            RemainingPublications = await _subscriptionService.GetRemainingPublicationsAsync(userId);
        }

        private async Task<int> CreatePropertyFromDraftAsync(PropertyTempData draftData, int userId)
        {
            try
            {
                // Usar el servicio de propiedades
                return await _propertyService.CreatePropertyFromDraftAsync(draftData, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear propiedad desde draft");
                throw;
            }
        }

        private async Task MoveTempImagesToPermanentAsync(List<TempImageInfo> tempImages, int propertyId)
        {
            if (tempImages?.Count > 0)
            {
                foreach (var tempImage in tempImages)
                {
                    try
                    {
                        // Mover la imagen de temp a permanente
                        var permanentPath = await _tempFileService.MoveToPermanentAsync(
                            tempImage.FileName,
                            propertyId);

                        // Registrar en la base de datos
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
                        _logger.LogError(ex, $"Error al mover imagen {tempImage.FileName}");
                        // Continuar con las demás imágenes
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task RecordPublicationAsync(int userId, int propertyId, int planId)
        {
            var plan = await _subscriptionService.GetPlanByIdAsync(planId);
            if (plan == null)
            {
                throw new ArgumentException($"Plan con ID {planId} no encontrado");
            }

            var publication = new PropertyPublication
            {
                PropertyId = propertyId,
                UserId = userId,
                PlanId = planId,
                PublishedAt = HoraArgentina.Now,
                ExpiresAt = plan.DurationDays > 0 ?
                    HoraArgentina.Now.AddDays(plan.DurationDays) : null,
                IsActive = true,
                Notes = $"Publicación desde borrador {DraftId}"
            };

            _context.PropertyPublications.Add(publication);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Publicación registrada: Propiedad {PropertyId}, Usuario {UserId}, Plan {PlanId}",
                propertyId, userId, planId);
        }

        private int GetCurrentUserId()
        {
            try
            {
                // Obtener el ID del usuario desde los claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Si usas un claim personalizado
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    userIdClaim = User.FindFirst("UserId")?.Value;
                }

                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }

                // Para desarrollo/testing - REMOVER EN PRODUCCIÓN
                _logger.LogWarning("No se pudo obtener el ID del usuario, usando valor por defecto 1");
                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ID de usuario");
                return 0;
            }
        }

        // Método para retroceder al paso 3
        public async Task<IActionResult> OnPostBackAsync()
        {
            if (!DraftId.HasValue)
            {
                return RedirectToPage("/Post/Post");
            }

            // Redirigir al paso 3 manteniendo el draft
            return RedirectToPage("/Post/PostStep3", new { draftId = DraftId.Value });
        }
    }
}