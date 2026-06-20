using catalogo_web_mvc.Data;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CatalogoWeb.Tests.Data
{
    public class DbSeederTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;

        public DbSeederTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null, null, null, null);

            _roleManagerMock
                .Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceProviderMock
                .Setup(s => s.GetService(typeof(UserManager<ApplicationUser>)))
                .Returns(_userManagerMock.Object);
            _serviceProviderMock
                .Setup(s => s.GetService(typeof(RoleManager<IdentityRole>)))
                .Returns(_roleManagerMock.Object);
        }

        [Fact]
        public async Task SeedAsync_UsuariosNoExisten_CreaAdminYUsuarioConRoles()
        {
            _userManagerMock
                .Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);
            _userManagerMock
                .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock
                .Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            await DbSeeder.SeedAsync(_serviceProviderMock.Object);

            _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Exactly(2));
            _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task SeedAsync_UsuarioExisteYBloqueado_DesbloqueLaCuenta()
        {
            var user = new ApplicationUser { Email = "admin@catalogo.com", UserName = "admin@catalogo.com" };

            _userManagerMock
                .Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _userManagerMock
                .Setup(u => u.IsLockedOutAsync(user))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(u => u.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock
                .Setup(u => u.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(u => u.IsInRoleAsync(user, It.IsAny<string>()))
                .ReturnsAsync(true);

            await DbSeeder.SeedAsync(_serviceProviderMock.Object);

            _userManagerMock.Verify(u => u.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset?>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SeedAsync_UsuarioExisteConPasswordIncorrecta_ResetearPassword()
        {
            var user = new ApplicationUser { Email = "admin@catalogo.com", UserName = "admin@catalogo.com" };
            const string token = "reset-token";

            _userManagerMock
                .Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _userManagerMock
                .Setup(u => u.IsLockedOutAsync(user))
                .ReturnsAsync(false);
            _userManagerMock
                .Setup(u => u.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(false);
            _userManagerMock
                .Setup(u => u.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync(token);
            _userManagerMock
                .Setup(u => u.ResetPasswordAsync(user, token, It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock
                .Setup(u => u.IsInRoleAsync(user, It.IsAny<string>()))
                .ReturnsAsync(true);

            await DbSeeder.SeedAsync(_serviceProviderMock.Object);

            _userManagerMock.Verify(u => u.ResetPasswordAsync(user, token, It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SeedAsync_UsuarioExisteSinRol_AsignaElRolCorrespondiente()
        {
            var user = new ApplicationUser { Email = "admin@catalogo.com", UserName = "admin@catalogo.com" };

            _userManagerMock
                .Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _userManagerMock
                .Setup(u => u.IsLockedOutAsync(user))
                .ReturnsAsync(false);
            _userManagerMock
                .Setup(u => u.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(u => u.IsInRoleAsync(user, It.IsAny<string>()))
                .ReturnsAsync(false);
            _userManagerMock
                .Setup(u => u.AddToRoleAsync(user, It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            await DbSeeder.SeedAsync(_serviceProviderMock.Object);

            _userManagerMock.Verify(u => u.AddToRoleAsync(user, It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SeedAsync_UsuarioExistePasswordCorrectaYConRol_NoHaceNadaExtra()
        {
            var user = new ApplicationUser { Email = "admin@catalogo.com", UserName = "admin@catalogo.com" };

            _userManagerMock
                .Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _userManagerMock
                .Setup(u => u.IsLockedOutAsync(user))
                .ReturnsAsync(false);
            _userManagerMock
                .Setup(u => u.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(u => u.IsInRoleAsync(user, It.IsAny<string>()))
                .ReturnsAsync(true);

            await DbSeeder.SeedAsync(_serviceProviderMock.Object);

            _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            _userManagerMock.Verify(u => u.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _userManagerMock.Verify(u => u.SetLockoutEndDateAsync(It.IsAny<ApplicationUser>(), It.IsAny<DateTimeOffset?>()), Times.Never);
        }
    }
}
