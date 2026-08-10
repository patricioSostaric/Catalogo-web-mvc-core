using catalogo_web_mvc.Interfaces.Email;
using catalogo_web_mvc.Models.Settings;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace catalogo_web_mvc.Services.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly ISmtpClient _smtpClient;
        private readonly SmtpSettings _settings;

        public SmtpEmailSender(ISmtpClient smtpClient, IOptions<SmtpSettings> options)
        {
            _smtpClient = smtpClient;
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("El destinatario es obligatorio.", nameof(toEmail));
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("El asunto es obligatorio.", nameof(subject));
            if (string.IsNullOrWhiteSpace(htmlBody))
                throw new ArgumentException("El cuerpo del correo es obligatorio.", nameof(htmlBody));

            // MailAddress valida el formato y rechaza saltos de línea, lo que evita
            // inyección de encabezados SMTP (CRLF injection / header injection - OWASP).
            var to = new MailAddress(toEmail);
            var from = new MailAddress(_settings.User, _settings.FromName);

            using var message = new MailMessage(from, to)
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            await _smtpClient.SendMailAsync(message);
        }
    }
}
