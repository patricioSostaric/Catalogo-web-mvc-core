using catalogo_web_mvc.Services.Email;

namespace CatalogoWeb.Tests.Services
{
    public class EmailTemplatesTests
    {
        [Fact]
        public void BuildWelcomeEmail_IncluyeElEmailDelUsuario()
        {
            var (subject, body) = EmailTemplates.BuildWelcomeEmail("user@test.com");

            Assert.False(string.IsNullOrWhiteSpace(subject));
            Assert.Contains("user@test.com", body);
        }

        [Fact]
        public void BuildWelcomeEmail_EscapaHtmlParaEvitarInyeccion()
        {
            var maliciosoEmail = "<script>alert(1)</script>@test.com";

            var (_, body) = EmailTemplates.BuildWelcomeEmail(maliciosoEmail);

            Assert.DoesNotContain("<script>", body);
        }

        [Fact]
        public void BuildPasswordResetEmail_IncluyeElEnlace()
        {
            var link = "https://catalogo.com/Account/ResetPassword?email=user@test.com&token=abc";

            var (subject, body) = EmailTemplates.BuildPasswordResetEmail(link);

            Assert.False(string.IsNullOrWhiteSpace(subject));
            Assert.Contains("ResetPassword", body);
        }
    }
}
