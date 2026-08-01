using catalogo_web_mvc.Controllers;
using catalogo_web_mvc.Interfaces.Pedidos;
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
            => new(_pedidoServiceMock.Object) { ControllerContext = ContextoAutenticado(userId) };

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
    }
}
