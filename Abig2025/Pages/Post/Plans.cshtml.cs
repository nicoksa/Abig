using Abig2025.Data;
using Abig2025.Helpers;
using Abig2025.Models.Subscriptions;
using Abig2025.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Abig2025.Pages.Post
{
    [Authorize]
    public class PlansModel : PageModel
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly AppDbContext _context;
        private readonly ILogger<PlansModel> _logger;

        [BindProperty]
        public int SelectedPlanId { get; set; }

        public UserSubscription? CurrentSubscription { get; set; }
        public List<SubscriptionPlan> AvailablePlans { get; set; } = new();
        public SubscriptionPlan? FreePlan { get; set; }
        public int RemainingPublications { get; set; }
        public int UsedPublications { get; set; }
        public bool NoPublications { get; set; }

        public PlansModel(
            ISubscriptionService subscriptionService,
            AppDbContext context,
            ILogger<PlansModel> logger)
        {
            _subscriptionService = subscriptionService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            try
            {
                var userId = GetUserId();
                if (userId <= 0) return RedirectToPage("/Account/Login");

                // Verificar si vino desde CheckPlan sin publicaciones
                NoPublications = TempData["NoPublications"] as bool? ?? false;
                RemainingPublications = TempData["RemainingPublications"] as int? ?? 0;

                // Obtener suscripción actual
                CurrentSubscription = await _subscriptionService.GetActiveUserSubscriptionAsync(userId);

                // SI TIENE SUSCRIPCIÓN, OBTENER PUBLICACIONES REALES
                if (CurrentSubscription != null)
                {
                    // Obtener publicaciones restantes REALES desde el servicio
                    RemainingPublications = await _subscriptionService.GetRemainingPublicationsAsync(userId);

                    // Obtener publicaciones usadas
                    UsedPublications = await _subscriptionService.GetUsedPublicationsAsync(userId);
                }

                // Obtener planes disponibles (excluyendo el gratuito si ya tiene suscripción)
                AvailablePlans = await _context.SubscriptionPlans
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Price)
                    .ToListAsync();

                // Obtener plan gratuito
                FreePlan = AvailablePlans.FirstOrDefault(p => p.Price == 0);

                // Si ya tiene suscripción, excluir el gratuito de AvailablePlans
                if (CurrentSubscription != null)
                {
                    AvailablePlans = AvailablePlans
                        .Where(p => p.Price > 0 || p.PlanId == CurrentSubscription.PlanId)
                        .ToList();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar planes");
                ModelState.AddModelError(string.Empty, "Error al cargar los planes disponibles");
                return Page();
            }
        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                var userId = GetUserId();
                if (userId <= 0) return RedirectToPage("/Account/Login");

                var selectedPlan = await _context.SubscriptionPlans
                    .FirstOrDefaultAsync(p => p.PlanId == SelectedPlanId && p.IsActive);

                if (selectedPlan == null)
                {
                    ModelState.AddModelError(string.Empty, "Plan no válido");
                    return await OnGet();
                }

                // Obtener suscripción actual (si existe)
                var currentSubscription = await _subscriptionService.GetActiveUserSubscriptionAsync(userId);

                // CAMBIAR O ASIGNAR PLAN AL USUARIO
                if (currentSubscription != null)
                {
                    // Si ya tiene suscripción, actualizarla al nuevo plan
                    await UpdateUserSubscriptionAsync(userId, currentSubscription.SubscriptionId, selectedPlan.PlanId);
                }
                else
                {
                    // Si no tiene suscripción, crear una nueva
                    await CreateUserSubscriptionAsync(userId, selectedPlan.PlanId);
                }

                // Guardar mensaje de éxito en TempData
                TempData["SuccessMessage"] = $"¡Plan {selectedPlan.Name} activado correctamente!";

                // Redirigir según el tipo de plan
                if (selectedPlan.Price == 0)
                {
                    // Plan gratuito - redirigir directamente a crear publicación
                    return RedirectToPage("/Post/Post");
                }
                else
                {
                    // Plan de pago - redirigir a página de confirmación o dashboard
                    // Por ahora redirigimos también a Post, pero luego podrás cambiar esto
                    return RedirectToPage("/Post/Post");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al seleccionar plan");
                ModelState.AddModelError(string.Empty, "Error al procesar la selección");
                return await OnGet();
            }
        }

        /// <summary>
        /// Crea una nueva suscripción para el usuario
        /// </summary>
        private async Task CreateUserSubscriptionAsync(int userId, int planId)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null) return;

            var subscription = new UserSubscription
            {
                UserId = userId,
                PlanId = planId,
                StartDate = HoraArgentina.Now,
                EndDate = CalculateEndDate(plan.DurationDays),
                IsActive = true
            };

            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nueva suscripción creada: UserId={UserId}, PlanId={PlanId}", userId, planId);
        }

        /// <summary>
        /// Actualiza una suscripción existente a un nuevo plan
        /// </summary>
        private async Task UpdateUserSubscriptionAsync(int userId, int subscriptionId, int newPlanId)
        {
            var subscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(us => us.SubscriptionId == subscriptionId && us.UserId == userId);

            if (subscription == null) return;

            var newPlan = await _context.SubscriptionPlans.FindAsync(newPlanId);
            if (newPlan == null) return;

            // Actualizar el plan
            subscription.PlanId = newPlanId;
            subscription.StartDate = HoraArgentina.Now;
            subscription.EndDate = CalculateEndDate(newPlan.DurationDays);
            subscription.IsActive = true;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Suscripción actualizada: SubscriptionId={SubscriptionId}, NuevoPlanId={NewPlanId}",
                subscriptionId, newPlanId);
        }

        /// <summary>
        /// Calcula la fecha de finalización basada en los días de duración del plan
        /// </summary>
        private DateTime CalculateEndDate(int durationDays)
        {
            return HoraArgentina.Now.AddDays(durationDays);
        }

        private async Task AssignFreePlanAsync(int userId, int planId)
        {
            // Verificar si ya tiene una suscripción activa gratuita
            var existingSubscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(us => us.UserId == userId &&
                                          us.PlanId == planId &&
                                          us.EndDate > HoraArgentina.Now);

            if (existingSubscription == null)
            {
                var subscription = new UserSubscription
                {
                    UserId = userId,
                    PlanId = planId,
                    StartDate = HoraArgentina.Now,
                    EndDate = HoraArgentina.Now.AddMonths(1), // 1 mes gratis
                    IsActive = true,
                };

                _context.UserSubscriptions.Add(subscription);
                await _context.SaveChangesAsync();
            }
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return 0;
        }
    }
}