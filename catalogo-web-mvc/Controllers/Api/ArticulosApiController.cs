using Microsoft.AspNetCore.Mvc;
using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Models.Dtos;

namespace catalogo_web_mvc.Controllers.Api
{
    [ApiController]
    [Route("api/articulos")]
    public class ArticulosApiController : ControllerBase
    {
        private readonly IArticuloService _service;

        public ArticulosApiController(IArticuloService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<ArticulosPaginadosDto>> Get(string? buscar = null, int page = 1, int pageSize = 6)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
          
            var articulos = await _service.BuscarAsync(buscar, false, null, null, null, page, pageSize, soloActivos: true);
            var respuesta = new ArticulosPaginadosDto
            {
                Pagina = articulos.PageNumber,
                TamanioPagina = articulos.PageSize,
                TotalArticulos = articulos.TotalItemCount,
                TotalPaginas = articulos.PageCount,
                Articulos = articulos.Select(a => new ArticuloDto
                {
                    Id = a.Id,
                    Nombre = a.Nombre,
                    Marca = a.Marca?.Descripcion ?? "",
                    Categoria = a.Categoria?.Descripcion ?? "",
                    Precio = a.Precio,
                    ImagenUrl = a.ImagenUrl ?? "", 
                    Disponible = a.Stock > 0
                }).ToList()

            };

            return Ok(respuesta);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArticuloDetalleDto>> GetPorId(int id)
        {
            // El servicio ya descarta los articulos dados de baja: devuelve null
            // tanto si el articulo no existe como si esta inactivo. Para quien
            // consulta desde afuera son el mismo caso, y conviene que lo sean:
            // distinguirlos revelaria que el articulo existe pero no se publica.
            var articulo = await _service.ObtenerDetallePublicoAsync(id);
            if (articulo == null) return NotFound();

            return Ok(new ArticuloDetalleDto
            {
                Id = articulo.Id,
                Nombre = articulo.Nombre,
                Descripcion = articulo.Descripcion,
                Marca = articulo.Marca ?? "",
                Categoria = articulo.Categoria ?? "",
                Precio = articulo.Precio,
                ImagenUrl = articulo.ImagenUrl ?? ""
            });
        }
    }
}