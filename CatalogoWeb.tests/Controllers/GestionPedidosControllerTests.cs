using catalogo_web_mvc.Controllers;
using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Pedidos;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;
using X.PagedList;

namespace CatalogoWeb.Tests.Controllers
{
    public class GestionPedidosControllerTests
    {
        private readonly Mock<IPedidoService> _pedidoServiceMock = new();
        private readonly Mock<IAuditService> _auditMock = new();

        private GestionPedidosController Controller()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "admin-id"),
                new(ClaimTypes.Email, "admin@catalogo.com")
            };
            var controller = new GestionPedidosController(_pedidoServiceMock.Object, _auditMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                    }
                }
            };
            controller.TempData = new TempDataDictionary(controller.HttpContext, Mock.Of<ITempDataProvider>());
            return controller;
        }

        [Fact]
        public async Task Index_SinFiltro_TraeTodosLosPedidos()
        {
            _pedidoServiceMock.Setup(s => s.GetTodosAsync(null)).ReturnsAsync(
            [
                new Pedido { Id = 1 },
                new Pedido { Id = 2 }
            ]);

            var resultado = await Controller().Index(null, null);

            var vista = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<IPagedList<Pedido>>(vista.Model);
            Assert.Equal(2, modelo.Count);
        }

        [Fact]
        public async Task Index_ConFiltro_LoPasaAlServicio()
        {
            _pedidoServiceMock.Setup(s => s.GetTodosAsync(It.IsAny<EstadoPedido?>())).ReturnsAsync([]);

            await Controller().Index(EstadoPedido.Enviado, null);

            _pedidoServiceMock.Verify(s => s.GetTodosAsync(EstadoPedido.Enviado), Times.Once);
        }

        [Fact]
        public async Task Avanzar_ConExito_RegistraElCambioEnLaAuditoria()
        {
            _pedidoServiceMock.Setup(s => s.AvanzarAsync(7))
                .ReturnsAsync(ResultadoCambioEstado.Ok(7, EstadoPedido.Enviado));

            await Controller().Avanzar(7, null, null);

            _auditMock.Verify(a => a.RegistrarAsync(
                "PEDIDO_ESTADO_CAMBIADO", "admin@catalogo.com", "admin-id", "Pedido #7 → Enviado"), Times.Once);
        }

        [Fact]
        public async Task Avanzar_SiFalla_NoRegistraEnLaAuditoria()
        {
            _pedidoServiceMock.Setup(s => s.AvanzarAsync(7))
                .ReturnsAsync(ResultadoCambioEstado.Falla("El pedido cambió de estado."));

            await Controller().Avanzar(7, null, null);

            _auditMock.Verify(a => a.RegistrarAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Avanzar_ConservaElFiltroYLaPagina()
        {
            _pedidoServiceMock.Setup(s => s.AvanzarAsync(7))
                .ReturnsAsync(ResultadoCambioEstado.Ok(7, EstadoPedido.Entregado));

            var resultado = await Controller().Avanzar(7, EstadoPedido.Enviado, 2);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal(EstadoPedido.Enviado, redirect.RouteValues!["estado"]);
            Assert.Equal(2, redirect.RouteValues["page"]);
        }
    }
}
