using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Avatar;
using catalogo_web_mvc.Interfaces.Email;
using catalogo_web_mvc.Interfaces.Favoritos;
using catalogo_web_mvc.Repository.Favoritos;
using catalogo_web_mvc.Services.Favoritos;
using catalogo_web_mvc.Services.Audit;
using catalogo_web_mvc.Interfaces.Carrito;
using catalogo_web_mvc.Interfaces.Categorias;
using catalogo_web_mvc.Interfaces.Marcas;
using catalogo_web_mvc.Interfaces.Pedidos;
using catalogo_web_mvc.Interfaces.Usuarios;
using catalogo_web_mvc.Models; // tu clase Usuario extendida de IdentityUser
using catalogo_web_mvc.Models.Settings;
using catalogo_web_mvc.Repository.Articulos;
using catalogo_web_mvc.Repository.Carrito;
using catalogo_web_mvc.Repository.Categorias;
using catalogo_web_mvc.Repository.Marcas;
using catalogo_web_mvc.Repository.Pedidos;
using catalogo_web_mvc.Services.Articulos;
using catalogo_web_mvc.Services.Avatar;
using catalogo_web_mvc.Services.Carrito;
using catalogo_web_mvc.Services.Categorias;
using catalogo_web_mvc.Services.Email;
using catalogo_web_mvc.Services.Identity;
using catalogo_web_mvc.Services.Marcas;
using catalogo_web_mvc.Services.Pedidos;
using catalogo_web_mvc.Services.Usuarios;
using Microsoft.AspNetCore.DataProtection;
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
// Los controladores de la API viven en Catalogo.Api, que en desarrollo corre como un
// proceso aparte detrás del gateway. En Azure no: el plan gratuito admite una sola
// instancia, y tres aplicaciones necesitarían tres. AddApplicationPart le dice a MVC que
// busque controladores también en ese ensamblado, así un único proceso atiende las vistas
// Razor y /api.
//
// Es una concesión al presupuesto, no al diseño: la separación real es la del compose, y
// volver a ella es quitar esta línea y la referencia del csproj. Que sea posible es
// justamente la prueba de que la API no depende del MVC.
//
// La contra a tener presente: producción y desarrollo dejan de compartir topología, y esa
// diferencia es de las que esconden errores.
builder.Services.AddControllersWithViews()
    .AddApplicationPart(typeof(catalogo_web_mvc.Controllers.Api.ArticulosApiController).Assembly);

// La cookie de sesion la emite esta aplicacion y la valida tambien Catalogo.Api,
// que corre en otro proceso. La cookie no dice quien es el usuario: es un texto
// cifrado con las claves que genera Data Protection. Sin un almacen comun cada
// aplicacion generaria el suyo y la API veria basura.
//
// El nombre de aplicacion tambien tiene que coincidir: forma parte del proposito
// con el que se derivan las claves, asi que con nombres distintos el descifrado
// falla aunque compartan el archivo.
var carpetaClaves = builder.Configuration["DataProtection:RutaClaves"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "..", "claves-compartidas");
Directory.CreateDirectory(carpetaClaves);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(carpetaClaves))
    .SetApplicationName("StoreSostaric");
// La base es serverless y se pausa sola: al despertar tarda cerca de un minuto y
// rechaza las conexiones mientras tanto. Sin reintentos, la migracion del arranque
// falla, la aplicacion muere y el contenedor reinicia en bucle sin llegar a esperarla.
builder.Services.AddDbContext<CatalogoContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(
        maxRetryCount: 6,
        maxRetryDelay: TimeSpan.FromSeconds(20),
        errorNumbersToAdd: null)));


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

    // Sin estas rutas Identity apunta a /Identity/Account/..., que no existe en este
    // proyecto: un usuario sin permisos recibia un 404 en lugar de una explicacion.
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    // Cuando este proceso aloja también los endpoints de la API (ver AddApplicationPart),
    // deja de correr el Program de Catalogo.Api, que es donde vivían estas reglas. Sin
    // ellas, una petición a /api sin sesión recibiría una redirección al login: el fetch
    // la seguiría, el navegador devolvería el HTML del formulario con estado 200, y el
    // front de React intentaría leerlo como JSON. El error aparecería lejos de la causa.
    //
    // Solo se altera /api. Una vista Razor sin sesión sigue yendo al login, que es lo que
    // corresponde cuando del otro lado hay una persona y no un fetch.
    options.Events.OnRedirectToLogin = contexto =>
    {
        if (contexto.Request.Path.StartsWithSegments("/api"))
        {
            contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        contexto.Response.Redirect(contexto.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = contexto =>
    {
        if (contexto.Request.Path.StartsWithSegments("/api"))
        {
            contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        contexto.Response.Redirect(contexto.RedirectUri);
        return Task.CompletedTask;
    };
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
builder.Services.AddScoped<ICarritoRepository, CarritoRepository>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IFavoritoRepository, FavoritoRepository>();
builder.Services.AddScoped<IFavoritoService, FavoritoService>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IUsuarioAdminService, UsuarioAdminService>();
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
// El front en React se compila con npm run build y aterriza en wwwroot/app.
// MapStaticAssets solo conoce los archivos que existian al compilar el proyecto,
// asi que esta carpeta necesita su propio middleware, igual que los avatares.
var carpetaFront = Path.Combine(app.Environment.WebRootPath, "app");
Directory.CreateDirectory(carpetaFront);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(carpetaFront),
    RequestPath = "/app"
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Los controladores de la API usan ruteo por atributos ([Route("api/...")]), que la ruta
// convencional de arriba no contempla. Detrás del gateway estos endpoints quedan opacados
// —/api va al otro proceso—, y en Azure son los que atienden.
app.MapControllers();

// El ruteo de React ocurre en el navegador: el servidor no conoce /app/articulo/2.
// Para cualquier ruta bajo /app se devuelve el index y React decide que pantalla
// corresponde. Sin esto la aplicacion navega bien pero falla al recargar.
app.MapFallbackToFile("/app/{*path}", "app/index.html");
using (var scope = app.Services.CreateScope())
{
    // En Docker la base arranca vacía: hay que aplicar las migraciones antes de
    // sembrar, o el seed corre contra tablas que todavía no existen.
    await scope.ServiceProvider.GetRequiredService<CatalogoContext>()
        .Database.MigrateAsync();

    // En produccion la contraseña del admin llega por configuracion. Si no se define,
    // el seeder cae al valor por defecto, que es publico y solo sirve para desarrollo.
    await DbSeeder.SeedAsync(
        scope.ServiceProvider,
        builder.Configuration["Seed:AdminPassword"],
        builder.Configuration["Seed:SuperAdminEmail"],
        builder.Configuration["Seed:SuperAdminPassword"]);
}

app.Run();

