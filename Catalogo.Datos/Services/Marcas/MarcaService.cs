using catalogo_web_mvc.Interfaces.Marcas;
using catalogo_web_mvc.Models;

namespace catalogo_web_mvc.Services.Marcas
{
    public class MarcaService : IMarcaService
    {
        private readonly IMarcaRepository _repo;

        public MarcaService(IMarcaRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Marca>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Marca?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task AddAsync(Marca marca) => _repo.AddAsync(marca);
        public Task UpdateAsync(Marca marca) => _repo.UpdateAsync(marca);
        public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
        public Task<bool> ExistsAsync(int id) => _repo.ExistsAsync(id);
    }
}
