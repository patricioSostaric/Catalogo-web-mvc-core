using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Avatar;
using catalogo_web_mvc.Interfaces.Email;
using catalogo_web_mvc.Services.Audit;
using catalogo_web_mvc.Interfaces.Categorias;
using catalogo_web_mvc.Interfaces.Marcas;
using catalogo_web_mvc.Models; // tu clase Usuario extendida de IdentityUser
using catalogo_web_mvc.Models.Settings;
using catalogo_web_mvc.Repository.Articulos;
using catalogo_web_mvc.Repository.Categorias;
using catalogo_web_mvc.Repository.Marcas;
using catalogo_web_mvc.Services.Articulos;
using catalogo_web_mvc.Services.Avatar;
using catalogo_web_mvc.Services.Categorias;
using catalogo_web_mvc.Services.Email;
using catalogo_web_mvc.Services.Identity;
using catalogo_web_mvc.Services.Marcas;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Globalization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configurar cultura por defecto (ejemplo Argentina)
var defaultCulture = new CultureInfo("es-AR");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};

// Activar User Secrets en desarrollo
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Obtener la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("CatalogoDB");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<CatalogoContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // Política de contraseña
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout por intentos fallidos
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<CatalogoContext>()
.AddErrorDescriber<SpanishIdentityErrorDescriber>()
// Suma el avatar como claim para que el navbar no consulte la base en cada request.
.AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    // En producción la cookie viaja solo por HTTPS. En desarrollo se usa SameAsRequest
    // porque el contenedor sirve HTTP plano: con Always el navegador descarta la cookie
    // y el login falla sin mostrar error.
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

// Detrás de un proxy (Azure App Service, Fly, Render) el TLS lo termina la plataforma
// y la app recibe HTTP plano. Sin estas cabeceras Kestrel cree que el request no es
// seguro: la cookie con SecurePolicy.Always nunca se emite y el login falla en silencio.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // La IP del proxy no se conoce de antemano en un contenedor, así que se vacían las
    // listas de confianza. Esto asume que la plataforma es el único camino de entrada:
    // si la app quedara accesible de forma directa, cualquiera podría falsear el origen.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAvatarService, AvatarService>();

// Techo de tamaño para los multipart. El límite real de la imagen lo aplica
// AvatarValidator (2 MB); este corta el request antes de bufferearlo entero.
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = AvatarValidator.MaxBytes;
});

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<ISmtpClient, SmtpClientAdapter>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

// Repository + Service
builder.Services.AddScoped<IArticuloRepository, ArticuloRepository>();
builder.Services.AddScoped<IArticuloService, ArticuloService>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IMarcaRepository, MarcaRepository>();
builder.Services.AddScoped<IMarcaService, MarcaService>();
var argentinaZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
builder.Services.AddSingleton(argentinaZone);

var app = builder.Build();

// Va primero: el resto del pipeline (HSTS, redirección a HTTPS, cookies) necesita
// saber si el request original era HTTPS antes de tomar cualquier decisión.
app.UseForwardedHeaders();

// Middleware de localización: esto es lo que hace que el binder entienda la coma
app.UseRequestLocalization(localizationOptions);


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' https: data:; " +
        "font-src 'self'; " +
        "connect-src 'self'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'none'");
    await next();
});

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// MapStaticAssets solo sirve los archivos que existían al compilar. Los avatares se
// suben en runtime, así que necesitan su propio middleware apuntado a esa carpeta.
// ServeUnknownFileTypes queda en false (default) a propósito: si un archivo llegara a
// tener una extensión desconocida, se responde 404 en lugar de servirlo a ciegas.
var carpetaUploads = Path.Combine(app.Environment.WebRootPath, "uploads");
Directory.CreateDirectory(carpetaUploads);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(carpetaUploads),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // Refuerzo sobre el contenido subido por usuarios: nunca se reinterpreta el
        // tipo y nunca se muestra inline como documento.
        ctx.Context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        ctx.Context.Response.Headers.Append("Content-Disposition", "inline");
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    // En Docker la base arranca vacía: hay que aplicar las migraciones antes de
    // sembrar, o el seed corre contra tablas que todavía no existen.
    await scope.ServiceProvider.GetRequiredService<CatalogoContext>()
        .Database.MigrateAsync();

    // En produccion la contraseña del admin llega por configuracion. Si no se define,
    // el seeder cae al valor por defecto, que es publico y solo sirve para desarrollo.
    await DbSeeder.SeedAsync(scope.ServiceProvider, builder.Configuration["Seed:AdminPassword"]);
}

app.Run();

