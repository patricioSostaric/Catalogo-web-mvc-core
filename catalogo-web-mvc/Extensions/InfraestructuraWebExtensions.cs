using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Email;
using catalogo_web_mvc.Models.Settings;
using catalogo_web_mvc.Services.Avatar;
using catalogo_web_mvc.Services.Email;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace catalogo_web_mvc.Extensions
{
    public static class InfraestructuraWebExtensions
    {
        public static IServiceCollection AddPersistencia(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("CatalogoDB");

            // La base es serverless y se pausa sola: al despertar tarda cerca de un minuto
            // y rechaza las conexiones mientras tanto. Sin reintentos, la migración del
            // arranque falla, la aplicación muere y el contenedor reinicia en bucle sin
            // llegar a esperarla.
            services.AddDbContext<CatalogoContext>(options =>
                options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(
                    maxRetryCount: 6,
                    maxRetryDelay: TimeSpan.FromSeconds(20),
                    errorNumbersToAdd: null)));

            return services;
        }

        public static IServiceCollection AddProteccionDeAbuso(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddFixedWindowLimiter("auth", opt =>
                {
                    opt.PermitLimit = 10;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });
            });

            // Techo de tamaño para los multipart. El límite real de la imagen lo aplica
            // AvatarValidator (2 MB); este corta el request antes de bufferearlo entero.
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = AvatarValidator.MaxBytes;
            });

            return services;
        }

        /// <summary>
        /// Cabeceras que envía el proxy de la plataforma de hosting.
        /// </summary>
        /// <remarks>
        /// Detrás de un proxy (Azure App Service, Fly, Render) el TLS lo termina la
        /// plataforma y la app recibe HTTP plano. Sin estas cabeceras Kestrel cree que el
        /// request no es seguro: la cookie con SecurePolicy.Always nunca se emite y el login
        /// falla en silencio.
        /// </remarks>
        public static IServiceCollection AddCabecerasDeProxy(this IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                // La IP del proxy no se conoce de antemano en un contenedor, así que se
                // vacían las listas de confianza. Esto asume que la plataforma es el único
                // camino de entrada: si la app quedara accesible de forma directa,
                // cualquiera podría falsear el origen.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            return services;
        }

        public static IServiceCollection AddCorreo(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
            services.AddScoped<ISmtpClient, SmtpClientAdapter>();
            services.AddScoped<IEmailSender, SmtpEmailSender>();

            return services;
        }
    }
}
