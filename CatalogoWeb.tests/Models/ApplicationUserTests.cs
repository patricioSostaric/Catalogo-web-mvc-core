using catalogo_web_mvc.Models;

namespace CatalogoWeb.Tests.Models
{
    public class ApplicationUserTests
    {
        // ── CalcularEdad ──────────────────────────────────────────────────────
        // Se pasa la fecha de referencia explícita para que los tests no dependan
        // del día en que se ejecutan.

        [Fact]
        public void CalcularEdad_CumpleanosYaPasadoEsteAnio_DevuelveLaEdadCompleta()
        {
            var nacimiento = new DateOnly(1990, 3, 15);
            var referencia = new DateOnly(2026, 7, 29);

            Assert.Equal(36, ApplicationUser.CalcularEdad(nacimiento, referencia));
        }

        [Fact]
        public void CalcularEdad_CumpleanosTodaviaNoLlego_RestaUnAnio()
        {
            var nacimiento = new DateOnly(1990, 12, 25);
            var referencia = new DateOnly(2026, 7, 29);

            Assert.Equal(35, ApplicationUser.CalcularEdad(nacimiento, referencia));
        }

        [Fact]
        public void CalcularEdad_JustoElDiaDelCumpleanos_YaCuentaElAnio()
        {
            var nacimiento = new DateOnly(2000, 7, 29);
            var referencia = new DateOnly(2026, 7, 29);

            Assert.Equal(26, ApplicationUser.CalcularEdad(nacimiento, referencia));
        }

        [Fact]
        public void CalcularEdad_ElDiaAntesDelCumpleanos_TodaviaNoCuenta()
        {
            var nacimiento = new DateOnly(2000, 7, 30);
            var referencia = new DateOnly(2026, 7, 29);

            Assert.Equal(25, ApplicationUser.CalcularEdad(nacimiento, referencia));
        }

        [Fact]
        public void CalcularEdad_NacidoEl29DeFebrero_CumpleEl28EnAnioNoBisiesto()
        {
            var nacimiento = new DateOnly(2000, 2, 29);
            var referencia = new DateOnly(2026, 2, 28); // 2026 no es bisiesto

            // DateOnly.AddYears recorta el 29/02 al 28/02 cuando el año destino no es
            // bisiesto, así que el cumpleaños se considera cumplido el 28. Es el criterio
            // que queremos: nadie que nació en año bisiesto debería quedar un año atrasado.
            Assert.Equal(26, ApplicationUser.CalcularEdad(nacimiento, referencia));
        }

        [Fact]
        public void CalcularEdad_NacidoEl29DeFebrero_ElDiaAnteriorTodaviaNoCumple()
        {
            var nacimiento = new DateOnly(2000, 2, 29);
            var referencia = new DateOnly(2026, 2, 27);

            Assert.Equal(25, ApplicationUser.CalcularEdad(nacimiento, referencia));
        }

        [Fact]
        public void CalcularEdad_SinFecha_DevuelveNull()
        {
            Assert.Null(ApplicationUser.CalcularEdad(null, new DateOnly(2026, 7, 29)));
        }

        [Fact]
        public void CalcularEdad_FechaFutura_DevuelveNull()
        {
            var nacimiento = new DateOnly(2030, 1, 1);
            var referencia = new DateOnly(2026, 7, 29);

            Assert.Null(ApplicationUser.CalcularEdad(nacimiento, referencia));
        }

        [Fact]
        public void Edad_SinFechaDeNacimiento_EsNull()
        {
            var user = new ApplicationUser();

            Assert.Null(user.Edad);
        }

        // ── NombreParaMostrar ─────────────────────────────────────────────────

        [Fact]
        public void NombreParaMostrar_ConNombreYApellido_DevuelveAmbos()
        {
            var user = new ApplicationUser { Nombre = "Patricio", Apellido = "Sostaric" };

            Assert.Equal("Patricio Sostaric", user.NombreParaMostrar);
        }

        [Fact]
        public void NombreParaMostrar_SoloNombre_NoDejaEspaciosSobrantes()
        {
            var user = new ApplicationUser { Nombre = "Patricio" };

            Assert.Equal("Patricio", user.NombreParaMostrar);
        }

        [Fact]
        public void NombreParaMostrar_SinNombre_UsaLaParteLocalDelEmail()
        {
            var user = new ApplicationUser { Email = "patricio@ejemplo.com" };

            Assert.Equal("patricio", user.NombreParaMostrar);
        }

        [Fact]
        public void NombreParaMostrar_SinNombreNiEmail_DevuelveUnValorPorDefecto()
        {
            var user = new ApplicationUser();

            Assert.Equal("Usuario", user.NombreParaMostrar);
        }

        [Fact]
        public void NombreParaMostrar_NuncaEsVacio()
        {
            var user = new ApplicationUser { Nombre = "   ", Apellido = "   ", Email = "x@y.com" };

            Assert.False(string.IsNullOrWhiteSpace(user.NombreParaMostrar));
        }
    }
}
