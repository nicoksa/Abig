using Abig2025.Models.Subscriptions;

namespace Abig2025.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<UserSubscription?> GetActiveUserSubscriptionAsync(int userId);
        Task<List<SubscriptionPlan>> GetAvailablePlansAsync();
        Task<bool> CanUserPublishAsync(int userId);
        Task<bool> DecrementPublicationCountAsync(int userId);
        Task<bool> HasPublicationQuotaAsync(int userId);
        Task<int> GetRemainingPublicationsAsync(int userId);
        Task<SubscriptionPlan?> GetPlanByIdAsync(int planId);

        Task<int> GetUsedPublicationsAsync(int userId);
        Task<bool> RegisterPublicationAsync(int userId, int propertyId, int planId);
    }
}