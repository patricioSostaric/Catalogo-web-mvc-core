using catalogo_web_mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CatalogoWeb.Tests.Models
{
    public class MarcaTest
    {
        private static Marca MarcaValida() => new()
        {
            MarcaId = 1,
            Descripcion = "Samsung",
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
        public void Marca_Valida_NoTieneErroresDeValidacion()
        {
            var marca = MarcaValida();
            var errores = Validar(marca);
            Assert.Empty(errores);
        }

        [Fact]
        public void Marca_ToString_DevuelveDescripcion()
        {
            var marca = MarcaValida();
            Assert.Equal("Samsung", marca.ToString());
        }

        [Fact]
        public void Marca_ArticulosInicia_ListaVacia()
        {
            var marca = MarcaValida();
            Assert.NotNull(marca.Articulos);
            Assert.Empty(marca.Articulos);
        }

        // ── Descripcion ───────────────────────────────────────────────────────

        [Fact]
        public void Marca_DescripcionNula_FallaValidacion()
        {
            var marca = MarcaValida();
            marca.Descripcion = null!;
            var errores = Validar(marca);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Marca.Descripcion)));
        }

        [Fact]
        public void Marca_DescripcionVacia_FallaValidacion()
        {
            var marca = MarcaValida();
            marca.Descripcion = string.Empty;
            var errores = Validar(marca);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Marca.Descripcion)));
        }

        [Fact]
        public void Marca_DescripcionExacto100Caracteres_EsValida()
        {
            var marca = MarcaValida();
            marca.Descripcion = new string('M', 100);
            var errores = Validar(marca);
            Assert.Empty(errores);
        }

        [Fact]
        public void Marca_DescripcionMasDe100Caracteres_FallaValidacion()
        {
            var marca = MarcaValida();
            marca.Descripcion = new string('M', 101);
            var errores = Validar(marca);
            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Marca.Descripcion)));
        }

        // ── relación ──────────────────────────────────────────────────────────

        [Fact]
        public void Marca_PuedeAgregarArticulos_AColeccion()
        {
            var marca = MarcaValida();
            var articulos = (List<Articulo>)marca.Articulos;
            articulos.Add(new Articulo { Id = 1, Codigo = "TV-001", Nombre = "Smart TV", Precio = 85000m });

            Assert.Single(marca.Articulos);
        }
    }
}
