namespace catalogo_web_mvc.Models.Dtos
{
    /// <summary>
    /// Vista publica de un articulo. No expone el codigo, que es dato de
    /// administracion y sirve para reponer stock, ni la cantidad disponible.
    /// </summary>
    public class ArticuloDetalleDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string ImagenUrl { get; set; } = string.Empty;
    }
}
