using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace catalogo_web_mvc.Models
{
    public class Articulo
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Código")]
        [Required]
        [StringLength(50)]
        public string Codigo { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [DisplayName("Descripción")]
        [StringLength(250)]
        public string Descripcion { get; set; }= string.Empty;

        [DisplayName("Marca")]
        public int MarcaId { get; set; }   // FK explícita
        public Marca Marca { get; set; }

        [DisplayName("Categoría")]
        public int CategoriaId { get; set; }   // FK explícita
        public Categoria Categoria { get; set; }
        
        public string? ImagenUrl { get; set; }

        [DisplayName("Precio")]
        [Required(ErrorMessage = "El precio es obligatorio.")]

        [Range(typeof(decimal), minimum: "0.01", maximum: "79228162514264337593543950335", ParseLimitsInInvariantCulture = true, ErrorMessage = "El precio debe ser mayor a 0")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Precio { get; set; }
        [DisplayName("Stock")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")] 
        public int Stock { get; set; }
    }
}
