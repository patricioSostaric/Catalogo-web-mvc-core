using catalogo_web_mvc.Models.ViewModels;

namespace catalogo_web_mvc.Interfaces.Usuarios
{
    public interface IUsuarioAdminService
    {
        Task<List<UsuarioAdminViewModel>> ListarAsync(string? filtroEmail = null);

        /// <summary>Levanta el lockout y pone en cero el contador de intentos fallidos.</summary>
        Task<ResultadoDesbloqueo> DesbloquearAsync(string userId);
    }
}
