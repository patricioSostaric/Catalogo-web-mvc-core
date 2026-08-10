using catalogo_web_mvc.Models;

namespace catalogo_web_mvc.Interfaces.Pedidos
{
    public interface IPedidoRepository
    {
        Task<Pedido?> GetByClaveIdempotenciaAsync(string clave);
        Task<Pedido?> GetByIdAsync(int id, string userId);

        /// <summary>Sin filtrar por usuario: solo para el panel de administración.</summary>
        Task<Pedido?> GetByIdAsync(int id);
        Task<List<Pedido>> GetByUsuarioAsync(string userId);

        /// <summary>
        /// Descuenta el stock, guarda el pedido y vacia el carrito en una sola transaccion.
        /// Devuelve false si algun articulo no tenia stock suficiente, en cuyo caso no se
        /// persiste nada.
        /// </summary>
        Task<bool> ConfirmarAsync(Pedido pedido);

        /// <summary>Todos los pedidos, para el panel de administración. Filtra por estado si se indica.</summary>
        Task<List<Pedido>> GetTodosAsync(EstadoPedido? estado = null);

        /// <summary>
        /// Mueve el pedido solo si sigue en el estado esperado. Devuelve false si otro lo
        /// cambió en el medio: la condición viaja dentro del UPDATE, así dos operaciones
        /// simultáneas no pueden aplicar ambas.
        /// </summary>
        Task<bool> CambiarEstadoAsync(int pedidoId, EstadoPedido esperado, EstadoPedido nuevo, DateTime fecha);

        /// <summary>
        /// Cancela y devuelve el stock, en una sola transacción. Devuelve false si el
        /// pedido ya no estaba confirmado, en cuyo caso no se toca nada.
        /// </summary>
        Task<bool> CancelarAsync(int pedidoId, DateTime fecha);
    }
}
