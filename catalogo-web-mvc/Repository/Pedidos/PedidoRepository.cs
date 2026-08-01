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

        public async Task<Pedido?> GetByIdAsync(int id)
            => await _context.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<List<Pedido>> GetByUsuarioAsync(string userId)
            => await _context.Pedidos
                .Where(p => p.UserId == userId)
                .Include(p => p.Detalles)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

        public async Task<List<Pedido>> GetTodosAsync(EstadoPedido? estado = null)
        {
            var query = _context.Pedidos.Include(p => p.Detalles).AsQueryable();

            if (estado.HasValue)
                query = query.Where(p => p.Estado == estado.Value);

            return await query.OrderByDescending(p => p.Fecha).ToListAsync();
        }

        public async Task<bool> CambiarEstadoAsync(int pedidoId, EstadoPedido esperado, EstadoPedido nuevo, DateTime fecha)
        {
            // Mismo patron que el descuento de stock: la condicion viaja dentro del UPDATE.
            // Si dos administradores despachan el mismo pedido a la vez, el segundo afecta
            // cero filas; leer el estado y despues escribir dejaria que ambos avanzaran.
            var filas = await _context.Pedidos
                .Where(p => p.Id == pedidoId && p.Estado == esperado)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Estado, nuevo)
                    .SetProperty(p => p.FechaUltimoEstado, fecha));

            return filas > 0;
        }

        public async Task<bool> CancelarAsync(int pedidoId, DateTime fecha)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();

            // Primero el cambio de estado, y condicionado: si el pedido ya fue cancelado
            // por otro request, esto afecta cero filas y el stock no se devuelve dos veces.
            var filas = await _context.Pedidos
                .Where(p => p.Id == pedidoId && p.Estado == EstadoPedido.Confirmado)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Estado, EstadoPedido.Cancelado)
                    .SetProperty(p => p.FechaUltimoEstado, fecha));

            if (filas == 0)
            {
                await transaccion.RollbackAsync();
                return false;
            }

            var detalles = await _context.PedidoDetalles
                .Where(d => d.PedidoId == pedidoId)
                .ToListAsync();

            foreach (var detalle in detalles)
            {
                // Devolver stock no necesita condicion: sumar siempre es valido.
                await _context.Articulos
                    .Where(a => a.Id == detalle.ArticuloId)
                    .ExecuteUpdateAsync(s => s.SetProperty(a => a.Stock, a => a.Stock + detalle.Cantidad));
            }

            await transaccion.CommitAsync();
            return true;
        }

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
