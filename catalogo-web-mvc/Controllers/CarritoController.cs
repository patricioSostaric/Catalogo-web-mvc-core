using catalogo_web_mvc.Interfaces.Carrito;
using catalogo_web_mvc.Interfaces.Pedidos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace catalogo_web_mvc.Controllers
{
    [Authorize]
    public class CarritoController : Controller
    {
        private readonly ICarritoService _carritoService;
        private readonly IPedidoService _pedidoService;

        public CarritoController(ICarritoService carritoService, IPedidoService pedidoService)
        {
            _carritoService = carritoService;
            _pedidoService = pedidoService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
            => View(await _carritoService.GetCarritoAsync(UserId));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(int id, int cantidad = 1, string? volverA = null)
        {
            var resultado = await _carritoService.AgregarAsync(UserId, id, cantidad);

            if (!resultado.Exito)
                TempData["CarritoError"] = resultado.Error;
            else
                TempData["CarritoOk"] = "Artículo agregado al carrito.";

            // Se vuelve a donde estaba el usuario, pero solo si la ruta es local: un
            // returnUrl sin validar es un redirect abierto.
            if (!string.IsNullOrEmpty(volverA) && Url.IsLocalUrl(volverA))
                return Redirect(volverA);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarCantidad(int id, int cantidad)
        {
            var resultado = await _carritoService.CambiarCantidadAsync(UserId, id, cantidad);

            if (!resultado.Exito)
                TempData["CarritoError"] = resultado.Error;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Quitar(int id)
        {
            await _carritoService.QuitarAsync(UserId, id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vaciar()
        {
            await _carritoService.VaciarAsync(UserId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(string claveIdempotencia)
        {
            var resultado = await _pedidoService.ConfirmarAsync(UserId, claveIdempotencia);

            if (!resultado.Exito)
            {
                TempData["CarritoError"] = resultado.Error;
                return RedirectToAction(nameof(Index));
            }

            TempData["PedidoId"] = resultado.PedidoId;
            TempData["PedidoTotal"] = resultado.Total.ToString("N2");
            TempData["PedidoArticulos"] = resultado.CantidadArticulos;

            return RedirectToAction(nameof(Index));
        }
    }
}
