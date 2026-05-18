using catalogo_web_mvc.Data;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Repository.Marcas;
using Microsoft.EntityFrameworkCore;

namespace CatalogoWeb.Tests.Repository
{
    public class MarcaRepositoryTests
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
            ctx.Marcas.AddRange(
                new Marca { MarcaId = 1, Descripcion = "Samsung" },
                new Marca { MarcaId = 2, Descripcion = "Apple" }
            );
            ctx.SaveChanges();
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_RetornaTodosLosRegistros()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new MarcaRepository(ctx);

            var resultado = await repo.GetAllAsync();

            Assert.Equal(2, resultado.Count);
        }

        [Fact]
        public async Task GetAllAsync_RetornaListaVacia_CuandoNoHayRegistros()
        {
            using var ctx = CrearContexto();
            var repo = new MarcaRepository(ctx);

            var resultado = await repo.GetAllAsync();

            Assert.Empty(resultado);
        }

        [Fact]
        public async Task GetAllAsync_RetornaDescripcionesCorrectas()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new MarcaRepository(ctx);

            var resultado = await repo.GetAllAsync();

            Assert.Contains(resultado, m => m.Descripcion == "Samsung");
            Assert.Contains(resultado, m => m.Descripcion == "Apple");
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_RetornaMarcaCorrecta()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new MarcaRepository(ctx);

            var resultado = await repo.GetByIdAsync(1);

            Assert.NotNull(resultado);
            Assert.Equal("Samsung", resultado.Descripcion);
        }

        [Fact]
        public async Task GetByIdAsync_RetornaNull_CuandoNoExiste()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new MarcaRepository(ctx);

            var resultado = await repo.GetByIdAsync(999);

            Assert.Null(resultado);
        }

        // ── AddAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task AddAsync_AgregaMarcaAlContexto()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new MarcaRepository(ctx);
            var nueva = new Marca { MarcaId = 10, Descripcion = "Sony" };

            await repo.AddAsync(nueva);

            Assert.Equal(3, ctx.Marcas.Count());
            Assert.True(await ctx.Marcas.AnyAsync(m => m.Descripcion == "Sony"));
        }

        [Fact]
        public async Task AddAsync_PersisteDatosCorrectamente()
        {
            using var ctx = CrearContexto();
            var repo = new MarcaRepository(ctx);
            var nueva = new Marca { MarcaId = 1, Descripcion = "Motorola" };

            await repo.AddAsync(nueva);

            var guardada = await ctx.Marcas.FindAsync(1);
            Assert.NotNull(guardada);
            Assert.Equal("Motorola", guardada.Descripcion);
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_ActualizaDescripcionCorrectamente()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new MarcaRepository(ctx);
            var marca = await ctx.Marcas.FindAsync(1);
            marca!.Descripcion = "Samsung Updated";

            await repo.UpdateAsync(marca);

            var actualizada = await ctx.Marcas.FindAsync(1);
            Assert.Equal("Samsung Updated", actualizada!.Descripcion);
        }

        // ── DeleteAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_EliminaMarcaExistente()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new MarcaRepository(ctx);

            await repo.DeleteAsync(1);

            Assert.Equal(1, ctx.Marcas.Count());
            Assert.False(await ctx.Marcas.AnyAsync(m => m.MarcaId == 1));
        }

        [Fact]
        public async Task DeleteAsync_NoFalla_CuandoNoExiste()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new MarcaRepository(ctx);

            await repo.DeleteAsync(999);

            Assert.Equal(2, ctx.Marcas.Count());
        }

        // ── ExistsAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task ExistsAsync_RetornaTrue_CuandoExiste()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new MarcaRepository(ctx);

            var existe = await repo.ExistsAsync(1);

            Assert.True(existe);
        }

        [Fact]
        public async Task ExistsAsync_RetornaFalse_CuandoNoExiste()
        {
            using var ctx = CrearContexto();
            SeedData(ctx);
            var repo = new MarcaRepository(ctx);

            var existe = await repo.ExistsAsync(999);

            Assert.False(existe);
        }
    }
}
