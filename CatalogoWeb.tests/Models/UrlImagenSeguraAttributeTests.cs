using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace CatalogoWeb.Tests.Models
{
    public class UrlImagenSeguraAttributeTests
    {
        private static ValidationResult? Validar(string? valor, string? prefijo = null)
        {
            var atributo = new UrlImagenSeguraAttribute { PrefijoLocalPermitido = prefijo };
            return atributo.GetValidationResult(valor, new ValidationContext(new object()));
        }

        // ── Esquemas peligrosos ────────────────────────────────────────────────

        [Theory]
        [InlineData("javascript:alert(1)")]
        [InlineData("JAVASCRIPT:alert(1)")]
        [InlineData("vbscript:msgbox(1)")]
        [InlineData("data:text/html,<script>alert(1)</script>")]
        [InlineData("data:image/svg+xml;base64,PHN2Zz48c2NyaXB0PmFsZXJ0KDEpPC9zY3JpcHQ+PC9zdmc+")]
        [InlineData("file:///C:/Windows/win.ini")]
        [InlineData("ftp://host/foto.png")]
        public void EsquemaPeligroso_EsInvalido(string url)
        {
            Assert.NotNull(Validar(url));
        }

        [Fact]
        public void HttpPlano_EsInvalido()
        {
            Assert.NotNull(Validar("http://ejemplo.com/foto.png"));
        }

        [Fact]
        public void ProtocolRelative_EsInvalido()
        {
            Assert.NotNull(Validar("//evil.com/foto.png"));
        }

        [Fact]
        public void EsquemaPartidoConSaltoDeLinea_EsInvalido()
        {
            Assert.NotNull(Validar("java\nscript:alert(1)"));
        }

        // ── Casos válidos ──────────────────────────────────────────────────────

        [Fact]
        public void HttpsAbsoluta_EsValida()
        {
            Assert.Null(Validar("https://ejemplo.com/foto.png"));
        }

        [Fact]
        public void HttpsConQueryString_EsValida()
        {
            // Varias de las URLs del seed traen query string; no deben romperse.
            Assert.Null(Validar("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9Gc&s"));
        }

        [Fact]
        public void Nulo_EsValido()
        {
            // El campo es opcional: la obligatoriedad la decide [Required].
            Assert.Null(Validar(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void VacioOEspacios_EsValido(string valor)
        {
            Assert.Null(Validar(valor));
        }

        // ── Rutas locales según el prefijo configurado ─────────────────────────

        [Fact]
        public void RutaLocalBajoElPrefijoPermitido_EsValida()
        {
            Assert.Null(Validar("/img/producto.png", prefijo: "/img/"));
        }

        [Fact]
        public void RutaLocalSinPrefijoConfigurado_EsInvalida()
        {
            // Sin prefijo permitido, solo pasan las https absolutas.
            Assert.NotNull(Validar("/img/producto.png"));
        }

        [Theory]
        [InlineData("/img/../appsettings.json")]
        [InlineData("/img/sub/producto.png")]
        [InlineData("/img/")]
        public void RutaLocalManipulada_EsInvalida(string ruta)
        {
            Assert.NotNull(Validar(ruta, prefijo: "/img/"));
        }

        [Fact]
        public void RutaLocalFueraDelPrefijo_EsInvalida()
        {
            Assert.NotNull(Validar("/appsettings.json", prefijo: "/img/"));
        }

        // ── Integración con el modelo Articulo ────────────────────────────────

        private static IList<ValidationResult> ValidarArticulo(string? imagenUrl)
        {
            var articulo = new Articulo
            {
                Codigo = "A01",
                Nombre = "Test",
                Descripcion = "Test",
                MarcaId = 1,
                CategoriaId = 1,
                Precio = 100m,
                Stock = 1,
                ImagenUrl = imagenUrl
            };

            var resultados = new List<ValidationResult>();
            Validator.TryValidateObject(articulo, new ValidationContext(articulo), resultados, validateAllProperties: true);
            return resultados;
        }

        [Fact]
        public void Articulo_ConImagenUrlHttps_EsValido()
        {
            Assert.Empty(ValidarArticulo("https://ejemplo.com/foto.png"));
        }

        [Fact]
        public void Articulo_SinImagenUrl_EsValido()
        {
            Assert.Empty(ValidarArticulo(null));
        }

        [Fact]
        public void Articulo_ConImagenUrlLocalBajoImg_EsValido()
        {
            Assert.Empty(ValidarArticulo("/img/producto.png"));
        }

        [Theory]
        [InlineData("javascript:alert(document.cookie)")]
        [InlineData("data:image/svg+xml,<svg onload=alert(1)>")]
        [InlineData("http://ejemplo.com/foto.png")]
        public void Articulo_ConImagenUrlPeligrosa_EsInvalido(string url)
        {
            var errores = ValidarArticulo(url);

            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Articulo.ImagenUrl)));
        }
    }
}
