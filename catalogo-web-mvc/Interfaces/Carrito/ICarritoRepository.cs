using catalogo_web_mvc.Models;

namespace catalogo_web_mvc.Interfaces.Carrito
{
    public interface ICarritoRepository
    {
        Task<List<ItemCarrito>> GetByUsuarioAsync(string userId);
        Task<ItemCarrito?> GetItemAsync(string userId, int articuloId);
        Task AddAsync(ItemCarrito item);
        Task UpdateAsync(ItemCarrito item);
        Task RemoveAsync(string userId, int articuloId);
        Task VaciarAsync(string userId);
        Task<int> ContarUnidadesAsync(string userId);
    }
}
