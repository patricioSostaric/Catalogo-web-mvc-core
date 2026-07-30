using catalogo_web_mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CatalogoWeb.Tests.Models
{
    public class ArticuloTest
    {
        // ── helpers ──────────────────────────────────────────────────────────

        private static Articulo ArticuloValido() => new()
        {
            Id = 1,
            Codigo = "ART-001",
            Nombre = "Tornillo M8",
            Precio = 150.50m,
            MarcaId = 1,
            CategoriaId = 1
        };

        /// <summary>Ejecuta DataAnnotations sobre el objeto y devuelve los errores.</summary>
        private static IList<ValidationResult> Validar(object obj)
        {
            var ctx = new ValidationContext(obj);
            var errores = new List<ValidationResult>();
            Validator.TryValidateObject(obj, ctx, errores, validateAllProperties: true);
            return errores;
        }

        // ── casos felices ─────────────────────────────────────────────────────

        [Fact]
        public void Articulo_Valido_NoTieneErroresDeValidacion()
        {
            var articulo = ArticuloValido();
            var errores = Validar(articulo);
            Assert.Empty(errores);
        }

        [Fact]
        public void Articulo_ImagenUrl_PuedeSerNull()
        {
            var articulo = ArticuloValido();
            articulo.ImagenUrl = null;
            var errores = Validar(articulo);
            Assert.Empty(errores);
        }

        [Fact]
        public void Articulo_Descripcion_PuedeSerNull()
        {
            var articulo = ArticuloValido();
            articulo.Descripcion = null;
            var errores = Validar(articulo);
            Assert.Empty(errores);
        }

        [Fact]
        public void Articulo_PrecioCero_FallaValidacion()
        {
            var articulo = ArticuloValido();
            articulo.Precio = 0m;
            var errores = Validar(articulo);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Articulo.Precio)));
        }

        [Fact]
        public void Articulo_PrecioNegativo_FallaValidacion()
        {
            var articulo = ArticuloValido();
            articulo.Precio = -1m;
            var errores = Validar(articulo);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Articulo.Precio)));
        }

        [Fact]
        public void Articulo_Precio_AdmiteDecimales()
        {
            var articulo = ArticuloValido();
            articulo.Precio = 9999.99m;
            var errores = Validar(articulo);
            Assert.Empty(errores);
        }

        // ── Codigo ────────────────────────────────────────────────────────────

        [Fact]
        public void Articulo_CodigoNulo_FallaValidacion()
        {
            var articulo = ArticuloValido();
            articulo.Codigo = null!;
            var errores = Validar(articulo);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Articulo.Codigo)));
        }

        [Fact]
        public void Articulo_CodigoVacio_FallaValidacion()
        {
            var articulo = ArticuloValido();
            articulo.Codigo = string.Empty;
            var errores = Validar(articulo);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Articulo.Codigo)));
        }

        [Fact]
        public void Articulo_CodigoExacto50Caracteres_EsValido()
        {
            var articulo = ArticuloValido();
            articulo.Codigo = new string('X', 50);
            var errores = Validar(articulo);
            Assert.Empty(errores);
        }

        [Fact]
        public void Articulo_CodigoMasDe50Caracteres_FallaValidacion()
        {
            var articulo = ArticuloValido();
            articulo.Codigo = new string('X', 51);
            var errores = Validar(articulo);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Articulo.Codigo)));
        }

        // ── Nombre ────────────────────────────────────────────────────────────

        [Fact]
        public void Articulo_NombreNulo_FallaValidacion()
        {
            var articulo = ArticuloValido();
            articulo.Nombre = null!;
            var errores = Validar(articulo);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Articulo.Nombre)));
        }

        [Fact]
        public void Articulo_NombreExacto100Caracteres_EsValido()
        {
            var articulo = ArticuloValido();
            articulo.Nombre = new string('N', 100);
            var errores = Validar(articulo);
            Assert.Empty(errores);
        }

        [Fact]
        public void Articulo_NombreMasDe100Caracteres_FallaValidacion()
        {
            var articulo = ArticuloValido();
            articulo.Nombre = new string('N', 101);
            var errores = Validar(articulo);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Articulo.Nombre)));
        }

        // ── Descripcion ───────────────────────────────────────────────────────

        [Fact]
        public void Articulo_DescripcionExacto250Caracteres_EsValido()
        {
            var articulo = ArticuloValido();
            articulo.Descripcion = new string('D', 250);
            var errores = Validar(articulo);
            Assert.Empty(errores);
        }

        [Fact]
        public void Articulo_DescripcionMasDe250Caracteres_FallaValidacion()
        {
            var articulo = ArticuloValido();
            articulo.Descripcion = new string('D', 251);
            var errores = Validar(articulo);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Articulo.Descripcion)));
        }

        // ── Stock ─────────────────────────────────────────────────────────────

        [Fact]
        public void Articulo_StockCero_EsValido()
        {
            var articulo = ArticuloValido();
            articulo.Stock = 0;
            var errores = Validar(articulo);
            Assert.Empty(errores);
        }

        [Fact]
        public void Articulo_StockPositivo_EsValido()
        {
            var articulo = ArticuloValido();
            articulo.Stock = 100;
            var errores = Validar(articulo);
            Assert.Empty(errores);
        }

        [Fact]
        public void Articulo_StockNegativo_FallaValidacion()
        {
            var articulo = ArticuloValido();
            articulo.Stock = -1;
            var errores = Validar(articulo);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Articulo.Stock)));
        }

        [Fact]
        public void Articulo_StockPorDefecto_EsCero()
        {
            var articulo = new Articulo();
            Assert.Equal(0, articulo.Stock);
        }

        // ── ImagenUrl ─────────────────────────────────────────────────────────

        // Desde que ImagenUrl valida el formato con [UrlImagenSegura], los tests de
        // longitud usan una URL https real rellenada hasta el largo buscado; una cadena
        // de 'x' fallaría por no ser una URL y no probaría el límite de caracteres.
        private static string UrlHttpsDeLargo(int largo)
        {
            const string prefijo = "https://ejemplo.com/";
            return prefijo + new string('x', largo - prefijo.Length);
        }

        [Fact]
        public void Articulo_ImagenUrlExacto500Caracteres_EsValido()
        {
            var articulo = ArticuloValido();
            articulo.ImagenUrl = UrlHttpsDeLargo(500);

            Assert.Equal(500, articulo.ImagenUrl.Length);
            Assert.Empty(Validar(articulo));
        }

        [Fact]
        public void Articulo_ImagenUrlMasDe500Caracteres_FallaValidacion()
        {
            var articulo = ArticuloValido();
            articulo.ImagenUrl = UrlHttpsDeLargo(501);
            var errores = Validar(articulo);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Articulo.ImagenUrl)));
        }

        // ── navegación ────────────────────────────────────────────────────────

        [Fact]
        public void Articulo_AsignarMarca_PropiedadNavegacionCorrecta()
        {
            var marca = new Marca { MarcaId = 5, Descripcion = "Samsung" };
            var articulo = ArticuloValido();
            articulo.Marca = marca;
            articulo.MarcaId = marca.MarcaId;

            Assert.Equal(5, articulo.MarcaId);
            Assert.Equal("Samsung", articulo.Marca.Descripcion);
        }

        [Fact]
        public void Articulo_AsignarCategoria_PropiedadNavegacionCorrecta()
        {
            var categoria = new Categoria { CategoriaId = 3, Descripcion = "Electrónica" };
            var articulo = ArticuloValido();
            articulo.Categoria = categoria;
            articulo.CategoriaId = categoria.CategoriaId;

            Assert.Equal(3, articulo.CategoriaId);
            Assert.Equal("Electrónica", articulo.Categoria.Descripcion);
        }

        // ── Activo ────────────────────────────────────────────────────────────

        [Fact]
        public void Articulo_Activo_ValorPorDefectoEsTrue()
        {
            var articulo = new Articulo();
            Assert.True(articulo.Activo);
        }

        [Fact]
        public void Articulo_Activo_PuedeSetearseTrue()
        {
            var articulo = ArticuloValido();
            articulo.Activo = true;
            var errores = Validar(articulo);
            Assert.Empty(errores);
            Assert.True(articulo.Activo);
        }

        [Fact]
        public void Articulo_Activo_PuedeSeteaseFalse()
        {
            var articulo = ArticuloValido();
            articulo.Activo = false;
            var errores = Validar(articulo);
            Assert.Empty(errores);
            Assert.False(articulo.Activo);
        }

        // ── Stock ─────────────────────────────────────────────────────────────

        [Fact]
        public void Articulo_Stock_ValorPorDefectoEsCero()
        {
            var articulo = new Articulo();
            Assert.Equal(0, articulo.Stock);
        }

        [Fact]
        public void Articulo_Stock_PuedeAsignarseValorPositivo()
        {
            var articulo = ArticuloValido();
            articulo.Stock = 100;
            var errores = Validar(articulo);
            Assert.Empty(errores);
            Assert.Equal(100, articulo.Stock);
        }

        [Fact]
        public void Articulo_Stock_PuedeSerCero()
        {
            var articulo = ArticuloValido();
            articulo.Stock = 0;
            var errores = Validar(articulo);
            Assert.Empty(errores);
        }

    }
}
