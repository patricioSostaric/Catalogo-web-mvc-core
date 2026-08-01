using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Pedidos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace catalogo_web_mvc.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly IPedidoService _pedidoService;
        private readonly IAuditService _audit;

        public PedidosController(IPedidoService pedidoService, IAuditService audit)
        {
            _pedidoService = pedidoService;
            _audit = audit;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
            => View(await _pedidoService.GetByUsuarioAsync(UserId));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var resultado = await _pedidoService.CancelarAsync(UserId, id);

            if (!resultado.Exito)
            {
                TempData["PedidosError"] = resultado.Error;
                return RedirectToAction(nameof(Index));
            }

            await _audit.RegistrarAsync(
                "PEDIDO_CANCELADO",
                User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name,
                UserId,
                $"Pedido #{id}");

            TempData["PedidosOk"] = $"Se canceló el pedido #{id} y se devolvió el stock.";

            return RedirectToAction(nameof(Index));
        }
    }
}
