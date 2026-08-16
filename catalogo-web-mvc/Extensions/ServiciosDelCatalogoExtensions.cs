using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Avatar;
using catalogo_web_mvc.Interfaces.Carrito;
using catalogo_web_mvc.Interfaces.Categorias;
using catalogo_web_mvc.Interfaces.Favoritos;
using catalogo_web_mvc.Interfaces.Marcas;
using catalogo_web_mvc.Interfaces.Pedidos;
using catalogo_web_mvc.Interfaces.Usuarios;
using catalogo_web_mvc.Repository.Articulos;
using catalogo_web_mvc.Repository.Carrito;
using catalogo_web_mvc.Repository.Categorias;
using catalogo_web_mvc.Repository.Favoritos;
using catalogo_web_mvc.Repository.Marcas;
using catalogo_web_mvc.Repository.Pedidos;
using catalogo_web_mvc.Services.Articulos;
using catalogo_web_mvc.Services.Audit;
using catalogo_web_mvc.Services.Avatar;
using catalogo_web_mvc.Services.Carrito;
using catalogo_web_mvc.Services.Categorias;
using catalogo_web_mvc.Services.Favoritos;
using catalogo_web_mvc.Services.Marcas;
using catalogo_web_mvc.Services.Pedidos;
using catalogo_web_mvc.Services.Usuarios;

namespace catalogo_web_mvc.Extensions
{
    public static class ServiciosDelCatalogoExtensions
    {
        /// <summary>
        /// Repositorios y servicios de negocio, un par por módulo.
        /// </summary>
        /// <remarks>
        /// Agrupados acá porque crecen juntos: cada módulo nuevo agrega dos líneas, y en el
        /// Program.cs quedaban mezcladas con la configuración de Identity, el rate limiter
        /// y las cabeceras de proxy, que no tienen nada que ver.
        /// </remarks>
        public static IServiceCollection AddServiciosDelCatalogo(this IServiceCollection services)
        {
            services.AddScoped<IArticuloRepository, ArticuloRepository>();
            services.AddScoped<IArticuloService, ArticuloService>();

            services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            services.AddScoped<ICategoriaService, CategoriaService>();

            services.AddScoped<IMarcaRepository, MarcaRepository>();
            services.AddScoped<IMarcaService, MarcaService>();

            services.AddScoped<ICarritoRepository, CarritoRepository>();
            services.AddScoped<ICarritoService, CarritoService>();

            services.AddScoped<IFavoritoRepository, FavoritoRepository>();
            services.AddScoped<IFavoritoService, FavoritoService>();

            services.AddScoped<IPedidoRepository, PedidoRepository>();
            services.AddScoped<IPedidoService, PedidoService>();

            services.AddScoped<IUsuarioAdminService, UsuarioAdminService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IAvatarService, AvatarService>();

            return services;
        }
    }
}
