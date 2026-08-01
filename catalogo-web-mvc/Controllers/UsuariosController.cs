using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using X.PagedList.Extensions;

namespace catalogo_web_mvc.Controllers
{
    // Administrar cuentas ajenas es una atribucion del superadministrador, igual que la
    // auditoria: el rol Admin esta pensado para poder compartirse.
    [Authorize(Roles = "SuperAdmin")]
    public class UsuariosController : Controller
    {
        private readonly IUsuarioAdminService _usuarioAdminService;
        private readonly IAuditService _audit;

        public UsuariosController(IUsuarioAdminService usuarioAdminService, IAuditService audit)
        {
            _usuarioAdminService = usuarioAdminService;
            _audit = audit;
        }

        public async Task<IActionResult> Index(string? email, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 20;

            var usuarios = await _usuarioAdminService.ListarAsync(email);

            ViewBag.FiltroEmail = email;

            return View(usuarios.ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desbloquear(string id, string? email, int? page)
        {
            var resultado = await _usuarioAdminService.DesbloquearAsync(id);

            if (!resultado.Exito)
            {
                TempData["UsuariosError"] = resultado.Error;
                return RedirectToAction(nameof(Index), new { email, page });
            }

            // Levantar el bloqueo de una cuenta ajena es una accion administrativa: tiene
            // que quedar registrada con quien la hizo y sobre quien.
            await _audit.RegistrarAsync(
                "USUARIO_DESBLOQUEADO",
                User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name,
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                resultado.EmailAfectado);

            TempData["UsuariosOk"] = $"Se desbloqueó la cuenta de {resultado.EmailAfectado}.";

            return RedirectToAction(nameof(Index), new { email, page });
        }
    }
}
