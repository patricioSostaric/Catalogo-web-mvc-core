using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace catalogo_web_mvc.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(50)]
        public string? Nombre { get; set; }

        [MaxLength(50)]
        public string? Apellido { get; set; }

        /// <summary>
        /// Se guarda la fecha de nacimiento en lugar de la edad: un número de edad
        /// queda desactualizado solo con el paso del tiempo, la fecha no.
        /// </summary>
        public DateOnly? FechaNacimiento { get; set; }

        [MaxLength(100)]
        public string? Localidad { get; set; }

        /// <summary>
        /// Acepta tanto el formato de 4 dígitos (1900) como el CPA argentino (B1900ABC).
        /// </summary>
        [MaxLength(8)]
        public string? CodigoPostal { get; set; }

        /// <summary>
        /// Ruta del avatar. Puede ser relativa (/uploads/avatars/xxx.jpg, subida por el
        /// usuario) o una URL https externa. En ambos casos pasa por AvatarUrlValidator
        /// antes de persistirse; nunca se escribe directo desde el input del usuario.
        /// </summary>
        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Edad derivada de <see cref="FechaNacimiento"/>. No se persiste.
        /// </summary>
        [NotMapped]
        public int? Edad => CalcularEdad(FechaNacimiento, DateOnly.FromDateTime(DateTime.Today));

        /// <summary>
        /// Nombre y apellido si están cargados; si no, la parte local del email.
        /// Se usa para el texto alternativo del avatar y el saludo del navbar.
        /// </summary>
        [NotMapped]
        public string NombreParaMostrar
        {
            get
            {
                var completo = $"{Nombre} {Apellido}".Trim();
                if (!string.IsNullOrWhiteSpace(completo)) return completo;

                var email = Email ?? UserName;
                if (string.IsNullOrWhiteSpace(email)) return "Usuario";

                var arroba = email.IndexOf('@');
                return arroba > 0 ? email[..arroba] : email;
            }
        }

        /// <summary>
        /// Calcula la edad restando un año si todavía no pasó el cumpleaños en el año
        /// de referencia. Expuesto como estático para poder testearlo sin depender
        /// de la fecha real del sistema.
        /// </summary>
        public static int? CalcularEdad(DateOnly? nacimiento, DateOnly referencia)
        {
            if (nacimiento is null) return null;
            if (nacimiento.Value > referencia) return null;

            var edad = referencia.Year - nacimiento.Value.Year;

            // Si el cumpleaños de este año todavía no llegó, resta uno.
            if (referencia < nacimiento.Value.AddYears(edad)) edad--;

            return edad;
        }
    }
}
