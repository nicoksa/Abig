namespace Abig2025.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlMessage);
        Task<bool> SendVerificationEmailAsync(string toEmail, string firstName, string verificationLink);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink);
    }
}
