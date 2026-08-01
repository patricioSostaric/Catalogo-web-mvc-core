using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Identity;

namespace catalogo_web_mvc.Data
{
    public static class DbSeeder
    {
        /// <summary>Contraseña del administrador cuando no se configura otra.</summary>
        /// <remarks>
        /// Solo sirve para que el proyecto arranque en local con un comando. En un entorno
        /// publicado hay que pasar <c>Seed:AdminPassword</c>: este valor esta en el codigo
        /// fuente de un repositorio publico, asi que no protege nada por si mismo.
        /// </remarks>
        private const string AdminPasswordPorDefecto = "Admin@1234";

        /// <summary>Credenciales del superadministrador cuando no se configuran otras.</summary>
        /// <remarks>
        /// Mismo criterio que <see cref="AdminPasswordPorDefecto"/>: sirven para levantar el
        /// proyecto en local, no protegen nada en un entorno publicado.
        /// </remarks>
        private const string SuperAdminEmailPorDefecto = "superadmin@catalogo.com";
        private const string SuperAdminPasswordPorDefecto = "Super@1234";

        public static async Task SeedAsync(
            IServiceProvider services,
            string? adminPassword = null,
            string? superAdminEmail = null,
            string? superAdminPassword = null)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            await SeedUsersAsync(userManager, roleManager, adminPassword, superAdminEmail, superAdminPassword);
        }

        private static async Task SeedUsersAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            string? adminPassword,
            string? superAdminEmail,
            string? superAdminPassword)
        {
            string[] roles = ["SuperAdmin", "Admin", "Usuario"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var passwordAdmin = string.IsNullOrWhiteSpace(adminPassword)
                ? AdminPasswordPorDefecto
                : adminPassword;

            var emailSuper = string.IsNullOrWhiteSpace(superAdminEmail)
                ? SuperAdminEmailPorDefecto
                : superAdminEmail;

            var passwordSuper = string.IsNullOrWhiteSpace(superAdminPassword)
                ? SuperAdminPasswordPorDefecto
                : superAdminPassword;

            // Lleva los dos roles: SuperAdmin habilita la auditoria, y Admin le mantiene el
            // ABM sin tener que duplicar el rol en cada [Authorize] de los controladores.
            await CrearUsuario(userManager, emailSuper, passwordSuper, "SuperAdmin", "Admin");

            // El admin comun administra el catalogo pero no ve la auditoria: ahi quedan
            // registradas las IP y los mails de quienes usan la demo, que son datos de
            // terceros y no tienen por que estar al alcance de una cuenta compartida.
            await CrearUsuario(userManager, "admin@catalogo.com", passwordAdmin, "Admin");

            // El usuario comun queda con contraseña conocida a proposito: es la cuenta que
            // el README ofrece para probar la demo.
            await CrearUsuario(userManager, "usuario@catalogo.com", "Usuario@1234", "Usuario");
        }

        private static async Task CrearUsuario(UserManager<ApplicationUser> userManager, string email, string password, params string[] roles)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    foreach (var rol in roles)
                        await userManager.AddToRoleAsync(user, rol);
                }
                return;
            }

            // Desbloquear si está en lockout
            if (await userManager.IsLockedOutAsync(user))
                await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MinValue);

            // No se resetea la contraseña de un usuario existente: si el admin la cambió,
            // debe mantenerse. Reponerla al valor seed reabriría un acceso hardcodeado.

            // Asegurar que los roles están asignados
            foreach (var rol in roles)
            {
                if (!await userManager.IsInRoleAsync(user, rol))
                    await userManager.AddToRoleAsync(user, rol);
            }
        }
    }
}
