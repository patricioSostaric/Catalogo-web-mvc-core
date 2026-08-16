using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Interfaces.Favoritos;
using catalogo_web_mvc.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace catalogo_web_mvc.Controllers.Api
{
    // Primer endpoint autenticado de la API. El usuario no se manda por parametro:
    // sale de la cookie que emitio el MVC y que esta aplicacion sabe descifrar por
    // compartir las claves de Data Protection. Si viniera por parametro, cualquiera
    // podria pedir los favoritos de cualquiera.
    [ApiController]
    [Route("api/favoritos")]
    [Authorize]
    public class FavoritosApiController : ControllerBase
    {
        private readonly IFavoritoService _service;
        private readonly IArticuloService _articulos;

        public FavoritosApiController(IFavoritoService service, IArticuloService articulos)
        {
            _service = service;
            _articulos = articulos;
        }

        [HttpGet]
        public async Task<ActionResult<List<FavoritoDto>>> Get()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var favoritos = await _service.ListarAsync(userId);

            return favoritos.Select(f => new FavoritoDto
            {
                ArticuloId = f.ArticuloId,
                Nombre = f.Articulo.Nombre,
                Descripcion = f.Articulo.Descripcion,
                Precio = f.Articulo.Precio,
                ImagenUrl = f.Articulo.ImagenUrl ?? ""
            }).ToList();
        }

        // El id va en el cuerpo y no en la ruta porque lo que se modifica es la coleccion:
        // POST /api/favoritos agrega un elemento, DELETE /api/favoritos/{id} quita uno.
        //
        // Responde 204 tanto si lo agrego como si ya estaba. Marcar dos veces el mismo
        // articulo no es un error: el estado que el cliente pidio es el que quedo. Asi el
        // front no tiene que distinguir entre "lo marque" y "ya estaba marcado".
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FavoritoNuevoDto nuevo)
        {
            // Sin esta comprobacion, marcar un id inexistente revienta contra la clave
            // foranea y devuelve 500. Y un articulo dado de baja no deberia poder marcarse:
            // no aparece en el catalogo, asi que llegar hasta aca significa que alguien
            // armo el pedido a mano.
            var articulo = await _articulos.GetByIdAsync(nuevo.ArticuloId);

            if (articulo == null || !articulo.Activo)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _service.AgregarAsync(userId, nuevo.ArticuloId);

            return NoContent();
        }

        // Se responde 204 tanto si habia favorito como si no. Quitar algo que ya no
        // esta es el estado que el cliente pidio, no un error: si el usuario toca dos
        // veces el boton, la segunda no tiene por que fallar.
        //
        // No lleva antiforgery como los formularios del MVC. Un DELETE no se puede
        // disparar desde un formulario de otro sitio, y la cookie se emite con
        // SameSite=Strict (ver ConfigureApplicationCookie en el MVC), asi que no viaja
        // en pedidos originados en otro sitio. El dia que el front viva en otro
        // dominio, las dos cosas cambian y esto hay que revisarlo.
        [HttpDelete("{articuloId}")]
        public async Task<IActionResult> Delete(int articuloId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _service.QuitarAsync(userId, articuloId);

            return NoContent();
        }
    }
}
