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
            .HasMaxLength(500); // suficiente para URLs largas

        }


    }

}
