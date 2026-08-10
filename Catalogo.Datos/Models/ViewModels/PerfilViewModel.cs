using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace catalogo_web_mvc.Models.ViewModels
{
    /// <summary>
    /// Pantalla de edición de perfil. El email se muestra pero no se edita acá:
    /// cambiarlo implica reconfirmar la cuenta, que es otro flujo.
    /// </summary>
    public class PerfilViewModel
    {
        [BindNever]
        public string Email { get; set; } = string.Empty;

        /// <summary>Edad calculada a partir de la fecha guardada, solo para mostrar.</summary>
        [BindNever]
        public int? Edad { get; set; }

        public PerfilCamposViewModel Datos { get; set; } = new();
    }
}
