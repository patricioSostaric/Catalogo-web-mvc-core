using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Interfaces.Carrito;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Services.Carrito;
using Moq;

namespace CatalogoWeb.Tests.Services
{
    public class CarritoServiceTests
    {
        private const string UserId = "user-1";

        private readonly Mock<ICarritoRepository> _repositoryMock = new();
        private readonly Mock<IArticuloService> _articuloServiceMock = new();
        private readonly CarritoService _service;

        public CarritoServiceTests()
        {
            _service = new CarritoService(_repositoryMock.Object, _articuloServiceMock.Object);
        }

        private static Articulo Articulo(int id = 1, int stock = 10, bool activo = true, decimal precio = 100m)
            => new()
            {
                Id = id,
                Nombre = "Artículo de prueba",
                Codigo = "A01",
                Descripcion = "Descripción",
                Precio = precio,
                Stock = stock,
                Activo = activo
            };

        [Fact]
        public async Task AgregarAsync_ArticuloInexistente_Falla()
        {
            _articuloServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Articulo?)null);

            var resultado = await _service.AgregarAsync(UserId, 1, 1);

            Assert.False(resultado.Exito);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ItemCarrito>()), Times.Never);
        }

        [Fact]
        public async Task AgregarAsync_ArticuloInactivo_Falla()
        {
            _articuloServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(Articulo(activo: false));

            var resultado = await _service.AgregarAsync(UserId, 1, 1);

            Assert.False(resultado.Exito);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ItemCarrito>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public async Task AgregarAsync_CantidadInvalida_Falla(int cantidad)
        {
            var resultado = await _service.AgregarAsync(UserId, 1, cantidad);

            Assert.False(resultado.Exito);
            _articuloServiceMock.Verify(s => s.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task AgregarAsync_SuperaElStock_Falla()
        {
            _articuloServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(Articulo(stock: 2));
            _repositoryMock.Setup(r => r.GetItemAsync(UserId, 1)).ReturnsAsync((ItemCarrito?)null);

            var resultado = await _service.AgregarAsync(UserId, 1, 3);

            Assert.False(resultado.Exito);
            Assert.Contains("2", resultado.Error);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ItemCarrito>()), Times.Never);
        }

        [Fact]
        public async Task AgregarAsync_ArticuloNuevo_CreaLaLinea()
        {
            _articuloServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(Articulo());
            _repositoryMock.Setup(r => r.GetItemAsync(UserId, 1)).ReturnsAsync((ItemCarrito?)null);

            var resultado = await _service.AgregarAsync(UserId, 1, 2);

            Assert.True(resultado.Exito);
            _repositoryMock.Verify(r => r.AddAsync(
                It.Is<ItemCarrito>(i => i.UserId == UserId && i.ArticuloId == 1 && i.Cantidad == 2)), Times.Once);
        }

        [Fact]
        public async Task AgregarAsync_ArticuloYaEnElCarrito_SumaLaCantidad()
        {
            var articulo = Articulo(stock: 10);
            _articuloServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(articulo);
            _repositoryMock.Setup(r => r.GetItemAsync(UserId, 1))
                .ReturnsAsync(new ItemCarrito { UserId = UserId, ArticuloId = 1, Cantidad = 3, Articulo = articulo });

            var resultado = await _service.AgregarAsync(UserId, 1, 2);

            Assert.True(resultado.Exito);
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<ItemCarrito>(i => i.Cantidad == 5)), Times.Once);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ItemCarrito>()), Times.Never);
        }

        [Fact]
        public async Task AgregarAsync_SumaSuperaElMaximoPorArticulo_Falla()
        {
            var articulo = Articulo(stock: 500);
            _articuloServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(articulo);
            _repositoryMock.Setup(r => r.GetItemAsync(UserId, 1))
                .ReturnsAsync(new ItemCarrito { UserId = UserId, ArticuloId = 1, Cantidad = 99, Articulo = articulo });

            var resultado = await _service.AgregarAsync(UserId, 1, 5);

            Assert.False(resultado.Exito);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ItemCarrito>()), Times.Never);
        }

        [Fact]
        public async Task AgregarAsync_DevuelveElNombreParaLaConfirmacion()
        {
            _articuloServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(Articulo());
            _repositoryMock.Setup(r => r.GetItemAsync(UserId, 1)).ReturnsAsync((ItemCarrito?)null);

            var resultado = await _service.AgregarAsync(UserId, 1, 1);

            Assert.True(resultado.Exito);
            Assert.Equal("Artículo de prueba", resultado.NombreArticulo);
        }

        [Fact]
        public async Task CambiarCantidadAsync_ACero_QuitaElArticulo()
        {
            var resultado = await _service.CambiarCantidadAsync(UserId, 1, 0);

            Assert.True(resultado.Exito);
            _repositoryMock.Verify(r => r.RemoveAsync(UserId, 1), Times.Once);
        }

        [Fact]
        public async Task CambiarCantidadAsync_SuperaElStock_Falla()
        {
            var articulo = Articulo(stock: 4);
            _repositoryMock.Setup(r => r.GetItemAsync(UserId, 1))
                .ReturnsAsync(new ItemCarrito { UserId = UserId, ArticuloId = 1, Cantidad = 1, Articulo = articulo });

            var resultado = await _service.CambiarCantidadAsync(UserId, 1, 9);

            Assert.False(resultado.Exito);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ItemCarrito>()), Times.Never);
        }

        [Fact]
        public async Task CambiarCantidadAsync_ArticuloFueraDelCarrito_Falla()
        {
            _repositoryMock.Setup(r => r.GetItemAsync(UserId, 1)).ReturnsAsync((ItemCarrito?)null);

            var resultado = await _service.CambiarCantidadAsync(UserId, 1, 2);

            Assert.False(resultado.Exito);
        }

        [Fact]
        public async Task GetCarritoAsync_CalculaSubtotalesYTotal()
        {
            _repositoryMock.Setup(r => r.GetByUsuarioAsync(UserId)).ReturnsAsync(
            [
                new ItemCarrito { ArticuloId = 1, Cantidad = 2, Articulo = Articulo(1, precio: 100m) },
                new ItemCarrito { ArticuloId = 2, Cantidad = 3, Articulo = Articulo(2, precio: 50m) }
            ]);

            var carrito = await _service.GetCarritoAsync(UserId);

            Assert.Equal(350m, carrito.Total);
            Assert.Equal(5, carrito.CantidadUnidades);
            Assert.False(carrito.EstaVacio);
        }

        [Fact]
        public async Task GetCarritoAsync_GeneraUnaClaveDeIdempotenciaPorRender()
        {
            _repositoryMock.Setup(r => r.GetByUsuarioAsync(UserId)).ReturnsAsync([]);

            var primero = await _service.GetCarritoAsync(UserId);
            var segundo = await _service.GetCarritoAsync(UserId);

            Assert.False(string.IsNullOrWhiteSpace(primero.ClaveIdempotencia));
            Assert.NotEqual(primero.ClaveIdempotencia, segundo.ClaveIdempotencia);
        }

        [Fact]
        public async Task GetCarritoAsync_MarcaLosItemsQueSuperanElStock()
        {
            _repositoryMock.Setup(r => r.GetByUsuarioAsync(UserId)).ReturnsAsync(
            [
                new ItemCarrito { ArticuloId = 1, Cantidad = 5, Articulo = Articulo(1, stock: 2) }
            ]);

            var carrito = await _service.GetCarritoAsync(UserId);

            Assert.True(carrito.Items[0].SuperaStock);
        }
    }
}
