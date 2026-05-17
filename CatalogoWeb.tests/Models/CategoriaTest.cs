using catalogo_web_mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CatalogoWeb.Tests.Models
{
    public class CategoriaTest
    {
        private static Categoria CategoriaValida() => new()
        {
            CategoriaId = 1,
            Descripcion = "Electrónica",
            Articulos = new List<Articulo>()
        };

        private static IList<ValidationResult> Validar(object obj)
        {
            var ctx = new ValidationContext(obj);
            var errores = new List<ValidationResult>();
            Validator.TryValidateObject(obj, ctx, errores, validateAllProperties: true);
            return errores;
        }

        // ── casos felices ─────────────────────────────────────────────────────

        [Fact]
        public void Categoria_Valida_NoTieneErroresDeValidacion()
        {
            var categoria = CategoriaValida();
            var errores = Validar(categoria);
            Assert.Empty(errores);
        }

        [Fact]
        public void Categoria_ToString_DevuelveDescripcion()
        {
            var categoria = CategoriaValida();
            Assert.Equal("Electrónica", categoria.ToString());
        }

        [Fact]
        public void Categoria_ArticulosInicia_ListaVacia()
        {
            var categoria = CategoriaValida();
            Assert.NotNull(categoria.Articulos);
            Assert.Empty(categoria.Articulos);
        }

        // ── Descripcion ───────────────────────────────────────────────────────

        [Fact]
        public void Categoria_DescripcionNula_FallaValidacion()
        {
            var categoria = CategoriaValida();
            categoria.Descripcion = null!;
            var errores = Validar(categoria);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Categoria.Descripcion)));
        }

        [Fact]
        public void Categoria_DescripcionVacia_FallaValidacion()
        {
            var categoria = CategoriaValida();
            categoria.Descripcion = string.Empty;
            var errores = Validar(categoria);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Categoria.Descripcion)));
        }

        [Fact]
        public void Categoria_DescripcionExacto100Caracteres_EsValida()
        {
            var categoria = CategoriaValida();
            categoria.Descripcion = new string('C', 100);
            var errores = Validar(categoria);
            Assert.Empty(errores);
        }

        [Fact]
        public void Categoria_DescripcionMasDe100Caracteres_FallaValidacion()
        {
            var categoria = CategoriaValida();
            categoria.Descripcion = new string('C', 101);
            var errores = Validar(categoria);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Categoria.Descripcion)));
        }

        // ── relación ──────────────────────────────────────────────────────────

        [Fact]
        public void Categoria_PuedeAgregarArticulos_AColeccion()
        {
            var categoria = CategoriaValida();
            var articulos = (List<Articulo>)categoria.Articulos;
            articulos.Add(new Articulo { Id = 1, Codigo = "A1", Nombre = "Auricular", Precio = 500m });

            Assert.Single(categoria.Articulos);
        }
    }
}
