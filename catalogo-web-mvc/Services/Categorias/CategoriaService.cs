using catalogo_web_mvc.Interfaces.Categorias;
using catalogo_web_mvc.Models;

namespace catalogo_web_mvc.Services.Categorias
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _repo;

        public CategoriaService(ICategoriaRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Categoria>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Categoria?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task AddAsync(Categoria categoria) => _repo.AddAsync(categoria);
        public Task UpdateAsync(Categoria categoria) => _repo.UpdateAsync(categoria);
        public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
        public Task<bool> ExistsAsync(int id) => _repo.ExistsAsync(id);
    }
}
