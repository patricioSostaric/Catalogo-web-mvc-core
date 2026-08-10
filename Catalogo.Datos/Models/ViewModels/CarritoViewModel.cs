namespace catalogo_web_mvc.Models.ViewModels
{
    public class CarritoViewModel
    {
        public List<ItemCarritoViewModel> Items { get; set; } = [];

        public decimal Total => Items.Sum(i => i.Subtotal);

        public int CantidadUnidades => Items.Sum(i => i.Cantidad);

        public bool EstaVacio => Items.Count == 0;

        /// <summary>
        /// Se genera al mostrar el carrito y viaja en el formulario de confirmacion. Que
        /// nazca aca y no en el POST es lo que la vuelve util: dos envios del mismo
        /// formulario comparten clave, y el segundo no crea un pedido nuevo.
        /// </summary>
        public string ClaveIdempotencia { get; set; } = string.Empty;
    }

    public class ItemCarritoViewModel
    {
        public int ArticuloId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public int StockDisponible { get; set; }

        public decimal Subtotal => Cantidad * Precio;

        public bool SuperaStock => Cantidad > StockDisponible;
    }
}
