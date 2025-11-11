using Abig2025.Models.Users;
using System.Security.Claims;

namespace Abig2025.Services.Interfaces
{
    public interface IExternalAuthService
    {
        Task<(bool success, string message, User user)> HandleGoogleLoginAsync(ClaimsPrincipal principal);
        Task<bool> IsGoogleUserRegisteredAsync(string googleId);
        Task<User> CreateUserFromGoogleAsync(string googleId, string email, string firstName, string lastName, string fullName = null);
    }
}