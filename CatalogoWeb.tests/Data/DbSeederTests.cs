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

            // Tres cuentas: superadmin, admin y usuario comun.
            _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Exactly(3));
            // Cuatro asignaciones: el superadmin lleva SuperAdmin y Admin, los otros una cada uno.
            _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Exactly(4));
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
        public async Task SeedAsync_UsuarioExisteConPasswordIncorrecta_NoResetearPassword()
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
                .ReturnsAsync(false);
            _userManagerMock
                .Setup(u => u.IsInRoleAsync(user, It.IsAny<string>()))
                .ReturnsAsync(true);

            await DbSeeder.SeedAsync(_serviceProviderMock.Object);

            _userManagerMock.Verify(u => u.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _userManagerMock.Verify(u => u.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
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

        [Fact]
        public async Task SeedAsync_CreaElRolSuperAdmin()
        {
            _roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            await DbSeeder.SeedAsync(_serviceProviderMock.Object);

            _roleManagerMock.Verify(r => r.CreateAsync(It.Is<IdentityRole>(x => x.Name == "SuperAdmin")), Times.Once);
        }

        [Fact]
        public async Task SeedAsync_ElSuperAdminLlevaLosDosRoles()
        {
            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            await DbSeeder.SeedAsync(_serviceProviderMock.Object, superAdminEmail: "jefe@ejemplo.com");

            // SuperAdmin habilita la auditoria; Admin le mantiene el ABM.
            _userManagerMock.Verify(u => u.AddToRoleAsync(
                It.Is<ApplicationUser>(x => x.Email == "jefe@ejemplo.com"), "SuperAdmin"), Times.Once);
            _userManagerMock.Verify(u => u.AddToRoleAsync(
                It.Is<ApplicationUser>(x => x.Email == "jefe@ejemplo.com"), "Admin"), Times.Once);
        }

        [Fact]
        public async Task SeedAsync_ElAdminComunNoRecibeElRolSuperAdmin()
        {
            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            await DbSeeder.SeedAsync(_serviceProviderMock.Object);

            _userManagerMock.Verify(u => u.AddToRoleAsync(
                It.Is<ApplicationUser>(x => x.Email == "admin@catalogo.com"), "SuperAdmin"), Times.Never);
        }

        [Fact]
        public async Task SeedAsync_ConEmailYPasswordDeSuperAdmin_LosUsa()
        {
            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            await DbSeeder.SeedAsync(_serviceProviderMock.Object,
                superAdminEmail: "jefe@ejemplo.com", superAdminPassword: "ClaveFuerte#99");

            _userManagerMock.Verify(u => u.CreateAsync(
                It.Is<ApplicationUser>(x => x.Email == "jefe@ejemplo.com"), "ClaveFuerte#99"), Times.Once);
        }

        [Fact]
        public async Task SeedAsync_SinConfigurarSuperAdmin_CaeAlValorPorDefecto()
        {
            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            await DbSeeder.SeedAsync(_serviceProviderMock.Object);

            _userManagerMock.Verify(u => u.CreateAsync(
                It.Is<ApplicationUser>(x => x.Email == "superadmin@catalogo.com"), "Super@1234"), Times.Once);
        }

        [Fact]
        public async Task SeedAsync_ConPasswordConfigurada_LaUsaParaElAdmin()
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

            await DbSeeder.SeedAsync(_serviceProviderMock.Object, "OtraClave#2024");

            _userManagerMock.Verify(u => u.CreateAsync(
                It.Is<ApplicationUser>(x => x.Email == "admin@catalogo.com"), "OtraClave#2024"), Times.Once);
            _userManagerMock.Verify(u => u.CreateAsync(
                It.Is<ApplicationUser>(x => x.Email == "admin@catalogo.com"), "Admin@1234"), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SeedAsync_SinPasswordConfigurada_CaeAlValorPorDefecto(string? password)
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

            await DbSeeder.SeedAsync(_serviceProviderMock.Object, password);

            _userManagerMock.Verify(u => u.CreateAsync(
                It.Is<ApplicationUser>(x => x.Email == "admin@catalogo.com"), "Admin@1234"), Times.Once);
        }

        [Fact]
        public async Task SeedAsync_PasswordConfigurada_NoAfectaAlUsuarioComun()
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

            await DbSeeder.SeedAsync(_serviceProviderMock.Object, "OtraClave#2024");

            _userManagerMock.Verify(u => u.CreateAsync(
                It.Is<ApplicationUser>(x => x.Email == "usuario@catalogo.com"), "Usuario@1234"), Times.Once);
        }
    }
}
