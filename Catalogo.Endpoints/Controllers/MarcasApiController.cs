using catalogo_web_mvc.Interfaces.Marcas;
using catalogo_web_mvc.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    }
}
