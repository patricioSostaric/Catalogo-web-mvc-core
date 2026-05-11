using catalogo_web_mvc.Models;

namespace catalogo_web_mvc.Interfaces.Articulos
{
    public interface IArticuloRepository
    {
        IQueryable<Articulo> GetAll();
        Task<Articulo?> GetByIdAsync(int id);
        Task AddAsync(Articulo articulo);
        Task UpdateAsync(Articulo articulo);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
