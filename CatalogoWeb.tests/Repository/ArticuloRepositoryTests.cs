using catalogo_web_mvc.Data;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Repository.Articulos;
using Microsoft.EntityFrameworkCore;

namespace CatalogoWeb.Tests.Repository
{
    public class ArticuloRepositoryTests
    {
        private static CatalogoContext CrearContexto()
        {
            var options = new DbContextOptionsBuilder<CatalogoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new CatalogoContext(options);
        }

        private static void SeedData(CatalogoContext ctx)
        {
            ctx.Marcas.AddRange(
                new Marca { MarcaId = 1, Descripcion = "Samsung" },
                new Marca { MarcaId = 2, Descripcion = "Apple" }
            );
            ctx.Categorias.AddRange(
                new Categoria { CategoriaId = 1, Descripcion = "Celulares" },
                new Categoria { CategoriaId = 2, Descripcion = "Televisores" }
            );
            ctx.Articulos.AddRange(
                new Articulo { Id = 1, Codigo = "S01", Nombre = "Galaxy S10", Descripcion = "Descripcion 1", Precio = 69999, MarcaId = 1, CategoriaId = 1 },
                new Articulo { Id = 2, Codigo = "A01", Nombre = "iPhone 14", Descripcion = "Descripcion 2", Precio = 120000, MarcaId = 2, CategoriaId = 2 }
            );
            ctx.SaveChanges();
        }

        // ── GetAll ────────────────────────────────────────────────────────────

        [Fact]
        public void GetAll_RetornaTodosLosArticulos()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new ArticuloRepository(ctx);

            var resultado = repo.GetAll().ToList();

            Assert.Equal(2, resultado.Count);
        }

        [Fact]
        public void GetAll_IncluyeNavegacionMarcaYCategoria()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new ArticuloRepository(ctx);

            var articulo = repo.GetAll().First();

            Assert.NotNull(articulo.Marca);
            Assert.NotNull(articulo.Categoria);
        }

        [Fact]
        public void GetAll_RetornaListaVacia_CuandoNoHayArticulos()
        {
            using var ctx = CrearContexto();
            var repo = new ArticuloRepository(ctx);

            var resultado = repo.GetAll().ToList();

            Assert.Empty(resultado);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_RetornaArticuloCorrecto()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new ArticuloRepository(ctx);

            var resultado = await repo.GetByIdAsync(1);

            Assert.NotNull(resultado);
            Assert.Equal("Galaxy S10", resultado.Nombre);
        }

        [Fact]
        public async Task GetByIdAsync_RetornaNull_CuandoNoExiste()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new ArticuloRepository(ctx);

            var resultado = await repo.GetByIdAsync(999);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task GetByIdAsync_IncluyeNavegacionMarcaYCategoria()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new ArticuloRepository(ctx);

            var resultado = await repo.GetByIdAsync(1);

            Assert.NotNull(resultado!.Marca);
            Assert.NotNull(resultado.Categoria);
        }

        // ── AddAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task AddAsync_AgregaArticuloAlContexto()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new ArticuloRepository(ctx);
            var nuevo = new Articulo { Id = 10, Codigo = "NEW", Nombre = "Nuevo Artículo", Descripcion = "Desc", Precio = 100, MarcaId = 1, CategoriaId = 1 };

            await repo.AddAsync(nuevo);

            Assert.Equal(3, ctx.Articulos.Count());
            Assert.True(await ctx.Articulos.AnyAsync(a => a.Codigo == "NEW"));
        }

        [Fact]
        public async Task AddAsync_PersistiDatosCorrectamente()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new ArticuloRepository(ctx);
            var nuevo = new Articulo { Id = 10, Codigo = "TEST-01", Nombre = "Artículo Test", Descripcion = "Desc", Precio = 999.99m, MarcaId = 1, CategoriaId = 1 };

            await repo.AddAsync(nuevo);

            var guardado = await ctx.Articulos.FindAsync(10);
            Assert.NotNull(guardado);
            Assert.Equal(999.99m, guardado.Precio);
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_ActualizaArticuloExistente()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new ArticuloRepository(ctx);
            var articulo = await ctx.Articulos.FindAsync(1);
            articulo!.Nombre = "Galaxy S10 Actualizado";

            await repo.UpdateAsync(articulo);

            var actualizado = await ctx.Articulos.FindAsync(1);
            Assert.Equal("Galaxy S10 Actualizado", actualizado!.Nombre);
        }

        [Fact]
        public async Task UpdateAsync_ActualizaPrecioCorrectamente()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new ArticuloRepository(ctx);
            var articulo = await ctx.Articulos.FindAsync(1);
            articulo!.Precio = 59999m;

            await repo.UpdateAsync(articulo);

            var actualizado = await ctx.Articulos.FindAsync(1);
            Assert.Equal(59999m, actualizado!.Precio);
        }

        // ── DeleteAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_EliminaArticuloExistente()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new ArticuloRepository(ctx);

            await repo.DeleteAsync(1);

            Assert.Equal(1, ctx.Articulos.Count());
            Assert.False(await ctx.Articulos.AnyAsync(a => a.Id == 1));
        }

        [Fact]
        public async Task DeleteAsync_NoFalla_CuandoArticuloNoExiste()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new ArticuloRepository(ctx);

            await repo.DeleteAsync(999);

            Assert.Equal(2, ctx.Articulos.Count());
        }

        // ── ExistsAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task ExistsAsync_RetornaTrue_CuandoExiste()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new ArticuloRepository(ctx);

            var existe = await repo.ExistsAsync(1);

            Assert.True(existe);
        }

        [Fact]
        public async Task ExistsAsync_RetornaFalse_CuandoNoExiste()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new ArticuloRepository(ctx);

            var existe = await repo.ExistsAsync(999);

            Assert.False(existe);
        }
    }
}
