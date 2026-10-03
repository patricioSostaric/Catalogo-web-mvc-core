using System.ComponentModel.DataAnnotations;

namespace catalogo_web_mvc.Models.Dtos
{
    /// <summary>
    /// Cuerpo de POST y PUT /api/categorias.
    /// </summary>
    public class CategoriaNuevaDto
    {
        [Required]
        [StringLength(100)]
        public string Descripcion { get; set; } = string.Empty;
    }
}