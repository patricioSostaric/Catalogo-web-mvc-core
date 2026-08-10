using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Carrito;
using catalogo_web_mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace catalogo_web_mvc.Repository.Carrito
{
    public class CarritoRepository : ICarritoRepository
    {
        private readonly CatalogoContext _context;

        public CarritoRepository(CatalogoContext context)
        {
            _context = context;
        }

        public async Task<List<ItemCarrito>> GetByUsuarioAsync(string userId)
            => await _context.ItemsCarrito
                .Where(i => i.UserId == userId)
                .Include(i => i.Articulo)
                .ToListAsync();

        public async Task<ItemCarrito?> GetItemAsync(string userId, int articuloId)
            => await _context.ItemsCarrito
                .Include(i => i.Articulo)
                .FirstOrDefaultAsync(i => i.UserId == userId && i.ArticuloId == articuloId);

        public async Task AddAsync(ItemCarrito item)
        {
            _context.ItemsCarrito.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ItemCarrito item)
        {
            _context.ItemsCarrito.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(string userId, int articuloId)
        {
            var item = await _context.ItemsCarrito
                .FirstOrDefaultAsync(i => i.UserId == userId && i.ArticuloId == articuloId);

            if (item != null)
            {
                _context.ItemsCarrito.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task VaciarAsync(string userId)
        {
            var items = await _context.ItemsCarrito.Where(i => i.UserId == userId).ToListAsync();
            if (items.Count > 0)
            {
                _context.ItemsCarrito.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> ContarUnidadesAsync(string userId)
            => await _context.ItemsCarrito
                .Where(i => i.UserId == userId)
                .SumAsync(i => (int?)i.Cantidad) ?? 0;
    }
}
