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

            await CrearUsuario(userManager, "admin@catalogo.com", "Admin123!", "Admin");
            await CrearUsuario(userManager, "usuario@catalogo.com", "Usuario123!", "Usuario");
        }

        private static async Task CrearUsuario(UserManager<ApplicationUser> userManager, string email, string password, string rol)
        {
            if (await userManager.FindByEmailAsync(email) != null) return;

            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, rol);
        }
    }
}
