using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Marcas;
using catalogo_web_mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace catalogo_web_mvc.Repository.Marcas
{
    public class MarcaRepository : IMarcaRepository
    {
        private readonly CatalogoContext _context;

        public MarcaRepository(CatalogoContext context)
        {
            _context = context;
        }

        public async Task<List<Marca>> GetAllAsync()
            => await _context.Marcas.ToListAsync();

        public async Task<Marca?> GetByIdAsync(int id)
            => await _context.Marcas.FirstOrDefaultAsync(m => m.MarcaId == id);

        public async Task AddAsync(Marca marca)
        {
            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Marca marca)
        {
            _context.Marcas.Update(marca);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entidad = await _context.Marcas.FindAsync(id);
            if (entidad != null)
            {
                _context.Marcas.Remove(entidad);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
            => await _context.Marcas.AnyAsync(m => m.MarcaId == id);
    }
}
