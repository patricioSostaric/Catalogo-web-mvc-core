using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using catalogo_web_mvc.Data;
using catalogo_web_mvc.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace catalogo_web_mvc.Controllers
{
    public class ArticuloController : Controller
    {
        private readonly CatalogoContext _context;

        public ArticuloController(CatalogoContext context)
        {
            _context = context;
        }

        // GET: Articulo
        public async Task<IActionResult> Index(string searchString, bool filtroAvanzado, string campo, string criterio, string filtro, int? page)
        {
            var query = _context.Articulos
                .Include(a => a.Categoria)
                .Include(a => a.Marca)
                .AsQueryable();

            // Filtro simple por nombre
            if (!string.IsNullOrEmpty(searchString) && !filtroAvanzado)
            {
                query = query.Where(a => a.Nombre.Contains(searchString));
            }

            // Filtro avanzado
            if (filtroAvanzado && !string.IsNullOrEmpty(campo) && !string.IsNullOrEmpty(criterio) && !string.IsNullOrEmpty(filtro))
            {
                switch (campo)
                {
                    case "Codigo":
                        if (criterio == "Contiene") query = query.Where(a => a.Codigo.Contains(filtro));
                        if (criterio == "Comienza con") query = query.Where(a => a.Codigo.StartsWith(filtro));
                        if (criterio == "Termina con") query = query.Where(a => a.Codigo.EndsWith(filtro));
                        break;

                    case "Nombre":
                        if (criterio == "Contiene") query = query.Where(a => a.Nombre.Contains(filtro));
                        if (criterio == "Comienza con") query = query.Where(a => a.Nombre.StartsWith(filtro));
                        if (criterio == "Termina con") query = query.Where(a => a.Nombre.EndsWith(filtro));
                        break;

                    case "Precio":
                        if (decimal.TryParse(filtro, out var precio))
                        {
                            if (criterio == "Igual a") query = query.Where(a => a.Precio == precio);
                            if (criterio == "Mayor a") query = query.Where(a => a.Precio > precio);
                            if (criterio == "Menor a") query = query.Where(a => a.Precio < precio);
                        }
                        else
                        {
                            ViewBag.Mensaje = "⚠ Ingresá un número válido para el campo Precio.";
                        }
                        break;

                    case "Marca":
                        query = query.Where(a => a.Marca.Descripcion.Contains(filtro));
                        break;

                    case "Categoria":
                        query = query.Where(a => a.Categoria.Descripcion.Contains(filtro));
                        break;
                }
            }

            // Paginación: 5 artículos por página
            int pageSize = 5;
            int pageNumber = page ?? 1;

            // Use synchronous ToPagedList (avoid missing async extension)
            var lista = query.ToPagedList(pageNumber, pageSize);

            if (!lista.Any())
            {
                ViewBag.Mensaje = "⚠ No se encontraron artículos con ese criterio.";
            }

            return View(lista);
        }

        // GET: Articulo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var articulo = await _context.Articulos
                .Include(a => a.Categoria)
                .Include(a => a.Marca)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (articulo == null) return NotFound();

            return View(articulo);
        }

        // GET: Articulo/Create
        public IActionResult Create()
        {
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "Descripcion");
            ViewData["MarcaId"] = new SelectList(_context.Marcas, "MarcaId", "Descripcion");
            return View();
        }

        // POST: Articulo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Codigo,Nombre,Descripcion,MarcaId,CategoriaId,ImagenUrl,Precio")] Articulo articulo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(articulo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "Descripcion", articulo.CategoriaId);
            ViewData["MarcaId"] = new SelectList(_context.Marcas, "MarcaId", "Descripcion", articulo.MarcaId);
            return View(articulo);
        }

        // GET: Articulo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var articulo = await _context.Articulos.FindAsync(id);
            if (articulo == null) return NotFound();

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "Descripcion", articulo.CategoriaId);
            ViewData["MarcaId"] = new SelectList(_context.Marcas, "MarcaId", "Descripcion", articulo.MarcaId);
            return View(articulo);
        }

        // POST: Articulo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nombre,Descripcion,MarcaId,CategoriaId,ImagenUrl,Precio")] Articulo articulo)
        {
            if (id != articulo.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(articulo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArticuloExists(articulo.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "Descripcion", articulo.CategoriaId);
            ViewData["MarcaId"] = new SelectList(_context.Marcas, "MarcaId", "Descripcion", articulo.MarcaId);
            return View(articulo);
        }

        // GET: Articulo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var articulo = await _context.Articulos
                .Include(a => a.Categoria)
                .Include(a => a.Marca)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (articulo == null) return NotFound();

            return View(articulo);
        }

        // POST: Articulo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var articulo = await _context.Articulos.FindAsync(id);
            if (articulo != null)
            {
                _context.Articulos.Remove(articulo);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ArticuloExists(int id)
        {
            return _context.Articulos.Any(e => e.Id == id);
        }
    }
}

