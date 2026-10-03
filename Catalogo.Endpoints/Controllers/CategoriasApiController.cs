using catalogo_web_mvc.Interfaces.Categorias;
using catalogo_web_mvc.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using catalogo_web_mvc.Models;

namespace catalogo_web_mvc.Controllers.Api
{
    [ApiController]
    [Route("api/categorias")]
    [Authorize(Roles = "Admin")]
    public class CategoriasApiController : ControllerBase
    {
        private readonly ICategoriaService _categorias;

        public CategoriasApiController(ICategoriaService categorias)
        {
            _categorias = categorias;
        }

        // GET: api/categorias
        [HttpGet]
        public async Task<ActionResult<List<CategoriaDto>>> Get()
        {
            var categorias = await _categorias.GetAllAsync();

            return categorias.Select(c => new CategoriaDto  
            {
                CategoriaId = c.CategoriaId,
                Descripcion = c.Descripcion
            }).ToList();
        }

        // GET: api/categorias/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaDto>> GetPorId(int id)
        {
            var categoria = await _categorias.GetByIdAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            var dto = new CategoriaDto
            {
                CategoriaId = categoria.CategoriaId,
                Descripcion = categoria.Descripcion
            };

            return Ok(dto);
        }
        [HttpPost]
        public async Task<ActionResult<CategoriaDto>> Post([FromBody] CategoriaNuevaDto nueva)
        {
            var categoria = new Categoria { Descripcion = nueva.Descripcion };
            await _categorias.AddAsync(categoria);

            var dto = new CategoriaDto
            {
                CategoriaId = categoria.CategoriaId,
                Descripcion = categoria.Descripcion
            };

            return CreatedAtAction(nameof(GetPorId), new { id = dto.CategoriaId }, dto);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _categorias.ExistsAsync(id))
                return NotFound();

            if (await _categorias.TieneArticulosAsync(id))
                return Conflict("No se puede eliminar una categoria que tiene articulos asociados.");

            await _categorias.DeleteAsync(id);

            return NoContent();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CategoriaNuevaDto modificada)
        {
            // Se trae la entidad y se le cambia solo el campo que llego, en lugar de armar
            // una nueva con el id: Update() marca todas las propiedades como modificadas,
            // asi que lo que no se asigna se guardaria con su valor por defecto. Con dos
            // campos da igual, pero el patron es el mismo para entidades mas grandes.
            var categoria = await _categorias.GetByIdAsync(id);

            if (categoria == null)
                return NotFound();

            categoria.Descripcion = modificada.Descripcion;
            await _categorias.UpdateAsync(categoria);

            return NoContent();
        }
    }
}
