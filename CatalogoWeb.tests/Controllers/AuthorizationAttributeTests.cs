using catalogo_web_mvc.Controllers;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace CatalogoWeb.Tests.Controllers
{
    public class AuthorizationAttributeTests
    {
        private static AuthorizeAttribute? ObtenerAtributo<T>() =>
            typeof(T).GetCustomAttribute<AuthorizeAttribute>();

        // ── Controladores protegidos ───────────────────────────────────────────

        [Fact]
        public void ArticuloController_TieneAuthorizeRolAdmin()
        {
            var atributo = ObtenerAtributo<ArticuloController>();
            Assert.NotNull(atributo);
            Assert.Equal("Admin", atributo.Roles);
        }

        [Fact]
        public void CategoriasController_TieneAuthorizeRolAdmin()
        {
            var atributo = ObtenerAtributo<CategoriasController>();
            Assert.NotNull(atributo);
            Assert.Equal("Admin", atributo.Roles);
        }

        [Fact]
        public void MarcasController_TieneAuthorizeRolAdmin()
        {
            var atributo = ObtenerAtributo<MarcasController>();
            Assert.NotNull(atributo);
            Assert.Equal("Admin", atributo.Roles);
        }

        [Fact]
        public void AuditLogController_TieneAuthorizeRolSuperAdmin()
        {
            var atributo = ObtenerAtributo<AuditLogController>();
            Assert.NotNull(atributo);
            Assert.Equal("SuperAdmin", atributo.Roles);
        }

        [Fact]
        public void AuditLogController_NoQuedaAlAlcanceDelRolAdmin()
        {
            // La auditoria expone IP y mails de terceros. El rol Admin puede estar en una
            // cuenta compartida para mostrar el ABM, asi que no debe alcanzar para verla.
            var roles = ObtenerAtributo<AuditLogController>()!.Roles!;

            Assert.DoesNotContain("Admin", roles.Split(',').Select(r => r.Trim()));
        }

        [Fact]
        public void CarritoController_ExigeSesionIniciada()
        {
            var atributo = ObtenerAtributo<CarritoController>();
            Assert.NotNull(atributo);
            Assert.True(string.IsNullOrEmpty(atributo.Roles));
        }

        [Fact]
        public void PedidosController_ExigeSesionIniciada()
        {
            var atributo = ObtenerAtributo<PedidosController>();
            Assert.NotNull(atributo);
            Assert.True(string.IsNullOrEmpty(atributo.Roles));
        }

        // ── Controladores públicos ─────────────────────────────────────────────

        [Fact]
        public void HomeController_NoTieneAtributoAuthorize()
        {
            var atributo = ObtenerAtributo<HomeController>();
            Assert.Null(atributo);
        }

        [Fact]
        public void AccountController_NoTieneAtributoAuthorize()
        {
            var atributo = ObtenerAtributo<AccountController>();
            Assert.Null(atributo);
        }
    }
}
