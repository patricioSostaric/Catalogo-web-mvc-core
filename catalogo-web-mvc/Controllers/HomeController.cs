using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace catalogo_web_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IArticuloService _service;

        public HomeController(IArticuloService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            var articulos = await _service.BuscarAsync(searchString, false, null, null, null, 1, 100);

            if (!articulos.Any())
                ViewBag.Mensaje = "No se encontraron artículos con ese criterio.";

            return View(articulos);
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
