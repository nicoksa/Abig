using Abig2025.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Abig2025.Pages.Post
{
    [Authorize]
    public class CheckPlanModel : PageModel
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<CheckPlanModel> _logger;

        public CheckPlanModel(
            ISubscriptionService subscriptionService,
            ILogger<CheckPlanModel> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        public int UserId { get; set; }
        public bool HasPublications { get; set; }
        public int RemainingPublications { get; set; }
        public string CurrentPlanName { get; set; } = "Plan Gratuito";

        public async Task<IActionResult> OnGet()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return RedirectToPage("/Account/Login");
                }

                UserId = userId;

                // Verificar suscripción activa
                var subscription = await _subscriptionService.GetActiveUserSubscriptionAsync(userId);

                if (subscription == null)
                {
                    // No tiene suscripción, redirigir a planes
                    return RedirectToPage("/Post/Plans");
                }

                // Obtener información del plan actual
                CurrentPlanName = subscription.Plan?.Name ?? "Plan Gratuito";

                // Obtener publicaciones usadas y restantes
                var usedPublications = await _subscriptionService.GetUsedPublicationsAsync(userId);
                RemainingPublications = await _subscriptionService.GetRemainingPublicationsAsync(userId);
                HasPublications = RemainingPublications > 0;

                // Redirigir según tenga publicaciones disponibles
                if (HasPublications)
                {
                    // Crear nuevo draft y redirigir al paso 1
                    return RedirectToPage("/Post/Post");
                }
                else
                {
                    // Guardar en TempData para mostrar mensaje en Plans
                    TempData["NoPublications"] = true;
                    TempData["RemainingPublications"] = RemainingPublications;
                    TempData["UsedPublications"] = usedPublications;
                    TempData["CurrentPlan"] = CurrentPlanName;

                    // Redirigir a página de planes
                    return RedirectToPage("/Post/Plans");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar plan del usuario");
                return RedirectToPage("/Post/Plans");
            }
        }
    }
}
