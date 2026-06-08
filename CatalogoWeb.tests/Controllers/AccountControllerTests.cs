using catalogo_web_mvc.Controllers;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;
using catalogo_web_mvc.Interfaces.Audit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CatalogoWeb.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<IAuditService> _auditMock;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,        
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                null, null, null, null);

            _auditMock = new Mock<IAuditService>();

            _controller = new AccountController(_userManagerMock.Object, _signInManagerMock.Object, _auditMock.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        // ── Login GET ──────────────────────────────────────────────────────────

        [Fact]
        public void Login_GET_RetornaVista()
        {
            var resultado = _controller.Login();

            Assert.IsType<ViewResult>(resultado);
        }

        // ── Login POST ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Login_POST_ModeloInvalido_RetornaVistaConMismoModelo()
        {
            _controller.ModelState.AddModelError("Email", "Requerido");
            var modelo = new LoginViewModel();

            var resultado = await _controller.Login(modelo);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal(modelo, viewResult.Model);
        }

        [Fact]
        public async Task Login_POST_CredencialesIncorrectas_RetornaVistaConError()
        {
            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var modelo = new LoginViewModel { Email = "x@x.com", Password = "incorrecta" };

            var resultado = await _controller.Login(modelo);

            Assert.IsType<ViewResult>(resultado);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Login_POST_Exitoso_RedirigePaginaDefault()
        {
            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var modelo = new LoginViewModel { Email = "admin@catalogo.com", Password = "Admin123!" };

            var resultado = await _controller.Login(modelo);

            var redirect = Assert.IsType<LocalRedirectResult>(resultado);
            Assert.Equal("/", redirect.Url);
        }

        [Fact]
        public async Task Login_POST_Exitoso_ConReturnUrl_RedirigePaginaIndicada()
        {
            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var modelo = new LoginViewModel { Email = "admin@catalogo.com", Password = "Admin123!" };

            var resultado = await _controller.Login(modelo, returnUrl: "/Articulo");

            var redirect = Assert.IsType<LocalRedirectResult>(resultado);
            Assert.Equal("/Articulo", redirect.Url);
        }

        // ── Register GET ───────────────────────────────────────────────────────

        [Fact]
        public void Register_GET_RetornaVista()
        {
            var resultado = _controller.Register();

            Assert.IsType<ViewResult>(resultado);
        }

        // ── Register POST ──────────────────────────────────────────────────────

        [Fact]
        public async Task Register_POST_ModeloInvalido_RetornaVistaConMismoModelo()
        {
            _controller.ModelState.AddModelError("Email", "Requerido");
            var modelo = new RegisterViewModel();

            var resultado = await _controller.Register(modelo);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal(modelo, viewResult.Model);
        }

        [Fact]
        public async Task Register_POST_Exitoso_RedirigePaginaInicio()
        {
            _userManagerMock
                .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _signInManagerMock
                .Setup(s => s.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string?>()))
                .Returns(Task.CompletedTask);

            var modelo = new RegisterViewModel { Email = "nuevo@test.com", Password = "Test123!", ConfirmPassword = "Test123!" };

            var resultado = await _controller.Register(modelo);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public async Task Register_POST_EmailDuplicado_RetornaVistaConError()
        {
            _userManagerMock
                .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "El email ya está en uso." }));

            var modelo = new RegisterViewModel { Email = "duplicado@test.com", Password = "Test123!", ConfirmPassword = "Test123!" };

            var resultado = await _controller.Register(modelo);

            Assert.IsType<ViewResult>(resultado);
            Assert.False(_controller.ModelState.IsValid);
        }

        // ── Logout ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Logout_POST_RedirigePaginaInicio()
        {
            _signInManagerMock
                .Setup(s => s.SignOutAsync())
                .Returns(Task.CompletedTask);

            var resultado = await _controller.Logout();

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }
    }
}
