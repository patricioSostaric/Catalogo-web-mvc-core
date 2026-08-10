using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Interfaces.Favoritos;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace catalogo_web_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IArticuloService _service;
        private readonly IFavoritoService _favoritos;

        public HomeController(IArticuloService service, IFavoritoService favoritos)
        {
            _service = service;
            _favoritos = favoritos;
        }

        public async Task<IActionResult> Index(string? searchString, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 6;
            var articulos = await _service.BuscarAsync(searchString, false, null, null, null, pageNumber, pageSize, soloActivos: true);

            if (!articulos.Any())
                ViewBag.Mensaje = "No se encontraron artículos con ese criterio.";

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                ViewBag.Favoritos = await _favoritos.IdsDeUsuarioAsync(userId);
            }
            else
            {
                ViewBag.Favoritos = new HashSet<int>();
            }

            return View(articulos);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var viewModel = await _service.ObtenerDetallePublicoAsync(id);
            if (viewModel == null) return NotFound();

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                ViewBag.EsFavorito = await _favoritos.EsFavoritoAsync(userId, id);
            }
            else
            {
                ViewBag.EsFavorito = false;
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
