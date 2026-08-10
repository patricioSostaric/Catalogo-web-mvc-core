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
                Fecha = Ahora(),
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

        public Task<List<Pedido>> GetTodosAsync(EstadoPedido? estado = null)
            => _pedidoRepository.GetTodosAsync(estado);

        public async Task<ResultadoCambioEstado> CancelarAsync(string userId, int pedidoId)
        {
            var pedido = await _pedidoRepository.GetByIdAsync(pedidoId, userId);

            // GetByIdAsync filtra por usuario, asi que un pedido ajeno se ve igual que uno
            // inexistente: no se confirma si existe ni de quien es.
            if (pedido == null)
                return ResultadoCambioEstado.Falla("El pedido no existe.");

            if (!pedido.Estado.SePuedeCancelar())
                return ResultadoCambioEstado.Falla(
                    $"No se puede cancelar un pedido {pedido.Estado.ToString().ToLower()}.");

            var cancelado = await _pedidoRepository.CancelarAsync(pedidoId, Ahora());

            // Puede fallar aunque la comprobacion anterior haya pasado: entre una y otra,
            // otro request pudo cancelarlo o el administrador pudo despacharlo.
            if (!cancelado)
                return ResultadoCambioEstado.Falla("El pedido cambió de estado mientras lo cancelabas.");

            return ResultadoCambioEstado.Ok(pedidoId, EstadoPedido.Cancelado);
        }

        public async Task<ResultadoCambioEstado> AvanzarAsync(int pedidoId)
        {
            var pedido = await _pedidoRepository.GetByIdAsync(pedidoId);

            if (pedido == null)
                return ResultadoCambioEstado.Falla("El pedido no existe.");

            var siguiente = pedido.Estado.SiguienteEstado();

            if (siguiente == null)
                return ResultadoCambioEstado.Falla(
                    $"Un pedido {pedido.Estado.ToString().ToLower()} no avanza a otro estado.");

            var movido = await _pedidoRepository.CambiarEstadoAsync(
                pedidoId, pedido.Estado, siguiente.Value, Ahora());

            if (!movido)
                return ResultadoCambioEstado.Falla("El pedido cambió de estado mientras lo actualizabas.");

            return ResultadoCambioEstado.Ok(pedidoId, siguiente.Value);
        }

        private DateTime Ahora() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _zonaHoraria);
    }
}
