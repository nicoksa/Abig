using Abig2025.Models.Users;

namespace Abig2025.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool success, string message)> RegisterAsync(User user, string password, UserProfile? userProfile = null);
        Task<bool> VerifyEmailAsync(string token);
        Task<(bool success, User user)> LoginAsync(string email, string password, string ipAddress, string userAgent);
        Task<bool> EmailExistsAsync(string email);
        Task LogLoginAttemptAsync(string email, bool isSuccessful, string ipAddress, string userAgent, int? userId = null);
    }
}
