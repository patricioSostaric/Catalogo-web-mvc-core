using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using X.PagedList;
using X.PagedList.Extensions;

namespace catalogo_web_mvc.Services.Articulos
{
    public class ArticuloService : IArticuloService
    {
        private readonly IArticuloRepository _repo;

        public ArticuloService(IArticuloRepository repo)
        {
            _repo = repo;
        }

        public Task<IPagedList<Articulo>> BuscarAsync(string? searchString, bool filtroAvanzado,
            string? campo, string? criterio, string? filtro, int pageNumber, int pageSize, bool soloActivos = false)
        {
            var query = _repo.GetAll();

            if (soloActivos)
                query = query.Where(a => a.Activo && a.Stock >= 1);

            if (!string.IsNullOrEmpty(searchString) && !filtroAvanzado)
                query = query.Where(a => a.Nombre.Contains(searchString));

            if (filtroAvanzado && !string.IsNullOrEmpty(campo) && !string.IsNullOrEmpty(criterio) && !string.IsNullOrEmpty(filtro))
            {
                query = campo switch
                {
                    "Codigo" => criterio switch
                    {
                        "Contiene" => query.Where(a => a.Codigo.Contains(filtro)),
                        "Comienza con" => query.Where(a => a.Codigo.StartsWith(filtro)),
                        "Termina con" => query.Where(a => a.Codigo.EndsWith(filtro)),
                        _ => query
                    },
                    "Nombre" => criterio switch
                    {
                        "Contiene" => query.Where(a => a.Nombre.Contains(filtro)),
                        "Comienza con" => query.Where(a => a.Nombre.StartsWith(filtro)),
                        "Termina con" => query.Where(a => a.Nombre.EndsWith(filtro)),
                        _ => query
                    },
                    "Precio" => decimal.TryParse(filtro, out var precio) ? criterio switch
                    {
                        "Igual a" => query.Where(a => a.Precio == precio),
                        "Mayor a" => query.Where(a => a.Precio > precio),
                        "Menor a" => query.Where(a => a.Precio < precio),
                        _ => query
                    } : query,
                    "Stock" => int.TryParse(filtro, out var stock) ? criterio switch
                    {
                        "Igual a" => query.Where(a => a.Stock == stock),
                        "Mayor a" => query.Where(a => a.Stock > stock),
                        "Menor a" => query.Where(a => a.Stock < stock),
                        _ => query
                    } : query,
                    "Marca" => query.Where(a => a.Marca.Descripcion.Contains(filtro)),
                    "Categoria" => query.Where(a => a.Categoria.Descripcion.Contains(filtro)),
                    _ => query
                };
            }

            
            return Task.FromResult(query.ToPagedList(pageNumber, pageSize));
        }

        public Task<Articulo?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<ArticuloDetalleViewModel?> ObtenerDetallePublicoAsync(int id)
        {
            var articulo = await _repo.GetByIdAsync(id);
            if (articulo == null || !articulo.Activo) return null;

            return new ArticuloDetalleViewModel
            {
                Id = articulo.Id,
                Codigo = articulo.Codigo,
                Nombre = articulo.Nombre,
                Descripcion = articulo.Descripcion,
                Marca = articulo.Marca?.Descripcion,
                Categoria = articulo.Categoria?.Descripcion,
                Precio = articulo.Precio,
                ImagenUrl = articulo.ImagenUrl
            };
        }

        public Task AddAsync(Articulo articulo) => _repo.AddAsync(articulo);
        public Task UpdateAsync(Articulo articulo) => _repo.UpdateAsync(articulo);
        public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
        public Task<bool> ExistsAsync(int id) => _repo.ExistsAsync(id);
    }
}
