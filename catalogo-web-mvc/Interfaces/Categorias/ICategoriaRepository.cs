using catalogo_web_mvc.Models;

namespace catalogo_web_mvc.Interfaces.Categorias
{
    public interface ICategoriaRepository
    {
        Task<List<Categoria>> GetAllAsync();
        Task<Categoria?> GetByIdAsync(int id);
        Task AddAsync(Categoria categoria);
        Task UpdateAsync(Categoria categoria);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
