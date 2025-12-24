using Abig2025.Data;
using Abig2025.Models.Subscriptions;
using Abig2025.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Abig2025.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(AppDbContext context, ILogger<SubscriptionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserSubscription?> GetActiveUserSubscriptionAsync(int userId)
        {
            try
            {
                var now = DateTime.UtcNow;

                return await _context.UserSubscriptions
                    .Include(us => us.Plan)
                    .Where(us => us.UserId == userId &&
                                us.StartDate <= now &&
                                us.EndDate >= now)
                    .OrderByDescending(us => us.StartDate)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener suscripción activa para usuario {UserId}", userId);
                return null;
            }
        }

        public async Task<List<SubscriptionPlan>> GetAvailablePlansAsync()
        {
            try
            {
                return await _context.SubscriptionPlans
                    .Where(p => p.PlanId > 1) // Excluir el plan gratuito (id 1) si queremos
                    .OrderBy(p => p.Price)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener planes disponibles");
                return new List<SubscriptionPlan>();
            }
        }

        public async Task<bool> CanUserPublishAsync(int userId)
        {
            try
            {
                // Obtener suscripción activa
                var subscription = await GetActiveUserSubscriptionAsync(userId);
                if (subscription == null)
                {
                    // Usuario sin suscripción activa
                    _logger.LogWarning("Usuario {UserId} no tiene suscripción activa", userId);
                    return false;
                }

                // Verificar si el plan permite publicaciones
                if (subscription.Plan.MaxPublications == 0)
                {
                    _logger.LogWarning("Plan {PlanId} no permite publicaciones", subscription.PlanId);
                    return false;
                }

                // Si es ilimitado (-1), siempre puede publicar
                if (subscription.Plan.MaxPublications == -1)
                {
                    return true;
                }

                // Contar publicaciones activas del usuario en el período de suscripción
                var publicationsCount = await _context.PropertyPublications
                    .Where(pp => pp.UserId == userId &&
                                pp.PublishedAt >= subscription.StartDate &&
                                pp.PublishedAt <= subscription.EndDate)
                    .CountAsync();

                return publicationsCount < subscription.Plan.MaxPublications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si usuario {UserId} puede publicar", userId);
                return false;
            }
        }

        public async Task<bool> DecrementPublicationCountAsync(int userId)
        {
            try
            {
                // Nota: En tu implementación actual, no hay un campo de contador
                // en UserSubscription. Aquí solo registramos la publicación.
                // Si necesitas decrementar un contador, necesitarías agregar un campo
                // PublicationsUsed a UserSubscription.

                // Por ahora, solo verificamos que puede publicar
                return await CanUserPublishAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al decrementar contador de publicaciones para usuario {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> HasPublicationQuotaAsync(int userId)
        {
            return await CanUserPublishAsync(userId);
        }

        public async Task<int> GetRemainingPublicationsAsync(int userId)
        {
            try
            {
                var subscription = await GetActiveUserSubscriptionAsync(userId);
                if (subscription == null || subscription.Plan.MaxPublications == 0)
                {
                    return 0;
                }

                // Si es ilimitado
                if (subscription.Plan.MaxPublications == -1)
                {
                    return int.MaxValue; // Representar ilimitado
                }

                var publicationsCount = await _context.PropertyPublications
                    .Where(pp => pp.UserId == userId &&
                                pp.PublishedAt >= subscription.StartDate &&
                                pp.PublishedAt <= subscription.EndDate)
                    .CountAsync();

                return Math.Max(0, subscription.Plan.MaxPublications - publicationsCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener publicaciones restantes para usuario {UserId}", userId);
                return 0;
            }
        }

        public async Task<SubscriptionPlan?> GetPlanByIdAsync(int planId)
        {
            try
            {
                return await _context.SubscriptionPlans
                    .FirstOrDefaultAsync(p => p.PlanId == planId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener plan con id {PlanId}", planId);
                return null;
            }
        }
    }
}