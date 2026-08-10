using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Interfaces.Favoritos;
using catalogo_web_mvc.Repository.Articulos;
using catalogo_web_mvc.Repository.Favoritos;
using catalogo_web_mvc.Services.Articulos;
using catalogo_web_mvc.Services.Favoritos;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Mismo almacen de claves y mismo nombre de aplicacion que el MVC. Es lo que le
// permite a esta aplicacion descifrar la cookie de sesion que emitio el otro
// proceso: la cookie no dice quien es el usuario, va cifrada.
var carpetaClaves = builder.Configuration["DataProtection:RutaClaves"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "..", "claves-compartidas");
Directory.CreateDirectory(carpetaClaves);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(carpetaClaves))
    .SetApplicationName("StoreSostaric");

// La API solo lee la cookie: no emite sesiones ni tiene pantalla de login. De eso
// se sigue ocupando el MVC, que es el unico que conoce las credenciales.
//
// El esquema y el nombre de la cookie son los que usa Identity por defecto; si no
// coincidieran, la API buscaria una cookie que nadie emite.
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.Cookie.Name = ".AspNetCore.Identity.Application";

        // Sin esto, una peticion sin sesion recibiria una redireccion al login,
        // que para un cliente que espera JSON es peor que un error claro.
        options.Events.OnRedirectToLogin = contexto =>
        {
            contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = contexto =>
        {
            contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("CatalogoDB");

// Mismos reintentos que el MVC: la base es serverless y al despertar rechaza
// conexiones durante cerca de un minuto.
builder.Services.AddDbContext<CatalogoContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(
        maxRetryCount: 6,
        maxRetryDelay: TimeSpan.FromSeconds(20),
        errorNumbersToAdd: null)));

builder.Services.AddScoped<IArticuloRepository, ArticuloRepository>();
builder.Services.AddScoped<IArticuloService, ArticuloService>();
builder.Services.AddScoped<IFavoritoRepository, FavoritoRepository>();
builder.Services.AddScoped<IFavoritoService, FavoritoService>();

builder.Services.AddControllers();

var app = builder.Build();

// No hay migraciones al arrancar: de eso se sigue ocupando el MVC, que es el
// dueno del esquema. Dos aplicaciones migrando la misma base a la vez es una
// carrera esperando a ocurrir.
// El orden importa: primero se averigua quien es (autenticacion) y despues si
// puede (autorizacion). Invertidos, los atributos [Authorize] evaluarian sobre un
// usuario que todavia nadie identifico.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
