using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace catalogo_web_mvc.Models
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser Usuario { get; set; } = null!;

        [DisplayName("Fecha")]
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Total calculado al confirmar. Se guarda en lugar de recalcularse desde los
        /// articulos: si manana cambia un precio, el pedido debe seguir mostrando lo que
        /// el usuario efectivamente pago.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Total { get; set; }

        /// <summary>
        /// Identifica el intento de compra, no el pedido. Si el usuario hace doble clic en
        /// confirmar o reintenta tras un corte, llega dos veces la misma clave y se
        /// devuelve el pedido ya creado en lugar de duplicarlo.
        /// </summary>
        [Required]
        [StringLength(36)]
        public string ClaveIdempotencia { get; set; } = string.Empty;

        public EstadoPedido Estado { get; set; } = EstadoPedido.Confirmado;

        /// <summary>Fecha del ultimo cambio de estado. Null mientras sigue como se creo.</summary>
        public DateTime? FechaUltimoEstado { get; set; }

        public List<PedidoDetalle> Detalles { get; set; } = [];
    }
}
