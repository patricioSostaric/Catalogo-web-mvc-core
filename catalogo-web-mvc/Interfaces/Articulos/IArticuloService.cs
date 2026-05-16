using catalogo_web_mvc.Models;
using X.PagedList;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace catalogo_web_mvc.Interfaces.Articulos
{
    public interface IArticuloService
    {
        Task<IPagedList<Articulo>> BuscarAsync(string? searchString, bool filtroAvanzado,
        string? campo, string? criterio, string? filtro, int pageNumber, int pageSize);

        Task<Articulo?> GetByIdAsync(int id);
        Task AddAsync(Articulo articulo);
        Task UpdateAsync(Articulo articulo);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        Task<SelectList> GetMarcasSelectList(int? selectedId = null);
        Task<SelectList> GetCategoriasSelectList(int? selectedId = null);
    }
}
