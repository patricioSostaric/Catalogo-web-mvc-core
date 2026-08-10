namespace catalogo_web_mvc.Models.ViewModels
{
    /// <summary>Fila del listado de usuarios del panel de superadministracion.</summary>
    public class UsuarioAdminViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];

        /// <summary>Momento hasta el que la cuenta esta bloqueada, si lo esta.</summary>
        public DateTimeOffset? BloqueadoHasta { get; set; }

        public int IntentosFallidos { get; set; }

        /// <summary>
        /// Identity deja la fecha de lockout cargada aunque ya haya vencido, asi que no
        /// alcanza con preguntar si tiene valor: hay que compararla contra el ahora.
        /// </summary>
        public bool EstaBloqueado => BloqueadoHasta.HasValue && BloqueadoHasta > DateTimeOffset.UtcNow;
    }

    public class ResultadoDesbloqueo
    {
        public bool Exito { get; init; }
        public string? Error { get; init; }
        public string? EmailAfectado { get; init; }

        public static ResultadoDesbloqueo Ok(string email) => new() { Exito = true, EmailAfectado = email };
        public static ResultadoDesbloqueo Falla(string error) => new() { Exito = false, Error = error };
    }
}
