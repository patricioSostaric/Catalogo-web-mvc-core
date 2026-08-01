using catalogo_web_mvc.Models.ViewModels;

namespace catalogo_web_mvc.Interfaces.Carrito
{
    public interface ICarritoService
    {
        Task<CarritoViewModel> GetCarritoAsync(string userId);
        Task<ResultadoCarrito> AgregarAsync(string userId, int articuloId, int cantidad);
        Task<ResultadoCarrito> CambiarCantidadAsync(string userId, int articuloId, int cantidad);
        Task QuitarAsync(string userId, int articuloId);
        Task VaciarAsync(string userId);
        Task<int> ContarUnidadesAsync(string userId);
    }
}
