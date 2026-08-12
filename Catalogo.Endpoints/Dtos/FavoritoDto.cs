namespace catalogo_web_mvc.Models.Dtos
{
    public class FavoritoDto
    {
        public int ArticuloId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string ImagenUrl { get; set; } = string.Empty;
    }
}
