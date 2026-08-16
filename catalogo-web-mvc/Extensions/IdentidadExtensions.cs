using catalogo_web_mvc.Data;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Services.Identity;
using Microsoft.AspNetCore.Identity;

namespace catalogo_web_mvc.Extensions
{
    public static class IdentidadExtensions
    {
        /// <summary>
        /// Identity con roles, política de contraseñas, bloqueo por intentos fallidos y la
        /// configuración de la cookie de sesión.
        /// </summary>
        public static IServiceCollection AddIdentidad(
            this IServiceCollection services,
            IHostEnvironment environment)
        {
            services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;

                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<CatalogoContext>()
            .AddErrorDescriber<SpanishIdentityErrorDescriber>()
            // Suma el avatar como claim para que el navbar no consulte la base en cada request.
            .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;

                // En producción la cookie viaja solo por HTTPS. En desarrollo se usa
                // SameAsRequest porque el contenedor sirve HTTP plano: con Always el
                // navegador descarta la cookie y el login falla sin mostrar error.
                options.Cookie.SecurePolicy = environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;

                options.Cookie.SameSite = SameSiteMode.Strict;

                // Sin estas rutas Identity apunta a /Identity/Account/..., que no existe en
                // este proyecto: un usuario sin permisos recibía un 404 en lugar de una
                // explicación.
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";

                // Cuando este proceso aloja también los endpoints de la API (ver
                // AddApplicationPart), deja de correr el Program de Catalogo.Api, que es
                // donde vivían estas reglas. Sin ellas, una petición a /api sin sesión
                // recibiría una redirección al login: el fetch la seguiría, el navegador
                // devolvería el HTML del formulario con estado 200, y el front de React
                // intentaría leerlo como JSON. El error aparecería lejos de la causa.
                //
                // Solo se altera /api. Una vista Razor sin sesión sigue yendo al login, que
                // es lo que corresponde cuando del otro lado hay una persona y no un fetch.
                options.Events.OnRedirectToLogin = contexto =>
                    ResponderSegunElCliente(contexto, StatusCodes.Status401Unauthorized);

                options.Events.OnRedirectToAccessDenied = contexto =>
                    ResponderSegunElCliente(contexto, StatusCodes.Status403Forbidden);
            });

            return services;
        }

        private static Task ResponderSegunElCliente(
            Microsoft.AspNetCore.Authentication.RedirectContext<Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions> contexto,
            int estadoParaLaApi)
        {
            if (contexto.Request.Path.StartsWithSegments("/api"))
            {
                contexto.Response.StatusCode = estadoParaLaApi;
                return Task.CompletedTask;
            }

            contexto.Response.Redirect(contexto.RedirectUri);
            return Task.CompletedTask;
        }
    }
}
