using catalogo_web_mvc.Controllers;
using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Avatar;
using catalogo_web_mvc.Interfaces.Email;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;

namespace CatalogoWeb.Tests.Controllers
{
    /// <summary>
    /// Tests del perfil y de la subida de avatar en el registro. Van aparte de
    /// AccountControllerTests para no mezclar los flujos de autenticación con los de perfil.
    /// </summary>
    public class AccountControllerPerfilTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<IAuditService> _auditMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly Mock<IAvatarService> _avatarMock;
        private readonly AccountController _controller;

        public AccountControllerPerfilTests()
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
            _avatarMock = new Mock<IAvatarService>();

            _controller = new AccountController(
                _userManagerMock.Object, _signInManagerMock.Object,
                _auditMock.Object, _emailSenderMock.Object, _avatarMock.Object);

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

            _controller.TempData = new TempDataDictionary(
                _controller.HttpContext, new Mock<ITempDataProvider>().Object);
        }

        private static ApplicationUser UsuarioDePrueba() => new()
        {
            Id = "test-user-id",
            Email = "test@test.com",
            UserName = "test@test.com",
            Nombre = "Patricio",
            Apellido = "Sostaric",
            FechaNacimiento = new DateOnly(1990, 5, 10),
            Localidad = "La Plata",
            CodigoPostal = "1900",
            AvatarUrl = "/uploads/avatars/viejo.jpg"
        };

        private void ConfigurarUsuarioActual(ApplicationUser? user) =>
            _userManagerMock
                .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

        private static IFormFile ArchivoFalso(string nombre = "foto.png")
        {
            var mock = new Mock<IFormFile>();
            mock.Setup(f => f.FileName).Returns(nombre);
            mock.Setup(f => f.Length).Returns(1024);
            return mock.Object;
        }

        // ── Perfil GET ────────────────────────────────────────────────────────

        [Fact]
        public async Task Perfil_GET_CargaLosDatosDelUsuario()
        {
            ConfigurarUsuarioActual(UsuarioDePrueba());

            var resultado = await _controller.Perfil();

            var vista = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsType<PerfilViewModel>(vista.Model);
            Assert.Equal("test@test.com", modelo.Email);
            Assert.Equal("Patricio", modelo.Datos.Nombre);
            Assert.Equal("La Plata", modelo.Datos.Localidad);
            Assert.Equal("/uploads/avatars/viejo.jpg", modelo.Datos.AvatarActual);
        }

        [Fact]
        public async Task Perfil_GET_CalculaLaEdadDesdeLaFecha()
        {
            var user = UsuarioDePrueba();
            ConfigurarUsuarioActual(user);

            var resultado = await _controller.Perfil();

            var modelo = Assert.IsType<PerfilViewModel>(Assert.IsType<ViewResult>(resultado).Model);
            Assert.Equal(user.Edad, modelo.Edad);
        }

        [Fact]
        public async Task Perfil_GET_SinUsuarioEnSesion_DevuelveChallenge()
        {
            ConfigurarUsuarioActual(null);

            var resultado = await _controller.Perfil();

            Assert.IsType<ChallengeResult>(resultado);
        }

        // ── Perfil POST ───────────────────────────────────────────────────────

        [Fact]
        public async Task Perfil_POST_DatosValidos_ActualizaYRedirige()
        {
            var user = UsuarioDePrueba();
            ConfigurarUsuarioActual(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var modelo = new PerfilViewModel
            {
                Datos = new PerfilCamposViewModel
                {
                    Nombre = "Nuevo",
                    Apellido = "Apellido",
                    Localidad = "Berisso",
                    CodigoPostal = "1923"
                }
            };

            var resultado = await _controller.Perfil(modelo);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal(nameof(AccountController.Perfil), redirect.ActionName);
            Assert.Equal("Nuevo", user.Nombre);
            Assert.Equal("Berisso", user.Localidad);
        }

        [Fact]
        public async Task Perfil_POST_ModeloInvalido_NoGuardaYVuelveALaVista()
        {
            var user = UsuarioDePrueba();
            ConfigurarUsuarioActual(user);
            _controller.ModelState.AddModelError("Datos.Nombre", "requerido");

            var resultado = await _controller.Perfil(new PerfilViewModel());

            Assert.IsType<ViewResult>(resultado);
            _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task Perfil_POST_ModeloInvalido_RepoblaEmailYAvatarActual()
        {
            var user = UsuarioDePrueba();
            ConfigurarUsuarioActual(user);
            _controller.ModelState.AddModelError("Datos.Nombre", "requerido");

            var resultado = await _controller.Perfil(new PerfilViewModel());

            var modelo = Assert.IsType<PerfilViewModel>(Assert.IsType<ViewResult>(resultado).Model);
            Assert.Equal("test@test.com", modelo.Email);
            Assert.Equal("/uploads/avatars/viejo.jpg", modelo.Datos.AvatarActual);
        }

        [Fact]
        public async Task Perfil_POST_SinUsuarioEnSesion_DevuelveChallenge()
        {
            ConfigurarUsuarioActual(null);

            var resultado = await _controller.Perfil(new PerfilViewModel());

            Assert.IsType<ChallengeResult>(resultado);
        }

        // ── Perfil POST: avatar ───────────────────────────────────────────────

        [Fact]
        public async Task Perfil_POST_AvatarValido_GuardaYBorraElAnterior()
        {
            var user = UsuarioDePrueba();
            var anterior = user.AvatarUrl;
            ConfigurarUsuarioActual(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _avatarMock
                .Setup(a => a.GuardarAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AvatarGuardadoResult.Ok("/uploads/avatars/nuevo.png"));

            var modelo = new PerfilViewModel
            {
                Datos = new PerfilCamposViewModel { Nombre = "Patricio", Apellido = "S", Avatar = ArchivoFalso() }
            };

            await _controller.Perfil(modelo);

            Assert.Equal("/uploads/avatars/nuevo.png", user.AvatarUrl);
            _avatarMock.Verify(a => a.Eliminar(anterior), Times.Once);
        }

        [Fact]
        public async Task Perfil_POST_AvatarInvalido_NoActualizaYMuestraElError()
        {
            var user = UsuarioDePrueba();
            ConfigurarUsuarioActual(user);
            _avatarMock
                .Setup(a => a.GuardarAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AvatarGuardadoResult.Falla("Formato no permitido."));

            var modelo = new PerfilViewModel
            {
                Datos = new PerfilCamposViewModel { Nombre = "Patricio", Apellido = "S", Avatar = ArchivoFalso("x.exe") }
            };

            var resultado = await _controller.Perfil(modelo);

            Assert.IsType<ViewResult>(resultado);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal("/uploads/avatars/viejo.jpg", user.AvatarUrl);
            _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task Perfil_POST_QuitarAvatar_DejaElCampoEnNull()
        {
            var user = UsuarioDePrueba();
            ConfigurarUsuarioActual(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var modelo = new PerfilViewModel
            {
                Datos = new PerfilCamposViewModel { Nombre = "Patricio", Apellido = "S", QuitarAvatar = true }
            };

            await _controller.Perfil(modelo);

            Assert.Null(user.AvatarUrl);
        }

        [Fact]
        public async Task Perfil_POST_SinArchivoNiQuitar_ConservaElAvatar()
        {
            var user = UsuarioDePrueba();
            ConfigurarUsuarioActual(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var modelo = new PerfilViewModel
            {
                Datos = new PerfilCamposViewModel { Nombre = "Patricio", Apellido = "S" }
            };

            await _controller.Perfil(modelo);

            Assert.Equal("/uploads/avatars/viejo.jpg", user.AvatarUrl);
            _avatarMock.Verify(a => a.Eliminar(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Perfil_POST_SiFallaElUpdate_DescartaLaImagenNueva()
        {
            var user = UsuarioDePrueba();
            ConfigurarUsuarioActual(user);
            _userManagerMock
                .Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "error de base" }));
            _avatarMock
                .Setup(a => a.GuardarAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AvatarGuardadoResult.Ok("/uploads/avatars/nuevo.png"));

            var modelo = new PerfilViewModel
            {
                Datos = new PerfilCamposViewModel { Nombre = "Patricio", Apellido = "S", Avatar = ArchivoFalso() }
            };

            var resultado = await _controller.Perfil(modelo);

            Assert.IsType<ViewResult>(resultado);
            _avatarMock.Verify(a => a.Eliminar("/uploads/avatars/nuevo.png"), Times.Once);
        }

        [Fact]
        public async Task Perfil_POST_Exitoso_RefrescaLaSesionParaActualizarLosClaims()
        {
            var user = UsuarioDePrueba();
            ConfigurarUsuarioActual(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var modelo = new PerfilViewModel
            {
                Datos = new PerfilCamposViewModel { Nombre = "Patricio", Apellido = "S" }
            };

            await _controller.Perfil(modelo);

            _signInManagerMock.Verify(s => s.RefreshSignInAsync(user), Times.Once);
        }

        [Fact]
        public async Task Perfil_POST_Exitoso_QuedaAuditado()
        {
            var user = UsuarioDePrueba();
            ConfigurarUsuarioActual(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var modelo = new PerfilViewModel
            {
                Datos = new PerfilCamposViewModel { Nombre = "Patricio", Apellido = "S" }
            };

            await _controller.Perfil(modelo);

            _auditMock.Verify(a => a.RegistrarAsync(
                "PERFIL_ACTUALIZADO", user.Email, user.Id, It.IsAny<string>()), Times.Once);
        }

        // ── Registro con avatar ───────────────────────────────────────────────

        [Fact]
        public async Task Register_POST_AvatarInvalido_NoCreaElUsuario()
        {
            _avatarMock
                .Setup(a => a.GuardarAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AvatarGuardadoResult.Falla("Formato no permitido."));

            var modelo = new RegisterViewModel
            {
                Email = "nuevo@test.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                Datos = new PerfilCamposViewModel { Nombre = "Patricio", Apellido = "S", Avatar = ArchivoFalso("x.exe") }
            };

            var resultado = await _controller.Register(modelo);

            Assert.IsType<ViewResult>(resultado);
            Assert.False(_controller.ModelState.IsValid);
            _userManagerMock.Verify(
                m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Register_POST_Exitoso_PersisteLosDatosDePerfilYElAvatar()
        {
            ApplicationUser? creado = null;
            _avatarMock
                .Setup(a => a.GuardarAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AvatarGuardadoResult.Ok("/uploads/avatars/nuevo.png"));
            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Callback<ApplicationUser, string>((u, _) => creado = u)
                .ReturnsAsync(IdentityResult.Success);

            var modelo = new RegisterViewModel
            {
                Email = "nuevo@test.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                Datos = new PerfilCamposViewModel
                {
                    Nombre = "Patricio",
                    Apellido = "Sostaric",
                    FechaNacimiento = new DateOnly(1990, 5, 10),
                    Localidad = "La Plata",
                    CodigoPostal = "B1900ABC",
                    Avatar = ArchivoFalso()
                }
            };

            await _controller.Register(modelo);

            Assert.NotNull(creado);
            Assert.Equal("Patricio", creado.Nombre);
            Assert.Equal("Sostaric", creado.Apellido);
            Assert.Equal(new DateOnly(1990, 5, 10), creado.FechaNacimiento);
            Assert.Equal("La Plata", creado.Localidad);
            Assert.Equal("B1900ABC", creado.CodigoPostal);
            Assert.Equal("/uploads/avatars/nuevo.png", creado.AvatarUrl);
        }

        [Fact]
        public async Task Register_POST_SinAvatar_NoLlamaAlServicioDeImagenes()
        {
            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var modelo = new RegisterViewModel
            {
                Email = "nuevo@test.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                Datos = new PerfilCamposViewModel { Nombre = "Patricio", Apellido = "S" }
            };

            await _controller.Register(modelo);

            _avatarMock.Verify(
                a => a.GuardarAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Register_POST_SiFallaLaCreacion_BorraElAvatarHuerfano()
        {
            _avatarMock
                .Setup(a => a.GuardarAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AvatarGuardadoResult.Ok("/uploads/avatars/huerfano.png"));
            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateEmail",
                    Description = "ya existe"
                }));

            var modelo = new RegisterViewModel
            {
                Email = "duplicado@test.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                Datos = new PerfilCamposViewModel { Nombre = "Patricio", Apellido = "S", Avatar = ArchivoFalso() }
            };

            await _controller.Register(modelo);

            _avatarMock.Verify(a => a.Eliminar("/uploads/avatars/huerfano.png"), Times.Once);
        }
    }
}
