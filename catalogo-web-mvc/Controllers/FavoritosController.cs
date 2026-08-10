using catalogo_web_mvc.Interfaces.Favoritos;
using catalogo_web_mvc.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using X.PagedList.Extensions;

namespace catalogo_web_mvc.Controllers
{
    [Authorize]
    public class FavoritosController : Controller
    {
        private readonly IFavoritoService _service;

        public FavoritosController(IFavoritoService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 6;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var favoritos = await _service.ListarAsync(userId);

            var modelo = favoritos.Select(f => new FavoritoViewModel
            {
                ArticuloId = f.ArticuloId,
                Nombre = f.Articulo.Nombre,
                Descripcion = f.Articulo.Descripcion,
                Precio = f.Articulo.Precio,
                ImagenUrl = f.Articulo.ImagenUrl
            }).ToList();

            return View(modelo.ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _service.AgregarAsync(userId, id);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _service.QuitarAsync(userId, id);

            return RedirectToAction("Index");
        }
    }
}
