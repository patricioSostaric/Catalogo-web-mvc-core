using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace catalogo_web_mvc.Data
{
    public class CatalogoContext : DbContext
    {
        public CatalogoContext(DbContextOptions<CatalogoContext> options): base(options){}

        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Marca> Marcas { get; set; }



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

            // Seed de Artículos (15 en total)
            modelBuilder.Entity<Articulo>().HasData(
                // Los 5 iniciales
                new Articulo { Id = 1, Codigo = "S01", Nombre = "Galaxy S10", Descripcion = "Una canoa cara", MarcaId = 1, CategoriaId = 1, ImagenUrl = "https://images.samsung.com/...jpg", Precio = 69999 },
                new Articulo { Id = 2, Codigo = "M03", Nombre = "Moto G Play 7ma Gen", Descripcion = "Ya siete de estos?", MarcaId = 5, CategoriaId = 1, ImagenUrl = "https://www.motorola.cl/...png", Precio = 15699 },
                new Articulo { Id = 3, Codigo = "S99", Nombre = "Play 4", Descripcion = "Ya no se cuantas versiones hay", MarcaId = 3, CategoriaId = 3, ImagenUrl = "sin_imagen_para_que_de_error....muejeje", Precio = 35000 },
                new Articulo { Id = 4, Codigo = "S56", Nombre = "Bravia 55", Descripcion = "Alta tele", MarcaId = 3, CategoriaId = 2, ImagenUrl = "https://intercompras.com/...jpg", Precio = 49500 },
                new Articulo { Id = 5, Codigo = "A23", Nombre = "Apple TV", Descripcion = "lindo loro", MarcaId = 2, CategoriaId = 3, ImagenUrl = "https://store.storeimages.cdn-apple.com/...jpg", Precio = 7850 },

                // 10 adicionales
                new Articulo { Id = 6, Codigo = "H01", Nombre = "Huawei P30", Descripcion = "Celular con triple cámara", MarcaId = 4, CategoriaId = 1, ImagenUrl = "https://consumer.huawei.com/...jpg", Precio = 45999 },
                new Articulo { Id = 7, Codigo = "S20", Nombre = "Samsung QLED 65", Descripcion = "Televisor de alta definición", MarcaId = 1, CategoriaId = 2, ImagenUrl = "https://images.samsung.com/...jpg", Precio = 120000 },
                new Articulo { Id = 8, Codigo = "A50", Nombre = "AirPods Pro", Descripcion = "Auriculares inalámbricos con cancelación", MarcaId = 2, CategoriaId = 4, ImagenUrl = "https://store.storeimages.cdn-apple.com/...jpg", Precio = 8999 },
                new Articulo { Id = 9, Codigo = "M10", Nombre = "Moto E20", Descripcion = "Celular económico", MarcaId = 5, CategoriaId = 1, ImagenUrl = "https://motorola.com/...jpg", Precio = 10999 },
                new Articulo { Id = 10, Codigo = "S77", Nombre = "Sony WH-1000XM4", Descripcion = "Auriculares con cancelación de ruido", MarcaId = 3, CategoriaId = 4, ImagenUrl = "https://sony.com/...jpg", Precio = 29999 },
                new Articulo { Id = 11, Codigo = "A99", Nombre = "Apple iPad Pro", Descripcion = "Tablet de alto rendimiento", MarcaId = 2, CategoriaId = 3, ImagenUrl = "https://store.storeimages.cdn-apple.com/...jpg", Precio = 150000 },
                new Articulo { Id = 12, Codigo = "H55", Nombre = "Huawei MateBook", Descripcion = "Notebook ligera", MarcaId = 4, CategoriaId = 3, ImagenUrl = "https://consumer.huawei.com/...jpg", Precio = 95000 },
                new Articulo { Id = 13, Codigo = "S88", Nombre = "Samsung Galaxy Buds", Descripcion = "Auriculares inalámbricos", MarcaId = 1, CategoriaId = 4, ImagenUrl = "https://images.samsung.com/...jpg", Precio = 12999 },
                new Articulo { Id = 14, Codigo = "M22", Nombre = "Motorola Edge 30", Descripcion = "Celular gama media", MarcaId = 5, CategoriaId = 1, ImagenUrl = "https://motorola.com/...jpg", Precio = 39999 },
                new Articulo { Id = 15, Codigo = "S90", Nombre = "Sony PlayStation 5", Descripcion = "Consola de última generación", MarcaId = 3, CategoriaId = 3, ImagenUrl = "https://sony.com/...jpg", Precio = 250000 }
            );
        }


    }

}
