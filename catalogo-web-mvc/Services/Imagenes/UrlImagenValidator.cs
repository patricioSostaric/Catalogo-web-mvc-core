namespace catalogo_web_mvc.Services.Imagenes
{
    /// <summary>
    /// Núcleo compartido de validación de URLs de imagen. Lo usan tanto el avatar del
    /// usuario como la imagen de los artículos, para que la regla de seguridad viva en
    /// un solo lugar y no se desincronice entre los dos.
    ///
    /// Se aceptan solo dos formas:
    ///  1. Una ruta relativa propia bajo un prefijo explícitamente permitido.
    ///  2. Una URL absoluta https con host.
    ///
    /// Lo que esto frena (OWASP A03 Injection):
    ///  - <c>javascript:</c> y <c>vbscript:</c>: si el valor se interpola en un href o en
    ///    un atributo distinto de src, ejecuta script.
    ///  - <c>data:</c>: embebe contenido arbitrario, incluido SVG con script adentro.
    ///  - <c>file:</c> y otros esquemas locales.
    ///  - <c>http://</c> plano: mixed content sobre una página https y expuesto a MITM.
    ///  - URLs protocol-relative (<c>//host</c>), que heredan el esquema del documento.
    ///  - Path traversal y subcarpetas en la parte relativa.
    /// </summary>
    public static class UrlImagenValidator
    {
        /// <summary>
        /// Intenta normalizar el valor. Un valor nulo o vacío se considera "sin imagen":
        /// es válido y normaliza a null.
        /// </summary>
        /// <param name="prefijoLocalPermitido">
        /// Prefijo de ruta relativa aceptado (por ejemplo "/uploads/avatars/").
        /// Si es null, solo se aceptan URLs https absolutas.
        /// </param>
        public static bool TryNormalizar(string? valor, string? prefijoLocalPermitido, out string? normalizado)
        {
            normalizado = null;

            if (string.IsNullOrWhiteSpace(valor)) return true;

            var candidato = valor.Trim();

            // Los caracteres de control se usan para partir el parseo del esquema
            // (por ejemplo "java\nscript:") y no tienen razón de estar en una URL.
            if (candidato.Any(char.IsControl)) return false;

            // Ruta local propia.
            if (prefijoLocalPermitido is not null &&
                candidato.StartsWith(prefijoLocalPermitido, StringComparison.Ordinal))
            {
                if (candidato.Contains("..", StringComparison.Ordinal)) return false;

                var archivo = candidato[prefijoLocalPermitido.Length..];
                if (archivo.Length == 0 || archivo.Contains('/') || archivo.Contains('\\')) return false;

                normalizado = candidato;
                return true;
            }

            // Se descarta protocol-relative antes de parsear: "//host/x" es absoluta para
            // el navegador pero Uri.TryCreate con Absolute la rechaza, así que sin este
            // corte explícito podría colarse por otra rama.
            if (candidato.StartsWith("//", StringComparison.Ordinal)) return false;

            // URL absoluta https.
            if (!Uri.TryCreate(candidato, UriKind.Absolute, out var uri)) return false;
            if (uri.Scheme != Uri.UriSchemeHttps) return false;
            if (string.IsNullOrEmpty(uri.Host)) return false;

            normalizado = uri.ToString();
            return true;
        }
    }
}
