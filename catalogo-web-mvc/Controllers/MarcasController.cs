using catalogo_web_mvc.Interfaces.Marcas;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace catalogo_web_mvc.Controllers
{
    public class MarcasController : Controller
    {
        private readonly IMarcaService _service;

        public MarcasController(IMarcaService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
            => View(await _service.GetAllAsync());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MarcaId,Descripcion")] Marca marca)
        {
            if (!ModelState.IsValid) return View(marca);
            await _service.AddAsync(marca);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var marca = await _service.GetByIdAsync(id.Value);
            if (marca == null) return NotFound();
            return View(marca);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MarcaId,Descripcion")] Marca marca)
        {
            if (id != marca.MarcaId) return NotFound();
            if (!ModelState.IsValid) return View(marca);

            try
            {
                await _service.UpdateAsync(marca);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _service.ExistsAsync(marca.MarcaId)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var marca = await _service.GetByIdAsync(id.Value);
            if (marca == null) return NotFound();
            return View(marca);
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
