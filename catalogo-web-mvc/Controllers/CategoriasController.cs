using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Categorias;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using X.PagedList.Extensions;

namespace catalogo_web_mvc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriasController : Controller
    {
        private readonly ICategoriaService _service;
        private readonly IAuditService _audit;

        public CategoriasController(ICategoriaService service, IAuditService audit)
        {
            _service = service;
            _audit = audit;
        }

        public async Task<IActionResult> Index(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;

            var categorias = await _service.GetAllAsync();
            return View(categorias.ToPagedList(pageNumber, pageSize));
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoriaId,Descripcion")] Categoria categoria)
        {
            if (!ModelState.IsValid) return View(categoria);
            await _service.AddAsync(categoria);
            await _audit.RegistrarAsync("CREATE", User.Identity?.Name, User.FindFirstValue(ClaimTypes.NameIdentifier), $"Categoría: {categoria.Descripcion}");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _service.GetByIdAsync(id.Value);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoriaId,Descripcion")] Categoria categoria)
        {
            if (id != categoria.CategoriaId) return NotFound();
            if (!ModelState.IsValid) return View(categoria);

            try
            {
                await _service.UpdateAsync(categoria);
                await _audit.RegistrarAsync("UPDATE", User.Identity?.Name, User.FindFirstValue(ClaimTypes.NameIdentifier), $"Categoría ID {id}: {categoria.Descripcion}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _service.ExistsAsync(categoria.CategoriaId)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _service.GetByIdAsync(id.Value);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            await _audit.RegistrarAsync("DELETE", User.Identity?.Name, User.FindFirstValue(ClaimTypes.NameIdentifier), $"Categoría ID {id}");
            return RedirectToAction(nameof(Index));
        }
    }
}
