using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace catalogo_web_mvc.Data
{
    public class CatalogoContext : IdentityDbContext<ApplicationUser>
    {
        public CatalogoContext(DbContextOptions<CatalogoContext> options): base(options){}

        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<ArticuloFavorito> ArticuloFavoritos { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relaciones
            modelBuilder.Entity<Articulo>()
                .HasOne(a => a.Marca)
                .WithMany(m => m.Articulos)
                .HasForeignKey(a => a.MarcaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Articulo>()
                .HasOne(a => a.Categoria)
                .WithMany(c => c.Articulos)
                .HasForeignKey(a => a.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de columnas
            modelBuilder.Entity<Articulo>()
                .Property(a => a.Precio)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Articulo>()
            .Property(a => a.ImagenUrl)
            .HasMaxLength(500);

            modelBuilder.Entity<Articulo>()
            .Property(a => a.Activo)
            .HasDefaultValue(true);

            modelBuilder.Entity<ArticuloFavorito>()
                .HasOne(f => f.Usuario)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArticuloFavorito>()
                .HasOne(f => f.Articulo)
                .WithMany()
                .HasForeignKey(f => f.ArticuloId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArticuloFavorito>()
                .HasIndex(f => new { f.UserId, f.ArticuloId })
                .IsUnique();

            // Seed de Marcas
            modelBuilder.Entity<Marca>().HasData(
                new Marca { MarcaId = 1, Descripcion = "Samsung" },
                new Marca { MarcaId = 2, Descripcion = "Apple" },
                new Marca { MarcaId = 3, Descripcion = "Sony" },
                new Marca { MarcaId = 4, Descripcion = "Huawei" },
                new Marca { MarcaId = 5, Descripcion = "Motorola" }
            );

            // Seed de Categorías
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { CategoriaId = 1, Descripcion = "Celulares" },
                new Categoria { CategoriaId = 2, Descripcion = "Televisores" },
                new Categoria { CategoriaId = 3, Descripcion = "Media" },
                new Categoria { CategoriaId = 4, Descripcion = "Audio" }
            );

            // Seed de Artículos
            modelBuilder.Entity<Articulo>().HasData(
                new Articulo { Id = 1,  Codigo = "S01", Nombre = "Galaxy S10",           Descripcion = "Una canoa cara",                          MarcaId = 1, CategoriaId = 1, ImagenUrl = "/imagen/articulos/s01.jpg", Precio = 69999,  Stock = 10, Activo = true },
                new Articulo { Id = 2,  Codigo = "M03", Nombre = "Moto G Play 7ma Gen",  Descripcion = "Ya siete de estos?",                      MarcaId = 5, CategoriaId = 1, ImagenUrl = "/imagen/articulos/m03.jpg",                                                                                                                        Precio = 15699,  Stock = 10, Activo = true },
                new Articulo { Id = 3,  Codigo = "S99", Nombre = "Play 4",               Descripcion = "Ya no se cuantas versiones hay",          MarcaId = 3, CategoriaId = 3, ImagenUrl = "/imagen/articulos/s99.jpg",                                                                                                            Precio = 35000,  Stock = 10, Activo = true },
                new Articulo { Id = 4,  Codigo = "S56", Nombre = "Bravia 55",            Descripcion = "Alta tele",                               MarcaId = 3, CategoriaId = 2, ImagenUrl = "/imagen/articulos/s56.jpg",                                                                                                                             Precio = 49500,  Stock = 10, Activo = true },
                new Articulo { Id = 5,  Codigo = "A23", Nombre = "Apple TV",             Descripcion = "lindo loro",                              MarcaId = 2, CategoriaId = 3, ImagenUrl = "/imagen/articulos/a23.jpg",                      Precio = 7850,   Stock = 10, Activo = true },
                new Articulo { Id = 6,  Codigo = "H01", Nombre = "Huawei P30",           Descripcion = "Celular con triple cámara",               MarcaId = 4, CategoriaId = 1, ImagenUrl = "/imagen/articulos/h01.jpeg", Precio = 45999, Stock = 10, Activo = true },
                new Articulo { Id = 7,  Codigo = "S20", Nombre = "Samsung QLED 65",      Descripcion = "Televisor de alta definición",            MarcaId = 1, CategoriaId = 2, ImagenUrl = "/imagen/articulos/s20.jpg",                                                                                    Precio = 120000, Stock = 10, Activo = true },
                new Articulo { Id = 8,  Codigo = "A50", Nombre = "AirPods Pro",          Descripcion = "Auriculares inalámbricos con cancelación", MarcaId = 2, CategoriaId = 4, ImagenUrl = "/imagen/articulos/a50.jpeg", Precio = 8999, Stock = 10, Activo = true },
                new Articulo { Id = 9,  Codigo = "M10", Nombre = "Moto E20",             Descripcion = "Celular económico",                       MarcaId = 5, CategoriaId = 1, ImagenUrl = "/imagen/articulos/m10.jpg",                                                                                                                       Precio = 10999,  Stock = 10, Activo = true },
                new Articulo { Id = 10, Codigo = "S77", Nombre = "Sony WH-1000XM4",      Descripcion = "Auriculares con cancelación de ruido",    MarcaId = 3, CategoriaId = 4, ImagenUrl = "/imagen/articulos/s77.webp", Precio = 29999, Stock = 10, Activo = true },
                new Articulo { Id = 11, Codigo = "A99", Nombre = "Apple iPad Pro",       Descripcion = "Tablet de alto rendimiento",              MarcaId = 2, CategoriaId = 3, ImagenUrl = "/imagen/articulos/a99.jpg",                                                                                    Precio = 150000, Stock = 10, Activo = true },
                new Articulo { Id = 12, Codigo = "H55", Nombre = "Huawei MateBook",      Descripcion = "Notebook ligera",                         MarcaId = 4, CategoriaId = 3, ImagenUrl = "/imagen/articulos/h55.jpg",                                                                                    Precio = 95000,  Stock = 10, Activo = true },
                new Articulo { Id = 13, Codigo = "S88", Nombre = "Samsung Galaxy Buds",  Descripcion = "Auriculares inalámbricos",                MarcaId = 1, CategoriaId = 4, ImagenUrl = "/imagen/articulos/s88.webp", Precio = 12999, Stock = 10, Activo = true },
                new Articulo { Id = 14, Codigo = "M22", Nombre = "Motorola Edge 30",     Descripcion = "Celular gama media",                      MarcaId = 5, CategoriaId = 1, ImagenUrl = "/imagen/articulos/m22.jpeg",                                                                                                        Precio = 39999,  Stock = 10, Activo = true },
                new Articulo { Id = 15, Codigo = "S90", Nombre = "Sony PlayStation 5",   Descripcion = "Consola de última generación",            MarcaId = 3, CategoriaId = 3, ImagenUrl = "/imagen/articulos/s90.jpeg", Precio = 250000, Stock = 10, Activo = true }
            );

        }


    }

}
