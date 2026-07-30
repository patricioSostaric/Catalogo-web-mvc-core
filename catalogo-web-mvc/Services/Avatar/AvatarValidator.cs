namespace catalogo_web_mvc.Services.Avatar
{
    /// <summary>
    /// Resultado de validar un archivo de avatar. <see cref="Error"/> viene con el
    /// mensaje listo para mostrar cuando <see cref="EsValido"/> es false.
    /// </summary>
    public sealed record AvatarValidationResult(bool EsValido, string? Error)
    {
        public static AvatarValidationResult Ok() => new(true, null);
        public static AvatarValidationResult Falla(string error) => new(false, error);
    }

    /// <summary>
    /// Validación de imágenes de perfil subidas por el usuario.
    ///
    /// Es una clase estática y sin dependencias a propósito: no toca el disco ni el
    /// HttpContext, así que se puede testear con arrays de bytes armados a mano.
    /// El IO queda del lado de <see cref="AvatarService"/>.
    ///
    /// Riesgos que cubre (OWASP A03 Injection / A04 Insecure Design):
    ///  - Subida de archivos arbitrarios: allowlist cerrada de extensiones.
    ///  - Extensión mentida: se compara la firma real del archivo (magic bytes)
    ///    contra la extensión declarada. Un .exe renombrado a .png no pasa.
    ///  - XSS almacenado vía SVG: el SVG puede contener &lt;script&gt; y el navegador lo
    ///    ejecuta al servirlo desde nuestro propio origen. Queda fuera de la allowlist.
    ///  - DoS por archivos gigantes: límite de tamaño.
    ///  - Path traversal: acá se rechazan los nombres con separadores o "..";
    ///    además AvatarService nunca usa el nombre del cliente para escribir.
    /// </summary>
    public static class AvatarValidator
    {
        /// <summary>2 MB. Suficiente para una foto de perfil de 40px sin habilitar un DoS barato.</summary>
        public const long MaxBytes = 2 * 1024 * 1024;

        /// <summary>Bytes necesarios para reconocer las firmas soportadas (WEBP necesita 12).</summary>
        public const int HeaderBytes = 12;

        private static readonly string[] ExtensionesPermitidas = [".jpg", ".jpeg", ".png", ".webp"];

        private static readonly byte[] FirmaPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        private static readonly byte[] FirmaJpeg = [0xFF, 0xD8, 0xFF];
        private static readonly byte[] FirmaRiff = [0x52, 0x49, 0x46, 0x46]; // "RIFF"
        private static readonly byte[] FirmaWebp = [0x57, 0x45, 0x42, 0x50]; // "WEBP" en el offset 8

        /// <summary>
        /// Valida nombre, tamaño y contenido. <paramref name="header"/> son los primeros
        /// <see cref="HeaderBytes"/> bytes del archivo (puede venir más corto).
        /// </summary>
        public static AvatarValidationResult Validar(string? nombreArchivo, long tamanoBytes, ReadOnlySpan<byte> header)
        {
            var previa = ValidarNombreYTamano(nombreArchivo, tamanoBytes);
            if (!previa.EsValido) return previa;

            return ValidarContenido(nombreArchivo!, header);
        }

        /// <summary>
        /// Chequeos que no necesitan leer el archivo: presencia, tamaño y extensión.
        /// </summary>
        public static AvatarValidationResult ValidarNombreYTamano(string? nombreArchivo, long tamanoBytes)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo))
                return AvatarValidationResult.Falla("No se recibió ningún archivo.");

            if (tamanoBytes <= 0)
                return AvatarValidationResult.Falla("El archivo está vacío.");

            if (tamanoBytes > MaxBytes)
                return AvatarValidationResult.Falla($"La imagen supera el máximo de {MaxBytes / (1024 * 1024)} MB.");

            // Un nombre con separadores o ".." nunca es legítimo viniendo de un input file.
            // No alcanza para explotar nada porque no usamos este nombre al escribir,
            // pero si aparece es señal de manipulación y se corta acá.
            if (nombreArchivo.Contains("..", StringComparison.Ordinal) ||
                nombreArchivo.Contains('/') || nombreArchivo.Contains('\\') ||
                nombreArchivo.Contains('\0'))
            {
                return AvatarValidationResult.Falla("El nombre del archivo no es válido.");
            }

            var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
                return AvatarValidationResult.Falla("Formato no permitido. Usá JPG, PNG o WEBP.");

            return AvatarValidationResult.Ok();
        }

        /// <summary>
        /// Compara la firma real del archivo contra la extensión declarada.
        /// </summary>
        public static AvatarValidationResult ValidarContenido(string nombreArchivo, ReadOnlySpan<byte> header)
        {
            var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();

            bool coincide = extension switch
            {
                ".png" => EmpiezaCon(header, FirmaPng),
                ".jpg" or ".jpeg" => EmpiezaCon(header, FirmaJpeg),
                ".webp" => EsWebp(header),
                _ => false
            };

            return coincide
                ? AvatarValidationResult.Ok()
                : AvatarValidationResult.Falla("El archivo no es una imagen válida o su extensión no coincide con el contenido.");
        }

        /// <summary>Extensión canónica a usar en el archivo destino, derivada del nombre original.</summary>
        public static string ExtensionCanonica(string nombreArchivo)
        {
            var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
            return extension == ".jpeg" ? ".jpg" : extension;
        }

        private static bool EmpiezaCon(ReadOnlySpan<byte> header, ReadOnlySpan<byte> firma) =>
            header.Length >= firma.Length && header[..firma.Length].SequenceEqual(firma);

        // WEBP = "RIFF" + 4 bytes de tamaño + "WEBP".
        private static bool EsWebp(ReadOnlySpan<byte> header) =>
            header.Length >= 12 &&
            EmpiezaCon(header, FirmaRiff) &&
            header[8..12].SequenceEqual(FirmaWebp);
    }
}
