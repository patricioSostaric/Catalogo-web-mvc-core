using catalogo_web_mvc.Models;

namespace CatalogoWeb.Tests.Models
{
    public class EstadoPedidoTests
    {
        [Theory]
        [InlineData(EstadoPedido.Confirmado, EstadoPedido.Enviado)]
        [InlineData(EstadoPedido.Enviado, EstadoPedido.Entregado)]
        public void SiguienteEstado_AvanzaEnLaSecuencia(EstadoPedido actual, EstadoPedido esperado)
        {
            Assert.Equal(esperado, actual.SiguienteEstado());
        }

        [Theory]
        [InlineData(EstadoPedido.Entregado)]
        [InlineData(EstadoPedido.Cancelado)]
        public void SiguienteEstado_DesdeUnTerminal_EsNull(EstadoPedido estado)
        {
            Assert.Null(estado.SiguienteEstado());
        }

        [Theory]
        [InlineData(EstadoPedido.Entregado, true)]
        [InlineData(EstadoPedido.Cancelado, true)]
        [InlineData(EstadoPedido.Confirmado, false)]
        [InlineData(EstadoPedido.Enviado, false)]
        public void EsTerminal_DistingueLosEstadosFinales(EstadoPedido estado, bool esperado)
        {
            Assert.Equal(esperado, estado.EsTerminal());
        }

        [Fact]
        public void SePuedeCancelar_SoloDesdeConfirmado()
        {
            Assert.True(EstadoPedido.Confirmado.SePuedeCancelar());
            Assert.False(EstadoPedido.Enviado.SePuedeCancelar());
            Assert.False(EstadoPedido.Entregado.SePuedeCancelar());
            Assert.False(EstadoPedido.Cancelado.SePuedeCancelar());
        }

        [Fact]
        public void Confirmado_EsElValorCero()
        {
            // La migracion agrega la columna con default 0: los pedidos que ya existian
            // tienen que quedar como confirmados.
            Assert.Equal(0, (int)EstadoPedido.Confirmado);
        }

        [Theory]
        [InlineData(EstadoPedido.Confirmado)]
        [InlineData(EstadoPedido.Enviado)]
        [InlineData(EstadoPedido.Entregado)]
        [InlineData(EstadoPedido.Cancelado)]
        public void ClaseBadge_DevuelveUnaClasePorEstado(EstadoPedido estado)
        {
            Assert.False(string.IsNullOrWhiteSpace(estado.ClaseBadge()));
        }
    }
}
