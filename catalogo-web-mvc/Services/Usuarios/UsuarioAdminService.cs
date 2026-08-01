using catalogo_web_mvc.Interfaces.Usuarios;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace catalogo_web_mvc.Services.Usuarios
{
    public class UsuarioAdminService : IUsuarioAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsuarioAdminService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<UsuarioAdminViewModel>> ListarAsync(string? filtroEmail = null)
        {
            var usuarios = _userManager.Users.ToList();

            if (!string.IsNullOrWhiteSpace(filtroEmail))
            {
                usuarios = usuarios
                    .Where(u => u.Email != null &&
                                u.Email.Contains(filtroEmail, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var filas = new List<UsuarioAdminViewModel>();

            foreach (var usuario in usuarios)
            {
                filas.Add(new UsuarioAdminViewModel
                {
                    Id = usuario.Id,
                    Email = usuario.Email ?? string.Empty,
                    NombreCompleto = usuario.NombreParaMostrar,
                    Roles = [.. await _userManager.GetRolesAsync(usuario)],
                    BloqueadoHasta = usuario.LockoutEnd,
                    IntentosFallidos = usuario.AccessFailedCount
                });
            }

            // Los bloqueados primero: son los que motivan entrar a esta pantalla.
            return [.. filas
                .OrderByDescending(f => f.EstaBloqueado)
                .ThenBy(f => f.Email)];
        }

        public async Task<ResultadoDesbloqueo> DesbloquearAsync(string userId)
        {
            var usuario = await _userManager.FindByIdAsync(userId);

            if (usuario == null)
                return ResultadoDesbloqueo.Falla("El usuario no existe.");

            // Las dos cosas hacen falta: quitar la fecha de lockout levanta el bloqueo
            // actual, y poner el contador en cero evita que el proximo intento fallido
            // vuelva a bloquear la cuenta de inmediato.
            var resultado = await _userManager.SetLockoutEndDateAsync(usuario, null);

            if (!resultado.Succeeded)
                return ResultadoDesbloqueo.Falla("No se pudo levantar el bloqueo.");

            await _userManager.ResetAccessFailedCountAsync(usuario);

            return ResultadoDesbloqueo.Ok(usuario.Email ?? usuario.Id);
        }
    }
}
