using System.Net;
using catalogo_web_mvc.Services.Audit;

namespace CatalogoWeb.Tests.Services
{
    public class IpAnonimizadorTests
    {
        // ── IPv4: se enmascara el ultimo octeto ────────────────────────────────

        [Theory]
        [InlineData("203.0.113.45", "203.0.113.0")]
        [InlineData("192.168.1.55", "192.168.1.0")]
        [InlineData("8.8.8.8", "8.8.8.0")]
        [InlineData("127.0.0.1", "127.0.0.0")]
        public void Anonimizar_IPv4_BorraElUltimoOcteto(string entrada, string esperado)
        {
            var resultado = IpAnonimizador.Anonimizar(IPAddress.Parse(entrada));

            Assert.Equal(esperado, resultado);
        }

        [Fact]
        public void Anonimizar_IPv4_ConservaLaRed()
        {
            // Dos equipos de la misma red quedan indistinguibles entre si, pero
            // siguen siendo distinguibles de otra red: es lo que hace util al log.
            var primero = IpAnonimizador.Anonimizar(IPAddress.Parse("203.0.113.45"));
            var segundo = IpAnonimizador.Anonimizar(IPAddress.Parse("203.0.113.240"));
            var otraRed = IpAnonimizador.Anonimizar(IPAddress.Parse("186.13.115.8"));

            Assert.Equal(primero, segundo);
            Assert.NotEqual(primero, otraRed);
        }

        // ── IPv4 mapeada en IPv6 ───────────────────────────────────────────────

        [Theory]
        [InlineData("::ffff:172.18.0.1", "172.18.0.0")]
        [InlineData("::ffff:203.0.113.45", "203.0.113.0")]
        public void Anonimizar_IPv4MapeadaEnIPv6_SeTrataComoIPv4(string entrada, string esperado)
        {
            // Detras de un proxy las direcciones llegan con este formato; si se las
            // tratara como IPv6 se conservarian octetos que en realidad identifican
            // al equipo.
            var resultado = IpAnonimizador.Anonimizar(IPAddress.Parse(entrada));

            Assert.Equal(esperado, resultado);
        }

        // ── IPv6: se conservan los primeros 48 bits ────────────────────────────

        [Theory]
        [InlineData("2001:db8:1234:5678:9abc:def0:1234:5678", "2001:db8:1234::")]
        [InlineData("2800:340:a1:b2:c3:d4:e5:f6", "2800:340:a1::")]
        public void Anonimizar_IPv6_ConservaSoloElPrefijoDeRed(string entrada, string esperado)
        {
            var resultado = IpAnonimizador.Anonimizar(IPAddress.Parse(entrada));

            Assert.Equal(esperado, resultado);
        }

        [Fact]
        public void Anonimizar_IPv6_DistintosEquiposDeLaMismaRedColapsan()
        {
            var primero = IpAnonimizador.Anonimizar(IPAddress.Parse("2001:db8:1234:1::1"));
            var segundo = IpAnonimizador.Anonimizar(IPAddress.Parse("2001:db8:1234:9::abcd"));

            Assert.Equal(primero, segundo);
        }

        // ── Ausencia de direccion ──────────────────────────────────────────────

        [Fact]
        public void Anonimizar_Null_DevuelveNull()
        {
            // Puede no haber HttpContext, por ejemplo en tareas de inicio.
            Assert.Null(IpAnonimizador.Anonimizar(null));
        }

        // ── El resultado nunca conserva la direccion original ───────────────────

        [Theory]
        [InlineData("203.0.113.45")]
        [InlineData("::ffff:203.0.113.45")]
        [InlineData("2001:db8:1234:5678:9abc:def0:1234:5678")]
        public void Anonimizar_NuncaDevuelveLaDireccionCompleta(string entrada)
        {
            var resultado = IpAnonimizador.Anonimizar(IPAddress.Parse(entrada));

            Assert.NotEqual(entrada, resultado);
        }
    }
}
