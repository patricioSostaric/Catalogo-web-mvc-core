using catalogo_web_mvc.Services.Imagenes;
using System.ComponentModel.DataAnnotations;

namespace catalogo_web_mvc.Models.Validation
{
    /// <summary>
    /// Exige que una URL de imagen sea https absoluta, o una ruta relativa bajo
    /// <see cref="PrefijoLocalPermitido"/> si se configura uno.
    ///
    /// Reemplaza al [Url] de DataAnnotations, que es demasiado permisivo: acepta http
    /// plano y no bloquea esquemas como javascript: ni data:. La regla real está en
    /// <see cref="UrlImagenValidator"/>, compartida con la validación del avatar.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class UrlImagenSeguraAttribute : ValidationAttribute
    {
        /// <summary>
        /// Prefijo de ruta relativa aceptado (por ejemplo "/img/"). Si queda en null,
        /// solo se aceptan URLs https absolutas.
        /// </summary>
        public string? PrefijoLocalPermitido { get; init; }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return ValidationResult.Success;

            // MemberNames hace que el error se muestre debajo del input con
            // asp-validation-for, y no solo en el resumen del formulario.
            string[] miembros = validationContext.MemberName is null
                ? []
                : [validationContext.MemberName];

            if (value is not string texto)
                return new ValidationResult("El valor no es una URL válida.", miembros);

            if (UrlImagenValidator.TryNormalizar(texto, PrefijoLocalPermitido, out _))
                return ValidationResult.Success;

            var detalle = PrefijoLocalPermitido is null
                ? "Tiene que ser una URL https."
                : $"Tiene que ser una URL https o una ruta que empiece con {PrefijoLocalPermitido}.";

            return new ValidationResult($"La URL de la imagen no es válida. {detalle}", miembros);
        }
    }
}
