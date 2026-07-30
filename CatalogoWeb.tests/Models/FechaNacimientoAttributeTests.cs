using catalogo_web_mvc.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace CatalogoWeb.Tests.Models
{
    public class FechaNacimientoAttributeTests
    {
        private static readonly DateOnly Hoy = new(2026, 7, 29);

        // El atributo recibe "Hoy" por init para que los tests no dependan del reloj.
        private static ValidationResult? Validar(DateOnly? fecha)
        {
            var atributo = new FechaNacimientoAttribute { Hoy = Hoy };
            var contexto = new ValidationContext(new object());

            return atributo.GetValidationResult(fecha, contexto);
        }

        [Fact]
        public void FechaPlausible_EsValida()
        {
            Assert.Null(Validar(new DateOnly(1990, 5, 10)));
        }

        [Fact]
        public void FechaNula_EsValida()
        {
            // El campo es opcional: la obligatoriedad la decide [Required], no este atributo.
            Assert.Null(Validar(null));
        }

        [Fact]
        public void FechaFutura_EsInvalida()
        {
            var resultado = Validar(new DateOnly(2030, 1, 1));

            Assert.NotNull(resultado);
            Assert.Contains("futura", resultado.ErrorMessage);
        }

        [Fact]
        public void FechaDeManana_EsInvalida()
        {
            var resultado = Validar(Hoy.AddDays(1));

            Assert.NotNull(resultado);
        }

        [Fact]
        public void MenorDeLaEdadMinima_EsInvalido()
        {
            // 10 años al 29/07/2026
            var resultado = Validar(new DateOnly(2016, 1, 1));

            Assert.NotNull(resultado);
            Assert.Contains("13", resultado.ErrorMessage);
        }

        [Fact]
        public void ExactamenteLaEdadMinima_EsValido()
        {
            var resultado = Validar(Hoy.AddYears(-13));

            Assert.Null(resultado);
        }

        [Fact]
        public void EdadAbsurdamenteAlta_EsInvalida()
        {
            var resultado = Validar(new DateOnly(1850, 1, 1));

            Assert.NotNull(resultado);
            Assert.Contains("no es válida", resultado.ErrorMessage);
        }

        [Fact]
        public void AceptaDateTimeAdemasDeDateOnly()
        {
            var atributo = new FechaNacimientoAttribute { Hoy = Hoy };
            var contexto = new ValidationContext(new object());

            var resultado = atributo.GetValidationResult(new DateTime(1990, 5, 10), contexto);

            Assert.Null(resultado);
        }
    }
}
