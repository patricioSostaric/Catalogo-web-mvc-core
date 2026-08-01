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

        public static async Task SeedAsync(IServiceProvider services, string? adminPassword = null)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            await SeedUsersAsync(userManager, roleManager, adminPassword);
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, string? adminPassword)
        {
            string[] roles = ["Admin", "Usuario"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var passwordAdmin = string.IsNullOrWhiteSpace(adminPassword)
                ? AdminPasswordPorDefecto
                : adminPassword;

            await CrearUsuario(userManager, "admin@catalogo.com", passwordAdmin, "Admin");
            // El usuario comun queda con contraseña conocida a proposito: es la cuenta que
            // el README ofrece para probar la demo.
            await CrearUsuario(userManager, "usuario@catalogo.com", "Usuario@1234", "Usuario");
        }

        private static async Task CrearUsuario(UserManager<ApplicationUser> userManager, string email, string password, string rol)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, rol);
                return;
            }

            // Desbloquear si está en lockout
            if (await userManager.IsLockedOutAsync(user))
                await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MinValue);

            // No se resetea la contraseña de un usuario existente: si el admin la cambió,
            // debe mantenerse. Reponerla al valor seed reabriría un acceso hardcodeado.

            // Asegurar que el rol está asignado
            if (!await userManager.IsInRoleAsync(user, rol))
                await userManager.AddToRoleAsync(user, rol);
        }
    }
}
