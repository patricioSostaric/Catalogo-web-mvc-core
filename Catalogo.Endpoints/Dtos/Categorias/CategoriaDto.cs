
namespace catalogo_web_mvc.Models.Dtos
{

    /// <summary>
    /// Respuesta de GET /api/categorias.
    /// </summary>

    public class CategoriaDto
    {
        public int CategoriaId { get; set; }

        /// <summary>
        /// Descripción o nombre de la categoría.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;
    }
}
