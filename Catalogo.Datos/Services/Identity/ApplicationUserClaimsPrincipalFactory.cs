using catalogo_web_mvc.Models;
using catalogo_web_mvc.Services.Avatar;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace catalogo_web_mvc.Services.Identity
{
    /// <summary>
    /// Agrega el avatar y el nombre para mostrar como claims de la cookie de sesión.
    ///
    /// El motivo es evitar una consulta a la base en cada request solo para pintar la
    /// imagen del navbar, que se renderiza en todas las páginas. La contra es que los
    /// claims quedan congelados hasta el próximo login, así que el controlador llama a
    /// RefreshSignInAsync cuando el perfil cambia.
    /// </summary>
    public class ApplicationUserClaimsPrincipalFactory
        : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public const string ClaimAvatar = "avatar_url";
        public const string ClaimNombreParaMostrar = "nombre_mostrar";

        public ApplicationUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Se guarda ya resuelto: si el valor de la base no es válido, en la cookie
            // queda directamente la ruta del avatar por defecto.
            identity.AddClaim(new Claim(ClaimAvatar, AvatarUrlValidator.ResolverParaMostrar(user.AvatarUrl)));
            identity.AddClaim(new Claim(ClaimNombreParaMostrar, user.NombreParaMostrar));

            return identity;
        }
    }
}
