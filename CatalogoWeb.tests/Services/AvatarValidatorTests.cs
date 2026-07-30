using catalogo_web_mvc.Services.Avatar;

namespace CatalogoWeb.Tests.Services
{
    public class AvatarValidatorTests
    {
        // Firmas reales de cada formato, para no depender de archivos en disco.
        private static byte[] HeaderPng() => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D];
        private static byte[] HeaderJpeg() => [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01];
        private static byte[] HeaderWebp() => [0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50];
        private static byte[] HeaderBasura() => [0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00];

        // ── Casos válidos ──────────────────────────────────────────────────────

        [Theory]
        [InlineData("foto.png")]
        [InlineData("FOTO.PNG")]
        public void Validar_PngConFirmaCorrecta_EsValido(string nombre)
        {
            var resultado = AvatarValidator.Validar(nombre, 1024, HeaderPng());

            Assert.True(resultado.EsValido);
            Assert.Null(resultado.Error);
        }

        [Theory]
        [InlineData("foto.jpg")]
        [InlineData("foto.jpeg")]
        public void Validar_JpegConFirmaCorrecta_EsValido(string nombre)
        {
            var resultado = AvatarValidator.Validar(nombre, 1024, HeaderJpeg());

            Assert.True(resultado.EsValido);
        }

        [Fact]
        public void Validar_WebpConFirmaCorrecta_EsValido()
        {
            var resultado = AvatarValidator.Validar("foto.webp", 1024, HeaderWebp());

            Assert.True(resultado.EsValido);
        }

        // ── Extensión mentida: el corazón de la validación ─────────────────────

        [Fact]
        public void Validar_EjecutableRenombradoAPng_EsRechazado()
        {
            // "MZ" es la cabecera de un .exe de Windows. Con solo mirar la extensión
            // este archivo pasaría; se rechaza porque la firma no coincide.
            var resultado = AvatarValidator.Validar("payload.png", 1024, HeaderBasura());

            Assert.False(resultado.EsValido);
            Assert.Contains("no coincide", resultado.Error);
        }

        [Fact]
        public void Validar_PngConFirmaDeJpeg_EsRechazado()
        {
            var resultado = AvatarValidator.Validar("foto.png", 1024, HeaderJpeg());

            Assert.False(resultado.EsValido);
        }

        [Fact]
        public void Validar_WebpSinElMarcadorWEBP_EsRechazado()
        {
            // Empieza con RIFF pero no es WEBP (podría ser un .wav).
            byte[] riffNoWebp = [0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45];

            var resultado = AvatarValidator.Validar("audio.webp", 1024, riffNoWebp);

            Assert.False(resultado.EsValido);
        }

        [Fact]
        public void Validar_HeaderMasCortoQueLaFirma_EsRechazado()
        {
            var resultado = AvatarValidator.Validar("foto.png", 2, [0x89, 0x50]);

            Assert.False(resultado.EsValido);
        }

        // ── Extensiones fuera de la allowlist ─────────────────────────────────

        [Theory]
        [InlineData("script.svg")]   // SVG puede contener <script>: XSS almacenado
        [InlineData("shell.php")]
        [InlineData("codigo.aspx")]
        [InlineData("archivo.exe")]
        [InlineData("texto.txt")]
        [InlineData("animado.gif")]
        [InlineData("sinextension")]
        public void ValidarNombreYTamano_ExtensionNoPermitida_EsRechazado(string nombre)
        {
            var resultado = AvatarValidator.ValidarNombreYTamano(nombre, 1024);

            Assert.False(resultado.EsValido);
            Assert.Contains("Formato no permitido", resultado.Error);
        }

        [Fact]
        public void ValidarNombreYTamano_DobleExtension_SeEvaluaLaUltima()
        {
            // "shell.php.png" termina en .png, así que pasa el filtro de extensión.
            // Lo que lo bloquea es la firma del contenido, y además el archivo se guarda
            // con un nombre GUID generado por el servidor.
            var permitida = AvatarValidator.ValidarNombreYTamano("shell.php.png", 1024);
            Assert.True(permitida.EsValido);

            var conContenido = AvatarValidator.Validar("shell.php.png", 1024, HeaderBasura());
            Assert.False(conContenido.EsValido);
        }

        // ── Path traversal en el nombre ───────────────────────────────────────

        [Theory]
        [InlineData("../../../web.config.png")]
        [InlineData("..\\..\\appsettings.json.png")]
        [InlineData("carpeta/foto.png")]
        [InlineData("carpeta\\foto.png")]
        public void ValidarNombreYTamano_NombreConRutas_EsRechazado(string nombre)
        {
            var resultado = AvatarValidator.ValidarNombreYTamano(nombre, 1024);

            Assert.False(resultado.EsValido);
            Assert.Contains("no es válido", resultado.Error);
        }

        [Fact]
        public void ValidarNombreYTamano_NombreConByteNulo_EsRechazado()
        {
            var resultado = AvatarValidator.ValidarNombreYTamano("foto.png\0.exe", 1024);

            Assert.False(resultado.EsValido);
        }

        // ── Tamaño ────────────────────────────────────────────────────────────

        [Fact]
        public void ValidarNombreYTamano_ArchivoQueSuperaElMaximo_EsRechazado()
        {
            var resultado = AvatarValidator.ValidarNombreYTamano("foto.png", AvatarValidator.MaxBytes + 1);

            Assert.False(resultado.EsValido);
            Assert.Contains("supera el máximo", resultado.Error);
        }

        [Fact]
        public void ValidarNombreYTamano_ArchivoExactamenteEnElMaximo_EsAceptado()
        {
            var resultado = AvatarValidator.ValidarNombreYTamano("foto.png", AvatarValidator.MaxBytes);

            Assert.True(resultado.EsValido);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ValidarNombreYTamano_ArchivoVacio_EsRechazado(long tamano)
        {
            var resultado = AvatarValidator.ValidarNombreYTamano("foto.png", tamano);

            Assert.False(resultado.EsValido);
            Assert.Contains("vacío", resultado.Error);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidarNombreYTamano_SinNombre_EsRechazado(string? nombre)
        {
            var resultado = AvatarValidator.ValidarNombreYTamano(nombre, 1024);

            Assert.False(resultado.EsValido);
        }

        // ── Extensión canónica ────────────────────────────────────────────────

        [Theory]
        [InlineData("foto.jpeg", ".jpg")]
        [InlineData("foto.JPEG", ".jpg")]
        [InlineData("foto.jpg", ".jpg")]
        [InlineData("foto.PNG", ".png")]
        [InlineData("foto.webp", ".webp")]
        public void ExtensionCanonica_NormalizaLaExtension(string nombre, string esperada)
        {
            Assert.Equal(esperada, AvatarValidator.ExtensionCanonica(nombre));
        }
    }
}
