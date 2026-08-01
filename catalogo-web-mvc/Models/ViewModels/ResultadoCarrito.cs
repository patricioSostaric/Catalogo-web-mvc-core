namespace catalogo_web_mvc.Models.ViewModels
{
    /// <summary>Resultado de agregar o modificar una linea del carrito.</summary>
    public class ResultadoCarrito
    {
        public bool Exito { get; init; }
        public string? Error { get; init; }

        public static ResultadoCarrito Ok() => new() { Exito = true };
        public static ResultadoCarrito Falla(string error) => new() { Exito = false, Error = error };
    }

    /// <summary>Resultado de confirmar la compra.</summary>
    public class ResultadoConfirmacion
    {
        public bool Exito { get; init; }
        public string? Error { get; init; }
        public int PedidoId { get; init; }
        public decimal Total { get; init; }
        public int CantidadArticulos { get; init; }

        /// <summary>
        /// True cuando la clave de idempotencia ya existia: el pedido no se creo ahora,
        /// se esta devolviendo el de un envio anterior.
        /// </summary>
        public bool YaExistia { get; init; }

        public static ResultadoConfirmacion Ok(Pedido pedido, bool yaExistia = false) => new()
        {
            Exito = true,
            PedidoId = pedido.Id,
            Total = pedido.Total,
            CantidadArticulos = pedido.Detalles.Sum(d => d.Cantidad),
            YaExistia = yaExistia
        };

        public static ResultadoConfirmacion Falla(string error) => new() { Exito = false, Error = error };
    }
}
