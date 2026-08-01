using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;

namespace catalogo_web_mvc.Interfaces.Pedidos
{
    public interface IPedidoService
    {
        Task<ResultadoConfirmacion> ConfirmarAsync(string userId, string claveIdempotencia);
        Task<List<Pedido>> GetByUsuarioAsync(string userId);
    }
}
