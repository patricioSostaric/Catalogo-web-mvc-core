using catalogo_web_mvc.Controllers.Api;
using catalogo_web_mvc.Data;
using catalogo_web_mvc.Interfaces.Categorias;
using catalogo_web_mvc.Interfaces.Marcas;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.Dtos;
using catalogo_web_mvc.Repository.Categorias;
using catalogo_web_mvc.Repository.Marcas;
using catalogo_web_mvc.Services.Categorias;
using catalogo_web_mvc.Services.Marcas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogoWeb.Tests.Controllers
{
    public class CategoriasApiControllerTests
    {
        private static CatalogoContext CrearContexto() =>
           new(new DbContextOptionsBuilder<CatalogoContext>()
               .UseInMemoryDatabase(Guid.NewGuid().ToString())
               .Options);

        // El servicio real sobre la base en memoria, igual que en el resto de los tests
        // de API: no se simula con Moq porque asi se prueba tambien la consulta.
        private static ICategoriaService Categorias(CatalogoContext context) =>
            new CategoriaService(new CategoriaRepository(context));

        // No hace falta armar un principal autenticado como en favoritos: este
        // controlador no lee el usuario, solo exige el rol via [Authorize].
        private static CategoriasApiController CrearController(CatalogoContext context) =>
            new(Categorias(context));

        private static async Task SembrarAsync(CatalogoContext context)
        {
            context.Categorias.AddRange(
                new Categoria { CategoriaId = 1, Descripcion = "Celulares" },
                new Categoria { CategoriaId = 2, Descripcion = "Tabletas" }
            );
            await context.SaveChangesAsync();
        }

        // Una categoria con un articulo asociado: es la que no se puede borrar.
        private static async Task SembrarConArticuloAsync(CatalogoContext context)
        {
            await SembrarAsync(context);
            context.Marcas.Add(new Marca { MarcaId = 1, Descripcion = "Samsung" });
            context.Articulos.Add(new Articulo
            {
                Id = 1,
                Codigo = "S01",
                Nombre = "Galaxy S10",
                Descripcion = "desc",
                MarcaId = 1,
                CategoriaId = 1,
                Precio = 239000
            });
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Get_DevuelveTodasLasCategorias()
        {
            using var context = CrearContexto();
            await SembrarAsync(context);

            var resultado = await CrearController(context).Get();

            var categorias = Assert.IsType<List<CategoriaDto>>(resultado.Value);
            Assert.Equal(2, categorias.Count);
            Assert.Contains(categorias, c => c.CategoriaId == 1 && c.Descripcion == "Celulares");
            Assert.Contains(categorias, c => c.CategoriaId == 2 && c.Descripcion == "Tabletas");
        }

        [Fact]
        public async Task Get_SinCategorias_DevuelveListaVacia()
        {
            using var context = CrearContexto();

            var resultado = await CrearController(context).Get();

            Assert.Empty(Assert.IsType<List<CategoriaDto>>(resultado.Value));
        }

        [Fact]
        public async Task GetPorId_Existente_DevuelveLaCategoria()
        {
            using var context = CrearContexto();
            await SembrarAsync(context);

            var resultado = await CrearController(context).GetPorId(1);

            var ok = Assert.IsType<OkObjectResult>(resultado.Result);
            var categoria = Assert.IsType<CategoriaDto>(ok.Value);
            Assert.Equal(1, categoria.CategoriaId);
            Assert.Equal("Celulares", categoria.Descripcion);
        }

        [Fact]
        public async Task GetPorId_Inexistente_DevuelveNotFound()
        {
            using var context = CrearContexto();
            await SembrarAsync(context);

            var resultado = await CrearController(context).GetPorId(99);

            Assert.IsType<NotFoundResult>(resultado.Result);
        }

        [Fact]
        public async Task Post_CreaLaMarcaYDevuelve201()
        {
            using var context = CrearContexto();
            await SembrarAsync(context);

            var resultado = await CrearController(context).Post(new CategoriaNuevaDto { Descripcion = "LG" });

            var creado = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            var dto = Assert.IsType<CategoriaDto>(creado.Value);
            Assert.Equal("LG", dto.Descripcion);

            // El id lo genera la base: el DTO no puede devolver 0.
            Assert.True(dto.CategoriaId > 0);

            // Y la categoria quedo guardada, no solo devuelta.
            Assert.Equal(3, await context.Categorias.CountAsync());
        }

        [Fact]
        public async Task Put_Existente_ActualizaLaDescripcion()
        {
            using var context = CrearContexto();
            await SembrarAsync(context);

            var resultado = await CrearController(context).Put(1, new CategoriaNuevaDto { Descripcion = "Samsung Electronics" });

            Assert.IsType<NoContentResult>(resultado);
            Assert.Equal("Samsung Electronics", (await context.Categorias.FindAsync(1))!.Descripcion);
        }

        [Fact]
        public async Task Put_Inexistente_DevuelveNotFound()
        {
            using var context = CrearContexto();
            await SembrarAsync(context);

            var resultado = await CrearController(context).Put(99, new CategoriaNuevaDto { Descripcion = "Nada" });

            Assert.IsType<NotFoundResult>(resultado);
        }

        [Fact]
        public async Task Delete_SinArticulos_BorraLaCategoria()
        {
            using var context = CrearContexto();
            await SembrarAsync(context);

            var resultado = await CrearController(context).Delete(2);

            Assert.IsType<NoContentResult>(resultado);
            Assert.Null(await context.Marcas.FindAsync(2));
        }

        // La razon de ser del 409: la clave foranea impide el borrado, y responder 500
        // diria que el servidor se rompio cuando en realidad rechazo el pedido.
        [Fact]
        public async Task Delete_ConArticulos_DevuelveConflict()
        {
            using var context = CrearContexto();
            await SembrarConArticuloAsync(context);

            var resultado = await CrearController(context).Delete(1);

            Assert.IsType<ConflictObjectResult>(resultado);
            Assert.NotNull(await context.Categorias.FindAsync(1));
        }

        [Fact]
        public async Task Delete_Inexistente_DevuelveNotFound()
        {
            using var context = CrearContexto();
            await SembrarAsync(context);

            var resultado = await CrearController(context).Delete(99);

            Assert.IsType<NotFoundResult>(resultado);
        }
    }
}

