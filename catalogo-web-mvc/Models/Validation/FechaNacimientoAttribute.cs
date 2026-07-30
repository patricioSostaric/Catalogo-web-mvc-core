using System.ComponentModel.DataAnnotations;

namespace catalogo_web_mvc.Models.Validation
{
    /// <summary>
    /// Valida que una fecha de nacimiento sea plausible: no futura y con una edad
    /// resultante dentro de un rango razonable.
    ///
    /// Se separa en un atributo propio (en lugar de resolverlo en el controlador)
    /// para que la regla se aplique igual en el registro y en la edición de perfil,
    /// y para poder testearla sin levantar MVC.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FechaNacimientoAttribute : ValidationAttribute
    {
        public int EdadMinima { get; init; } = 13;
        public int EdadMaxima { get; init; } = 120;

        /// <summary>
        /// Fecha tomada como "hoy". Existe para que los tests sean deterministas;
        /// en producción queda en null y se usa la fecha del sistema.
        /// </summary>
        public DateOnly? Hoy { get; init; }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return ValidationResult.Success;

            // MemberNames hace que el error se muestre debajo del input con
            // asp-validation-for, y no solo en el resumen del formulario.
            string[] miembros = validationContext.MemberName is null
                ? []
                : [validationContext.MemberName];

            DateOnly nacimiento = value switch
            {
                DateOnly d => d,
                DateTime dt => DateOnly.FromDateTime(dt),
                _ => default
            };

            if (nacimiento == default)
                return new ValidationResult("La fecha de nacimiento no es válida.", miembros);

            var hoy = Hoy ?? DateOnly.FromDateTime(DateTime.Today);

            if (nacimiento > hoy)
                return new ValidationResult("La fecha de nacimiento no puede ser futura.", miembros);

            var edad = ApplicationUser.CalcularEdad(nacimiento, hoy);

            if (edad < EdadMinima)
                return new ValidationResult($"Tenés que ser mayor de {EdadMinima} años para registrarte.", miembros);

            if (edad > EdadMaxima)
                return new ValidationResult("Revisá la fecha de nacimiento: la edad resultante no es válida.", miembros);

            return ValidationResult.Success;
        }
    }
}
