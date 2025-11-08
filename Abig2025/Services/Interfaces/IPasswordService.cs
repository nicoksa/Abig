namespace Abig2025.Services.Interfaces
{
    public interface IPasswordService
    {
        Task<(bool success, string message)> ForgotPasswordAsync(string email);
        Task<(bool success, string message)> ResetPasswordAsync(string token,string email, string newPassword);
        Task<(bool success, string message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ValidatePasswordAsync(string password);
    }
}
