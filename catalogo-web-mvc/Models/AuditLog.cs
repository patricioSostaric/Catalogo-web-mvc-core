namespace catalogo_web_mvc.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public string Accion { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? UserId { get; set; }
        public string? Detalle { get; set; }
        public string? IpAddress { get; set; }
    }
}
