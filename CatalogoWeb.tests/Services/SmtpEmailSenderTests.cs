using catalogo_web_mvc.Interfaces.Email;
using catalogo_web_mvc.Models.Settings;
using catalogo_web_mvc.Services.Email;
using Microsoft.Extensions.Options;
using Moq;
using System.Net.Mail;

namespace CatalogoWeb.Tests.Services
{
    public class SmtpEmailSenderTests
    {
        private readonly Mock<ISmtpClient> _smtpClientMock;
        private readonly SmtpEmailSender _sender;

        public SmtpEmailSenderTests()
        {
            _smtpClientMock = new Mock<ISmtpClient>();
            var settings = Options.Create(new SmtpSettings
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                User = "no-reply@catalogo.com",
                Password = "app-password",
                FromName = "Catálogo Web"
            });

            _sender = new SmtpEmailSender(_smtpClientMock.Object, settings);
        }

        [Fact]
        public async Task SendEmailAsync_DestinatarioVacio_LanzaArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sender.SendEmailAsync("", "Asunto", "<p>Cuerpo</p>"));
        }

        [Fact]
        public async Task SendEmailAsync_AsuntoVacio_LanzaArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sender.SendEmailAsync("user@test.com", "", "<p>Cuerpo</p>"));
        }

        [Fact]
        public async Task SendEmailAsync_CuerpoVacio_LanzaArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sender.SendEmailAsync("user@test.com", "Asunto", ""));
        }

        [Fact]
        public async Task SendEmailAsync_EmailConSaltoDeLinea_LanzaFormatException()
        {
            // Protección OWASP contra inyección de encabezados SMTP (CRLF injection)
            var maliciosoEmail = "user@test.com\r\nBcc:atacante@evil.com";

            await Assert.ThrowsAsync<FormatException>(() =>
                _sender.SendEmailAsync(maliciosoEmail, "Asunto", "<p>Cuerpo</p>"));
        }

        [Fact]
        public async Task SendEmailAsync_DatosValidos_EnviaMailConDestinatarioYAsuntoCorrectos()
        {
            MailMessage? mensajeEnviado = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>()))
                .Callback<MailMessage>(m => mensajeEnviado = m)
                .Returns(Task.CompletedTask);

            await _sender.SendEmailAsync("user@test.com", "Bienvenido", "<p>Hola</p>");

            _smtpClientMock.Verify(c => c.SendMailAsync(It.IsAny<MailMessage>()), Times.Once);
            Assert.NotNull(mensajeEnviado);
            Assert.Equal("user@test.com", mensajeEnviado!.To[0].Address);
            Assert.Equal("Bienvenido", mensajeEnviado.Subject);
            Assert.True(mensajeEnviado.IsBodyHtml);
        }
    }
}
