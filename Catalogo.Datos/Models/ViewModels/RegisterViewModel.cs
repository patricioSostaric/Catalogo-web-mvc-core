using System.ComponentModel.DataAnnotations;

namespace catalogo_web_mvc.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Mínimo {2} caracteres", MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmá la contraseña")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Datos de perfil e imagen. Comparte clase con la pantalla de edición para no
        /// duplicar validaciones ni el partial del formulario.
        /// </summary>
        public PerfilCamposViewModel Datos { get; set; } = new();
    }
}
