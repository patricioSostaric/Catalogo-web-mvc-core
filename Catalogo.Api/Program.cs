using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Repository.Articulos;
using catalogo_web_mvc.Services.Articulos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddControllers();

var app = builder.Build();

// No hay migraciones al arrancar: de eso se sigue ocupando el MVC, que es el
// dueno del esquema. Dos aplicaciones migrando la misma base a la vez es una
// carrera esperando a ocurrir.
app.MapControllers();

app.Run();
