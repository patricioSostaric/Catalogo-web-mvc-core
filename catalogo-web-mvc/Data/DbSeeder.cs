using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Identity;

namespace catalogo_web_mvc.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            await SeedUsersAsync(userManager, roleManager);
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roles = ["Admin", "Usuario"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            await CrearUsuario(userManager, "admin@catalogo.com", "Admin@1234", "Admin");
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

            // Resetear password si no coincide con el valor seed
            if (!await userManager.CheckPasswordAsync(user, password))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, token, password);
            }

            // Asegurar que el rol está asignado
            if (!await userManager.IsInRoleAsync(user, rol))
                await userManager.AddToRoleAsync(user, rol);
        }
    }
}
