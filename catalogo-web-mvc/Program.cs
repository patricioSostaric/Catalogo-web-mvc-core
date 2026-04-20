using catalogo_web_mvc.Data;
using catalogo_web_mvc.Models; // tu clase Usuario extendida de IdentityUser
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Configurar cultura por defecto (ejemplo Argentina)
var defaultCulture = new CultureInfo("es-AR");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMvc();
builder.Services.AddDbContext<CatalogoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogoDB")));

var app = builder.Build();

// Middleware de localización: esto es lo que hace que el binder entienda la coma
app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Articulo}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
