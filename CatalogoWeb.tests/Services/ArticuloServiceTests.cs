using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Services.Articulos;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moq;
using X.PagedList;
using X.PagedList.Extensions;

namespace CatalogoWeb.Tests.Services
{
    public class ArticuloServiceTests
    {
        private readonly Mock<IArticuloRepository> _repoMock;
        private readonly CatalogoContext _context;
        private readonly ArticuloService _service;

        public ArticuloServiceTests()
        {
            _repoMock = new Mock<IArticuloRepository>();

            var options = new DbContextOptionsBuilder<CatalogoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new CatalogoContext(options);
            SeedContexto(_context);

            _service = new ArticuloService(_repoMock.Object, _context);
        }

        private static void SeedContexto(CatalogoContext ctx)
        {
            ctx.Marcas.AddRange(
                new Marca { MarcaId = 1, Descripcion = "Samsung" },
                new Marca { MarcaId = 2, Descripcion = "Apple" }
            );
            ctx.Categorias.AddRange(
                new Categoria { CategoriaId = 1, Descripcion = "Celulares" },
                new Categoria { CategoriaId = 2, Descripcion = "Televisores" }
            );
            ctx.SaveChanges();
        }

        private static List<Articulo> ArticulosDeEjemplo() => new()
        {
            new Articulo
            {
                Id = 1, Codigo = "S01", Nombre = "Galaxy S10", Precio = 69999,
                MarcaId = 1, CategoriaId = 1,
                Marca = new Marca { MarcaId = 1, Descripcion = "Samsung" },
                Categoria = new Categoria { CategoriaId = 1, Descripcion = "Celulares" }
            },
            new Articulo
            {
                Id = 2, Codigo = "A01", Nombre = "iPhone 14", Precio = 120000,
                MarcaId = 2, CategoriaId = 2,
                Marca = new Marca { MarcaId = 2, Descripcion = "Apple" },
                Categoria = new Categoria { CategoriaId = 2, Descripcion = "Televisores" }
            },
            new Articulo
            {
                Id = 3, Codigo = "SONY01", Nombre = "Bravia 55", Precio = 49500,
                MarcaId = 1, CategoriaId = 2,
                Marca = new Marca { MarcaId = 1, Descripcion = "Samsung" },
                Categoria = new Categoria { CategoriaId = 2, Descripcion = "Televisores" }
            }
        };

        private void ConfigurarGetAll(List<Articulo> articulos)
            => _repoMock.Setup(r => r.GetAll()).Returns(articulos.AsQueryable());

        // ── BuscarAsync – sin filtros ─────────────────────────────────────────

        [Fact]
        public async Task BuscarAsync_SinFiltros_RetornaTodosLosArticulos()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            var resultado = await _service.BuscarAsync(null, false, null, null, null, 1, 10);

            Assert.Equal(3, resultado.TotalItemCount);
        }

        [Fact]
        public async Task BuscarAsync_SearchStringVacio_RetornaTodos()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            var resultado = await _service.BuscarAsync(string.Empty, false, null, null, null, 1, 10);

            Assert.Equal(3, resultado.TotalItemCount);
        }

        // ── BuscarAsync – búsqueda simple ─────────────────────────────────────

        [Fact]
        public async Task BuscarAsync_BusquedaSimple_FiltraPorNombre()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            var resultado = await _service.BuscarAsync("Galaxy", false, null, null, null, 1, 10);

            Assert.Single(resultado);
            Assert.Equal("Galaxy S10", resultado.First().Nombre);
        }

        [Fact]
        public async Task BuscarAsync_BusquedaSimple_RetornaVacio_CuandoNoHayCoincidencia()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            var resultado = await _service.BuscarAsync("PRODUCTO_INEXISTENTE_XYZ", false, null, null, null, 1, 10);

            Assert.Empty(resultado);
        }

        [Fact]
        public async Task BuscarAsync_BusquedaSimple_NoAplicaFiltro_CuandoFiltroAvanzadoEsTrue()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            // Con filtroAvanzado=true y sin campo/criterio/filtro, no filtra nada
            var resultado = await _service.BuscarAsync("Galaxy", true, null, null, null, 1, 10);

            Assert.Equal(3, resultado.TotalItemCount);
        }

        // ── BuscarAsync – filtro avanzado: Codigo ─────────────────────────────

        [Fact]
        public async Task BuscarAsync_FiltroAvanzado_CodigoContiene_RetornaCoincidencias()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            // S01 y SONY01 contienen "S"
            var resultado = await _service.BuscarAsync(null, true, "Codigo", "Contiene", "S", 1, 10);

            Assert.Equal(2, resultado.TotalItemCount);
        }

        [Fact]
        public async Task BuscarAsync_FiltroAvanzado_CodigoComienza_RetornaCoincidencias()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            var resultado = await _service.BuscarAsync(null, true, "Codigo", "Comienza con", "A", 1, 10);

            Assert.Single(resultado);
            Assert.Equal("A01", resultado.First().Codigo);
        }

        [Fact]
        public async Task BuscarAsync_FiltroAvanzado_CodigoTermina_RetornaCoincidencias()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            // Solo SONY01 termina con "Y01"
            var resultado = await _service.BuscarAsync(null, true, "Codigo", "Termina con", "Y01", 1, 10);

            Assert.Single(resultado);
            Assert.Equal("SONY01", resultado.First().Codigo);
        }

        // ── BuscarAsync – filtro avanzado: Nombre ─────────────────────────────

        [Fact]
        public async Task BuscarAsync_FiltroAvanzado_NombreContiene_RetornaCoincidencias()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            var resultado = await _service.BuscarAsync(null, true, "Nombre", "Contiene", "iPhone", 1, 10);

            Assert.Single(resultado);
            Assert.Equal("iPhone 14", resultado.First().Nombre);
        }

        [Fact]
        public async Task BuscarAsync_FiltroAvanzado_NombreComienza_RetornaCoincidencias()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            var resultado = await _service.BuscarAsync(null, true, "Nombre", "Comienza con", "Galaxy", 1, 10);

            Assert.Single(resultado);
            Assert.Equal("Galaxy S10", resultado.First().Nombre);
        }

        [Fact]
        public async Task BuscarAsync_FiltroAvanzado_NombreTermina_RetornaCoincidencias()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            // Solo "Bravia 55" termina con "55"
            var resultado = await _service.BuscarAsync(null, true, "Nombre", "Termina con", "55", 1, 10);

            Assert.Single(resultado);
            Assert.Equal("Bravia 55", resultado.First().Nombre);
        }

        // ── BuscarAsync – filtro avanzado: Precio ─────────────────────────────

        [Fact]
        public async Task BuscarAsync_FiltroAvanzado_PrecioIgualA_RetornaCoincidencia()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            var resultado = await _service.BuscarAsync(null, true, "Precio", "Igual a", "69999", 1, 10);

            Assert.Single(resultado);
            Assert.Equal(69999m, resultado.First().Precio);
        }

        [Fact]
        public async Task BuscarAsync_FiltroAvanzado_PrecioMayorA_RetornaCoincidencias()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            // 69999 y 120000 son > 60000; 49500 no
            var resultado = await _service.BuscarAsync(null, true, "Precio", "Mayor a", "60000", 1, 10);

            Assert.Equal(2, resultado.TotalItemCount);
        }

        [Fact]
        public async Task BuscarAsync_FiltroAvanzado_PrecioMenorA_RetornaCoincidencias()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            // Solo 49500 < 60000
            var resultado = await _service.BuscarAsync(null, true, "Precio", "Menor a", "60000", 1, 10);

            Assert.Single(resultado);
            Assert.Equal(49500m, resultado.First().Precio);
        }

        [Fact]
        public async Task BuscarAsync_FiltroAvanzado_PrecioInvalido_RetornaTodos()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            // "abc" no parsea como decimal → no aplica filtro
            var resultado = await _service.BuscarAsync(null, true, "Precio", "Mayor a", "abc", 1, 10);

            Assert.Equal(3, resultado.TotalItemCount);
        }

        // ── BuscarAsync – filtro avanzado: Marca y Categoria ─────────────────

        [Fact]
        public async Task BuscarAsync_FiltroAvanzado_MarcaContiene_RetornaCoincidencias()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            var resultado = await _service.BuscarAsync(null, true, "Marca", "Contiene", "Apple", 1, 10);

            Assert.Single(resultado);
            Assert.Equal("iPhone 14", resultado.First().Nombre);
        }

        [Fact]
        public async Task BuscarAsync_FiltroAvanzado_CategoriaContiene_RetornaCoincidencias()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            // iPhone 14 y Bravia 55 son de Televisores
            var resultado = await _service.BuscarAsync(null, true, "Categoria", "Contiene", "Televisores", 1, 10);

            Assert.Equal(2, resultado.TotalItemCount);
        }

        // ── BuscarAsync – paginación ──────────────────────────────────────────

        [Fact]
        public async Task BuscarAsync_Paginacion_RetornaPrimeraPagina()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            var resultado = await _service.BuscarAsync(null, false, null, null, null, 1, 2);

            Assert.Equal(2, resultado.Count);
            Assert.Equal(3, resultado.TotalItemCount);
        }

        [Fact]
        public async Task BuscarAsync_Paginacion_RetornaSegundaPagina()
        {
            ConfigurarGetAll(ArticulosDeEjemplo());

            var resultado = await _service.BuscarAsync(null, false, null, null, null, 2, 2);

            Assert.Single(resultado);
        }

        // ── BuscarAsync – soloActivos ─────────────────────────────────────────

        private static List<Articulo> ArticulosConVariosEstados() => new()
        {
            new Articulo { Id = 1, Codigo = "A1", Nombre = "Activo Con Stock",    MarcaId = 1, CategoriaId = 1, Activo = true,  Stock = 5,  Precio = 100,
                Marca = new Marca { MarcaId = 1, Descripcion = "Samsung" }, Categoria = new Categoria { CategoriaId = 1, Descripcion = "Celulares" } },
            new Articulo { Id = 2, Codigo = "A2", Nombre = "Activo Sin Stock",    MarcaId = 1, CategoriaId = 1, Activo = true,  Stock = 0,  Precio = 200,
                Marca = new Marca { MarcaId = 1, Descripcion = "Samsung" }, Categoria = new Categoria { CategoriaId = 1, Descripcion = "Celulares" } },
            new Articulo { Id = 3, Codigo = "A3", Nombre = "Inactivo Con Stock",  MarcaId = 1, CategoriaId = 1, Activo = false, Stock = 10, Precio = 300,
                Marca = new Marca { MarcaId = 1, Descripcion = "Samsung" }, Categoria = new Categoria { CategoriaId = 1, Descripcion = "Celulares" } },
            new Articulo { Id = 4, Codigo = "A4", Nombre = "Inactivo Sin Stock",  MarcaId = 1, CategoriaId = 1, Activo = false, Stock = 0,  Precio = 400,
                Marca = new Marca { MarcaId = 1, Descripcion = "Samsung" }, Categoria = new Categoria { CategoriaId = 1, Descripcion = "Celulares" } },
        };

        [Fact]
        public async Task BuscarAsync_SoloActivos_RetornaSoloActivosConStock()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(ArticulosConVariosEstados().AsQueryable());

            var resultado = await _service.BuscarAsync(null, false, null, null, null, 1, 10, soloActivos: true);

            Assert.Single(resultado);
            Assert.Equal("Activo Con Stock", resultado.First().Nombre);
        }

        [Fact]
        public async Task BuscarAsync_SoloActivos_ExcluyeActivosSinStock()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(ArticulosConVariosEstados().AsQueryable());

            var resultado = await _service.BuscarAsync(null, false, null, null, null, 1, 10, soloActivos: true);

            Assert.DoesNotContain(resultado, a => a.Nombre == "Activo Sin Stock");
        }

        [Fact]
        public async Task BuscarAsync_SoloActivos_ExcluyeInactivos()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(ArticulosConVariosEstados().AsQueryable());

            var resultado = await _service.BuscarAsync(null, false, null, null, null, 1, 10, soloActivos: true);

            Assert.DoesNotContain(resultado, a => a.Activo == false);
        }

        [Fact]
        public async Task BuscarAsync_SinSoloActivos_RetornaTodosLosArticulos()
        {
            _repoMock.Setup(r => r.GetAll()).Returns(ArticulosConVariosEstados().AsQueryable());

            var resultado = await _service.BuscarAsync(null, false, null, null, null, 1, 10, soloActivos: false);

            Assert.Equal(4, resultado.TotalItemCount);
        }

        // ── SelectLists ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetMarcasSelectList_RetornaTodasLasMarcas()
        {
            var result = await _service.GetMarcasSelectList();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetMarcasSelectList_RetornaListaOrdenadaAlfabeticamente()
        {
            var result = await _service.GetMarcasSelectList();

            var items = result.Cast<SelectListItem>().ToList();
            Assert.Equal("Apple", items[0].Text);
            Assert.Equal("Samsung", items[1].Text);
        }

        [Fact]
        public async Task GetCategoriasSelectList_RetornaTodasLasCategorias()
        {
            var result = await _service.GetCategoriasSelectList();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetCategoriasSelectList_RetornaListaOrdenadaAlfabeticamente()
        {
            var result = await _service.GetCategoriasSelectList();

            var items = result.Cast<SelectListItem>().ToList();
            Assert.Equal("Celulares", items[0].Text);
            Assert.Equal("Televisores", items[1].Text);
        }

        // ── Delegación al repositorio ─────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_DelegaAlRepositorio()
        {
            var articulo = new Articulo { Id = 5, Codigo = "X", Nombre = "Test", MarcaId = 1, CategoriaId = 1 };
            _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(articulo);

            var resultado = await _service.GetByIdAsync(5);

            Assert.Equal(articulo, resultado);
            _repoMock.Verify(r => r.GetByIdAsync(5), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_RetornaNull_CuandoNoExiste()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Articulo?)null);

            var resultado = await _service.GetByIdAsync(99);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task AddAsync_DelegaAlRepositorio()
        {
            var articulo = new Articulo { Id = 5, Codigo = "X", Nombre = "Test", MarcaId = 1, CategoriaId = 1 };
            _repoMock.Setup(r => r.AddAsync(articulo)).Returns(Task.CompletedTask);

            await _service.AddAsync(articulo);

            _repoMock.Verify(r => r.AddAsync(articulo), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_DelegaAlRepositorio()
        {
            var articulo = new Articulo { Id = 5, Codigo = "X", Nombre = "Test", MarcaId = 1, CategoriaId = 1 };
            _repoMock.Setup(r => r.UpdateAsync(articulo)).Returns(Task.CompletedTask);

            await _service.UpdateAsync(articulo);

            _repoMock.Verify(r => r.UpdateAsync(articulo), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DelegaAlRepositorio()
        {
            _repoMock.Setup(r => r.DeleteAsync(5)).Returns(Task.CompletedTask);

            await _service.DeleteAsync(5);

            _repoMock.Verify(r => r.DeleteAsync(5), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_RetornaTrue_DelegaAlRepositorio()
        {
            _repoMock.Setup(r => r.ExistsAsync(5)).ReturnsAsync(true);

            var existe = await _service.ExistsAsync(5);

            Assert.True(existe);
            _repoMock.Verify(r => r.ExistsAsync(5), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_RetornaFalse_CuandoNoExiste()
        {
            _repoMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

            var existe = await _service.ExistsAsync(99);

            Assert.False(existe);
        }
    }
}
