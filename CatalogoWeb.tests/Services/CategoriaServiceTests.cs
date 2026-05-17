using catalogo_web_mvc.Interfaces.Categorias;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Services.Categorias;
using Moq;

namespace CatalogoWeb.Tests.Services
{
    public class CategoriaServiceTests
    {
        private readonly Mock<ICategoriaRepository> _repoMock;
        private readonly CategoriaService _service;

        public CategoriaServiceTests()
        {
            _repoMock = new Mock<ICategoriaRepository>();
            _service = new CategoriaService(_repoMock.Object);
        }

        private static List<Categoria> CategoriasDeEjemplo() => new()
        {
            new Categoria { CategoriaId = 1, Descripcion = "Celulares" },
            new Categoria { CategoriaId = 2, Descripcion = "Televisores" },
            new Categoria { CategoriaId = 3, Descripcion = "Audio" }
        };

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_DelegaAlRepositorio()
        {
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(CategoriasDeEjemplo());

            var resultado = await _service.GetAllAsync();

            Assert.Equal(3, resultado.Count);
            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_RetornaListaVacia_CuandoRepositorioEstaVacio()
        {
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Categoria>());

            var resultado = await _service.GetAllAsync();

            Assert.Empty(resultado);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_RetornaCategoriaCorrecta()
        {
            var categoria = new Categoria { CategoriaId = 1, Descripcion = "Celulares" };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(categoria);

            var resultado = await _service.GetByIdAsync(1);

            Assert.Equal(categoria, resultado);
            _repoMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_RetornaNull_CuandoNoExiste()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Categoria?)null);

            var resultado = await _service.GetByIdAsync(99);

            Assert.Null(resultado);
        }

        // ── AddAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task AddAsync_DelegaAlRepositorio()
        {
            var categoria = new Categoria { CategoriaId = 5, Descripcion = "Media" };
            _repoMock.Setup(r => r.AddAsync(categoria)).Returns(Task.CompletedTask);

            await _service.AddAsync(categoria);

            _repoMock.Verify(r => r.AddAsync(categoria), Times.Once);
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_DelegaAlRepositorio()
        {
            var categoria = new Categoria { CategoriaId = 1, Descripcion = "Smartphones" };
            _repoMock.Setup(r => r.UpdateAsync(categoria)).Returns(Task.CompletedTask);

            await _service.UpdateAsync(categoria);

            _repoMock.Verify(r => r.UpdateAsync(categoria), Times.Once);
        }

        // ── DeleteAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_DelegaAlRepositorio()
        {
            _repoMock.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            await _service.DeleteAsync(1);

            _repoMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        // ── ExistsAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task ExistsAsync_RetornaTrue_DelegaAlRepositorio()
        {
            _repoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

            var existe = await _service.ExistsAsync(1);

            Assert.True(existe);
            _repoMock.Verify(r => r.ExistsAsync(1), Times.Once);
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
