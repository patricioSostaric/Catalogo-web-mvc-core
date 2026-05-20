using catalogo_web_mvc.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace catalogo_web_mvc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly CatalogoContext _context;

        public AuditLogController(CatalogoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? accion, string? email, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 20;

            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(accion))
                query = query.Where(a => a.Accion == accion);

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(a => a.Email != null && a.Email.Contains(email));

            var total = await query.CountAsync();
            var logs = await query
                .OrderByDescending(a => a.Fecha)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalPaginas = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.PaginaActual = pageNumber;
            ViewBag.FiltroAccion = accion;
            ViewBag.FiltroEmail = email;

            return View(logs);
        }
    }
}
