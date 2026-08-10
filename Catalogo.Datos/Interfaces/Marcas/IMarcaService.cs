using catalogo_web_mvc.Models;

namespace catalogo_web_mvc.Interfaces.Marcas
{
    public interface IMarcaService
    {
        Task<List<Marca>> GetAllAsync();
        Task<Marca?> GetByIdAsync(int id);
        Task AddAsync(Marca marca);
        Task UpdateAsync(Marca marca);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
