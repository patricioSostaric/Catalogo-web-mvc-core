using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        [StringLength(100)]
        public string Nombre { get; set; }

        [DisplayName("Descripción")]
        [StringLength(250)]
        public string Descripcion { get; set; }

        [DisplayName("Marca")]
        public int MarcaId { get; set; }   // FK explícita
        public Marca Marca { get; set; }

        [DisplayName("Categoría")]
        public int CategoriaId { get; set; }   // FK explícita
        public Categoria Categoria { get; set; }
        [Url]
        public string? ImagenUrl { get; set; }
        [Range(0, 999999)]
        public decimal Precio { get; set; }
    }
}
