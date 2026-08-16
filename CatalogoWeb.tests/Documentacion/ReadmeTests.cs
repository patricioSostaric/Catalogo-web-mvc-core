using System.Reflection;
using System.Text.RegularExpressions;

namespace CatalogoWeb.Tests.Documentacion
{
    /// <summary>
    /// El README se desactualizó dos veces sin que nadie lo notara: faltaba un proyecto
    /// entero en la estructura, decía 12 migraciones cuando eran 14, y las instrucciones
    /// para levantar el front apuntaban a un puerto que había dejado de existir. Alguien
    /// que clonara el repo y las siguiera no podía arrancarlo.
    ///
    /// Estas pruebas cubren lo que una máquina puede comparar: cantidades, nombres de
    /// proyectos y rutas. No revisan la prosa — una explicación equivocada sigue
    /// necesitando ojos — pero sí todo lo que se desincronizó hasta ahora.
    /// </summary>
    public class ReadmeTests
    {
        private static readonly string RaizRepo = BuscarRaiz();
        private static readonly string Readme =
            File.ReadAllText(Path.Combine(RaizRepo, "README.md"));

        // Los tests corren desde bin/Debug/netX, asi que hay que subir hasta encontrar
        // la solucion en vez de asumir una profundidad fija.
        private static string BuscarRaiz()
        {
            var directorio = new DirectoryInfo(AppContext.BaseDirectory);

            while (directorio != null && !File.Exists(Path.Combine(directorio.FullName, "catalogo-web-mvc.slnx")))
                directorio = directorio.Parent;

            return directorio?.FullName
                ?? throw new InvalidOperationException("No se encontró la raíz del repositorio.");
        }

        // ── Cantidad de tests ──────────────────────────────────────────────────

        // Un [Theory] no es un test: son tantos como casos de [InlineData] tenga. Contarlos
        // como uno daria un numero menor al que informa dotnet test, que es el que figura
        // en el README.
        private static int ContarTests()
        {
            var metodos = typeof(ReadmeTests).Assembly
                .GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Where(m => m.GetCustomAttributes()
                    .Any(a => a.GetType().Name == "FactAttribute" || a.GetType().Name == "TheoryAttribute"));

            return metodos.Sum(m =>
            {
                var casos = m.GetCustomAttributes()
                    .Count(a => a.GetType().Name == "InlineDataAttribute");

                return casos == 0 ? 1 : casos;
            });
        }

        [Fact]
        public void ElBadgeDeTests_CoincideConLaCantidadReal()
        {
            var total = ContarTests();

            Assert.True(
                Readme.Contains($"tests-{total}%20passing"),
                $"El badge del README no dice {total} tests. Actualizalo o revisá el conteo.");
        }

        [Fact]
        public void ElTextoDeTests_CoincideConLaCantidadReal()
        {
            var total = ContarTests();

            Assert.True(
                Readme.Contains($"**{total} tests unitarios"),
                $"El README no dice «{total} tests unitarios» en la sección de testing.");

            Assert.True(
                Readme.Contains($"Total: {total}"),
                $"La salida de ejemplo del README no informa Total: {total}.");

            Assert.True(
                Readme.Contains($"CatalogoWeb.tests/        {total} tests unitarios"),
                $"El árbol de la estructura no dice {total} tests unitarios.");
        }

        // ── Estructura del proyecto ────────────────────────────────────────────

        [Fact]
        public void TodosLosProyectosDeLaSolucion_FiguranEnElReadme()
        {
            var solucion = File.ReadAllText(Path.Combine(RaizRepo, "catalogo-web-mvc.slnx"));

            var carpetas = Regex.Matches(solucion, @"Path=""([^""]+)\.csproj""")
                .Select(m => m.Groups[1].Value.Replace('\\', '/').Split('/')[0])
                .Distinct();

            var ausentes = carpetas.Where(c => !Readme.Contains(c)).ToList();

            Assert.True(
                ausentes.Count == 0,
                $"Estos proyectos están en la solución pero no en el README: {string.Join(", ", ausentes)}");
        }

        // Los dos documentos nombran la cantidad de migraciones, y los dos se desfasaron:
        // el README decia 12 y ARQUITECTURA.md 9 cuando ya eran 14. Por eso se comprueban
        // ambos: alcanzaba con actualizar uno y creer que estaba hecho.
        [Theory]
        [InlineData("README.md")]
        [InlineData("ARQUITECTURA.md")]
        public void LaCantidadDeMigraciones_CoincideConLasQueHay(string documento)
        {
            // Cada migracion genera tambien un .Designer.cs, y el snapshot no es una
            // migracion: contarlos infla el numero.
            var migraciones = Directory
                .GetFiles(Path.Combine(RaizRepo, "Catalogo.Datos", "Migrations"), "*.cs")
                .Count(f => !f.EndsWith(".Designer.cs") && !f.EndsWith("ModelSnapshot.cs"));

            var texto = File.ReadAllText(Path.Combine(RaizRepo, documento));

            Assert.True(
                texto.Contains($"{migraciones} migraciones"),
                $"{documento} no dice «{migraciones} migraciones». Hay {migraciones} en Catalogo.Datos/Migrations.");
        }

        // ── Endpoints ──────────────────────────────────────────────────────────

        [Fact]
        public void TodasLasRutasDeLaApi_EstanDocumentadas()
        {
            var controladores = Directory.GetFiles(
                Path.Combine(RaizRepo, "Catalogo.Endpoints", "Controllers"), "*.cs");

            var rutas = controladores
                .SelectMany(archivo => Regex.Matches(File.ReadAllText(archivo), @"\[Route\(""(api/[^""]+)""\)\]")
                    .Select(m => m.Groups[1].Value))
                .Distinct()
                .ToList();

            Assert.NotEmpty(rutas);

            var sinDocumentar = rutas.Where(r => !Readme.Contains(r)).ToList();

            Assert.True(
                sinDocumentar.Count == 0,
                $"Estas rutas de la API no aparecen en el README: {string.Join(", ", sinDocumentar)}");
        }
    }
}
