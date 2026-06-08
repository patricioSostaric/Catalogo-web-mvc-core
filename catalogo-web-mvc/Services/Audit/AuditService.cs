using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Models;

namespace catalogo_web_mvc.Services.Audit
{
    public class AuditService : IAuditService
    {
        private readonly CatalogoContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditService> _logger;

        public AuditService(CatalogoContext context, IHttpContextAccessor httpContextAccessor, ILogger<AuditService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task RegistrarAsync(string accion, string? email = null, string? userId = null, string? detalle = null)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            var log = new AuditLog
            {
                Accion = accion,
                Email = email,
                UserId = userId,
                Detalle = detalle,
                IpAddress = ip,
                Fecha = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("[AUDIT] {Accion} | {Email} | {Detalle} | IP: {Ip}", accion, email, detalle, ip);
        }
    }
}
