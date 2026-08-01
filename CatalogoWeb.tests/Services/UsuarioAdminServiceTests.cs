using catalogo_web_mvc.Models;
using catalogo_web_mvc.Services.Usuarios;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CatalogoWeb.Tests.Services
{
    public class UsuarioAdminServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly UsuarioAdminService _service;

        public UsuarioAdminServiceTests()
        {
            var storeMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                storeMock.Object, null, null, null, null, null, null, null, null);

            _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync([]);

            _service = new UsuarioAdminService(_userManagerMock.Object);
        }

        private void ConUsuarios(params ApplicationUser[] usuarios)
            => _userManagerMock.Setup(u => u.Users).Returns(usuarios.AsQueryable());

        private static ApplicationUser Usuario(
            string id, string email, DateTimeOffset? lockoutEnd = null, int fallidos = 0)
            => new()
            {
                Id = id,
                Email = email,
                UserName = email,
                LockoutEnd = lockoutEnd,
                AccessFailedCount = fallidos
            };

        [Fact]
        public async Task ListarAsync_DevuelveTodosLosUsuarios()
        {
            ConUsuarios(Usuario("1", "ana@ejemplo.com"), Usuario("2", "beto@ejemplo.com"));

            var filas = await _service.ListarAsync();

            Assert.Equal(2, filas.Count);
        }

        [Fact]
        public async Task ListarAsync_ConFiltro_DevuelveSoloLosQueCoinciden()
        {
            ConUsuarios(Usuario("1", "ana@ejemplo.com"), Usuario("2", "beto@otro.com"));

            var filas = await _service.ListarAsync("ejemplo");

            Assert.Equal("ana@ejemplo.com", Assert.Single(filas).Email);
        }

        [Fact]
        public async Task ListarAsync_ElFiltroIgnoraMayusculas()
        {
            ConUsuarios(Usuario("1", "Ana@Ejemplo.com"));

            var filas = await _service.ListarAsync("ANA");

            Assert.Single(filas);
        }

        [Fact]
        public async Task ListarAsync_PoneLosBloqueadosPrimero()
        {
            ConUsuarios(
                Usuario("1", "activo@ejemplo.com"),
                Usuario("2", "bloqueado@ejemplo.com", DateTimeOffset.UtcNow.AddMinutes(5)));

            var filas = await _service.ListarAsync();

            Assert.Equal("bloqueado@ejemplo.com", filas[0].Email);
        }

        [Fact]
        public async Task ListarAsync_TraeLosRolesDeCadaUsuario()
        {
            var usuario = Usuario("1", "jefe@ejemplo.com");
            ConUsuarios(usuario);
            _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(["SuperAdmin", "Admin"]);

            var filas = await _service.ListarAsync();

            Assert.Equal(["SuperAdmin", "Admin"], filas[0].Roles);
        }

        [Fact]
        public async Task ListarAsync_UnLockoutVencidoNoCuentaComoBloqueo()
        {
            // Identity deja la fecha cargada aunque ya haya pasado: preguntar solo si tiene
            // valor marcaria como bloqueada una cuenta que ya se libero sola.
            ConUsuarios(Usuario("1", "ana@ejemplo.com", DateTimeOffset.UtcNow.AddMinutes(-1)));

            var filas = await _service.ListarAsync();

            Assert.False(filas[0].EstaBloqueado);
        }

        [Fact]
        public async Task ListarAsync_UnLockoutVigenteCuentaComoBloqueo()
        {
            ConUsuarios(Usuario("1", "ana@ejemplo.com", DateTimeOffset.UtcNow.AddMinutes(5)));

            var filas = await _service.ListarAsync();

            Assert.True(filas[0].EstaBloqueado);
        }

        [Fact]
        public async Task DesbloquearAsync_UsuarioInexistente_Falla()
        {
            _userManagerMock.Setup(u => u.FindByIdAsync("999")).ReturnsAsync((ApplicationUser?)null);

            var resultado = await _service.DesbloquearAsync("999");

            Assert.False(resultado.Exito);
            _userManagerMock.Verify(u => u.SetLockoutEndDateAsync(
                It.IsAny<ApplicationUser>(), It.IsAny<DateTimeOffset?>()), Times.Never);
        }

        [Fact]
        public async Task DesbloquearAsync_LevantaElBloqueoYReiniciaElContador()
        {
            var usuario = Usuario("1", "ana@ejemplo.com", DateTimeOffset.UtcNow.AddMinutes(5), fallidos: 5);
            _userManagerMock.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(usuario);
            _userManagerMock.Setup(u => u.SetLockoutEndDateAsync(usuario, null))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.ResetAccessFailedCountAsync(usuario))
                .ReturnsAsync(IdentityResult.Success);

            var resultado = await _service.DesbloquearAsync("1");

            Assert.True(resultado.Exito);
            Assert.Equal("ana@ejemplo.com", resultado.EmailAfectado);
            _userManagerMock.Verify(u => u.SetLockoutEndDateAsync(usuario, null), Times.Once);
            // Sin reiniciar el contador, el proximo intento fallido volveria a bloquear.
            _userManagerMock.Verify(u => u.ResetAccessFailedCountAsync(usuario), Times.Once);
        }

        [Fact]
        public async Task DesbloquearAsync_SiFallaElLockout_NoReiniciaElContador()
        {
            var usuario = Usuario("1", "ana@ejemplo.com", DateTimeOffset.UtcNow.AddMinutes(5));
            _userManagerMock.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(usuario);
            _userManagerMock.Setup(u => u.SetLockoutEndDateAsync(usuario, null))
                .ReturnsAsync(IdentityResult.Failed());

            var resultado = await _service.DesbloquearAsync("1");

            Assert.False(resultado.Exito);
            _userManagerMock.Verify(u => u.ResetAccessFailedCountAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }
    }
}
