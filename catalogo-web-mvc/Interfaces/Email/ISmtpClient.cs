using System.Net.Mail;

namespace catalogo_web_mvc.Interfaces.Email
{
    public interface ISmtpClient
    {
        Task SendMailAsync(MailMessage message);
    }
}
