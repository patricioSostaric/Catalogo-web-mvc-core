using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Marcas;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace catalogo_web_mvc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MarcasController : Controller
    {
        private readonly IMarcaService _service;
        private readonly IAuditService _audit;

        public MarcasController(IMarcaService service, IAuditService audit)
        {
            _service = service;
            _audit = audit;
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
            await _audit.RegistrarAsync("CREATE", User.Identity?.Name, User.FindFirstValue(ClaimTypes.NameIdentifier), $"Marca: {marca.Descripcion}");
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
                await _audit.RegistrarAsync("UPDATE", User.Identity?.Name, User.FindFirstValue(ClaimTypes.NameIdentifier), $"Marca ID {id}: {marca.Descripcion}");
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
            await _audit.RegistrarAsync("DELETE", User.Identity?.Name, User.FindFirstValue(ClaimTypes.NameIdentifier), $"Marca ID {id}");
            return RedirectToAction(nameof(Index));
        }
    }
}
