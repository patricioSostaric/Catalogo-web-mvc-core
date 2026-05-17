using catalogo_web_mvc.Data;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Repository.Categorias;
using Microsoft.EntityFrameworkCore;

namespace CatalogoWeb.Tests.Repository
{
    public class CategoriaRepositoryTests
    {
        private static CatalogoContext CrearContexto()
        {
            var options = new DbContextOptionsBuilder<CatalogoContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new CatalogoContext(options);
        }

        private static void SeedData(CatalogoContext ctx)
        {
            ctx.Categorias.AddRange(
                new Categoria { CategoriaId = 1, Descripcion = "Celulares" },
                new Categoria { CategoriaId = 2, Descripcion = "Televisores" }
            );
            ctx.SaveChanges();
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_RetornaTodosLosRegistros()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new CategoriaRepository(ctx);

            var resultado = await repo.GetAllAsync();

            Assert.Equal(2, resultado.Count);
        }

        [Fact]
        public async Task GetAllAsync_RetornaListaVacia_CuandoNoHayRegistros()
        {
            using var ctx = CrearContexto();
            var repo = new CategoriaRepository(ctx);

            var resultado = await repo.GetAllAsync();

            Assert.Empty(resultado);
        }

        [Fact]
        public async Task GetAllAsync_RetornaDescripcionesCorrectas()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new CategoriaRepository(ctx);

            var resultado = await repo.GetAllAsync();

            Assert.Contains(resultado, c => c.Descripcion == "Celulares");
            Assert.Contains(resultado, c => c.Descripcion == "Televisores");
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_RetornaCategoriaCorrecta()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new CategoriaRepository(ctx);

            var resultado = await repo.GetByIdAsync(1);

            Assert.NotNull(resultado);
            Assert.Equal("Celulares", resultado.Descripcion);
        }

        [Fact]
        public async Task GetByIdAsync_RetornaNull_CuandoNoExiste()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new CategoriaRepository(ctx);

            var resultado = await repo.GetByIdAsync(999);

            Assert.Null(resultado);
        }

        // ── AddAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task AddAsync_AgregaCategoriaAlContexto()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new CategoriaRepository(ctx);
            var nueva = new Categoria { CategoriaId = 10, Descripcion = "Audio" };

            await repo.AddAsync(nueva);

            Assert.Equal(3, ctx.Categorias.Count());
            Assert.True(await ctx.Categorias.AnyAsync(c => c.Descripcion == "Audio"));
        }

        [Fact]
        public async Task AddAsync_PersisteDatosCorrectamente()
        {
            using var ctx = CrearContexto();
            var repo = new CategoriaRepository(ctx);
            var nueva = new Categoria { CategoriaId = 1, Descripcion = "Computación" };

            await repo.AddAsync(nueva);

            var guardada = await ctx.Categorias.FindAsync(1);
            Assert.NotNull(guardada);
            Assert.Equal("Computación", guardada.Descripcion);
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_ActualizaDescripcionCorrectamente()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new CategoriaRepository(ctx);
            var categoria = await ctx.Categorias.FindAsync(1);
            categoria!.Descripcion = "Smartphones";

            await repo.UpdateAsync(categoria);

            var actualizada = await ctx.Categorias.FindAsync(1);
            Assert.Equal("Smartphones", actualizada!.Descripcion);
        }

        // ── DeleteAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_EliminaCategoriaExistente()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new CategoriaRepository(ctx);

            await repo.DeleteAsync(1);

            Assert.Equal(1, ctx.Categorias.Count());
            Assert.False(await ctx.Categorias.AnyAsync(c => c.CategoriaId == 1));
        }

        [Fact]
        public async Task DeleteAsync_NoFalla_CuandoNoExiste()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new CategoriaRepository(ctx);

            await repo.DeleteAsync(999);

            Assert.Equal(2, ctx.Categorias.Count());
        }

        // ── ExistsAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task ExistsAsync_RetornaTrue_CuandoExiste()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new CategoriaRepository(ctx);

            var existe = await repo.ExistsAsync(1);

            Assert.True(existe);
        }

        [Fact]
        public async Task ExistsAsync_RetornaFalse_CuandoNoExiste()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new CategoriaRepository(ctx);

            var existe = await repo.ExistsAsync(999);

            Assert.False(existe);
        }
    }
}
