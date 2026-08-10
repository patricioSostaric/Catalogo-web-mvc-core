using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Categorias;
using catalogo_web_mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace catalogo_web_mvc.Repository.Categorias
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly CatalogoContext _context;

        public CategoriaRepository(CatalogoContext context)
        {
            _context = context;
        }

        public async Task<List<Categoria>> GetAllAsync()
            => await _context.Categorias.ToListAsync();

        public async Task<Categoria?> GetByIdAsync(int id)
            => await _context.Categorias.FirstOrDefaultAsync(c => c.CategoriaId == id);

        public async Task AddAsync(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Categoria categoria)
        {
            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entidad = await _context.Categorias.FindAsync(id);
            if (entidad != null)
            {
                _context.Categorias.Remove(entidad);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
            => await _context.Categorias.AnyAsync(c => c.CategoriaId == id);
    }
}
