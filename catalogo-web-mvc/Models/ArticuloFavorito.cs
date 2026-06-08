using System.ComponentModel.DataAnnotations.Schema;

namespace catalogo_web_mvc.Models
{
    public class ArticuloFavorito
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        [ForeignKey(nameof(UserId))]
        public ApplicationUser Usuario { get; set; } = null!;
        public int ArticuloId { get; set; }
        public Articulo Articulo { get; set; } = null!;
    }
}
