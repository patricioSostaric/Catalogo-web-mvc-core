using System.ComponentModel.DataAnnotations;

namespace catalogo_web_mvc.Models
{
    /// <summary>Linea del carrito de un usuario, previa a la confirmacion del pedido.</summary>
    public class ItemCarrito
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser Usuario { get; set; } = null!;

        public int ArticuloId { get; set; }
        public Articulo Articulo { get; set; } = null!;

        [Range(1, 10, ErrorMessage = "La cantidad debe estar entre {1} y {2}.")]
        public int Cantidad { get; set; } = 1;
    }
}
