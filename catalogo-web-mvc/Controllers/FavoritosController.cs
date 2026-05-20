using catalogo_web_mvc.Data;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using X.PagedList.Extensions;

namespace catalogo_web_mvc.Controllers
{
    [Authorize]
    public class FavoritosController : Controller
    {
        private readonly CatalogoContext _context;

        public FavoritosController(CatalogoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 6;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favoritos = await _context.ArticuloFavoritos
                .Where(f => f.UserId == userId)
                .Include(f => f.Articulo)
                .Select(f => new FavoritoViewModel
                {
                    ArticuloId = f.ArticuloId,
                    Nombre = f.Articulo.Nombre,
                    Descripcion = f.Articulo.Descripcion,
                    Precio = f.Articulo.Precio,
                    ImagenUrl = f.Articulo.ImagenUrl
                })
                .ToListAsync();

            return View(favoritos.ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool yaExiste = await _context.ArticuloFavoritos
                .AnyAsync(f => f.UserId == userId && f.ArticuloId == id);

            if (!yaExiste)
            {
                _context.ArticuloFavoritos.Add(new ArticuloFavorito
                {
                    UserId = userId!,
                    ArticuloId = id
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favorito = await _context.ArticuloFavoritos
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ArticuloId == id);

            if (favorito != null)
            {
                _context.ArticuloFavoritos.Remove(favorito);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
