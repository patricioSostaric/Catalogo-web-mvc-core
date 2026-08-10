using catalogo_web_mvc.Services.Imagenes;

namespace catalogo_web_mvc.Services.Avatar
{
    /// <summary>
    /// Valida el valor que termina guardado en <c>ApplicationUser.AvatarUrl</c>.
    ///
    /// Las reglas de seguridad viven en <see cref="UrlImagenValidator"/>, compartidas con
    /// la imagen de los artículos. Acá queda solo lo propio del avatar: cuál es la carpeta
    /// local aceptada y cuál es la imagen por defecto.
    /// </summary>
    public static class AvatarUrlValidator
    {
        public const string PrefijoLocal = "/uploads/avatars/";

        /// <summary>Avatar que se muestra cuando el usuario no cargó ninguno.</summary>
        public const string RutaPorDefecto = "/img/avatar-default.svg";

        /// <summary>
        /// Intenta normalizar el valor recibido. Devuelve false si no es una forma aceptada.
        /// Un valor nulo o vacío se considera "sin avatar": es válido y normaliza a null.
        /// </summary>
        public static bool TryNormalizar(string? valor, out string? normalizado) =>
            UrlImagenValidator.TryNormalizar(valor, PrefijoLocal, out normalizado);

        /// <summary>
        /// Devuelve el avatar a renderizar: el del usuario si es válido, o la imagen
        /// por defecto. Nunca devuelve null, así la vista no necesita ramificar.
        /// </summary>
        public static string ResolverParaMostrar(string? avatarUrl, string rutaPorDefecto = RutaPorDefecto) =>
            TryNormalizar(avatarUrl, out var normalizado) && normalizado is not null
                ? normalizado
                : rutaPorDefecto;
    }
}
