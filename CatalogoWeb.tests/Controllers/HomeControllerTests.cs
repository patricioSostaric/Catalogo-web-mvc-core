using catalogo_web_mvc.Controllers;
using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using X.PagedList;
using X.PagedList.Extensions;

namespace CatalogoWeb.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<IArticuloService> _serviceMock;

        public HomeControllerTests()
        {
            _serviceMock = new Mock<IArticuloService>();
        }

        private static CatalogoContext CrearContexto() =>
            new(new DbContextOptionsBuilder<CatalogoContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static IPagedList<Articulo> ListaPaginada(IEnumerable<Articulo> articulos) =>
            articulos.ToList().ToPagedList(1, 100);

        private static ControllerContext ContextoAutenticado(string userId = "user-1")
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        private static ControllerContext ContextoAnonimo()
        {
            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };
        }

        private static List<Articulo> ArticulosDeEjemplo() =>
        [
            new() { Id = 1, Codigo = "S01", Nombre = "Galaxy S10", Descripcion = "desc", MarcaId = 1, CategoriaId = 1, Precio = 69999 },
            new() { Id = 2, Codigo = "M03", Nombre = "Moto G Play", Descripcion = "desc", MarcaId = 5, CategoriaId = 1, Precio = 15699 }
        ];

        // ── Index ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task Index_RetornaVista_ConListaDeArticulos()
        {
            using var context = CrearContexto();
            _serviceMock.Setup(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(ListaPaginada(ArticulosDeEjemplo()));
            var controller = new HomeController(_serviceMock.Object, context)
            {
                ControllerContext = ContextoAnonimo()
            };

            var resultado = await controller.Index(null);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.IsAssignableFrom<IEnumerable<Articulo>>(viewResult.Model);
        }

        [Fact]
        public async Task Index_SinResultados_AgregaMensajeEnViewBag()
        {
            using var context = CrearContexto();
            _serviceMock.Setup(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(ListaPaginada([]));
            var controller = new HomeController(_serviceMock.Object, context)
            {
                ControllerContext = ContextoAnonimo()
            };

            var resultado = await controller.Index("zzz");

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.NotNull(viewResult.ViewData["Mensaje"] ?? controller.ViewBag.Mensaje);
        }

        [Fact]
        public async Task Index_UsuarioNoLogueado_FavoritosEsHashSetVacio()
        {
            using var context = CrearContexto();
            _serviceMock.Setup(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(ListaPaginada(ArticulosDeEjemplo()));
            var controller = new HomeController(_serviceMock.Object, context)
            {
                ControllerContext = ContextoAnonimo()
            };

            await controller.Index(null);

            var favoritos = controller.ViewBag.Favoritos as HashSet<int>;
            Assert.NotNull(favoritos);
            Assert.Empty(favoritos);
        }

        [Fact]
        public async Task Index_UsuarioLogueado_FavoritosContieneSusArticulos()
        {
            using var context = CrearContexto();
            context.ArticuloFavoritos.Add(new ArticuloFavorito { UserId = "user-1", ArticuloId = 1 });
            await context.SaveChangesAsync();

            _serviceMock.Setup(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(ListaPaginada(ArticulosDeEjemplo()));
            var controller = new HomeController(_serviceMock.Object, context)
            {
                ControllerContext = ContextoAutenticado("user-1")
            };

            await controller.Index(null);

            var favoritos = controller.ViewBag.Favoritos as HashSet<int>;
            Assert.NotNull(favoritos);
            Assert.Contains(1, favoritos);
            Assert.DoesNotContain(2, favoritos);
        }

        // ── Detalle ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Detalle_ArticuloExistente_RetornaVista()
        {
            using var context = CrearContexto();
            _serviceMock.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(ArticulosDeEjemplo()[0]);
            var controller = new HomeController(_serviceMock.Object, context);

            var resultado = await controller.Detalle(1);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsType<Articulo>(viewResult.Model);
            Assert.Equal(1, modelo.Id);
        }

        [Fact]
        public async Task Detalle_ArticuloInexistente_RetornaNotFound()
        {
            using var context = CrearContexto();
            _serviceMock.Setup(s => s.GetByIdAsync(99))
                .ReturnsAsync((Articulo?)null);
            var controller = new HomeController(_serviceMock.Object, context);

            var resultado = await controller.Detalle(99);

            Assert.IsType<NotFoundResult>(resultado);
        }
    }
}
