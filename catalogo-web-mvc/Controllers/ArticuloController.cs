using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

namespace catalogo_web_mvc.Controllers
{
    public class ArticuloController : Controller
    {
        private readonly IArticuloService _service;

        public ArticuloController(IArticuloService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(string searchString, bool filtroAvanzado,
            string campo, string criterio, string filtro, int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var lista = await _service.BuscarAsync(searchString, filtroAvanzado, campo, criterio, filtro, pageNumber, pageSize);

            if (!lista.Any())
                ModelState.AddModelError("", "⚠ No se encontraron artículos con ese criterio.");

            return View(lista);
        }

        public async Task<IActionResult> Details(int id)
        {
            var articulo = await _service.GetByIdAsync(id);
            if (articulo == null) return NotFound();
            return View(articulo);
        }

        // GET: Articulo/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.MarcaId = await _service.GetMarcasSelectList();
            ViewBag.CategoriaId = await _service.GetCategoriasSelectList();
            return View();
        }

        // GET: Articulo/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var articulo = await _service.GetByIdAsync(id);
            if (articulo == null) return NotFound();
            ViewBag.MarcaId = await _service.GetMarcasSelectList(articulo.MarcaId);
            ViewBag.CategoriaId = await _service.GetCategoriasSelectList(articulo.CategoriaId);
            return View(articulo);
        }

        // GET: Articulo/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var articulo = await _service.GetByIdAsync(id);
            if (articulo == null) return NotFound();
            return View(articulo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Articulo articulo)
        {
            if (ModelState.IsValid)
            {
                await _service.AddAsync(articulo);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MarcaId = await _service.GetMarcasSelectList(articulo.MarcaId);
            ViewBag.CategoriaId = await _service.GetCategoriasSelectList(articulo.CategoriaId);
            return View(articulo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Articulo articulo)
        {
            if (id != articulo.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _service.UpdateAsync(articulo);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MarcaId = await _service.GetMarcasSelectList(articulo.MarcaId);
            ViewBag.CategoriaId = await _service.GetCategoriasSelectList(articulo.CategoriaId);
            return View(articulo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
   