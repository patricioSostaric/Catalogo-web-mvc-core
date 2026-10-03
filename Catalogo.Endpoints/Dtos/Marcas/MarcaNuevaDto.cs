
using System.ComponentModel.DataAnnotations;

namespace catalogo_web_mvc.Models.Dtos
{
    /// <summary>
    /// Cuerpo de POST y PUT /api/marcas.
    /// </summary>
    public class MarcaNuevaDto
    {
        [Required]
        [StringLength(100)]
        public string Descripcion { get; set; } = string.Empty;
    }
}
