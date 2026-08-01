using catalogo_web_mvc.Controllers;
using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Pedidos;
using catalogo_web_mvc.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace CatalogoWeb.Tests.Controllers
{
    public class PedidosControllerTests
    {
        private readonly Mock<IPedidoService> _pedidoServiceMock = new();
        private readonly Mock<IAuditService> _auditMock = new();

        private static ControllerContext ContextoAutenticado(string userId = "user-1")
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        private PedidosController Controller(string userId = "user-1")
        {
            var controller = new PedidosController(_pedidoServiceMock.Object, _auditMock.Object)
            {
                ControllerContext = ContextoAutenticado(userId)
            };
            controller.TempData = new TempDataDictionary(controller.HttpContext, Mock.Of<ITempDataProvider>());
            return controller;
        }

        [Fact]
        public async Task Index_DevuelveLosPedidosDelUsuario()
        {
            var pedidos = new List<Pedido>
            {
                new() { Id = 1, UserId = "user-1", Total = 500m },
                new() { Id = 2, UserId = "user-1", Total = 120m }
            };
            _pedidoServiceMock.Setup(s => s.GetByUsuarioAsync("user-1")).ReturnsAsync(pedidos);

            var resultado = await Controller().Index();

            var vista = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsType<List<Pedido>>(vista.Model);
            Assert.Equal(2, modelo.Count);
        }

        [Fact]
        public async Task Index_ConsultaSoloPorElUsuarioLogueado()
        {
            _pedidoServiceMock.Setup(s => s.GetByUsuarioAsync(It.IsAny<string>())).ReturnsAsync([]);

            await Controller("user-42").Index();

            _pedidoServiceMock.Verify(s => s.GetByUsuarioAsync("user-42"), Times.Once);
            _pedidoServiceMock.Verify(s => s.GetByUsuarioAsync(It.IsNotIn("user-42")), Times.Never);
        }

        [Fact]
        public async Task Index_SinPedidos_DevuelveListaVacia()
        {
            _pedidoServiceMock.Setup(s => s.GetByUsuarioAsync("user-1")).ReturnsAsync([]);

            var resultado = await Controller().Index();

            var vista = Assert.IsType<ViewResult>(resultado);
            Assert.Empty(Assert.IsType<List<Pedido>>(vista.Model));
        }

        [Fact]
        public async Task Cancelar_ConExito_RegistraEnLaAuditoria()
        {
            _pedidoServiceMock.Setup(s => s.CancelarAsync("user-1", 7))
                .ReturnsAsync(ResultadoCambioEstado.Ok(7, catalogo_web_mvc.Models.EstadoPedido.Cancelado));

            await Controller().Cancelar(7);

            _auditMock.Verify(a => a.RegistrarAsync(
                "PEDIDO_CANCELADO", It.IsAny<string>(), "user-1", "Pedido #7"), Times.Once);
        }

        [Fact]
        public async Task Cancelar_SiFalla_NoRegistraEnLaAuditoria()
        {
            _pedidoServiceMock.Setup(s => s.CancelarAsync("user-1", 7))
                .ReturnsAsync(ResultadoCambioEstado.Falla("No se puede cancelar."));

            await Controller().Cancelar(7);

            _auditMock.Verify(a => a.RegistrarAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Cancelar_CancelaSoloPedidosDelUsuarioLogueado()
        {
            _pedidoServiceMock.Setup(s => s.CancelarAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(ResultadoCambioEstado.Falla("x"));

            await Controller("user-42").Cancelar(7);

            _pedidoServiceMock.Verify(s => s.CancelarAsync("user-42", 7), Times.Once);
        }
    }
}
