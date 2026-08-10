using catalogo_web_mvc.Models.Validation;
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
        
        [DisplayName("Imagen URL")]
        [StringLength(500, ErrorMessage = "La URL de la imagen no puede superar los 500 caracteres.")]
        // Este valor se renderiza en el src de un <img> del catálogo, así que se exige
        // https (o una ruta local bajo /imagen/articulos/) y se bloquean esquemas como
        // javascript: y data:.
        //
        // El prefijo incluye la subcarpeta a propósito: el validador rechaza barras en el
        // resto de la ruta para frenar el path traversal, así que un prefijo más corto
        // como "/imagen/" invalidaría las rutas reales del catálogo.
        [UrlImagenSegura(PrefijoLocalPermitido = "/imagen/articulos/")]
        public string? ImagenUrl { get; set; }
        
        
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 1000000000000, ErrorMessage = "El precio debe estar entre {1} y {2}.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Precio { get; set; }

        [DisplayName("Activo")]
        public bool Activo { get; set; } = true;

        [DisplayName("Stock")]
        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser un número entero no negativo.")]
        public int Stock { get; set; } = 0;
    }
}
