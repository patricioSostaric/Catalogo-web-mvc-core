
namespace catalogo_web_mvc.Models.Dtos
{
    /// <summary>
    /// Respuesta de GET /api/marcas.
    /// </summary>
    public class MarcaDto
    {
        public int MarcaId { get; set; }

        /// <summary>
        /// Descripción o nombre de la marca.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;
    }
}
