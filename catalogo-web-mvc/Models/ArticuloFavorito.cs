using System.ComponentModel.DataAnnotations;

namespace catalogo_web_mvc.Models
{
    public class ArticuloFavorito
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser Usuario { get; set; } = null!;

        public int ArticuloId { get; set; }
        public Articulo Articulo { get; set; } = null!;
    }
}
