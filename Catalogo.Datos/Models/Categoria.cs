using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace catalogo_web_mvc.Models
{
    public class Categoria
    {
        public int CategoriaId { get; set; }
        
        [DisplayName("Descripción")]
        [Required]
        [StringLength(100)]
        public string Descripcion { get; set; }

        // Relación inversa
        public ICollection<Articulo> Articulos { get; set; }
        

        public override string ToString()
        {
            return Descripcion;
        }
    }
}
