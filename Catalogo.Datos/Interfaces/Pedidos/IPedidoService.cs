using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;

namespace catalogo_web_mvc.Interfaces.Pedidos
{
    public interface IPedidoService
    {
        Task<ResultadoConfirmacion> ConfirmarAsync(string userId, string claveIdempotencia);
        Task<List<Pedido>> GetByUsuarioAsync(string userId);
        Task<List<Pedido>> GetTodosAsync(EstadoPedido? estado = null);

        /// <summary>Cancela un pedido propio. Solo se permite mientras está confirmado.</summary>
        Task<ResultadoCambioEstado> CancelarAsync(string userId, int pedidoId);

        /// <summary>Avanza el pedido al siguiente estado de la secuencia. Es tarea del administrador.</summary>
        Task<ResultadoCambioEstado> AvanzarAsync(int pedidoId);
    }
}
