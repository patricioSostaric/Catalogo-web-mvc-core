var builder = WebApplication.CreateBuilder(args);

// Las rutas y destinos se leen de appsettings: cambiar a donde va cada ruta no
// exige recompilar, que es lo que hace practico mover una funcionalidad por vez.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();

app.Run();