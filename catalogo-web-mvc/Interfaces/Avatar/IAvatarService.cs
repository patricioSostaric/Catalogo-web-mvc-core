using catalogo_web_mvc.Services.Avatar;

namespace catalogo_web_mvc.Interfaces.Avatar
{
    /// <summary>
    /// Resultado de intentar guardar un avatar. <see cref="RutaRelativa"/> es lo que
    /// se persiste en <c>ApplicationUser.AvatarUrl</c> cuando la operación sale bien.
    /// </summary>
    public sealed record AvatarGuardadoResult(bool EsValido, string? RutaRelativa, string? Error)
    {
        public static AvatarGuardadoResult Ok(string rutaRelativa) => new(true, rutaRelativa, null);
        public static AvatarGuardadoResult Falla(string error) => new(false, null, error);
    }

    public interface IAvatarService
    {
        /// <summary>
        /// Valida y guarda la imagen subida. El nombre del archivo destino lo genera el
        /// servicio; el nombre que manda el cliente nunca se usa para escribir.
        /// </summary>
        Task<AvatarGuardadoResult> GuardarAsync(IFormFile? archivo, CancellationToken ct = default);

        /// <summary>
        /// Borra un avatar subido previamente. Ignora las URLs externas y los valores
        /// inválidos: solo toca archivos dentro de la carpeta de avatares.
        /// </summary>
        void Eliminar(string? avatarUrl);
    }
}
