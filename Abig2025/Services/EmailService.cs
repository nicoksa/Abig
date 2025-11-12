namespace Abig2025.Services
{
    using global::Abig2025.Services.Interfaces;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System.Net;
    using System.Net.Mail;

   
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
            var subject = "Verifica tu dirección de correo electrónico - Abig";
            var htmlMessage = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    /* Reset CSS */
                    body, html {{ margin: 0; padding: 0; font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333333; background-color: #f5f5f5; }}
                    .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }}
                    .header {{ background: linear-gradient(135deg, #005555, #008080); color: white; padding: 30px 20px; text-align: center; }}
                    .header h1 {{ margin: 0; font-size: 24px; font-weight: 600; }}
                    .content {{ padding: 40px 30px; }}
                    .greeting {{ font-size: 18px; color: #005555; margin-bottom: 20px; font-weight: 600; }}
                    .message {{ margin-bottom: 25px; color: #555555; font-size: 15px; line-height: 1.7; }}
                    .button-container {{ text-align: center; margin: 30px 0; }}
                   
                    .link-text {{ background: #f8f9fa; padding: 15px; border-radius: 4px; border-left: 4px solid #005555; margin: 20px 0; word-break: break-all; font-size: 13px; color: #666666; }}
                    .expiry-notice {{ background: #fff3cd; padding: 12px; border-radius: 4px; border: 1px solid #ffeaa7; margin: 20px 0; font-size: 14px; color: #856404; }}
                    .security-notice {{ background: #d1ecf1; padding: 12px; border-radius: 4px; border: 1px solid #bee5eb; margin: 20px 0; font-size: 14px; color: #0c5460; }}
                    .footer {{ background: #f8f9fa; padding: 25px 20px; text-align: center; font-size: 12px; color: #6c757d; border-top: 1px solid #e9ecef; }}
                    .company-info {{ margin-bottom: 10px; }}
                    .contact-info {{ font-size: 11px; color: #868e96; }}
                    @media only screen and (max-width: 600px) {{
                        .container {{ margin: 10px; }}
                        .content {{ padding: 25px 20px; }}
                        .header {{ padding: 25px 15px; }}
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Abig</h1>
                    </div>
                    <div class='content'>
                        <div class='greeting'>Estimado/a {firstName},</div>
                        
                        <div class='message'>
                            Le damos la más cordial bienvenida a Abig. Para comenzar a utilizar todos los servicios, necesitamos verificar que esta dirección de correo electrónico le pertenece.
                        </div>
                        
                        <div class='button-container'>
                            <a href='{verificationLink}' style='display: inline-block; padding: 14px 32px; background: #005555; color: white; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 15px;'>Verificar Email</a>
                        </div>
                        
                        <div class='expiry-notice'>
                            <strong>⏰ Enlace válido por 24 horas:</strong> Por seguridad, este enlace de verificación expirará en 24 horas.
                        </div>
                        
                        <div class='message'>
                            Si experimenta problemas con el botón anterior, copie y pegue la siguiente URL en su navegador:
                        </div>
                        
                        <div class='link-text'>{verificationLink}</div>
                        
                        <div class='security-notice'>
                            <strong>🔒 Nota de seguridad:</strong> Si no reconoce esta actividad o no solicitó crear una cuenta en Abig, por favor ignore este mensaje.
                        </div>
                    </div>
                    <div class='footer'>
                        <div class='company-info'>
                            <strong>Abig</strong> 
                        </div>
                        <div class='contact-info'>
                            © {DateTime.Now.Year} Abig. Todos los derechos reservados.<br>
                            Este es un mensaje automático, por favor no responda a este correo.
                        </div>
                    </div>
                </div>
            </body>
            </html>";

            return await SendEmailAsync(toEmail, subject, htmlMessage);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var subject = "Solicitud de restablecimiento de contraseña - Abig";
            var htmlMessage = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body, html {{ margin: 0; padding: 0; font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333333; background-color: #f5f5f5; }}
                    .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }}
                    .header {{ background: linear-gradient(135deg, #dc3545, #e35d6a); color: white; padding: 30px 20px; text-align: center; }}
                    .header h1 {{ margin: 0; font-size: 24px; font-weight: 600; }}
                    .content {{ padding: 40px 30px; }}
                    .message {{ margin-bottom: 25px; color: #555555; font-size: 15px; line-height: 1.7; }}
                    .button-container {{ text-align: center; margin: 30px 0; }}
                    
                    .urgent-notice {{ background: #f8d7da; padding: 15px; border-radius: 4px; border-left: 4px solid #dc3545; margin: 20px 0; font-size: 14px; color: #721c24; }}
                    .link-text {{ background: #f8f9fa; padding: 15px; border-radius: 4px; border-left: 4px solid #dc3545; margin: 20px 0; word-break: break-all; font-size: 13px; color: #666666; }}
                    .security-info {{ background: #e2e3e5; padding: 12px; border-radius: 4px; margin: 20px 0; font-size: 14px; color: #383d41; }}
                    .footer {{ background: #f8f9fa; padding: 25px 20px; text-align: center; font-size: 12px; color: #6c757d; border-top: 1px solid #e9ecef; }}
                    .company-info {{ margin-bottom: 10px; }}
                    .contact-info {{ font-size: 11px; color: #868e96; }}
                    @media only screen and (max-width: 600px) {{
                        .container {{ margin: 10px; }}
                        .content {{ padding: 25px 20px; }}
                        .header {{ padding: 25px 15px; }}
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Restablecer Contraseña</h1>
                    </div>
                    <div class='content'>
                        <div class='message'>
                            Hemos recibido una solicitud para restablecer la contraseña de su cuenta en Abig. Si usted realizó esta solicitud, utilice el siguiente enlace para crear una nueva contraseña.
                        </div>
                        
                        <div class='button-container'>
                            <a href='{resetLink}' style='display: inline-block; padding: 14px 32px; background: #dc3545; color: white; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 15px;'>Restablecer Contraseña</a>
                        </div>
                        
                        <div class='urgent-notice'>
                            <strong>⚠️ Enlace de uso único:</strong> Este enlace es válido por 1 hora y solo puede ser utilizado una vez.
                        </div>
                        
                        <div class='message'>
                            Si el botón no funciona, copie y pegue la siguiente dirección en la barra de direcciones de su navegador:
                        </div>
                        
                        <div class='link-text'>{resetLink}</div>
                        
                        <div class='security-info'>
                            <strong>¿No solicitó este cambio?</strong><br>
                            Si no reconoce esta solicitud, le recomendamos ignorar este mensaje y verificar la seguridad de su cuenta. Su contraseña actual permanecerá activa.
                        </div>
                    </div>
                    <div class='footer'>
                        <div class='company-info'>
                            <strong>Abig</strong> - Protegiendo su información
                        </div>
                        <div class='contact-info'>
                            © {DateTime.Now.Year} Abig. Todos los derechos reservados.<br>
                            Por su seguridad, nunca comparta este enlace con otras personas.
                        </div>
                    </div>
                </div>
            </body>
            </html>";

            return await SendEmailAsync(toEmail, subject, htmlMessage);
        }
    }
}
    

