using catalogo_web_mvc.Services.Avatar;

namespace CatalogoWeb.Tests.Services
{
    public class AvatarUrlValidatorTests
    {
        // ── Esquemas peligrosos ────────────────────────────────────────────────

        [Theory]
        [InlineData("javascript:alert(1)")]
        [InlineData("JavaScript:alert(1)")]
        [InlineData("vbscript:msgbox(1)")]
        [InlineData("data:image/svg+xml;base64,PHN2Zz48c2NyaXB0PmFsZXJ0KDEpPC9zY3JpcHQ+PC9zdmc+")]
        [InlineData("data:text/html,<script>alert(1)</script>")]
        [InlineData("file:///C:/Windows/System32/drivers/etc/hosts")]
        [InlineData("ftp://host/foto.png")]
        public void TryNormalizar_EsquemaPeligroso_EsRechazado(string url)
        {
            var valido = AvatarUrlValidator.TryNormalizar(url, out var normalizado);

            Assert.False(valido);
            Assert.Null(normalizado);
        }

        [Fact]
        public void TryNormalizar_HttpPlano_EsRechazado()
        {
            // Se exige https: http rompe la página por mixed content y expone a MITM.
            Assert.False(AvatarUrlValidator.TryNormalizar("http://ejemplo.com/foto.png", out _));
        }

        [Theory]
        [InlineData("//ejemplo.com/foto.png")]
        [InlineData("//evil.com/x.png")]
        public void TryNormalizar_UrlProtocolRelative_EsRechazada(string url)
        {
            // El navegador la resuelve heredando el esquema, así que esquivaría el filtro.
            Assert.False(AvatarUrlValidator.TryNormalizar(url, out _));
        }

        [Fact]
        public void TryNormalizar_EsquemaPartidoConCaracterDeControl_EsRechazado()
        {
            // "java\nscript:" es un truco clásico para partir el parseo del esquema.
            Assert.False(AvatarUrlValidator.TryNormalizar("java\nscript:alert(1)", out _));
        }

        // ── URLs https válidas ────────────────────────────────────────────────

        [Fact]
        public void TryNormalizar_HttpsAbsoluta_EsAceptada()
        {
            var valido = AvatarUrlValidator.TryNormalizar("https://ejemplo.com/foto.png", out var normalizado);

            Assert.True(valido);
            Assert.Equal("https://ejemplo.com/foto.png", normalizado);
        }

        [Fact]
        public void TryNormalizar_HttpsConEspaciosAlrededor_SeRecorta()
        {
            var valido = AvatarUrlValidator.TryNormalizar("  https://ejemplo.com/foto.png  ", out var normalizado);

            Assert.True(valido);
            Assert.Equal("https://ejemplo.com/foto.png", normalizado);
        }

        // ── Rutas locales ─────────────────────────────────────────────────────

        [Fact]
        public void TryNormalizar_RutaLocalDeAvatares_EsAceptada()
        {
            var ruta = "/uploads/avatars/abc123.jpg";

            var valido = AvatarUrlValidator.TryNormalizar(ruta, out var normalizado);

            Assert.True(valido);
            Assert.Equal(ruta, normalizado);
        }

        [Theory]
        [InlineData("/uploads/avatars/../../appsettings.json")]
        [InlineData("/uploads/avatars/sub/foto.jpg")]
        [InlineData("/uploads/avatars/sub\\foto.jpg")]
        [InlineData("/uploads/avatars/")]
        public void TryNormalizar_RutaLocalManipulada_EsRechazada(string ruta)
        {
            Assert.False(AvatarUrlValidator.TryNormalizar(ruta, out _));
        }

        [Theory]
        [InlineData("/img/logo.png")]
        [InlineData("/appsettings.json")]
        [InlineData("/uploads/otracarpeta/foto.jpg")]
        public void TryNormalizar_RutaLocalFueraDeAvatares_EsRechazada(string ruta)
        {
            Assert.False(AvatarUrlValidator.TryNormalizar(ruta, out _));
        }

        // ── Sin avatar ────────────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TryNormalizar_ValorVacio_EsValidoYNormalizaANull(string? valor)
        {
            var valido = AvatarUrlValidator.TryNormalizar(valor, out var normalizado);

            Assert.True(valido);
            Assert.Null(normalizado);
        }

        // ── Resolución para la vista ──────────────────────────────────────────

        [Fact]
        public void ResolverParaMostrar_AvatarValido_DevuelveElAvatar()
        {
            var resultado = AvatarUrlValidator.ResolverParaMostrar("/uploads/avatars/abc.jpg");

            Assert.Equal("/uploads/avatars/abc.jpg", resultado);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("javascript:alert(1)")]
        [InlineData("http://ejemplo.com/foto.png")]
        public void ResolverParaMostrar_AvatarInvalidoOAusente_DevuelveElPorDefecto(string? valor)
        {
            var resultado = AvatarUrlValidator.ResolverParaMostrar(valor);

            Assert.Equal(AvatarUrlValidator.RutaPorDefecto, resultado);
        }

        [Fact]
        public void ResolverParaMostrar_NuncaDevuelveNull()
        {
            Assert.NotNull(AvatarUrlValidator.ResolverParaMostrar(null));
        }
    }
}
