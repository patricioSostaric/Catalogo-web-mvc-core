using catalogo_web_mvc.Data;
using catalogo_web_mvc.Extensions;
using catalogo_web_mvc.Models; // tu clase Usuario extendida de IdentityUser
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Globalization;

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
builder.Services.AddControllersWithViews(options =>
{
    // Los mensajes por defecto de model binding vienen en inglés y no dicen qué se
    // esperaba. Con cultura es-AR el separador decimal es la coma, así que quien escribe
    // "35000.50" —lo que sale por costumbre y con el teclado numérico— recibía "The field
    // Precio must be a number" habiendo escrito un número perfectamente razonable.
    //
    // Estos accesores alcanzan a las dos validaciones: la del servidor y la del navegador,
    // porque el atributo data-val-number del formulario sale de acá.
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        campo => $"El campo {campo} debe ser un número. Usá coma para los decimales (por ejemplo: 35000,50).");

    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        valor => $"El valor {valor} no es válido.");

    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(
        campo => $"El campo {campo} es obligatorio.");

    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
        campo => $"El campo {campo} no puede estar vacío.");

    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (valor, campo) => $"El valor {valor} no es válido para {campo}.");
})
    .AddApplicationPart(typeof(catalogo_web_mvc.Controllers.Api.ArticulosApiController).Assembly);

// Cada bloque de configuracion vive en su propia extension sobre IServiceCollection:
// asi este archivo dice QUE se configura, y cada extension COMO. Antes eran doscientas
// lineas donde convivian Identity, el rate limiter, las cabeceras del proxy y el registro
// de nueve modulos, sin separacion visible entre una cosa y la otra.
builder.Services.AddClavesCompartidas(builder.Configuration, builder.Environment);
builder.Services.AddPersistencia(builder.Configuration);
builder.Services.AddIdentidad(builder.Environment);
builder.Services.AddProteccionDeAbuso();
builder.Services.AddCabecerasDeProxy();
builder.Services.AddCorreo(builder.Configuration);
builder.Services.AddServiciosDelCatalogo();

builder.Services.AddHttpContextAccessor();

// Las fechas de la auditoria se muestran en hora argentina sin importar donde corra el
// contenedor, que en Azure esta en UTC.
builder.Services.AddSingleton(TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));

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

