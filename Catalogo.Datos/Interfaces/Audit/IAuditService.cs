namespace catalogo_web_mvc.Interfaces.Audit
{
    public interface IAuditService
    {
        Task RegistrarAsync(string accion, string? email = null, string? userId = null, string? detalle = null);
    }
}
