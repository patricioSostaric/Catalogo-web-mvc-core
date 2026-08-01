using catalogo_web_mvc.Models;

namespace catalogo_web_mvc.Interfaces.Pedidos
{
    public interface IPedidoRepository
    {
        Task<Pedido?> GetByClaveIdempotenciaAsync(string clave);
        Task<Pedido?> GetByIdAsync(int id, string userId);
        Task<List<Pedido>> GetByUsuarioAsync(string userId);

        /// <summary>
        /// Descuenta el stock, guarda el pedido y vacia el carrito en una sola transaccion.
        /// Devuelve false si algun articulo no tenia stock suficiente, en cuyo caso no se
        /// persiste nada.
        /// </summary>
        Task<bool> ConfirmarAsync(Pedido pedido);
    }
}
