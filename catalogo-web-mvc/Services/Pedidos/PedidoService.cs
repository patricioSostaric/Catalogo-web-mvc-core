using catalogo_web_mvc.Interfaces.Carrito;
using catalogo_web_mvc.Interfaces.Pedidos;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;

namespace catalogo_web_mvc.Services.Pedidos
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly ICarritoRepository _carritoRepository;
        private readonly TimeZoneInfo _zonaHoraria;

        public PedidoService(
            IPedidoRepository pedidoRepository,
            ICarritoRepository carritoRepository,
            TimeZoneInfo zonaHoraria)
        {
            _pedidoRepository = pedidoRepository;
            _carritoRepository = carritoRepository;
            _zonaHoraria = zonaHoraria;
        }

        public async Task<ResultadoConfirmacion> ConfirmarAsync(string userId, string claveIdempotencia)
        {
            if (string.IsNullOrWhiteSpace(claveIdempotencia))
                return ResultadoConfirmacion.Falla("Falta la clave de confirmación. Volvé a intentar desde el carrito.");

            // Primera linea de defensa contra el doble envio: si esta clave ya genero un
            // pedido, se devuelve ese mismo en lugar de crear otro. El usuario ve la misma
            // confirmacion que la primera vez.
            var existente = await _pedidoRepository.GetByClaveIdempotenciaAsync(claveIdempotencia);
            if (existente != null)
            {
                if (existente.UserId != userId)
                    return ResultadoConfirmacion.Falla("La clave de confirmación no corresponde a esta cuenta.");

                return ResultadoConfirmacion.Ok(existente, yaExistia: true);
            }

            var items = await _carritoRepository.GetByUsuarioAsync(userId);

            if (items.Count == 0)
                return ResultadoConfirmacion.Falla("El carrito está vacío.");

            var pedido = new Pedido
            {
                UserId = userId,
                Fecha = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _zonaHoraria),
                ClaveIdempotencia = claveIdempotencia,
                Detalles = items.Select(i => new PedidoDetalle
                {
                    ArticuloId = i.ArticuloId,
                    NombreArticulo = i.Articulo.Nombre,
                    Cantidad = i.Cantidad,
                    // El precio se congela: si manana cambia, el pedido conserva el valor
                    // que el usuario vio al comprar.
                    PrecioUnitario = i.Articulo.Precio
                }).ToList()
            };

            pedido.Total = pedido.Detalles.Sum(d => d.Subtotal);

            var confirmado = await _pedidoRepository.ConfirmarAsync(pedido);

            if (!confirmado)
                return ResultadoConfirmacion.Falla(
                    "Algún artículo se quedó sin stock mientras completabas la compra. Revisá el carrito.");

            return ResultadoConfirmacion.Ok(pedido);
        }

        public Task<List<Pedido>> GetByUsuarioAsync(string userId)
            => _pedidoRepository.GetByUsuarioAsync(userId);
    }
}
