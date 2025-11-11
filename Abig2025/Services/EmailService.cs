namespace Abig2025.Services
{
    using global::Abig2025.Services.Interfaces;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System.Net;
    using System.Net.Mail;

    namespace Abig2025.Services
    {
        public class EmailService : IEmailService
        {
            private readonly IConfiguration _configuration;
            private readonly ILogger<EmailService> _logger;

            public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
            {
                _configuration = configuration;
                _logger = logger;
            }

            public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlMessage)
            {
                try
                {
                    var smtpSettings = _configuration.GetSection("SmtpSettings");

                    var smtpServer = smtpSettings["Server"];
                    var smtpPort = int.Parse(smtpSettings["Port"] ?? "587");
                    var fromAddress = smtpSettings["FromAddress"];
                    var fromName = smtpSettings["FromName"];
                    var userName = smtpSettings["UserName"];
                    var password = smtpSettings["Password"];
                    var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

                    using (var client = new SmtpClient(smtpServer, smtpPort))
                    {
                        client.Credentials = new NetworkCredential(userName, password);
                        client.EnableSsl = enableSsl;

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(fromAddress, fromName),
                            Subject = subject,
                            Body = htmlMessage,
                            IsBodyHtml = true
                        };

                        mailMessage.To.Add(toEmail);

                        await client.SendMailAsync(mailMessage);

                        _logger.LogInformation("Email enviado exitosamente a {Email}", toEmail);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando email a {Email}", toEmail);
                    return false;
                }
            }

            public async Task<bool> SendVerificationEmailAsync(string toEmail, string firstName, string verificationLink)
            {
                var subject = "Verifica tu email - Abig";
                var htmlMessage = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: #005555; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background: #f9f9f9; }}
                        .button {{ display: inline-block; padding: 12px 24px; background: #005555; color: #ffffff !important; text-decoration: none; border-radius: 5px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Bienvenido a Abig</h1>
                        </div>
                        <div class='content'>
                            <h2>Hola {firstName},</h2>
                            <p>Gracias por registrarte en Abig. Para activar tu cuenta, por favor verifica tu dirección de email haciendo clic en el siguiente botón:</p>
                            <p style='text-align: center;'>
                                <a href='{verificationLink}' class='button'>Verificar Email</a>
                            </p>
                            <p>Si el botón no funciona, copia y pega el siguiente enlace en tu navegador:</p>
                            <p><a href='{verificationLink}'>{verificationLink}</a></p>
                            <p><strong>Este enlace expirará en 24 horas.</strong></p>
                            <br>
                            <p>Si no creaste esta cuenta, puedes ignorar este mensaje.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; Abig. Todos los derechos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";

                return await SendEmailAsync(toEmail, subject, htmlMessage);
            }

            public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink)
            {
                var subject = "Restablecer contraseña - Abig";
                var htmlMessage = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: #dc3545; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background: #f9f9f9; }}
                        .button {{ display: inline-block; padding: 12px 24px; background: #dc3545; color: #ffffff !important; text-decoration: none; border-radius: 5px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Restablecer Contraseña</h1>
                        </div>
                        <div class='content'>
                            <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta.</p>
                            <p>Para restablecer tu contraseña, haz clic en el siguiente botón:</p>
                            <p style='text-align: center;'>
                                <a href='{resetLink}' class='button'>Restablecer Contraseña</a>
                            </p>
                            <p>Si el botón no funciona, copia y pega el siguiente enlace en tu navegador:</p>
                            <p><a href='{resetLink}'>{resetLink}</a></p>
                            <p><strong>Este enlace expirará en 1 hora.</strong></p>
                            <br>
                            <p>Si no solicitaste restablecer tu contraseña, puedes ignorar este mensaje.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 Abig. Todos los derechos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";

                return await SendEmailAsync(toEmail, subject, htmlMessage);
            }
        }
    }
}
