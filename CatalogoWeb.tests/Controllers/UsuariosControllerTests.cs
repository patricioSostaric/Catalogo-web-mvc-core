using catalogo_web_mvc.Controllers;
using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Usuarios;
using catalogo_web_mvc.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;
using X.PagedList;

namespace CatalogoWeb.Tests.Controllers
{
    public class UsuariosControllerTests
    {
        private readonly Mock<IUsuarioAdminService> _servicioMock = new();
        private readonly Mock<IAuditService> _auditMock = new();

        private UsuariosController Controller()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "super-id"),
                new(ClaimTypes.Email, "jefe@ejemplo.com")
            };
            var controller = new UsuariosController(_servicioMock.Object, _auditMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                    }
                }
            };
            controller.TempData = new TempDataDictionary(
                controller.HttpContext, Mock.Of<ITempDataProvider>());
            return controller;
        }

        [Fact]
        public async Task Index_DevuelveLosUsuariosPaginados()
        {
            _servicioMock.Setup(s => s.ListarAsync(null)).ReturnsAsync(
            [
                new UsuarioAdminViewModel { Id = "1", Email = "ana@ejemplo.com" },
                new UsuarioAdminViewModel { Id = "2", Email = "beto@ejemplo.com" }
            ]);

            var resultado = await Controller().Index(null, null);

            var vista = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<IPagedList<UsuarioAdminViewModel>>(vista.Model);
            Assert.Equal(2, modelo.Count);
        }

        [Fact]
        public async Task Index_PasaElFiltroAlServicio()
        {
            _servicioMock.Setup(s => s.ListarAsync(It.IsAny<string>())).ReturnsAsync([]);

            await Controller().Index("ana", null);

            _servicioMock.Verify(s => s.ListarAsync("ana"), Times.Once);
        }

        [Fact]
        public async Task Desbloquear_ConExito_RegistraEnLaAuditoria()
        {
            _servicioMock.Setup(s => s.DesbloquearAsync("1"))
                .ReturnsAsync(ResultadoDesbloqueo.Ok("ana@ejemplo.com"));

            await Controller().Desbloquear("1", null, null);

            _auditMock.Verify(a => a.RegistrarAsync(
                "USUARIO_DESBLOQUEADO", "jefe@ejemplo.com", "super-id", "ana@ejemplo.com"), Times.Once);
        }

        [Fact]
        public async Task Desbloquear_ConExito_VuelveAlListado()
        {
            _servicioMock.Setup(s => s.DesbloquearAsync("1"))
                .ReturnsAsync(ResultadoDesbloqueo.Ok("ana@ejemplo.com"));

            var resultado = await Controller().Desbloquear("1", null, null);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Desbloquear_SiFalla_NoRegistraEnLaAuditoria()
        {
            _servicioMock.Setup(s => s.DesbloquearAsync("999"))
                .ReturnsAsync(ResultadoDesbloqueo.Falla("El usuario no existe."));

            await Controller().Desbloquear("999", null, null);

            _auditMock.Verify(a => a.RegistrarAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Desbloquear_ConservaElFiltroYLaPagina()
        {
            _servicioMock.Setup(s => s.DesbloquearAsync("1"))
                .ReturnsAsync(ResultadoDesbloqueo.Ok("ana@ejemplo.com"));

            var resultado = await Controller().Desbloquear("1", "ana", 3);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("ana", redirect.RouteValues!["email"]);
            Assert.Equal(3, redirect.RouteValues["page"]);
        }
    }
}
