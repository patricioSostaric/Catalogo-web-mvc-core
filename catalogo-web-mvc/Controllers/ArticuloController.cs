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
using Microsoft.AspNetCore.Authorization;
using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Marcas;
using catalogo_web_mvc.Interfaces.Categorias;
using System.Security.Claims;

namespace catalogo_web_mvc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ArticuloController : Controller
    {
        private readonly IArticuloService _service;
        private readonly IAuditService _audit;
        private readonly IMarcaService _marcas;
        private readonly ICategoriaService _categorias;

        public ArticuloController(IArticuloService service, IAuditService audit,
            IMarcaService marcas, ICategoriaService categorias)
        {
            _service = service;
            _audit = audit;
            _marcas = marcas;
            _categorias = categorias;
        }

        // SelectList es un tipo de la capa de vistas: existe para llenar un <select>
        // de Razor. Armarlo aca y no en el servicio evita que la capa de negocio
        // sepa como se dibuja un formulario, y que la API arrastre esa dependencia
        // sin usarla nunca.
        private async Task CargarDesplegablesAsync(int? marcaId = null, int? categoriaId = null)
        {
            var marcas = await _marcas.GetAllAsync();
            var categorias = await _categorias.GetAllAsync();

            ViewBag.MarcaId = new SelectList(
                marcas.OrderBy(m => m.Descripcion), "MarcaId", "Descripcion", marcaId);
            ViewBag.CategoriaId = new SelectList(
                categorias.OrderBy(c => c.Descripcion), "CategoriaId", "Descripcion", categoriaId);
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
            await CargarDesplegablesAsync();
            return View();
        }

        // GET: Articulo/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var articulo = await _service.GetByIdAsync(id);
            if (articulo == null) return NotFound();
            await CargarDesplegablesAsync(articulo.MarcaId, articulo.CategoriaId);
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
                await _audit.RegistrarAsync("CREATE", User.Identity?.Name, User.FindFirstValue(ClaimTypes.NameIdentifier), $"Artículo: {articulo.Nombre} (Cód: {articulo.Codigo})");
                return RedirectToAction(nameof(Index));
            }
            await CargarDesplegablesAsync(articulo.MarcaId, articulo.CategoriaId);
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
                await _audit.RegistrarAsync("UPDATE", User.Identity?.Name, User.FindFirstValue(ClaimTypes.NameIdentifier), $"Artículo ID {id}: {articulo.Nombre}");
                return RedirectToAction(nameof(Index));
            }
            await CargarDesplegablesAsync(articulo.MarcaId, articulo.CategoriaId);
            return View(articulo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            await _audit.RegistrarAsync("DELETE", User.Identity?.Name, User.FindFirstValue(ClaimTypes.NameIdentifier), $"Artículo ID {id}");
            return RedirectToAction(nameof(Index));
        }
    }
}
   