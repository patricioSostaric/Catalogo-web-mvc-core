using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Http;

namespace catalogo_web_mvc.Services.Audit
{
    public class AuditService : IAuditService
    {
        private static readonly TimeZoneInfo ArgentinaZone =
            TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");

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
            // Se anonimiza antes de guardar: la IP completa no llega ni a la base ni al log.
            var ip = IpAnonimizador.Anonimizar(_httpContextAccessor.HttpContext?.Connection.RemoteIpAddress);

            var log = new AuditLog
            {
                Fecha = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ArgentinaZone),
                Accion = accion,
                Email = email,
                UserId = userId,
                Detalle = detalle,
                IpAddress = ip
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("[AUDIT] {Accion} | Email: {Email} | IP: {Ip} | {Detalle}",
                accion, email ?? "-", ip ?? "-", detalle ?? "-");
        }
    }
}
