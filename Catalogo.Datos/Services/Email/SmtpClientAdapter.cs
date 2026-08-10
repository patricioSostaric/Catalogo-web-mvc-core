using catalogo_web_mvc.Interfaces.Email;
using catalogo_web_mvc.Models.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace catalogo_web_mvc.Services.Email
{
    public class SmtpClientAdapter : ISmtpClient, IDisposable
    {
        private readonly SmtpClient _client;

        public SmtpClientAdapter(IOptions<SmtpSettings> options)
        {
            var settings = options.Value;
            _client = new SmtpClient(settings.Host, settings.Port)
            {
                EnableSsl = settings.EnableSsl,
                Credentials = new NetworkCredential(settings.User, settings.Password)
            };
        }

        public Task SendMailAsync(MailMessage message) => _client.SendMailAsync(message);

        public void Dispose() => _client.Dispose();
    }
}
