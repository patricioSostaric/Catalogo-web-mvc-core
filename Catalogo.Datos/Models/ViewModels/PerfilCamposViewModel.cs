using catalogo_web_mvc.Models.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace catalogo_web_mvc.Models.ViewModels
{
    /// <summary>
    /// Campos de perfil compartidos por el registro y la edición de perfil.
    ///
    /// Vive en una sola clase para que las validaciones y el partial de la vista
    /// (_CamposPerfil) no estén duplicados en dos lugares que después se desincronizan.
    /// Se usa como propiedad anidada: los inputs quedan como "Datos.Nombre", etc.
    /// </summary>
    public class PerfilCamposViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "Máximo {1} caracteres")]
        [RegularExpression(@"^[\p{L}\p{M}\s'\-\.]+$", ErrorMessage = "El nombre solo puede tener letras, espacios, guiones y apóstrofos")]
        [Display(Name = "Nombre")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "Máximo {1} caracteres")]
        [RegularExpression(@"^[\p{L}\p{M}\s'\-\.]+$", ErrorMessage = "El apellido solo puede tener letras, espacios, guiones y apóstrofos")]
        [Display(Name = "Apellido")]
        public string? Apellido { get; set; }

        /// <summary>
        /// Se pide la fecha y no la edad: un número de edad se desactualiza solo.
        /// La edad se calcula al mostrarla (ver ApplicationUser.Edad).
        /// </summary>
        [DataType(DataType.Date)]
        [FechaNacimiento]
        [Display(Name = "Fecha de nacimiento")]
        public DateOnly? FechaNacimiento { get; set; }

        [StringLength(100, ErrorMessage = "Máximo {1} caracteres")]
        [Display(Name = "Localidad")]
        public string? Localidad { get; set; }

        [RegularExpression(@"^(\d{4}|[A-Za-z]\d{4}[A-Za-z]{3})$",
            ErrorMessage = "Ingresá 4 dígitos (1900) o el CPA completo (B1900ABC)")]
        [Display(Name = "Código postal")]
        public string? CodigoPostal { get; set; }

        /// <summary>
        /// Imagen nueva. El contenido lo valida AvatarValidator del lado del servidor;
        /// el atributo accept del input es solo comodidad para el usuario.
        /// </summary>
        [Display(Name = "Imagen de perfil")]
        public IFormFile? Avatar { get; set; }

        /// <summary>
        /// Avatar ya guardado, para previsualizarlo en la pantalla de perfil.
        /// No se bindea desde el POST: se rellena desde la base en cada request.
        /// </summary>
        [BindNever]
        public string? AvatarActual { get; set; }

        /// <summary>Marcar para volver al avatar por defecto sin subir una imagen nueva.</summary>
        [Display(Name = "Quitar la imagen actual")]
        public bool QuitarAvatar { get; set; }
    }
}
