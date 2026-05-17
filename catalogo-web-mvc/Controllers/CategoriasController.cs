using catalogo_web_mvc.Interfaces.Categorias;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace catalogo_web_mvc.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly ICategoriaService _service;

        public CategoriasController(ICategoriaService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
            => View(await _service.GetAllAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _service.GetByIdAsync(id.Value);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoriaId,Descripcion")] Categoria categoria)
        {
            if (!ModelState.IsValid) return View(categoria);
            await _service.AddAsync(categoria);
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
            return RedirectToAction(nameof(Index));
        }
    }
}
