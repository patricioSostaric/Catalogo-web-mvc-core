using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Data;
using catalogo_web_mvc.Models;

namespace catalogo_web_mvc.Repository.Articulos
{
    public class ArticuloRepository : IArticuloRepository
    {
        private readonly CatalogoContext _context;

        public ArticuloRepository(CatalogoContext context)
        {
            _context = context;
        }

        public IQueryable<Articulo> GetAll()
        {
            return _context.Set<Articulo>().AsNoTracking();
        }

        public async Task<Articulo?> GetByIdAsync(int id)
        {
            return await _context.Set<Articulo>().FindAsync(id);
        }

        public async Task AddAsync(Articulo articulo)
        {
            await _context.Set<Articulo>().AddAsync(articulo);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Articulo articulo)
        {
            _context.Set<Articulo>().Update(articulo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entidad = await _context.Set<Articulo>().FindAsync(id);
            if (entidad != null)
            {
                _context.Set<Articulo>().Remove(entidad);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            var entidad = await _context.Set<Articulo>().FindAsync(id);
            return entidad != null;
        }
    }
}
