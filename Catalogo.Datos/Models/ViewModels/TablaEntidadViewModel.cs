namespace catalogo_web_mvc.Models.ViewModels
{
    public class TablaEntidadViewModel
    {
        public string NombreEntidad { get; set; } = "";
        public List<string> Columnas { get; set; } = new();
        public List<FilaTabla> Filas { get; set; } = new();
    }

    public class FilaTabla
    {
        public int Id { get; set; }
        public List<string> Valores { get; set; } = new();
    }
}
