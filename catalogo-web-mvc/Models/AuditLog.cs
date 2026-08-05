using System.ComponentModel.DataAnnotations;

namespace catalogo_web_mvc.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public DateTime Fecha { get; set; }

        [Required]
        public string Accion { get; set; } = string.Empty;  // LOGIN_OK, LOGIN_FAIL, LOCKOUT, REGISTER, LOGOUT, CREATE, UPDATE, DELETE

        public string? Email { get; set; }
        public string? UserId { get; set; }
        public string? Detalle { get; set; }
        // Guarda la red de origen, no la IP completa: el ultimo octeto se enmascara
        // antes de persistir. Ver IpAnonimizador.
        public string? IpAddress { get; set; }
    }
}
