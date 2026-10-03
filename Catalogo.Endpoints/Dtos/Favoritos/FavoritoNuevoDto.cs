using System.ComponentModel.DataAnnotations;

namespace catalogo_web_mvc.Models.Dtos
{
    /// <summary>
    /// Cuerpo de POST /api/favoritos. El usuario no viaja acá: sale de la cookie.
    /// </summary>
    public class FavoritoNuevoDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador del artículo no es válido.")]
        public int ArticuloId { get; set; }
    }
}
