using catalogo_web_mvc.Controllers;
using catalogo_web_mvc.Interfaces.Categorias;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CatalogoWeb.Tests.Controllers
{
    public class CategoriasControllerTests
    {
        private readonly Mock<ICategoriaService> _serviceMock;
        private readonly CategoriasController _controller;

        public CategoriasControllerTests()
        {
            _serviceMock = new Mock<ICategoriaService>();
            _controller = new CategoriasController(_serviceMock.Object);
        }

        private static List<Categoria> CategoriasDeEjemplo() => new()
        {
            new Categoria { CategoriaId = 1, Descripcion = "Celulares" },
            new Categoria { CategoriaId = 2, Descripcion = "Televisores" }
        };

        // ── Index ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Index_RetornaVista_ConListaDeCategorias()
        {
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(CategoriasDeEjemplo());

            var resultado = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<List<Categoria>>(viewResult.Model);
            Assert.Equal(2, modelo.Count);
        }

        [Fact]
        public async Task Index_RetornaVistaConListaVacia_CuandoNoHayCategorias()
        {
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Categoria>());

            var resultado = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<List<Categoria>>(viewResult.Model);
            Assert.Empty(modelo);
        }

        // ── Create GET ────────────────────────────────────────────────────────

        [Fact]
        public void Create_Get_RetornaVista()
        {
            var resultado = _controller.Create();

            Assert.IsType<ViewResult>(resultado);
        }

        // ── Create POST ───────────────────────────────────────────────────────

        [Fact]
        public async Task Create_Post_ConModeloValido_AgregaYRedirecciona()
        {
            var categoria = new Categoria { CategoriaId = 3, Descripcion = "Audio" };
            _serviceMock.Setup(s => s.AddAsync(categoria)).Returns(Task.CompletedTask);

            var resultado = await _controller.Create(categoria);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Index", redirect.ActionName);
            _serviceMock.Verify(s => s.AddAsync(categoria), Times.Once);
        }

        [Fact]
        public async Task Create_Post_ConModeloInvalido_RetornaVista_SinLlamarAdd()
        {
            _controller.ModelState.AddModelError("Descripcion", "Requerido");
            var categoria = new Categoria { CategoriaId = 3 };

            var resultado = await _controller.Create(categoria);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal(categoria, viewResult.Model);
            _serviceMock.Verify(s => s.AddAsync(It.IsAny<Categoria>()), Times.Never);
        }

        // ── Edit GET ──────────────────────────────────────────────────────────

        [Fact]
        public async Task Edit_Get_RetornaVista_CuandoExiste()
        {
            var categoria = new Categoria { CategoriaId = 1, Descripcion = "Celulares" };
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(categoria);

            var resultado = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal(categoria, viewResult.Model);
        }

        [Fact]
        public async Task Edit_Get_RetornaNotFound_CuandoIdEsNull()
        {
            var resultado = await _controller.Edit(null);

            Assert.IsType<NotFoundResult>(resultado);
        }

        [Fact]
        public async Task Edit_Get_RetornaNotFound_CuandoNoExiste()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Categoria?)null);

            var resultado = await _controller.Edit(99);

            Assert.IsType<NotFoundResult>(resultado);
        }

        // ── Edit POST ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Edit_Post_ConModeloValido_ActualizaYRedirecciona()
        {
            var categoria = new Categoria { CategoriaId = 1, Descripcion = "Smartphones" };
            _serviceMock.Setup(s => s.UpdateAsync(categoria)).Returns(Task.CompletedTask);

            var resultado = await _controller.Edit(1, categoria);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Index", redirect.ActionName);
            _serviceMock.Verify(s => s.UpdateAsync(categoria), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_IdNoCoincide_RetornaNotFound()
        {
            var categoria = new Categoria { CategoriaId = 5, Descripcion = "Audio" };

            var resultado = await _controller.Edit(1, categoria);

            Assert.IsType<NotFoundResult>(resultado);
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<Categoria>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_ConModeloInvalido_RetornaVista_SinLlamarUpdate()
        {
            _controller.ModelState.AddModelError("Descripcion", "Requerido");
            var categoria = new Categoria { CategoriaId = 1 };

            var resultado = await _controller.Edit(1, categoria);

            Assert.IsType<ViewResult>(resultado);
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<Categoria>()), Times.Never);
        }

        // ── Delete GET ────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_Get_RetornaVista_CuandoExiste()
        {
            var categoria = new Categoria { CategoriaId = 1, Descripcion = "Celulares" };
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(categoria);

            var resultado = await _controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal(categoria, viewResult.Model);
        }

        [Fact]
        public async Task Delete_Get_RetornaNotFound_CuandoIdEsNull()
        {
            var resultado = await _controller.Delete(null);

            Assert.IsType<NotFoundResult>(resultado);
        }

        [Fact]
        public async Task Delete_Get_RetornaNotFound_CuandoNoExiste()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Categoria?)null);

            var resultado = await _controller.Delete(99);

            Assert.IsType<NotFoundResult>(resultado);
        }

        // ── DeleteConfirmed ───────────────────────────────────────────────────

        [Fact]
        public async Task DeleteConfirmed_LlamaDeleteYRedireccionaAIndex()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);

            var resultado = await _controller.DeleteConfirmed(1);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Index", redirect.ActionName);
            _serviceMock.Verify(s => s.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_LlamaDeleteConIdCorrecto()
        {
            _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            await _controller.DeleteConfirmed(42);

            _serviceMock.Verify(s => s.DeleteAsync(42), Times.Once);
        }
    }
}
