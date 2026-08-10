using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;
using X.PagedList;

namespace catalogo_web_mvc.Interfaces.Articulos
{
    public interface IArticuloService
    {
        Task<IPagedList<Articulo>> BuscarAsync(string? searchString, bool filtroAvanzado,
        string? campo, string? criterio, string? filtro, int pageNumber, int pageSize, bool soloActivos = false);

        Task<Articulo?> GetByIdAsync(int id);
        Task<ArticuloDetalleViewModel?> ObtenerDetallePublicoAsync(int id);
        Task AddAsync(Articulo articulo);
        Task UpdateAsync(Articulo articulo);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
