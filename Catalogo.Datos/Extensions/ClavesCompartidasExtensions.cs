using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace catalogo_web_mvc.Extensions
{
    public static class ClavesCompartidasExtensions
    {
        // El nombre forma parte del proposito con el que se derivan las claves: con nombres
        // distintos el descifrado falla aunque las dos aplicaciones compartan el archivo.
        // Por eso es una constante y no un parametro: que dependa de quien llame seria
        // volver a abrir la puerta que este metodo existe para cerrar.
        private const string NombreDeAplicacion = "StoreSostaric";

        /// <summary>
        /// Almacen de claves de Data Protection compartido entre el MVC y la API.
        /// </summary>
        /// <remarks>
        /// La cookie de sesion la emite el MVC y la valida tambien Catalogo.Api, que corre
        /// en otro proceso. La cookie no dice quien es el usuario: es un texto cifrado. Sin
        /// un almacen comun cada aplicacion generaria el suyo y la API veria basura.
        ///
        /// Vive en la biblioteca compartida a proposito. Estaba copiado en los dos
        /// Program.cs, y con una configuracion que tiene que coincidir exactamente, dos
        /// copias son dos oportunidades de que dejen de coincidir. El dia que cambie,
        /// cambia en un solo lugar.
        /// </remarks>
        public static IServiceCollection AddClavesCompartidas(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            var carpeta = configuration["DataProtection:RutaClaves"]
                ?? Path.Combine(environment.ContentRootPath, "..", "claves-compartidas");

            Directory.CreateDirectory(carpeta);

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(carpeta))
                .SetApplicationName(NombreDeAplicacion);

            return services;
        }
    }
}
