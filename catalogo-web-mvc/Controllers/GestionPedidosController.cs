using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Pedidos;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using X.PagedList.Extensions;

namespace catalogo_web_mvc.Controllers
{
    /// <summary>
    /// Panel de pedidos del administrador: despachar y marcar entregados. Es distinto de
    /// <see cref="PedidosController"/>, que muestra los pedidos propios del usuario.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class GestionPedidosController : Controller
    {
        private readonly IPedidoService _pedidoService;
        private readonly IAuditService _audit;

        public GestionPedidosController(IPedidoService pedidoService, IAuditService audit)
        {
            _pedidoService = pedidoService;
            _audit = audit;
        }

        public async Task<IActionResult> Index(EstadoPedido? estado, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 20;

            var pedidos = await _pedidoService.GetTodosAsync(estado);

            ViewBag.FiltroEstado = estado;

            return View(pedidos.ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Avanzar(int id, EstadoPedido? estado, int? page)
        {
            var resultado = await _pedidoService.AvanzarAsync(id);

            if (!resultado.Exito)
            {
                TempData["GestionError"] = resultado.Error;
                return RedirectToAction(nameof(Index), new { estado, page });
            }

            await _audit.RegistrarAsync(
                "PEDIDO_ESTADO_CAMBIADO",
                User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name,
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                $"Pedido #{id} → {resultado.EstadoNuevo}");

            TempData["GestionOk"] = $"El pedido #{id} pasó a {resultado.EstadoNuevo}.";

            return RedirectToAction(nameof(Index), new { estado, page });
        }
    }
}
