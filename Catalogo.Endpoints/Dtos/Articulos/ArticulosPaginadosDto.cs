namespace catalogo_web_mvc.Models.Dtos
{
    public class ArticulosPaginadosDto
    {
        public int Pagina { get; set; }
        public int TamanioPagina { get; set; }
        public int TotalArticulos { get; set; }
        public int TotalPaginas { get; set; }
        public List<ArticuloDto> Articulos { get; set; } = new();
    }
}
