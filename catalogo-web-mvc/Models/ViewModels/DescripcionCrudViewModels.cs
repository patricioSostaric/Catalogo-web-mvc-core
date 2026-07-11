using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace catalogo_web_mvc.Models.ViewModels
{
    public class DescripcionFormViewModel
    {
        public int? Id { get; set; }
        public string IdFieldName { get; set; } = "";
        public string NombreEntidad { get; set; } = "";
        public string Accion { get; set; } = "";
        public string FormAction { get; set; } = "";
        public string SubmitText { get; set; } = "";
        public string VolverTexto { get; set; } = "";

        [DisplayName("Descripción")]
        [Required]
        [StringLength(100)]
        public string Descripcion { get; set; } = "";
    }

    public class DescripcionDeleteViewModel
    {
        public int Id { get; set; }
        public string IdFieldName { get; set; } = "";
        public string NombreEntidad { get; set; } = "";
        public string VolverTexto { get; set; } = "";
        public string Descripcion { get; set; } = "";
    }
}
