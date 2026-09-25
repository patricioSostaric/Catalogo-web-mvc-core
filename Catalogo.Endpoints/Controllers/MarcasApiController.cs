using catalogo_web_mvc.Interfaces.Marcas;
using catalogo_web_mvc.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using catalogo_web_mvc.Models;

namespace catalogo_web_mvc.Controllers.Api
{
    [ApiController]
    [Route("api/marcas")]
    [Authorize(Roles = "Admin")]
    public class MarcasApiController : ControllerBase
    {
        private readonly IMarcaService _marcas;

        public MarcasApiController(IMarcaService marcas)
        {
            _marcas = marcas;
        }

        // GET: api/marcas
        [HttpGet]
        public async Task<ActionResult<List<MarcaDto>>> Get()
        {
            var marcas = await _marcas.GetAllAsync();

            return marcas.Select(m => new MarcaDto
            {
                MarcaId = m.MarcaId,
                Descripcion = m.Descripcion
            }).ToList();
        }

        // GET: api/marcas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MarcaDto>> GetPorId(int id)
        {
            var marca = await _marcas.GetByIdAsync(id);

            if (marca == null)
            {
                return NotFound();
            }

            var dto = new MarcaDto
            {
                MarcaId = marca.MarcaId,
                Descripcion = marca.Descripcion
            };

            return Ok(dto);
        }
        [HttpPost]
        public async Task<ActionResult<MarcaDto>> Post([FromBody] MarcaNuevaDto nueva)
        {
            var marca = new Marca { Descripcion = nueva.Descripcion };
            await _marcas.AddAsync(marca);

            var dto = new MarcaDto
            {
                MarcaId = marca.MarcaId,
                Descripcion = marca.Descripcion
            };

            return CreatedAtAction(nameof(GetPorId), new { id = dto.MarcaId }, dto);

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _marcas.ExistsAsync(id))
                return NotFound();

            if (await _marcas.TieneArticulosAsync(id))
                return Conflict("No se puede eliminar una marca que tiene articulos asociados.");

            await _marcas.DeleteAsync(id);

            return NoContent();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] MarcaNuevaDto modificada)
        {
            // Se trae la entidad y se le cambia solo el campo que llego, en lugar de armar
            // una nueva con el id: Update() marca todas las propiedades como modificadas,
            // asi que lo que no se asigna se guardaria con su valor por defecto. Con dos
            // campos da igual, pero el patron es el mismo para entidades mas grandes.
            var marca = await _marcas.GetByIdAsync(id);

            if (marca == null)
                return NotFound();

            marca.Descripcion = modificada.Descripcion;
            await _marcas.UpdateAsync(marca);

            return NoContent();
        }
    }
}
