using catalogo_web_mvc.Services.Identity;

namespace CatalogoWeb.Tests.Services
{
    public class SpanishIdentityErrorDescriberTests
    {
        private readonly SpanishIdentityErrorDescriber _describer = new();

        [Fact]
        public void PasswordRequiresNonAlphanumeric_DevuelveMensajeEnEspaniol()
        {
            var error = _describer.PasswordRequiresNonAlphanumeric();

            Assert.Contains("no alfanumérico", error.Description);
        }

        [Fact]
        public void DuplicateEmail_DevuelveMensajeEnEspaniolConElEmail()
        {
            var error = _describer.DuplicateEmail("user@test.com");

            Assert.Contains("user@test.com", error.Description);
            Assert.Contains("ya está en uso", error.Description);
        }

        [Fact]
        public void PasswordTooShort_DevuelveMensajeEnEspaniolConLaLongitud()
        {
            var error = _describer.PasswordTooShort(8);

            Assert.Contains("8", error.Description);
            Assert.Contains("mínimo", error.Description);
        }
    }
}
