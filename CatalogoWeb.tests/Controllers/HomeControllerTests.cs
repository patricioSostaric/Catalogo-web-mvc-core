using catalogo_web_mvc.Controllers;
using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Interfaces.Favoritos;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Repository.Favoritos;
using catalogo_web_mvc.Services.Favoritos;
using catalogo_web_mvc.Models.ViewModels;
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

        private static IFavoritoService Favoritos(CatalogoContext context) =>
            new FavoritoService(new FavoritoRepository(context));

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
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(ListaPaginada(ArticulosDeEjemplo()));
            var controller = new HomeController(_serviceMock.Object, Favoritos(context))
            {
                ControllerContext = ContextoAnonimo()
            };

            var resultado = await controller.Index(null, null);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.IsAssignableFrom<IEnumerable<Articulo>>(viewResult.Model);
        }

        [Fact]
        public async Task Index_SinResultados_AgregaMensajeEnViewBag()
        {
            using var context = CrearContexto();
            _serviceMock.Setup(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(ListaPaginada([]));
            var controller = new HomeController(_serviceMock.Object, Favoritos(context))
            {
                ControllerContext = ContextoAnonimo()
            };

            var resultado = await controller.Index("zzz", null);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.NotNull(viewResult.ViewData["Mensaje"] ?? controller.ViewBag.Mensaje);
        }

        [Fact]
        public async Task Index_UsuarioNoLogueado_FavoritosEsHashSetVacio()
        {
            using var context = CrearContexto();
            _serviceMock.Setup(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(ListaPaginada(ArticulosDeEjemplo()));
            var controller = new HomeController(_serviceMock.Object, Favoritos(context))
            {
                ControllerContext = ContextoAnonimo()
            };

            await controller.Index(null, null);

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
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(ListaPaginada(ArticulosDeEjemplo()));
            var controller = new HomeController(_serviceMock.Object, Favoritos(context))
            {
                ControllerContext = ContextoAutenticado("user-1")
            };

            await controller.Index(null, null);

            var favoritos = controller.ViewBag.Favoritos as HashSet<int>;
            Assert.NotNull(favoritos);
            Assert.Contains(1, favoritos);
            Assert.DoesNotContain(2, favoritos);
        }

        // ── Detalle ────────────────────────────────────────────────────────────

        private static ArticuloDetalleViewModel DetalleDeEjemplo(int id = 1) => new()
        {
            Id = id,
            Codigo = "S01",
            Nombre = "Galaxy S10",
            Descripcion = "desc",
            Precio = 69999
        };

        [Fact]
        public async Task Detalle_ArticuloExistente_RetornaVista()
        {
            using var context = CrearContexto();
            _serviceMock.Setup(s => s.ObtenerDetallePublicoAsync(1))
                .ReturnsAsync(DetalleDeEjemplo(1));
            var controller = new HomeController(_serviceMock.Object, Favoritos(context))
            {
                ControllerContext = ContextoAnonimo()
            };

            var resultado = await controller.Detalle(1);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsType<ArticuloDetalleViewModel>(viewResult.Model);
            Assert.Equal(1, modelo.Id);
        }

        [Fact]
        public async Task Detalle_ArticuloInexistente_RetornaNotFound()
        {
            using var context = CrearContexto();
            _serviceMock.Setup(s => s.ObtenerDetallePublicoAsync(99))
                .ReturnsAsync((ArticuloDetalleViewModel?)null);
            var controller = new HomeController(_serviceMock.Object, Favoritos(context));

            var resultado = await controller.Detalle(99);

            Assert.IsType<NotFoundResult>(resultado);
        }

        [Fact]
        public async Task Detalle_UsuarioNoLogueado_EsFavoritoFalse()
        {
            using var context = CrearContexto();
            _serviceMock.Setup(s => s.ObtenerDetallePublicoAsync(1))
                .ReturnsAsync(DetalleDeEjemplo(1));
            var controller = new HomeController(_serviceMock.Object, Favoritos(context))
            {
                ControllerContext = ContextoAnonimo()
            };

            await controller.Detalle(1);

            Assert.False((bool)controller.ViewBag.EsFavorito);
        }

        [Fact]
        public async Task Detalle_UsuarioLogueadoConFavorito_EsFavoritoTrue()
        {
            using var context = CrearContexto();
            context.ArticuloFavoritos.Add(new ArticuloFavorito { UserId = "user-1", ArticuloId = 1 });
            await context.SaveChangesAsync();

            _serviceMock.Setup(s => s.ObtenerDetallePublicoAsync(1))
                .ReturnsAsync(DetalleDeEjemplo(1));
            var controller = new HomeController(_serviceMock.Object, Favoritos(context))
            {
                ControllerContext = ContextoAutenticado("user-1")
            };

            await controller.Detalle(1);

            Assert.True((bool)controller.ViewBag.EsFavorito);
        }

        [Fact]
        public async Task Detalle_UsuarioLogueadoSinFavorito_EsFavoritoFalse()
        {
            using var context = CrearContexto();
            _serviceMock.Setup(s => s.ObtenerDetallePublicoAsync(1))
                .ReturnsAsync(DetalleDeEjemplo(1));
            var controller = new HomeController(_serviceMock.Object, Favoritos(context))
            {
                ControllerContext = ContextoAutenticado("user-1")
            };

            await controller.Detalle(1);

            Assert.False((bool)controller.ViewBag.EsFavorito);
        }

        // ── soloActivos ────────────────────────────────────────────────────────

        [Fact]
        public async Task Index_LlamaBuscarAsync_ConSoloActivosTrue()
        {
            using var context = CrearContexto();
            _serviceMock.Setup(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(ListaPaginada(ArticulosDeEjemplo()));
            var controller = new HomeController(_serviceMock.Object, Favoritos(context))
            {
                ControllerContext = ContextoAnonimo()
            };

            await controller.Index(null, null);

            _serviceMock.Verify(s => s.BuscarAsync(
                It.IsAny<string>(), false,
                null, null, null,
                It.IsAny<int>(), It.IsAny<int>(),
                true), Times.Once);
        }
    }
}
