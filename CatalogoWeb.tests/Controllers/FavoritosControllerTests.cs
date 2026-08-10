using catalogo_web_mvc.Controllers;
using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Favoritos;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Repository.Favoritos;
using catalogo_web_mvc.Services.Favoritos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CatalogoWeb.Tests.Controllers
{
    public class FavoritosControllerTests
    {
        private static CatalogoContext CrearContexto() =>
            new(new DbContextOptionsBuilder<CatalogoContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        // Se usa el servicio real sobre la base en memoria, no un mock: lo que interesa
        // probar acá es el recorrido completo hasta los datos, igual que antes de que
        // la consulta se moviera del controlador al servicio.
        private static IFavoritoService Favoritos(CatalogoContext context) =>
            new FavoritoService(new FavoritoRepository(context));

        private static ControllerContext ContextoAutenticado(string userId = "user-1")
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        // ── Index ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task Index_RetornaVista_ConFavoritosDelUsuario()
        {
            using var context = CrearContexto();
            context.Marcas.Add(new Marca { MarcaId = 1, Descripcion = "Samsung" });
            context.Categorias.Add(new Categoria { CategoriaId = 1, Descripcion = "Celulares" });
            context.Articulos.Add(new Articulo { Id = 1, Codigo = "S01", Nombre = "Galaxy S10", Descripcion = "desc", MarcaId = 1, CategoriaId = 1, Precio = 69999 });
            context.ArticuloFavoritos.Add(new ArticuloFavorito { UserId = "user-1", ArticuloId = 1 });
            await context.SaveChangesAsync();

            var controller = new FavoritosController(Favoritos(context))
            {
                ControllerContext = ContextoAutenticado("user-1")
            };

            var resultado = await controller.Index(null);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<IEnumerable<catalogo_web_mvc.Models.ViewModels.FavoritoViewModel>>(viewResult.Model);
            Assert.Single(modelo);
            Assert.Equal("Galaxy S10", modelo.First().Nombre);
        }

        [Fact]
        public async Task Index_UsuarioSinFavoritos_RetornaListaVacia()
        {
            using var context = CrearContexto();
            var controller = new FavoritosController(Favoritos(context))
            {
                ControllerContext = ContextoAutenticado("user-sin-favoritos")
            };

            var resultado = await controller.Index(null);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<IEnumerable<catalogo_web_mvc.Models.ViewModels.FavoritoViewModel>>(viewResult.Model);
            Assert.Empty(modelo);
        }

        [Fact]
        public async Task Index_SoloMuestraFavoritosDelUsuarioActual()
        {
            using var context = CrearContexto();
            context.Marcas.Add(new Marca { MarcaId = 1, Descripcion = "Samsung" });
            context.Categorias.Add(new Categoria { CategoriaId = 1, Descripcion = "Celulares" });
            context.Articulos.AddRange(
                new Articulo { Id = 1, Codigo = "S01", Nombre = "Galaxy S10", Descripcion = "d", MarcaId = 1, CategoriaId = 1, Precio = 1 },
                new Articulo { Id = 2, Codigo = "S02", Nombre = "Galaxy S20", Descripcion = "d", MarcaId = 1, CategoriaId = 1, Precio = 2 }
            );
            context.ArticuloFavoritos.AddRange(
                new ArticuloFavorito { UserId = "user-1", ArticuloId = 1 },
                new ArticuloFavorito { UserId = "user-2", ArticuloId = 2 }
            );
            await context.SaveChangesAsync();

            var controller = new FavoritosController(Favoritos(context))
            {
                ControllerContext = ContextoAutenticado("user-1")
            };

            var resultado = await controller.Index(null);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<IEnumerable<catalogo_web_mvc.Models.ViewModels.FavoritoViewModel>>(viewResult.Model);
            Assert.Single(modelo);
            Assert.Equal(1, modelo.First().ArticuloId);
        }

        // ── Add ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Add_ArticuloNuevo_AgregaFavorito_YRedirige()
        {
            using var context = CrearContexto();
            var controller = new FavoritosController(Favoritos(context))
            {
                ControllerContext = ContextoAutenticado("user-1")
            };

            var resultado = await controller.Add(5);

            Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Single(context.ArticuloFavoritos);
            Assert.Equal("user-1", context.ArticuloFavoritos.First().UserId);
            Assert.Equal(5, context.ArticuloFavoritos.First().ArticuloId);
        }

        [Fact]
        public async Task Add_ArticuloYaFavorito_NoAgregaDuplicado()
        {
            using var context = CrearContexto();
            context.ArticuloFavoritos.Add(new ArticuloFavorito { UserId = "user-1", ArticuloId = 5 });
            await context.SaveChangesAsync();

            var controller = new FavoritosController(Favoritos(context))
            {
                ControllerContext = ContextoAutenticado("user-1")
            };

            await controller.Add(5);

            Assert.Single(context.ArticuloFavoritos);
        }

        [Fact]
        public async Task Add_RedirigePaginaIndex()
        {
            using var context = CrearContexto();
            var controller = new FavoritosController(Favoritos(context))
            {
                ControllerContext = ContextoAutenticado("user-1")
            };

            var resultado = await controller.Add(1);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Index", redirect.ActionName);
        }

        // ── Remove ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Remove_FavoritoExistente_EliminaYRedirige()
        {
            using var context = CrearContexto();
            context.ArticuloFavoritos.Add(new ArticuloFavorito { UserId = "user-1", ArticuloId = 3 });
            await context.SaveChangesAsync();

            var controller = new FavoritosController(Favoritos(context))
            {
                ControllerContext = ContextoAutenticado("user-1")
            };

            var resultado = await controller.Remove(3);

            Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Empty(context.ArticuloFavoritos);
        }

        [Fact]
        public async Task Remove_FavoritoInexistente_NoCrashYRedirige()
        {
            using var context = CrearContexto();
            var controller = new FavoritosController(Favoritos(context))
            {
                ControllerContext = ContextoAutenticado("user-1")
            };

            var resultado = await controller.Remove(99);

            Assert.IsType<RedirectToActionResult>(resultado);
        }

        [Fact]
        public async Task Remove_SoloEliminaFavoritoDelUsuarioActual()
        {
            using var context = CrearContexto();
            context.ArticuloFavoritos.AddRange(
                new ArticuloFavorito { UserId = "user-1", ArticuloId = 3 },
                new ArticuloFavorito { UserId = "user-2", ArticuloId = 3 }
            );
            await context.SaveChangesAsync();

            var controller = new FavoritosController(Favoritos(context))
            {
                ControllerContext = ContextoAutenticado("user-1")
            };

            await controller.Remove(3);

            Assert.Single(context.ArticuloFavoritos);
            Assert.Equal("user-2", context.ArticuloFavoritos.First().UserId);
        }
    }
}
