namespace catalogo_web_mvc.Models.ViewModels
{
    public class FavoritoViewModel
    {
        public int ArticuloId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string? ImagenUrl { get; set; }
    }
}
