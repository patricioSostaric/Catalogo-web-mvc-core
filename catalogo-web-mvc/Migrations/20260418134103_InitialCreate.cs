using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace catalogo_web_mvc.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.CategoriaId);
                });

            migrationBuilder.CreateTable(
                name: "Marcas",
                columns: table => new
                {
                    MarcaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marcas", x => x.MarcaId);
                });

            migrationBuilder.CreateTable(
                name: "Articulos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    MarcaId = table.Column<int>(type: "int", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articulos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Articulos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "CategoriaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Articulos_Marcas_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "Marcas",
                        principalColumn: "MarcaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "CategoriaId", "Descripcion" },
                values: new object[,]
                {
                    { 1, "Celulares" },
                    { 2, "Televisores" },
                    { 3, "Media" },
                    { 4, "Audio" }
                });

            migrationBuilder.InsertData(
                table: "Marcas",
                columns: new[] { "MarcaId", "Descripcion" },
                values: new object[,]
                {
                    { 1, "Samsung" },
                    { 2, "Apple" },
                    { 3, "Sony" },
                    { 4, "Huawei" },
                    { 5, "Motorola" }
                });

            migrationBuilder.InsertData(
                table: "Articulos",
                columns: new[] { "Id", "CategoriaId", "Codigo", "Descripcion", "ImagenUrl", "MarcaId", "Nombre", "Precio" },
                values: new object[,]
                {
                    { 1, 1, "S01", "Una canoa cara", "https://images.samsung.com/...jpg", 1, "Galaxy S10", 69999m },
                    { 2, 1, "M03", "Ya siete de estos?", "https://www.motorola.cl/...png", 5, "Moto G Play 7ma Gen", 15699m },
                    { 3, 3, "S99", "Ya no se cuantas versiones hay", "sin_imagen_para_que_de_error....muejeje", 3, "Play 4", 35000m },
                    { 4, 2, "S56", "Alta tele", "https://intercompras.com/...jpg", 3, "Bravia 55", 49500m },
                    { 5, 3, "A23", "lindo loro", "https://store.storeimages.cdn-apple.com/...jpg", 2, "Apple TV", 7850m },
                    { 6, 1, "H01", "Celular con triple cámara", "https://consumer.huawei.com/...jpg", 4, "Huawei P30", 45999m },
                    { 7, 2, "S20", "Televisor de alta definición", "https://images.samsung.com/...jpg", 1, "Samsung QLED 65", 120000m },
                    { 8, 4, "A50", "Auriculares inalámbricos con cancelación", "https://store.storeimages.cdn-apple.com/...jpg", 2, "AirPods Pro", 8999m },
                    { 9, 1, "M10", "Celular económico", "https://motorola.com/...jpg", 5, "Moto E20", 10999m },
                    { 10, 4, "S77", "Auriculares con cancelación de ruido", "https://sony.com/...jpg", 3, "Sony WH-1000XM4", 29999m },
                    { 11, 3, "A99", "Tablet de alto rendimiento", "https://store.storeimages.cdn-apple.com/...jpg", 2, "Apple iPad Pro", 150000m },
                    { 12, 3, "H55", "Notebook ligera", "https://consumer.huawei.com/...jpg", 4, "Huawei MateBook", 95000m },
                    { 13, 4, "S88", "Auriculares inalámbricos", "https://images.samsung.com/...jpg", 1, "Samsung Galaxy Buds", 12999m },
                    { 14, 1, "M22", "Celular gama media", "https://motorola.com/...jpg", 5, "Motorola Edge 30", 39999m },
                    { 15, 3, "S90", "Consola de última generación", "https://sony.com/...jpg", 3, "Sony PlayStation 5", 250000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articulos_CategoriaId",
                table: "Articulos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Articulos_MarcaId",
                table: "Articulos",
                column: "MarcaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Articulos");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Marcas");
        }
    }
}
