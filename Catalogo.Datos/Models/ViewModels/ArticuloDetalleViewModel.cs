namespace catalogo_web_mvc.Models.ViewModels
{
    public class ArticuloDetalleViewModel
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public string? Categoria { get; set; }
        public decimal Precio { get; set; }
        public string? ImagenUrl { get; set; }
    }
}
