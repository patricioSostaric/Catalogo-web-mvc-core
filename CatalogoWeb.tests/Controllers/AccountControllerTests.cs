using catalogo_web_mvc.Controllers;
using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Email;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using System.Security.Claims;

namespace CatalogoWeb.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<IAuditService> _auditMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
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
            _emailSenderMock = new Mock<IEmailSender>();

            _controller = new AccountController(_userManagerMock.Object, _signInManagerMock.Object, _auditMock.Object, _emailSenderMock.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                        new Claim(ClaimTypes.Email, "test@test.com")
                    }, "TestAuth"))
                }
            };

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns("https://localhost/Account/ResetPassword?email=user%40test.com&token=abc");
            _controller.Url = urlHelperMock.Object;
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

        // ── Register: mail de bienvenida ──────────────────────────────────────

        [Fact]
        public async Task Register_POST_Exitoso_EnviaMailDeBienvenida()
        {
            _userManagerMock
                .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _signInManagerMock
                .Setup(s => s.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string?>()))
                .Returns(Task.CompletedTask);

            var modelo = new RegisterViewModel { Email = "nuevo@test.com", Password = "Test123!", ConfirmPassword = "Test123!" };

            await _controller.Register(modelo);

            _emailSenderMock.Verify(e => e.SendEmailAsync("nuevo@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Register_POST_Exitoso_SiFallaEnvioDeMail_IgualRedirige()
        {
            _userManagerMock
                .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _signInManagerMock
                .Setup(s => s.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string?>()))
                .Returns(Task.CompletedTask);
            _emailSenderMock
                .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("SMTP caído"));

            var modelo = new RegisterViewModel { Email = "nuevo@test.com", Password = "Test123!", ConfirmPassword = "Test123!" };

            var resultado = await _controller.Register(modelo);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Index", redirect.ActionName);
        }

        // ── ForgotPassword ─────────────────────────────────────────────────────

        [Fact]
        public void ForgotPassword_GET_RetornaVista()
        {
            var resultado = _controller.ForgotPassword();

            Assert.IsType<ViewResult>(resultado);
        }

        [Fact]
        public async Task ForgotPassword_POST_EmailExistente_GeneraTokenYEnviaMail()
        {
            var usuario = new ApplicationUser { Id = "user-1", Email = "existe@test.com", UserName = "existe@test.com" };
            _userManagerMock
                .Setup(u => u.FindByEmailAsync("existe@test.com"))
                .ReturnsAsync(usuario);
            _userManagerMock
                .Setup(u => u.GeneratePasswordResetTokenAsync(usuario))
                .ReturnsAsync("token-123");

            var modelo = new ForgotPasswordViewModel { Email = "existe@test.com" };

            var resultado = await _controller.ForgotPassword(modelo);

            _emailSenderMock.Verify(e => e.SendEmailAsync("existe@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.IsType<ViewResult>(resultado);
        }

        [Fact]
        public async Task ForgotPassword_POST_EmailInexistente_NoReveleInformacionYNoEnviaMail()
        {
            _userManagerMock
                .Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            var modelo = new ForgotPasswordViewModel { Email = "noexiste@test.com" };

            var resultado = await _controller.ForgotPassword(modelo);

            _emailSenderMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.IsType<ViewResult>(resultado);
        }

        // ── ResetPassword ──────────────────────────────────────────────────────

        [Fact]
        public void ResetPassword_GET_SinParametros_RetornaBadRequest()
        {
            var resultado = _controller.ResetPassword();

            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public void ResetPassword_GET_ConParametros_RetornaVistaConModelo()
        {
            var resultado = _controller.ResetPassword(email: "user@test.com", token: "token-123");

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsType<ResetPasswordViewModel>(viewResult.Model);
            Assert.Equal("user@test.com", modelo.Email);
            Assert.Equal("token-123", modelo.Token);
        }

        [Fact]
        public async Task ResetPassword_POST_TokenValido_RestableceContrasenia()
        {
            var usuario = new ApplicationUser { Id = "user-1", Email = "user@test.com", UserName = "user@test.com" };
            _userManagerMock
                .Setup(u => u.FindByEmailAsync("user@test.com"))
                .ReturnsAsync(usuario);
            _userManagerMock
                .Setup(u => u.ResetPasswordAsync(usuario, "token-123", "NuevaPass123!"))
                .ReturnsAsync(IdentityResult.Success);

            var modelo = new ResetPasswordViewModel
            {
                Email = "user@test.com",
                Token = "token-123",
                Password = "NuevaPass123!",
                ConfirmPassword = "NuevaPass123!"
            };

            var resultado = await _controller.ResetPassword(modelo);

            Assert.IsType<ViewResult>(resultado);
        }

        [Fact]
        public async Task ResetPassword_POST_TokenInvalido_RetornaVistaConError()
        {
            var usuario = new ApplicationUser { Id = "user-1", Email = "user@test.com", UserName = "user@test.com" };
            _userManagerMock
                .Setup(u => u.FindByEmailAsync("user@test.com"))
                .ReturnsAsync(usuario);
            _userManagerMock
                .Setup(u => u.ResetPasswordAsync(usuario, "token-invalido", "NuevaPass123!"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Token inválido o expirado." }));

            var modelo = new ResetPasswordViewModel
            {
                Email = "user@test.com",
                Token = "token-invalido",
                Password = "NuevaPass123!",
                ConfirmPassword = "NuevaPass123!"
            };

            var resultado = await _controller.ResetPassword(modelo);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(modelo, viewResult.Model);
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
