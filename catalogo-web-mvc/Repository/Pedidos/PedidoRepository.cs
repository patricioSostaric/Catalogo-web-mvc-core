using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Pedidos;
using catalogo_web_mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace catalogo_web_mvc.Repository.Pedidos
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly CatalogoContext _context;

        public PedidoRepository(CatalogoContext context)
        {
            _context = context;
        }

        public async Task<Pedido?> GetByClaveIdempotenciaAsync(string clave)
            => await _context.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.ClaveIdempotencia == clave);

        public async Task<Pedido?> GetByIdAsync(int id, string userId)
            => await _context.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        public async Task<List<Pedido>> GetByUsuarioAsync(string userId)
            => await _context.Pedidos
                .Where(p => p.UserId == userId)
                .Include(p => p.Detalles)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

        public async Task<bool> ConfirmarAsync(Pedido pedido)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();

            foreach (var detalle in pedido.Detalles)
            {
                // Clave del asunto: la condicion viaja dentro del UPDATE en lugar de
                // resolverse leyendo primero. Si dos compras concurrentes van por la ultima
                // unidad, la segunda afecta 0 filas y se rechaza; leer y despues escribir
                // dejaria que ambas creyeran tener stock.
                var filas = await _context.Articulos
                    .Where(a => a.Id == detalle.ArticuloId && a.Stock >= detalle.Cantidad)
                    .ExecuteUpdateAsync(s => s.SetProperty(a => a.Stock, a => a.Stock - detalle.Cantidad));

                if (filas == 0)
                {
                    await transaccion.RollbackAsync();
                    return false;
                }
            }

            _context.Pedidos.Add(pedido);

            var itemsCarrito = await _context.ItemsCarrito
                .Where(i => i.UserId == pedido.UserId)
                .ToListAsync();
            _context.ItemsCarrito.RemoveRange(itemsCarrito);

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();
            return true;
        }
    }
}
