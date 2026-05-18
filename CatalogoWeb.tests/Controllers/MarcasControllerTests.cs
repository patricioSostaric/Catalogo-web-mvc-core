using catalogo_web_mvc.Controllers;
using catalogo_web_mvc.Interfaces.Marcas;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CatalogoWeb.Tests.Controllers
{
    public class MarcasControllerTests
    {
        private readonly Mock<IMarcaService> _serviceMock;
        private readonly MarcasController _controller;

        public MarcasControllerTests()
        {
            _serviceMock = new Mock<IMarcaService>();
            _controller = new MarcasController(_serviceMock.Object);
        }

        private static List<Marca> MarcasDeEjemplo() => new()
        {
            new Marca { MarcaId = 1, Descripcion = "Samsung" },
            new Marca { MarcaId = 2, Descripcion = "Apple" }
        };

        // ── Index ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Index_RetornaVista_ConListaDeMarcas()
        {
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(MarcasDeEjemplo());

            var resultado = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<List<Marca>>(viewResult.Model);
            Assert.Equal(2, modelo.Count);
        }

        [Fact]
        public async Task Index_RetornaVistaConListaVacia_CuandoNoHayMarcas()
        {
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Marca>());

            var resultado = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(resultado);
            var modelo = Assert.IsAssignableFrom<List<Marca>>(viewResult.Model);
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
            var marca = new Marca { MarcaId = 3, Descripcion = "Sony" };
            _serviceMock.Setup(s => s.AddAsync(marca)).Returns(Task.CompletedTask);

            var resultado = await _controller.Create(marca);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Index", redirect.ActionName);
            _serviceMock.Verify(s => s.AddAsync(marca), Times.Once);
        }

        [Fact]
        public async Task Create_Post_ConModeloInvalido_RetornaVista_SinLlamarAdd()
        {
            _controller.ModelState.AddModelError("Descripcion", "Requerido");
            var marca = new Marca { MarcaId = 3 };

            var resultado = await _controller.Create(marca);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal(marca, viewResult.Model);
            _serviceMock.Verify(s => s.AddAsync(It.IsAny<Marca>()), Times.Never);
        }

        // ── Edit GET ──────────────────────────────────────────────────────────

        [Fact]
        public async Task Edit_Get_RetornaVista_CuandoExiste()
        {
            var marca = new Marca { MarcaId = 1, Descripcion = "Samsung" };
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(marca);

            var resultado = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal(marca, viewResult.Model);
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
            _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Marca?)null);

            var resultado = await _controller.Edit(99);

            Assert.IsType<NotFoundResult>(resultado);
        }

        // ── Edit POST ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Edit_Post_ConModeloValido_ActualizaYRedirecciona()
        {
            var marca = new Marca { MarcaId = 1, Descripcion = "Samsung Pro" };
            _serviceMock.Setup(s => s.UpdateAsync(marca)).Returns(Task.CompletedTask);

            var resultado = await _controller.Edit(1, marca);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Index", redirect.ActionName);
            _serviceMock.Verify(s => s.UpdateAsync(marca), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_IdNoCoincide_RetornaNotFound()
        {
            var marca = new Marca { MarcaId = 5, Descripcion = "Sony" };

            var resultado = await _controller.Edit(1, marca);

            Assert.IsType<NotFoundResult>(resultado);
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<Marca>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_ConModeloInvalido_RetornaVista_SinLlamarUpdate()
        {
            _controller.ModelState.AddModelError("Descripcion", "Requerido");
            var marca = new Marca { MarcaId = 1 };

            var resultado = await _controller.Edit(1, marca);

            Assert.IsType<ViewResult>(resultado);
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<Marca>()), Times.Never);
        }

        // ── Delete GET ────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_Get_RetornaVista_CuandoExiste()
        {
            var marca = new Marca { MarcaId = 1, Descripcion = "Samsung" };
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(marca);

            var resultado = await _controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal(marca, viewResult.Model);
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
            _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Marca?)null);

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
