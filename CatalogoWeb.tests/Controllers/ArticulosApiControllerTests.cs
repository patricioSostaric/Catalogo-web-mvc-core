using catalogo_web_mvc.Controllers.Api;
using catalogo_web_mvc.Interfaces.Articulos;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.Dtos;
using catalogo_web_mvc.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using X.PagedList.Extensions;

namespace CatalogoWeb.Tests.Controllers
{
    public class ArticulosApiControllerTests
    {
        private readonly Mock<IArticuloService> _serviceMock = new();

        private static List<Articulo> ArticulosDeEjemplo() =>
        [
            new()
            {
                Id = 1, Codigo = "S01", Nombre = "Galaxy S10", Descripcion = "desc",
                Precio = 699999, Stock = 5, ImagenUrl = "/imagen/articulos/s01.jpg",
                MarcaId = 1, Marca = new Marca { MarcaId = 1, Descripcion = "Samsung" },
                CategoriaId = 1, Categoria = new Categoria { CategoriaId = 1, Descripcion = "Celulares" }
            }
        ];

        // El servicio real consulta la base; acá se lo reemplaza por uno falso al
        // que se le dicta la respuesta. Asi el test corre en milisegundos y puede
        // plantear situaciones que en la base serian incomodas de preparar.
        private void ConfigurarServicio(List<Articulo> articulos, int page = 1, int pageSize = 6)
        {
            _serviceMock.Setup(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(articulos.ToPagedList(page, pageSize));
        }

        // ── Mapeo a DTO ────────────────────────────────────────────────────────

        [Fact]
        public async Task Get_MapeaMarcaYCategoriaComoTexto()
        {
            // Preparar
            ConfigurarServicio(ArticulosDeEjemplo());
            var controller = new ArticulosApiController(_serviceMock.Object);

            // Ejecutar
            var resultado = await controller.Get();

            // Verificar: el contrato publico expone los nombres, no los objetos
            // ni los identificadores internos.
            var ok = Assert.IsType<OkObjectResult>(resultado.Result);
            var dto = Assert.IsType<ArticulosPaginadosDto>(ok.Value);
            var articulo = Assert.Single(dto.Articulos);
            Assert.Equal("Samsung", articulo.Marca);
            Assert.Equal("Celulares", articulo.Categoria);
        }

        [Fact]
        public async Task Get_SinStock_MarcaComoNoDisponible()
        {
            var sinStock = ArticulosDeEjemplo();
            sinStock[0].Stock = 0;
            ConfigurarServicio(sinStock);
            var controller = new ArticulosApiController(_serviceMock.Object);

            var resultado = await controller.Get();

            // El catalogo publico solo necesita saber si hay o no hay: la cantidad
            // exacta es informacion del negocio y no se expone.
            var ok = Assert.IsType<OkObjectResult>(resultado.Result);
            var dto = Assert.IsType<ArticulosPaginadosDto>(ok.Value);
            Assert.False(Assert.Single(dto.Articulos).Disponible);
        }

        [Fact]
        public async Task Get_ConStock_MarcaComoDisponible()
        {
            ConfigurarServicio(ArticulosDeEjemplo());
            var controller = new ArticulosApiController(_serviceMock.Object);

            var resultado = await controller.Get();

            var ok = Assert.IsType<OkObjectResult>(resultado.Result);
            var dto = Assert.IsType<ArticulosPaginadosDto>(ok.Value);
            Assert.True(Assert.Single(dto.Articulos).Disponible);
        }

        // ── Envoltorio del paginado ────────────────────────────────────────────

        [Fact]
        public async Task Get_DevuelveElPaginadoDelServicio()
        {
            var articulos = Enumerable.Range(1, 15)
                .Select(i => new Articulo
                {
                    Id = i, Codigo = $"C{i}", Nombre = $"Articulo {i}", Descripcion = "desc",
                    Precio = 1000, Stock = 1,
                    Marca = new Marca { MarcaId = 1, Descripcion = "Samsung" },
                    Categoria = new Categoria { CategoriaId = 1, Descripcion = "Celulares" }
                })
                .ToList();
            ConfigurarServicio(articulos, page: 2, pageSize: 6);
            var controller = new ArticulosApiController(_serviceMock.Object);

            var resultado = await controller.Get(page: 2);

            // Quien consume necesita estos cuatro datos para dibujar su paginador:
            // sin ellos solo sabria que recibio una lista, no en que parte del total.
            var ok = Assert.IsType<OkObjectResult>(resultado.Result);
            var dto = Assert.IsType<ArticulosPaginadosDto>(ok.Value);
            Assert.Equal(2, dto.Pagina);
            Assert.Equal(6, dto.TamanioPagina);
            Assert.Equal(15, dto.TotalArticulos);
            Assert.Equal(3, dto.TotalPaginas);
            Assert.Equal(6, dto.Articulos.Count);
        }

        // ── Argumentos con los que se llama al servicio ────────────────────────

        [Fact]
        public async Task Get_PideSoloArticulosActivos()
        {
            ConfigurarServicio(ArticulosDeEjemplo());
            var controller = new ArticulosApiController(_serviceMock.Object);

            await controller.Get();

            // Un articulo dado de baja no debe aparecer en el catalogo publico.
            // El ultimo argumento es soloActivos: se exige true, no cualquier valor.
            _serviceMock.Verify(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<int>(), true), Times.Once);
        }

        [Fact]
        public async Task Get_ConTerminoDeBusqueda_SeLoPasaAlServicio()
        {
            ConfigurarServicio(ArticulosDeEjemplo());
            var controller = new ArticulosApiController(_serviceMock.Object);

            await controller.Get(buscar: "galaxy");

            // El filtrado lo resuelve el servicio, el mismo que usa la vista Razor:
            // el controlador solo tiene que dejar pasar el termino.
            _serviceMock.Verify(s => s.BuscarAsync("galaxy", It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task Get_SinTerminoDeBusqueda_NoFiltra()
        {
            ConfigurarServicio(ArticulosDeEjemplo());
            var controller = new ArticulosApiController(_serviceMock.Object);

            await controller.Get();

            // Sin termino el catalogo se devuelve completo: el parametro es opcional
            // y su ausencia no debe traducirse en una cadena vacia ni en un filtro.
            _serviceMock.Verify(s => s.BuscarAsync(null, It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task Get_PageSizeExcesivo_SeAcotaAlMaximo()
        {
            ConfigurarServicio(ArticulosDeEjemplo());
            var controller = new ArticulosApiController(_serviceMock.Object);

            await controller.Get(pageSize: 100000);

            // Sin este tope, una sola peticion podria pedir el catalogo entero.
            _serviceMock.Verify(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>(), 50, It.IsAny<bool>()), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task Get_PageInvalida_SeCorrigeALaPrimera(int page)
        {
            ConfigurarServicio(ArticulosDeEjemplo());
            var controller = new ArticulosApiController(_serviceMock.Object);

            await controller.Get(page: page);

            // X.PagedList no admite paginas menores a 1: sin esta correccion un
            // parametro mal escrito devolveria un 500 en lugar de una respuesta.
            _serviceMock.Verify(s => s.BuscarAsync(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                1, It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
        }

        // ── Detalle ────────────────────────────────────────────────────────────

        private static ArticuloDetalleViewModel DetalleDeEjemplo() => new()
        {
            Id = 1,
            Codigo = "S01",
            Nombre = "Galaxy S10",
            Descripcion = "Una canoa cara",
            Marca = "Samsung",
            Categoria = "Celulares",
            Precio = 699999,
            ImagenUrl = "/imagen/articulos/s01.jpg"
        };

        [Fact]
        public async Task GetPorId_DevuelveElArticulo()
        {
            _serviceMock.Setup(s => s.ObtenerDetallePublicoAsync(1))
                .ReturnsAsync(DetalleDeEjemplo());
            var controller = new ArticulosApiController(_serviceMock.Object);

            var resultado = await controller.GetPorId(1);

            var ok = Assert.IsType<OkObjectResult>(resultado.Result);
            var dto = Assert.IsType<ArticuloDetalleDto>(ok.Value);
            Assert.Equal("Galaxy S10", dto.Nombre);
            Assert.Equal("Una canoa cara", dto.Descripcion);
            Assert.Equal("Samsung", dto.Marca);
            Assert.Equal("Celulares", dto.Categoria);
        }

        [Fact]
        public async Task GetPorId_NoExponeElCodigo()
        {
            // El codigo es dato de administracion: sirve para reponer stock y no
            // le aporta nada a quien consume el catalogo. Este test falla si
            // alguien agrega la propiedad al DTO por costumbre.
            _serviceMock.Setup(s => s.ObtenerDetallePublicoAsync(1))
                .ReturnsAsync(DetalleDeEjemplo());
            var controller = new ArticulosApiController(_serviceMock.Object);

            var resultado = await controller.GetPorId(1);

            var ok = Assert.IsType<OkObjectResult>(resultado.Result);
            var propiedades = ok.Value!.GetType().GetProperties().Select(p => p.Name);
            Assert.DoesNotContain("Codigo", propiedades);
        }

        [Fact]
        public async Task GetPorId_ArticuloInexistenteOInactivo_Devuelve404()
        {
            // El servicio devuelve null en los dos casos, y la API no los
            // distingue: decir "existe pero no se publica" seria filtrar
            // informacion sobre el catalogo interno.
            _serviceMock.Setup(s => s.ObtenerDetallePublicoAsync(It.IsAny<int>()))
                .ReturnsAsync((ArticuloDetalleViewModel?)null);
            var controller = new ArticulosApiController(_serviceMock.Object);

            var resultado = await controller.GetPorId(999);

            Assert.IsType<NotFoundResult>(resultado.Result);
        }

        [Fact]
        public async Task GetPorId_LePasaElIdAlServicio()
        {
            _serviceMock.Setup(s => s.ObtenerDetallePublicoAsync(It.IsAny<int>()))
                .ReturnsAsync(DetalleDeEjemplo());
            var controller = new ArticulosApiController(_serviceMock.Object);

            await controller.GetPorId(42);

            _serviceMock.Verify(s => s.ObtenerDetallePublicoAsync(42), Times.Once);
        }
    }
}
