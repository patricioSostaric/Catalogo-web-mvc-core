using catalogo_web_mvc.Interfaces.Carrito;
using catalogo_web_mvc.Interfaces.Pedidos;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Services.Pedidos;
using Moq;

namespace CatalogoWeb.Tests.Services
{
    public class PedidoServiceTests
    {
        private const string UserId = "user-1";
        private const string Clave = "9f1d2c33-4a55-4c1e-8f0b-2b7d6e5a1c90";

        private readonly Mock<IPedidoRepository> _pedidoRepositoryMock = new();
        private readonly Mock<ICarritoRepository> _carritoRepositoryMock = new();
        private readonly PedidoService _service;

        public PedidoServiceTests()
        {
            _service = new PedidoService(
                _pedidoRepositoryMock.Object,
                _carritoRepositoryMock.Object,
                TimeZoneInfo.Utc);
        }

        private static ItemCarrito Item(int articuloId, int cantidad, decimal precio, string nombre = "Artículo")
            => new()
            {
                ArticuloId = articuloId,
                Cantidad = cantidad,
                Articulo = new Articulo
                {
                    Id = articuloId,
                    Nombre = nombre,
                    Codigo = $"A{articuloId}",
                    Descripcion = "Descripción",
                    Precio = precio,
                    Stock = 100
                }
            };

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ConfirmarAsync_SinClave_Falla(string clave)
        {
            var resultado = await _service.ConfirmarAsync(UserId, clave);

            Assert.False(resultado.Exito);
            _pedidoRepositoryMock.Verify(r => r.ConfirmarAsync(It.IsAny<Pedido>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmarAsync_CarritoVacio_Falla()
        {
            _pedidoRepositoryMock.Setup(r => r.GetByClaveIdempotenciaAsync(Clave)).ReturnsAsync((Pedido?)null);
            _carritoRepositoryMock.Setup(r => r.GetByUsuarioAsync(UserId)).ReturnsAsync([]);

            var resultado = await _service.ConfirmarAsync(UserId, Clave);

            Assert.False(resultado.Exito);
            _pedidoRepositoryMock.Verify(r => r.ConfirmarAsync(It.IsAny<Pedido>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmarAsync_ClaveYaUsada_DevuelveElPedidoExistenteSinCrearOtro()
        {
            var previo = new Pedido
            {
                Id = 7,
                UserId = UserId,
                Total = 500m,
                ClaveIdempotencia = Clave,
                Detalles = [new PedidoDetalle { Cantidad = 2, PrecioUnitario = 250m }]
            };
            _pedidoRepositoryMock.Setup(r => r.GetByClaveIdempotenciaAsync(Clave)).ReturnsAsync(previo);

            var resultado = await _service.ConfirmarAsync(UserId, Clave);

            Assert.True(resultado.Exito);
            Assert.True(resultado.YaExistia);
            Assert.Equal(7, resultado.PedidoId);
            Assert.Equal(500m, resultado.Total);
            _pedidoRepositoryMock.Verify(r => r.ConfirmarAsync(It.IsAny<Pedido>()), Times.Never);
            _carritoRepositoryMock.Verify(r => r.GetByUsuarioAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmarAsync_ClaveDeOtroUsuario_Falla()
        {
            _pedidoRepositoryMock.Setup(r => r.GetByClaveIdempotenciaAsync(Clave))
                .ReturnsAsync(new Pedido { Id = 7, UserId = "otro-usuario", ClaveIdempotencia = Clave });

            var resultado = await _service.ConfirmarAsync(UserId, Clave);

            Assert.False(resultado.Exito);
            _pedidoRepositoryMock.Verify(r => r.ConfirmarAsync(It.IsAny<Pedido>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmarAsync_CarritoConItems_CalculaElTotal()
        {
            _pedidoRepositoryMock.Setup(r => r.GetByClaveIdempotenciaAsync(Clave)).ReturnsAsync((Pedido?)null);
            _carritoRepositoryMock.Setup(r => r.GetByUsuarioAsync(UserId)).ReturnsAsync(
            [
                Item(1, 2, 100m),
                Item(2, 1, 350m)
            ]);
            _pedidoRepositoryMock.Setup(r => r.ConfirmarAsync(It.IsAny<Pedido>())).ReturnsAsync(true);

            var resultado = await _service.ConfirmarAsync(UserId, Clave);

            Assert.True(resultado.Exito);
            Assert.False(resultado.YaExistia);
            Assert.Equal(550m, resultado.Total);
            Assert.Equal(3, resultado.CantidadArticulos);
        }

        [Fact]
        public async Task ConfirmarAsync_CongelaElPrecioYElNombreDelArticulo()
        {
            Pedido? capturado = null;
            _pedidoRepositoryMock.Setup(r => r.GetByClaveIdempotenciaAsync(Clave)).ReturnsAsync((Pedido?)null);
            _carritoRepositoryMock.Setup(r => r.GetByUsuarioAsync(UserId))
                .ReturnsAsync([Item(1, 2, 100m, "Galaxy S10")]);
            _pedidoRepositoryMock.Setup(r => r.ConfirmarAsync(It.IsAny<Pedido>()))
                .Callback<Pedido>(p => capturado = p)
                .ReturnsAsync(true);

            await _service.ConfirmarAsync(UserId, Clave);

            Assert.NotNull(capturado);
            var detalle = Assert.Single(capturado!.Detalles);
            Assert.Equal(100m, detalle.PrecioUnitario);
            Assert.Equal("Galaxy S10", detalle.NombreArticulo);
            Assert.Equal(200m, detalle.Subtotal);
        }

        [Fact]
        public async Task ConfirmarAsync_GuardaLaClaveEnElPedido()
        {
            Pedido? capturado = null;
            _pedidoRepositoryMock.Setup(r => r.GetByClaveIdempotenciaAsync(Clave)).ReturnsAsync((Pedido?)null);
            _carritoRepositoryMock.Setup(r => r.GetByUsuarioAsync(UserId)).ReturnsAsync([Item(1, 1, 10m)]);
            _pedidoRepositoryMock.Setup(r => r.ConfirmarAsync(It.IsAny<Pedido>()))
                .Callback<Pedido>(p => capturado = p)
                .ReturnsAsync(true);

            await _service.ConfirmarAsync(UserId, Clave);

            Assert.Equal(Clave, capturado!.ClaveIdempotencia);
            Assert.Equal(UserId, capturado.UserId);
        }

        // ── Cancelación ───────────────────────────────────────────────────────

        [Fact]
        public async Task CancelarAsync_PedidoInexistenteOAjeno_Falla()
        {
            // GetByIdAsync filtra por usuario: un pedido ajeno devuelve null igual que uno
            // que no existe, asi que la respuesta no revela cual de los dos casos es.
            _pedidoRepositoryMock.Setup(r => r.GetByIdAsync(7, UserId)).ReturnsAsync((Pedido?)null);

            var resultado = await _service.CancelarAsync(UserId, 7);

            Assert.False(resultado.Exito);
            _pedidoRepositoryMock.Verify(r => r.CancelarAsync(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Theory]
        [InlineData(EstadoPedido.Enviado)]
        [InlineData(EstadoPedido.Entregado)]
        [InlineData(EstadoPedido.Cancelado)]
        public async Task CancelarAsync_FueraDeConfirmado_Falla(EstadoPedido estado)
        {
            _pedidoRepositoryMock.Setup(r => r.GetByIdAsync(7, UserId))
                .ReturnsAsync(new Pedido { Id = 7, UserId = UserId, Estado = estado });

            var resultado = await _service.CancelarAsync(UserId, 7);

            Assert.False(resultado.Exito);
            _pedidoRepositoryMock.Verify(r => r.CancelarAsync(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public async Task CancelarAsync_PedidoConfirmado_LoCancela()
        {
            _pedidoRepositoryMock.Setup(r => r.GetByIdAsync(7, UserId))
                .ReturnsAsync(new Pedido { Id = 7, UserId = UserId, Estado = EstadoPedido.Confirmado });
            _pedidoRepositoryMock.Setup(r => r.CancelarAsync(7, It.IsAny<DateTime>())).ReturnsAsync(true);

            var resultado = await _service.CancelarAsync(UserId, 7);

            Assert.True(resultado.Exito);
            Assert.Equal(EstadoPedido.Cancelado, resultado.EstadoNuevo);
        }

        [Fact]
        public async Task CancelarAsync_SiOtroLoCambioEnElMedio_Falla()
        {
            // El repositorio devuelve false cuando el UPDATE condicional no afecta filas:
            // entre la lectura y la escritura, alguien lo cancelo o lo despacho.
            _pedidoRepositoryMock.Setup(r => r.GetByIdAsync(7, UserId))
                .ReturnsAsync(new Pedido { Id = 7, UserId = UserId, Estado = EstadoPedido.Confirmado });
            _pedidoRepositoryMock.Setup(r => r.CancelarAsync(7, It.IsAny<DateTime>())).ReturnsAsync(false);

            var resultado = await _service.CancelarAsync(UserId, 7);

            Assert.False(resultado.Exito);
            Assert.Contains("cambió de estado", resultado.Error);
        }

        // ── Avance de estado ──────────────────────────────────────────────────

        [Fact]
        public async Task AvanzarAsync_PedidoInexistente_Falla()
        {
            _pedidoRepositoryMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync((Pedido?)null);

            var resultado = await _service.AvanzarAsync(7);

            Assert.False(resultado.Exito);
        }

        [Theory]
        [InlineData(EstadoPedido.Confirmado, EstadoPedido.Enviado)]
        [InlineData(EstadoPedido.Enviado, EstadoPedido.Entregado)]
        public async Task AvanzarAsync_MueveAlSiguienteEstado(EstadoPedido actual, EstadoPedido esperado)
        {
            _pedidoRepositoryMock.Setup(r => r.GetByIdAsync(7))
                .ReturnsAsync(new Pedido { Id = 7, Estado = actual });
            _pedidoRepositoryMock.Setup(r => r.CambiarEstadoAsync(7, actual, esperado, It.IsAny<DateTime>()))
                .ReturnsAsync(true);

            var resultado = await _service.AvanzarAsync(7);

            Assert.True(resultado.Exito);
            Assert.Equal(esperado, resultado.EstadoNuevo);
        }

        [Theory]
        [InlineData(EstadoPedido.Entregado)]
        [InlineData(EstadoPedido.Cancelado)]
        public async Task AvanzarAsync_DesdeUnTerminal_Falla(EstadoPedido estado)
        {
            _pedidoRepositoryMock.Setup(r => r.GetByIdAsync(7))
                .ReturnsAsync(new Pedido { Id = 7, Estado = estado });

            var resultado = await _service.AvanzarAsync(7);

            Assert.False(resultado.Exito);
            _pedidoRepositoryMock.Verify(r => r.CambiarEstadoAsync(
                It.IsAny<int>(), It.IsAny<EstadoPedido>(), It.IsAny<EstadoPedido>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public async Task AvanzarAsync_SiOtroLoMovioEnElMedio_Falla()
        {
            _pedidoRepositoryMock.Setup(r => r.GetByIdAsync(7))
                .ReturnsAsync(new Pedido { Id = 7, Estado = EstadoPedido.Confirmado });
            _pedidoRepositoryMock.Setup(r => r.CambiarEstadoAsync(
                7, EstadoPedido.Confirmado, EstadoPedido.Enviado, It.IsAny<DateTime>())).ReturnsAsync(false);

            var resultado = await _service.AvanzarAsync(7);

            Assert.False(resultado.Exito);
        }

        [Fact]
        public async Task AvanzarAsync_PasaElEstadoActualComoEsperado()
        {
            // Es lo que hace segura la operacion: el UPDATE solo aplica si el pedido sigue
            // en el estado que se leyo.
            _pedidoRepositoryMock.Setup(r => r.GetByIdAsync(7))
                .ReturnsAsync(new Pedido { Id = 7, Estado = EstadoPedido.Enviado });
            _pedidoRepositoryMock.Setup(r => r.CambiarEstadoAsync(
                It.IsAny<int>(), It.IsAny<EstadoPedido>(), It.IsAny<EstadoPedido>(), It.IsAny<DateTime>()))
                .ReturnsAsync(true);

            await _service.AvanzarAsync(7);

            _pedidoRepositoryMock.Verify(r => r.CambiarEstadoAsync(
                7, EstadoPedido.Enviado, EstadoPedido.Entregado, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task ConfirmarAsync_SinStockAlDescontar_FallaYAvisa()
        {
            _pedidoRepositoryMock.Setup(r => r.GetByClaveIdempotenciaAsync(Clave)).ReturnsAsync((Pedido?)null);
            _carritoRepositoryMock.Setup(r => r.GetByUsuarioAsync(UserId)).ReturnsAsync([Item(1, 1, 10m)]);
            // El repositorio devuelve false cuando el UPDATE condicional no afecta filas:
            // otra compra se quedo con la ultima unidad entre medio.
            _pedidoRepositoryMock.Setup(r => r.ConfirmarAsync(It.IsAny<Pedido>())).ReturnsAsync(false);

            var resultado = await _service.ConfirmarAsync(UserId, Clave);

            Assert.False(resultado.Exito);
            Assert.Contains("stock", resultado.Error, StringComparison.OrdinalIgnoreCase);
        }
    }
}
