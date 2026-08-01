using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Interfaces.Carrito;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;

namespace catalogo_web_mvc.Services.Carrito
{
    public class CarritoService : ICarritoService
    {
        private const int CantidadMaximaPorArticulo = 100;

        private readonly ICarritoRepository _repository;
        private readonly IArticuloService _articuloService;

        public CarritoService(ICarritoRepository repository, IArticuloService articuloService)
        {
            _repository = repository;
            _articuloService = articuloService;
        }

        public async Task<CarritoViewModel> GetCarritoAsync(string userId)
        {
            var items = await _repository.GetByUsuarioAsync(userId);

            return new CarritoViewModel
            {
                Items = items.Select(i => new ItemCarritoViewModel
                {
                    ArticuloId = i.ArticuloId,
                    Nombre = i.Articulo.Nombre,
                    ImagenUrl = i.Articulo.ImagenUrl,
                    Precio = i.Articulo.Precio,
                    Cantidad = i.Cantidad,
                    StockDisponible = i.Articulo.Stock
                }).ToList(),
                // Una clave por render del carrito: si el usuario envia dos veces el mismo
                // formulario, ambos envios comparten clave y solo se crea un pedido.
                ClaveIdempotencia = Guid.NewGuid().ToString()
            };
        }

        public async Task<ResultadoCarrito> AgregarAsync(string userId, int articuloId, int cantidad)
        {
            if (cantidad < 1)
                return ResultadoCarrito.Falla("La cantidad debe ser al menos 1.");

            var articulo = await _articuloService.GetByIdAsync(articuloId);

            if (articulo == null || !articulo.Activo)
                return ResultadoCarrito.Falla("El artículo no está disponible.");

            var existente = await _repository.GetItemAsync(userId, articuloId);
            var cantidadFinal = (existente?.Cantidad ?? 0) + cantidad;

            if (cantidadFinal > CantidadMaximaPorArticulo)
                return ResultadoCarrito.Falla($"No se pueden llevar más de {CantidadMaximaPorArticulo} unidades del mismo artículo.");

            // Se valida contra el stock actual para no dejar agregar de mas, pero esta
            // comprobacion es informativa: el stock real se descuenta al confirmar, con una
            // condicion dentro del UPDATE. Entre este chequeo y la confirmacion el stock
            // puede cambiar, y ahi es donde se decide de verdad.
            if (cantidadFinal > articulo.Stock)
                return ResultadoCarrito.Falla($"Solo quedan {articulo.Stock} unidades de {articulo.Nombre}.");

            if (existente == null)
            {
                await _repository.AddAsync(new ItemCarrito
                {
                    UserId = userId,
                    ArticuloId = articuloId,
                    Cantidad = cantidad
                });
            }
            else
            {
                existente.Cantidad = cantidadFinal;
                await _repository.UpdateAsync(existente);
            }

            return ResultadoCarrito.Ok(articulo.Nombre);
        }

        public async Task<ResultadoCarrito> CambiarCantidadAsync(string userId, int articuloId, int cantidad)
        {
            if (cantidad < 1)
            {
                await _repository.RemoveAsync(userId, articuloId);
                return ResultadoCarrito.Ok();
            }

            var item = await _repository.GetItemAsync(userId, articuloId);

            if (item == null)
                return ResultadoCarrito.Falla("El artículo no está en el carrito.");

            if (cantidad > CantidadMaximaPorArticulo)
                return ResultadoCarrito.Falla($"No se pueden llevar más de {CantidadMaximaPorArticulo} unidades del mismo artículo.");

            if (cantidad > item.Articulo.Stock)
                return ResultadoCarrito.Falla($"Solo quedan {item.Articulo.Stock} unidades de {item.Articulo.Nombre}.");

            item.Cantidad = cantidad;
            await _repository.UpdateAsync(item);

            return ResultadoCarrito.Ok();
        }

        public Task QuitarAsync(string userId, int articuloId)
            => _repository.RemoveAsync(userId, articuloId);

        public Task VaciarAsync(string userId)
            => _repository.VaciarAsync(userId);

        public Task<int> ContarUnidadesAsync(string userId)
            => _repository.ContarUnidadesAsync(userId);
    }
}
