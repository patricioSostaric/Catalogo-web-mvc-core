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
