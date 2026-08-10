using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Favoritos;
using catalogo_web_mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace catalogo_web_mvc.Repository.Favoritos
{
    public class FavoritoRepository : IFavoritoRepository
    {
        private readonly CatalogoContext _context;

        public FavoritoRepository(CatalogoContext context)
        {
            _context = context;
        }

        public async Task<List<ArticuloFavorito>> GetByUsuarioAsync(string userId)
            => await _context.ArticuloFavoritos
                .Where(f => f.UserId == userId)
                .Include(f => f.Articulo)
                .ToListAsync();

        // Un HashSet en vez de una lista porque quien lo usa solo pregunta "¿está este id?"
        // una vez por tarjeta del catálogo.
        public async Task<HashSet<int>> GetIdsArticulosAsync(string userId)
            => await _context.ArticuloFavoritos
                .Where(f => f.UserId == userId)
                .Select(f => f.ArticuloId)
                .ToHashSetAsync();

        public async Task<bool> ExisteAsync(string userId, int articuloId)
            => await _context.ArticuloFavoritos
                .AnyAsync(f => f.UserId == userId && f.ArticuloId == articuloId);

        public async Task AddAsync(ArticuloFavorito favorito)
        {
            _context.ArticuloFavoritos.Add(favorito);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(string userId, int articuloId)
        {
            var favorito = await _context.ArticuloFavoritos
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ArticuloId == articuloId);

            if (favorito != null)
            {
                _context.ArticuloFavoritos.Remove(favorito);
                await _context.SaveChangesAsync();
            }
        }
    }
}
