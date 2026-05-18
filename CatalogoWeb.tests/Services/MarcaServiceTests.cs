using catalogo_web_mvc.Interfaces.Marcas;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Services.Marcas;
using Moq;

namespace CatalogoWeb.Tests.Services
{
    public class MarcaServiceTests
    {
        private readonly Mock<IMarcaRepository> _repoMock;
        private readonly MarcaService _service;

        public MarcaServiceTests()
        {
            _repoMock = new Mock<IMarcaRepository>();
            _service = new MarcaService(_repoMock.Object);
        }

        private static List<Marca> MarcasDeEjemplo() => new()
        {
            new Marca { MarcaId = 1, Descripcion = "Samsung" },
            new Marca { MarcaId = 2, Descripcion = "Apple" },
            new Marca { MarcaId = 3, Descripcion = "Sony" }
        };

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_DelegaAlRepositorio()
        {
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(MarcasDeEjemplo());

            var resultado = await _service.GetAllAsync();

            Assert.Equal(3, resultado.Count);
            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_RetornaListaVacia_CuandoRepositorioEstaVacio()
        {
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Marca>());

            var resultado = await _service.GetAllAsync();

            Assert.Empty(resultado);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_RetornaMarcaCorrecta()
        {
            var marca = new Marca { MarcaId = 1, Descripcion = "Samsung" };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(marca);

            var resultado = await _service.GetByIdAsync(1);

            Assert.Equal(marca, resultado);
            _repoMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_RetornaNull_CuandoNoExiste()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Marca?)null);

            var resultado = await _service.GetByIdAsync(99);

            Assert.Null(resultado);
        }

        // ── AddAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task AddAsync_DelegaAlRepositorio()
        {
            var marca = new Marca { MarcaId = 5, Descripcion = "Huawei" };
            _repoMock.Setup(r => r.AddAsync(marca)).Returns(Task.CompletedTask);

            await _service.AddAsync(marca);

            _repoMock.Verify(r => r.AddAsync(marca), Times.Once);
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_DelegaAlRepositorio()
        {
            var marca = new Marca { MarcaId = 1, Descripcion = "Samsung Pro" };
            _repoMock.Setup(r => r.UpdateAsync(marca)).Returns(Task.CompletedTask);

            await _service.UpdateAsync(marca);

            _repoMock.Verify(r => r.UpdateAsync(marca), Times.Once);
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
