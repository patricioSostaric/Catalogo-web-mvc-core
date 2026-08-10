namespace catalogo_web_mvc.Models
{
    /// <summary>
    /// Estados por los que pasa un pedido.
    ///
    /// <code>
    /// Confirmado ──> Enviado ──> Entregado
    ///      │
    ///      └──> Cancelado
    /// </code>
    ///
    /// <see cref="Entregado"/> y <see cref="Cancelado"/> son terminales. Cancelar solo se
    /// permite desde <see cref="Confirmado"/>: una vez despachado el pedido, decidir que
    /// pasa con la mercaderia excede lo que esta aplicacion modela.
    /// </summary>
    public enum EstadoPedido
    {
        Confirmado = 0,
        Enviado = 1,
        Entregado = 2,
        Cancelado = 3
    }

    public static class EstadoPedidoExtensiones
    {
        /// <summary>Estado al que puede avanzar, o null si es terminal.</summary>
        public static EstadoPedido? SiguienteEstado(this EstadoPedido estado) => estado switch
        {
            EstadoPedido.Confirmado => EstadoPedido.Enviado,
            EstadoPedido.Enviado => EstadoPedido.Entregado,
            _ => null
        };

        public static bool EsTerminal(this EstadoPedido estado)
            => estado is EstadoPedido.Entregado or EstadoPedido.Cancelado;

        public static bool SePuedeCancelar(this EstadoPedido estado)
            => estado == EstadoPedido.Confirmado;

        /// <summary>Clase de Bootstrap para el badge, para no repetir el switch en cada vista.</summary>
        public static string ClaseBadge(this EstadoPedido estado) => estado switch
        {
            EstadoPedido.Confirmado => "bg-primary",
            EstadoPedido.Enviado => "bg-info text-dark",
            EstadoPedido.Entregado => "bg-success",
            EstadoPedido.Cancelado => "bg-secondary",
            _ => "bg-light text-dark"
        };
    }
}
