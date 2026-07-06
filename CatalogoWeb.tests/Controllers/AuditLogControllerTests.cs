using catalogo_web_mvc.Controllers;
using catalogo_web_mvc.Data;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace CatalogoWeb.Tests.Controllers
{
    public class AuditLogControllerTests
    {
        private static CatalogoContext CrearContexto() =>
            new(new DbContextOptionsBuilder<CatalogoContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static AuditLog Log(string accion, string? email = null, int diasAtras = 0) => new()
        {
            Accion = accion,
            Email = email,
            Fecha = DateTime.UtcNow.AddDays(-diasAtras)
        };

        // ── Index ──────────────────────────────────────────────────────────────

        [Fact]
        public void Index_RetornaVista_ConLogsPaginados()
        {
            using var context = CrearContexto();
            context.AuditLogs.AddRange(Log("LOGIN_OK"), Log("LOGOUT"));
            context.SaveChanges();

            var controller = new AuditLogController(context);

            var resultado = controller.Index(null, null, null);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<IPagedList<AuditLog>>(viewResult.Model);
            Assert.Equal(2, modelo.Count);
        }

        [Fact]
        public void Index_SinLogs_RetornaVistaVacia()
        {
            using var context = CrearContexto();
            var controller = new AuditLogController(context);

            var resultado = controller.Index(null, null, null);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<IPagedList<AuditLog>>(viewResult.Model);
            Assert.Empty(modelo);
        }

        [Fact]
        public void Index_OrdenaLogsPorFechaDescendente()
        {
            using var context = CrearContexto();
            context.AuditLogs.AddRange(
                Log("LOGIN_OK", diasAtras: 5),
                Log("LOGOUT", diasAtras: 0),
                Log("REGISTER", diasAtras: 2)
            );
            context.SaveChanges();

            var controller = new AuditLogController(context);

            var resultado = controller.Index(null, null, null);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<IPagedList<AuditLog>>(viewResult.Model);
            Assert.Equal("LOGOUT", modelo[0].Accion);
            Assert.Equal("REGISTER", modelo[1].Accion);
            Assert.Equal("LOGIN_OK", modelo[2].Accion);
        }

        [Fact]
        public void Index_FiltraPorAccion()
        {
            using var context = CrearContexto();
            context.AuditLogs.AddRange(Log("LOGIN_OK"), Log("LOGIN_FAIL"), Log("LOGIN_OK"));
            context.SaveChanges();

            var controller = new AuditLogController(context);

            var resultado = controller.Index("LOGIN_OK", null, null);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<IPagedList<AuditLog>>(viewResult.Model);
            Assert.Equal(2, modelo.Count);
            Assert.All(modelo, log => Assert.Equal("LOGIN_OK", log.Accion));
        }

        [Fact]
        public void Index_FiltraPorEmail_UsandoContains()
        {
            using var context = CrearContexto();
            context.AuditLogs.AddRange(
                Log("LOGIN_OK", email: "juan@test.com"),
                Log("LOGIN_OK", email: "maria@test.com")
            );
            context.SaveChanges();

            var controller = new AuditLogController(context);

            var resultado = controller.Index(null, "juan", null);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<IPagedList<AuditLog>>(viewResult.Model);
            Assert.Single(modelo);
            Assert.Equal("juan@test.com", modelo[0].Email);
        }

        [Fact]
        public void Index_PaginaLogs_SegunPageSizeYNumeroDePagina()
        {
            using var context = CrearContexto();
            for (int i = 0; i < 25; i++)
                context.AuditLogs.Add(Log("LOGIN_OK", diasAtras: i));
            context.SaveChanges();

            var controller = new AuditLogController(context);

            var resultado = controller.Index(null, null, 2);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<IPagedList<AuditLog>>(viewResult.Model);
            Assert.Equal(2, modelo.PageNumber);
            Assert.Equal(5, modelo.Count);
            Assert.Equal(25, modelo.TotalItemCount);
        }

        [Fact]
        public void Index_SinPage_UsaPrimeraPagina()
        {
            using var context = CrearContexto();
            for (int i = 0; i < 25; i++)
                context.AuditLogs.Add(Log("LOGIN_OK", diasAtras: i));
            context.SaveChanges();

            var controller = new AuditLogController(context);

            var resultado = controller.Index(null, null, null);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<IPagedList<AuditLog>>(viewResult.Model);
            Assert.Equal(1, modelo.PageNumber);
            Assert.Equal(20, modelo.Count);
        }

        [Fact]
        public void Index_ExponeFiltrosEnViewBag()
        {
            using var context = CrearContexto();
            var controller = new AuditLogController(context);

            var resultado = controller.Index("LOGIN_FAIL", "juan@test.com", null);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal("LOGIN_FAIL", viewResult.ViewData["FiltroAccion"]);
            Assert.Equal("juan@test.com", viewResult.ViewData["FiltroEmail"]);
        }
    }
}
