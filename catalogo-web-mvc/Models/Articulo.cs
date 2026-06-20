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
        public string Codigo { get; set; } = null!;
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = null!;

        [DisplayName("Descripción")]
        [StringLength(250)]
        public string Descripcion { get; set; } = null!;

        [DisplayName("Marca")]
        public int MarcaId { get; set; }
        public Marca? Marca { get; set; }

        [DisplayName("Categoría")]
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
        
        public string? ImagenUrl { get; set; }
        
        
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public bool Activo { get; set; } = true;
    }
}
