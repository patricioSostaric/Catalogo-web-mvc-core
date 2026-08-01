using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace catalogo_web_mvc.Models
{
    public class PedidoDetalle
    {
        [Key]
        public int Id { get; set; }

        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; } = null!;

        public int ArticuloId { get; set; }
        public Articulo Articulo { get; set; } = null!;

        /// <summary>Nombre al momento de la compra: el articulo puede renombrarse despues.</summary>
        [Required]
        [StringLength(100)]
        public string NombreArticulo { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        [DisplayName("Precio unitario")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrecioUnitario { get; set; }

        [NotMapped]
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
