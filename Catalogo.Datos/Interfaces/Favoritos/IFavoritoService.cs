using catalogo_web_mvc.Models;

namespace catalogo_web_mvc.Interfaces.Favoritos
{
    public interface IFavoritoService
    {
        /// <summary>
        /// Devuelve las entidades con el articulo cargado. No devuelve ViewModels ni DTOs
        /// a proposito: de este servicio dependen el MVC y la API, y cada uno arma la
        /// forma que necesita. Si devolviera un ViewModel, la API cargaria con un tipo
        /// pensado para una vista Razor.
        /// </summary>
        Task<List<ArticuloFavorito>> ListarAsync(string userId);

        Task<HashSet<int>> IdsDeUsuarioAsync(string userId);
        Task<bool> EsFavoritoAsync(string userId, int articuloId);

        /// <summary>Agregar dos veces el mismo articulo no es un error: la segunda no hace nada.</summary>
        Task AgregarAsync(string userId, int articuloId);

        Task QuitarAsync(string userId, int articuloId);
    }
}
