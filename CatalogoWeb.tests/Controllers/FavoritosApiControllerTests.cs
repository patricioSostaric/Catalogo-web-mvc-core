using catalogo_web_mvc.Controllers.Api;
using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Favoritos;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.Dtos;
using catalogo_web_mvc.Repository.Favoritos;
using catalogo_web_mvc.Services.Favoritos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CatalogoWeb.Tests.Controllers
{
    public class FavoritosApiControllerTests
    {
        private static CatalogoContext CrearContexto() =>
            new(new DbContextOptionsBuilder<CatalogoContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static IFavoritoService Favoritos(CatalogoContext context) =>
            new FavoritoService(new FavoritoRepository(context));

        // La API no recibe el usuario por parametro: lo saca del principal que dejo la
        // autenticacion por cookie. En el test se arma ese principal a mano.
        private static ControllerContext ContextoAutenticado(string userId) =>
            new()
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "TestAuth"))
                }
            };

        private static FavoritosApiController CrearController(CatalogoContext context, string userId = "user-1") =>
            new(Favoritos(context)) { ControllerContext = ContextoAutenticado(userId) };

        private static async Task SembrarAsync(CatalogoContext context)
        {
            context.Marcas.Add(new Marca { MarcaId = 1, Descripcion = "Samsung" });
            context.Categorias.Add(new Categoria { CategoriaId = 1, Descripcion = "Celulares" });
            context.Articulos.AddRange(
                new Articulo { Id = 1, Codigo = "S01", Nombre = "Galaxy S10", Descripcion = "desc", MarcaId = 1, CategoriaId = 1, Precio = 699999, ImagenUrl = "/imagen/articulos/s01.jpg" },
                new Articulo { Id = 2, Codigo = "S02", Nombre = "Galaxy S20", Descripcion = "otra", MarcaId = 1, CategoriaId = 1, Precio = 899999, ImagenUrl = null }
            );
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Get_DevuelveLosFavoritosDelUsuario()
        {
            using var context = CrearContexto();
            await SembrarAsync(context);
            context.ArticuloFavoritos.Add(new ArticuloFavorito { UserId = "user-1", ArticuloId = 1 });
            await context.SaveChangesAsync();

            var resultado = await CrearController(context).Get();

            var favoritos = Assert.IsType<List<FavoritoDto>>(resultado.Value);
            var favorito = Assert.Single(favoritos);
            Assert.Equal(1, favorito.ArticuloId);
            Assert.Equal("Galaxy S10", favorito.Nombre);
            Assert.Equal(699999, favorito.Precio);
            Assert.Equal("/imagen/articulos/s01.jpg", favorito.ImagenUrl);
        }

        [Fact]
        public async Task Get_SinFavoritos_DevuelveListaVacia()
        {
            using var context = CrearContexto();
            await SembrarAsync(context);

            var resultado = await CrearController(context).Get();

            Assert.Empty(Assert.IsType<List<FavoritoDto>>(resultado.Value));
        }

        // Esta es la razon de ser del endpoint autenticado: el usuario sale de la cookie
        // y no de la peticion, asi que no hay forma de pedir los favoritos de otro.
        [Fact]
        public async Task Get_NoDevuelveFavoritosDeOtroUsuario()
        {
            using var context = CrearContexto();
            await SembrarAsync(context);
            context.ArticuloFavoritos.AddRange(
                new ArticuloFavorito { UserId = "user-1", ArticuloId = 1 },
                new ArticuloFavorito { UserId = "user-2", ArticuloId = 2 }
            );
            await context.SaveChangesAsync();

            var resultado = await CrearController(context, "user-2").Get();

            var favorito = Assert.Single(Assert.IsType<List<FavoritoDto>>(resultado.Value));
            Assert.Equal(2, favorito.ArticuloId);
        }

        // ── Delete ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_QuitaElFavorito_YDevuelve204()
        {
            using var context = CrearContexto();
            await SembrarAsync(context);
            context.ArticuloFavoritos.Add(new ArticuloFavorito { UserId = "user-1", ArticuloId = 1 });
            await context.SaveChangesAsync();

            var resultado = await CrearController(context).Delete(1);

            Assert.IsType<NoContentResult>(resultado);
            Assert.Empty(context.ArticuloFavoritos);
        }

        // Tocar dos veces el boton no puede romper nada: la segunda llamada encuentra
        // el estado que se pedia y responde lo mismo.
        [Fact]
        public async Task Delete_FavoritoInexistente_TambienDevuelve204()
        {
            using var context = CrearContexto();
            await SembrarAsync(context);

            var resultado = await CrearController(context).Delete(99);

            Assert.IsType<NoContentResult>(resultado);
        }

        // El id del articulo viene de la URL, pero el del usuario no: sale de la cookie.
        // Sin eso, cualquiera podria vaciarle los favoritos a otro.
        [Fact]
        public async Task Delete_NoTocaElFavoritoDeOtroUsuario()
        {
            using var context = CrearContexto();
            await SembrarAsync(context);
            context.ArticuloFavoritos.AddRange(
                new ArticuloFavorito { UserId = "user-1", ArticuloId = 1 },
                new ArticuloFavorito { UserId = "user-2", ArticuloId = 1 }
            );
            await context.SaveChangesAsync();

            await CrearController(context, "user-2").Delete(1);

            var restante = Assert.Single(context.ArticuloFavoritos);
            Assert.Equal("user-1", restante.UserId);
        }

        // Sin imagen el DTO expone cadena vacia y no null: el front no tiene que
        // distinguir entre "no hay campo" y "no hay imagen".
        [Fact]
        public async Task Get_ArticuloSinImagen_DevuelveCadenaVacia()
        {
            using var context = CrearContexto();
            await SembrarAsync(context);
            context.ArticuloFavoritos.Add(new ArticuloFavorito { UserId = "user-1", ArticuloId = 2 });
            await context.SaveChangesAsync();

            var resultado = await CrearController(context).Get();

            var favorito = Assert.Single(Assert.IsType<List<FavoritoDto>>(resultado.Value));
            Assert.Equal("", favorito.ImagenUrl);
        }
    }
}
