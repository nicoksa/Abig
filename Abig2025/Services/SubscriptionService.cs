using Abig2025.Data;
using Abig2025.Helpers;
using Abig2025.Models.Properties;
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
            var remaining = await GetRemainingPublicationsAsync(userId);
            return remaining > 0;
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
                if (subscription == null)
                    return 0;

                var used = await GetUsedPublicationsAsync(userId);
                var max = subscription.Plan.MaxPublications;

                // Si es ilimitado, retornar un número grande o -1
                if (max == -1)
                    return int.MaxValue;

                return Math.Max(0, max - used);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener publicaciones restantes para el usuario {UserId}", userId);
                return 0;
            }
        }

        public async Task<int> GetUsedPublicationsAsync(int userId)
        {
            try
            {
                // Buscar suscripción activa
                var subscription = await GetActiveUserSubscriptionAsync(userId);
                if (subscription == null)
                    return 0;

                // Contar publicaciones activas en este período de suscripción
                var usedPublications = await _context.PropertyPublications
                    .Where(pp => pp.UserId == userId
                        && pp.IsActive
                        && pp.PublishedAt >= subscription.StartDate
                        && pp.PublishedAt <= subscription.EndDate)
                    .CountAsync();

                return usedPublications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener publicaciones usadas para el usuario {UserId}", userId);
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


        public async Task<bool> RegisterPublicationAsync(int userId, int propertyId, int planId)
        {
            try
            {
                _logger.LogInformation("Registrando publicación: User={UserId}, Property={PropertyId}, Plan={PlanId}",
                    userId, propertyId, planId);

                // 1. Verificar que existan las entidades (pero NO cargarlas)
                var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
                var propertyExists = await _context.Properties.AnyAsync(p => p.PropertyId == propertyId);
                var planExists = await _context.SubscriptionPlans.AnyAsync(p => p.PlanId == planId);

                if (!userExists || !propertyExists || !planExists)
                {
                    _logger.LogError("Validación falló: UserExists={UserExists}, PropertyExists={PropertyExists}, PlanExists={PlanExists}",
                        userExists, propertyExists, planExists);
                    return false;
                }

                // 2. Obtener el plan (solo para DurationDays)
                var plan = await _context.SubscriptionPlans
                    .AsNoTracking() // ¡IMPORTANTE: No tracking!
                    .FirstOrDefaultAsync(p => p.PlanId == planId);

                if (plan == null) return false;

                // 3. Crear publicación - SOLO con IDs
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
                    Notes = "Publicación registrada automáticamente"
                };

                // 4. IMPORTANTE: Asegurarse de que no hay navegaciones
                publication.Property = null!;
                publication.User = null!;
                publication.Plan = null!;

                _context.PropertyPublications.Add(publication);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Publicación registrada exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar publicación");
                return false;
            }
        }
    }
}