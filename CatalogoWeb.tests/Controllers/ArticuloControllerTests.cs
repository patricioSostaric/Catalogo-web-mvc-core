using catalogo_web_mvc.Controllers;
using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Categorias;
using catalogo_web_mvc.Interfaces.Marcas;
using catalogo_web_mvc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using System.Security.Claims;
using X.PagedList;
using X.PagedList.Extensions;

namespace CatalogoWeb.Tests.Controllers
{
    public class ArticuloControllerTests
    {
        private readonly Mock<IArticuloService> _serviceMock;
        private readonly Mock<IAuditService> _auditMock;
        private readonly Mock<IMarcaService> _marcasMock;
        private readonly Mock<ICategoriaService> _categoriasMock;
        private readonly ArticuloController _controller;

        public ArticuloControllerTests()
        {
            _serviceMock = new Mock<IArticuloService>();
            _auditMock = new Mock<IAuditService>();
            _marcasMock = new Mock<IMarcaService>();
            _categoriasMock = new Mock<ICategoriaService>();
            _controller = new ArticuloController(_serviceMock.Object, _auditMock.Object,
                _marcasMock.Object, _categoriasMock.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                        new Claim(ClaimTypes.Name, "test@test.com")
                    }, "TestAuth"))
                }
            };
        }

        private static IPagedList<Articulo> ListaPaginada(IEnumerable<Articulo> articulos, int page = 1, int size = 10)
            => articulos.ToList().ToPagedList(page, size);

        // Los desplegables ya no los arma el servicio de articulos: el controlador
        // pide las listas a los servicios de marcas y categorias y construye el
        // SelectList, que es un tipo de la capa de vistas.
        private void ConfigurarSelectLists(int? marcaId = null, int? categoriaId = null)
        {
            _marcasMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Marca>());
            _categoriasMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Categoria>());
        }

        // ── Index ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Index_RetornaVista_ConListaPaginada()
        {
            var articulos = ListaPaginada(new List<Articulo>
            {
                new() { Id = 1, Codigo = "S01", Nombre = "Galaxy S10", MarcaId = 1, CategoriaId = 1 }
            });
            _serviceMock.Setup(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(articulos);

            var resultado = await _controller.Index(null, false, null, null, null, null);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.IsAssignableFrom<IPagedList<Articulo>>(viewResult.Model);
        }

        [Fact]
        public async Task Index_AgregaErrorDeModelo_CuandoNoHayResultados()
        {
            var listaVacia = ListaPaginada(new List<Articulo>());
            _serviceMock.Setup(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(listaVacia);

            await _controller.Index("criterio_sin_match", false, null, null, null, null);

            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(""));
        }

        [Fact]
        public async Task Index_NoAgregaError_CuandoHayResultados()
        {
            var articulos = ListaPaginada(new List<Articulo>
            {
                new() { Id = 1, Codigo = "S01", Nombre = "Galaxy S10", MarcaId = 1, CategoriaId = 1 }
            });
            _serviceMock.Setup(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(articulos);

            await _controller.Index(null, false, null, null, null, null);

            Assert.True(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Index_UsaPageNumber1_CuandoPageEsNull()
        {
            var articulos = ListaPaginada(new List<Articulo>());
            _serviceMock.Setup(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1, 5))
                .ReturnsAsync(articulos);

            await _controller.Index(null, false, null, null, null, null);

            _serviceMock.Verify(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1, 5, false), Times.Once);
        }

        [Fact]
        public async Task Index_UsaPageNumberProporcionado_CuandoPageTieneValor()
        {
            var articulos = ListaPaginada(new List<Articulo>());
            _serviceMock.Setup(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 3, 5))
                .ReturnsAsync(articulos);

            await _controller.Index(null, false, null, null, null, page: 3);

            _serviceMock.Verify(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 3, 5, false), Times.Once);
        }

        // ── Details ───────────────────────────────────────────────────────────

        [Fact]
        public async Task Details_RetornaVista_CuandoExisteElArticulo()
        {
            var articulo = new Articulo { Id = 1, Codigo = "S01", Nombre = "Galaxy S10", MarcaId = 1, CategoriaId = 1 };
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(articulo);

            var resultado = await _controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal(articulo, viewResult.Model);
        }

        [Fact]
        public async Task Details_RetornaNotFound_CuandoNoExisteElArticulo()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Articulo?)null);

            var resultado = await _controller.Details(99);

            Assert.IsType<NotFoundResult>(resultado);
        }

        // ── Create GET ────────────────────────────────────────────────────────

        [Fact]
        public async Task Create_Get_RetornaVista()
        {
            ConfigurarSelectLists();

            var resultado = await _controller.Create();

            Assert.IsType<ViewResult>(resultado);
        }

        [Fact]
        public async Task Create_Get_LlamaMarcasYCategoriasSelectList()
        {
            ConfigurarSelectLists();

            await _controller.Create();

            _marcasMock.Verify(s => s.GetAllAsync(), Times.Once);
            _categoriasMock.Verify(s => s.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Create_Get_OrdenaLosDesplegablesAlfabeticamente()
        {
            // El orden lo aplicaba el servicio cuando armaba el SelectList. Al pasar
            // esa responsabilidad al controlador, la garantia se verifica aca.
            _marcasMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Marca>
            {
                new() { MarcaId = 1, Descripcion = "Samsung" },
                new() { MarcaId = 2, Descripcion = "Apple" }
            });
            _categoriasMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Categoria>
            {
                new() { CategoriaId = 1, Descripcion = "Televisores" },
                new() { CategoriaId = 2, Descripcion = "Celulares" }
            });

            await _controller.Create();

            // Los metodos de extension no se resuelven sobre dynamic, asi que hay
            // que salir de ViewBag antes de usar LINQ.
            SelectList desplegableMarcas = _controller.ViewBag.MarcaId;
            var marcas = desplegableMarcas.Cast<SelectListItem>().ToList();
            Assert.Equal("Apple", marcas[0].Text);
            Assert.Equal("Samsung", marcas[1].Text);

            SelectList desplegableCategorias = _controller.ViewBag.CategoriaId;
            var categorias = desplegableCategorias.Cast<SelectListItem>().ToList();
            Assert.Equal("Celulares", categorias[0].Text);
            Assert.Equal("Televisores", categorias[1].Text);
        }

        // ── Create POST ───────────────────────────────────────────────────────

        [Fact]
        public async Task Create_Post_ConModeloValido_AgregaYRedirecciona()
        {
            var articulo = new Articulo { Id = 0, Codigo = "NEW", Nombre = "Nuevo", MarcaId = 1, CategoriaId = 1 };
            _serviceMock.Setup(s => s.AddAsync(articulo)).Returns(Task.CompletedTask);

            var resultado = await _controller.Create(articulo);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Index", redirect.ActionName);
            _serviceMock.Verify(s => s.AddAsync(articulo), Times.Once);
        }

        [Fact]
        public async Task Create_Post_ConModeloInvalido_RetornaVista_SinLlamarAdd()
        {
            _controller.ModelState.AddModelError("Nombre", "El campo Nombre es requerido");
            var articulo = new Articulo { Codigo = "NEW", MarcaId = 1, CategoriaId = 1 };
            ConfigurarSelectLists();

            var resultado = await _controller.Create(articulo);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal(articulo, viewResult.Model);
            _serviceMock.Verify(s => s.AddAsync(It.IsAny<Articulo>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_ConModeloInvalido_RecargaSelectLists()
        {
            _controller.ModelState.AddModelError("Nombre", "Requerido");
            var articulo = new Articulo { Codigo = "NEW", MarcaId = 1, CategoriaId = 1 };
            ConfigurarSelectLists();

            await _controller.Create(articulo);

            _marcasMock.Verify(s => s.GetAllAsync(), Times.Once);
            _categoriasMock.Verify(s => s.GetAllAsync(), Times.Once);
        }

        // ── Edit GET ──────────────────────────────────────────────────────────

        [Fact]
        public async Task Edit_Get_RetornaVista_CuandoExisteElArticulo()
        {
            var articulo = new Articulo { Id = 1, Codigo = "S01", Nombre = "Galaxy S10", MarcaId = 1, CategoriaId = 1 };
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(articulo);
            ConfigurarSelectLists();

            var resultado = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal(articulo, viewResult.Model);
        }

        [Fact]
        public async Task Edit_Get_RetornaNotFound_CuandoNoExiste()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Articulo?)null);

            var resultado = await _controller.Edit(99);

            Assert.IsType<NotFoundResult>(resultado);
        }

        [Fact]
        public async Task Edit_Get_LlamaMarcasYCategoriasSelectList()
        {
            var articulo = new Articulo { Id = 1, Codigo = "S01", Nombre = "Galaxy S10", MarcaId = 1, CategoriaId = 1 };
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(articulo);
            ConfigurarSelectLists();

            await _controller.Edit(1);

            _marcasMock.Verify(s => s.GetAllAsync(), Times.Once);
            _categoriasMock.Verify(s => s.GetAllAsync(), Times.Once);
        }

        // ── Edit POST ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Edit_Post_ConModeloValido_ActualizaYRedirecciona()
        {
            var articulo = new Articulo { Id = 1, Codigo = "S01", Nombre = "Actualizado", MarcaId = 1, CategoriaId = 1 };
            _serviceMock.Setup(s => s.UpdateAsync(articulo)).Returns(Task.CompletedTask);

            var resultado = await _controller.Edit(1, articulo);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Index", redirect.ActionName);
            _serviceMock.Verify(s => s.UpdateAsync(articulo), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_IdNoCoincide_RetornaNotFound()
        {
            var articulo = new Articulo { Id = 5, Codigo = "S01", Nombre = "Test", MarcaId = 1, CategoriaId = 1 };

            var resultado = await _controller.Edit(1, articulo);

            Assert.IsType<NotFoundResult>(resultado);
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<Articulo>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_ConModeloInvalido_RetornaVista_SinLlamarUpdate()
        {
            _controller.ModelState.AddModelError("Nombre", "Requerido");
            var articulo = new Articulo { Id = 1, Codigo = "S01", MarcaId = 1, CategoriaId = 1 };
            ConfigurarSelectLists();

            var resultado = await _controller.Edit(1, articulo);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal(articulo, viewResult.Model);
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<Articulo>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_ConModeloInvalido_RecargaSelectLists()
        {
            _controller.ModelState.AddModelError("Nombre", "Requerido");
            var articulo = new Articulo { Id = 1, Codigo = "S01", MarcaId = 1, CategoriaId = 1 };
            ConfigurarSelectLists();

            await _controller.Edit(1, articulo);

            _marcasMock.Verify(s => s.GetAllAsync(), Times.Once);
            _categoriasMock.Verify(s => s.GetAllAsync(), Times.Once);
        }

        // ── Delete GET ────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_Get_RetornaVista_CuandoExisteElArticulo()
        {
            var articulo = new Articulo { Id = 1, Codigo = "S01", Nombre = "Galaxy S10", MarcaId = 1, CategoriaId = 1 };
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(articulo);

            var resultado = await _controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal(articulo, viewResult.Model);
        }

        [Fact]
        public async Task Delete_Get_RetornaNotFound_CuandoNoExiste()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Articulo?)null);

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
