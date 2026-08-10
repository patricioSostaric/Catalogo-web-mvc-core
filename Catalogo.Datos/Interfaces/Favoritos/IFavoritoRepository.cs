using catalogo_web_mvc.Models;

namespace catalogo_web_mvc.Interfaces.Favoritos
{
    public interface IFavoritoRepository
    {
        Task<List<ArticuloFavorito>> GetByUsuarioAsync(string userId);
        Task<HashSet<int>> GetIdsArticulosAsync(string userId);
        Task<bool> ExisteAsync(string userId, int articuloId);
        Task AddAsync(ArticuloFavorito favorito);
        Task RemoveAsync(string userId, int articuloId);
    }
}
