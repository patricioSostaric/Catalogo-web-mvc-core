using catalogo_web_mvc.Interfaces.Avatar;

namespace catalogo_web_mvc.Services.Avatar
{
    /// <summary>
    /// Guarda y borra avatares en <c>wwwroot/uploads/avatars</c>.
    ///
    /// Toda la decisión de "esto es una imagen aceptable" vive en
    /// <see cref="AvatarValidator"/>; acá queda solo el acceso a disco.
    /// </summary>
    public class AvatarService : IAvatarService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AvatarService> _logger;

        private const string CarpetaRelativa = "uploads/avatars";

        public AvatarService(IWebHostEnvironment env, ILogger<AvatarService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<AvatarGuardadoResult> GuardarAsync(IFormFile? archivo, CancellationToken ct = default)
        {
            if (archivo is null || archivo.Length == 0)
                return AvatarGuardadoResult.Falla("No se recibió ningún archivo.");

            var previa = AvatarValidator.ValidarNombreYTamano(archivo.FileName, archivo.Length);
            if (!previa.EsValido)
                return AvatarGuardadoResult.Falla(previa.Error!);

            // Se lee solo la cabecera para comparar la firma sin cargar el archivo entero.
            var header = new byte[AvatarValidator.HeaderBytes];
            int leidos;
            await using (var stream = archivo.OpenReadStream())
            {
                leidos = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);
            }

            var contenido = AvatarValidator.ValidarContenido(archivo.FileName, header.AsSpan(0, leidos));
            if (!contenido.EsValido)
                return AvatarGuardadoResult.Falla(contenido.Error!);

            var carpetaDestino = Path.Combine(_env.WebRootPath, CarpetaRelativa);
            Directory.CreateDirectory(carpetaDestino);

            // El nombre lo generamos nosotros: un GUID más la extensión ya validada.
            // Esto elimina de raíz el path traversal y las colisiones entre usuarios,
            // y evita nombres tipo "shell.php.png" heredados del cliente.
            var nombreDestino = $"{Guid.NewGuid():N}{AvatarValidator.ExtensionCanonica(archivo.FileName)}";
            var rutaFisica = Path.Combine(carpetaDestino, nombreDestino);

            try
            {
                await using var destino = new FileStream(rutaFisica, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await using var origen = archivo.OpenReadStream();
                await origen.CopyToAsync(destino, ct);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "No se pudo guardar el avatar en {Ruta}", rutaFisica);
                return AvatarGuardadoResult.Falla("No se pudo guardar la imagen. Intentá de nuevo.");
            }

            return AvatarGuardadoResult.Ok($"/{CarpetaRelativa}/{nombreDestino}");
        }

        public void Eliminar(string? avatarUrl)
        {
            // Solo se borran rutas locales que pasen el validador. Una URL externa o un
            // valor manipulado no llega al filesystem.
            if (!AvatarUrlValidator.TryNormalizar(avatarUrl, out var normalizado)) return;
            if (normalizado is null) return;
            if (!normalizado.StartsWith(AvatarUrlValidator.PrefijoLocal, StringComparison.Ordinal)) return;

            var nombreArchivo = Path.GetFileName(normalizado);
            if (string.IsNullOrEmpty(nombreArchivo)) return;

            var rutaFisica = Path.Combine(_env.WebRootPath, CarpetaRelativa, nombreArchivo);

            // Defensa en profundidad: confirmamos que la ruta resuelta sigue estando
            // dentro de la carpeta de avatares antes de borrar.
            var carpetaEsperada = Path.GetFullPath(Path.Combine(_env.WebRootPath, CarpetaRelativa));
            if (!Path.GetFullPath(rutaFisica).StartsWith(carpetaEsperada, StringComparison.Ordinal)) return;

            try
            {
                if (File.Exists(rutaFisica)) File.Delete(rutaFisica);
            }
            catch (IOException ex)
            {
                // Un avatar viejo que no se pudo borrar no debe romper la actualización del perfil.
                _logger.LogWarning(ex, "No se pudo eliminar el avatar {Ruta}", rutaFisica);
            }
        }
    }
}
