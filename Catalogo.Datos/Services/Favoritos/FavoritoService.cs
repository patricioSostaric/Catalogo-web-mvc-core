using catalogo_web_mvc.Interfaces.Favoritos;
using catalogo_web_mvc.Models;

namespace catalogo_web_mvc.Services.Favoritos
{
    public class FavoritoService : IFavoritoService
    {
        private readonly IFavoritoRepository _repository;

        public FavoritoService(IFavoritoRepository repository)
        {
            _repository = repository;
        }

        public Task<List<ArticuloFavorito>> ListarAsync(string userId)
            => _repository.GetByUsuarioAsync(userId);

        public Task<HashSet<int>> IdsDeUsuarioAsync(string userId)
            => _repository.GetIdsArticulosAsync(userId);

        public Task<bool> EsFavoritoAsync(string userId, int articuloId)
            => _repository.ExisteAsync(userId, articuloId);

        public async Task AgregarAsync(string userId, int articuloId)
        {
            // Sin esta comprobación, marcar dos veces el mismo artículo dejaría filas
            // duplicadas y la lista lo mostraría repetido.
            if (await _repository.ExisteAsync(userId, articuloId))
                return;

            await _repository.AddAsync(new ArticuloFavorito
            {
                UserId = userId,
                ArticuloId = articuloId
            });
        }

        public Task QuitarAsync(string userId, int articuloId)
            => _repository.RemoveAsync(userId, articuloId);
    }
}
