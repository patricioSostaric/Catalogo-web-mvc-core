using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using X.PagedList;
using X.PagedList.Extensions;

namespace catalogo_web_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IArticuloService _service;
        private readonly CatalogoContext _context;

        public HomeController(IArticuloService service, CatalogoContext context)
        {
            _service = service;
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 6;

            var todos = await _service.BuscarAsync(searchString, false, null, null, null, 1, 1000);

            if (!todos.Any())
                ViewBag.Mensaje = "No se encontraron artículos con ese criterio.";

            // Favoritos del usuario autenticado
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var favIds = await _context.ArticuloFavoritos
                    .Where(f => f.UserId == userId)
                    .Select(f => f.ArticuloId)
                    .ToListAsync();
                ViewBag.Favoritos = new HashSet<int>(favIds);
            }
            else
            {
                ViewBag.Favoritos = new HashSet<int>();
            }

            var paged = todos.ToPagedList(pageNumber, pageSize);
            return View(paged);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var articulo = await _service.GetByIdAsync(id);
            if (articulo == null) return NotFound();
            return View(articulo);
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
