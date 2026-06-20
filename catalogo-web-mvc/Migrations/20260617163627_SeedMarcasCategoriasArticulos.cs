using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace catalogo_web_mvc.Migrations
{
    /// <inheritdoc />
    public partial class SeedMarcasCategoriasArticulos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                columns: new[] { "Id", "Activo", "CategoriaId", "Codigo", "Descripcion", "ImagenUrl", "MarcaId", "Nombre", "Precio", "Stock" },
                values: new object[,]
                {
                    { 1, true, 1, "S01", "Una canoa cara", "https://images.samsung.com/is/image/samsung/assets/ar/p6_gro2/p6_initial_mktpd/smartphones/galaxy-s10/specs/galaxy-s10-plus_specs_design_colors_prism_black.jpg?$163_346_PNG$", 1, "Galaxy S10", 69999m, 10 },
                    { 2, true, 1, "M03", "Ya siete de estos?", "https://i.blogs.es/baafde/motorola-moto-g7/1366_2000.webp", 5, "Moto G Play 7ma Gen", 15699m, 10 },
                    { 3, true, 3, "S99", "Ya no se cuantas versiones hay", "https://images.fravega.com/f1000/b88b497b0887aa2110d09fe389a29054.jpg", 3, "Play 4", 35000m, 10 },
                    { 4, true, 2, "S56", "Alta tele", "https://fulltec.com.bo/medios/2021/08/KD-55X725E-4.jpg", 3, "Bravia 55", 49500m, 10 },
                    { 5, true, 3, "A23", "lindo loro", "https://www.apple.com/newsroom/images/2023/12/redesigned-apple-tv-app-simplifies-the-viewing-experience/article/Apple-TV-app-home-screen_big.jpg.large_2x.jpg", 2, "Apple TV", 7850m, 10 },
                    { 6, true, 1, "H01", "Celular con triple cámara", "https://encrypted-tbn1.gstatic.com/shopping?q=tbn:ANd9GcRMFcq4Q7dPGPOLzhbIaeSTcZ7wuDSmHkWLmhlm0Nz-PsY6yWBwF5DnqdOfFx7UkKiCRDwPNOdtgK8SRX1mtGmYHZZnCwFlQJv1H4FpukAAJhbR2889GyFxaMswxa7o4gWZeZ7FvKm2Eg&usqp=CAc", 4, "Huawei P30", 45999m, 10 },
                    { 7, true, 2, "S20", "Televisor de alta definición", "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ9iqqSfTfIlR4odS9nlUOxO8wH1lu98ceyvA&s", 1, "Samsung QLED 65", 120000m, 10 },
                    { 8, true, 4, "A50", "Auriculares inalámbricos con cancelación", "https://encrypted-tbn1.gstatic.com/shopping?q=tbn:ANd9GcRJ9QKeD42BVe8PcDCAbL9fvLS2Jzwe94dXAqJ0oavHxv_j0czNzcjJ_GV5mb08uhWwYxgQ-jiG9OcKmrtnzto6LFQC8jj9_KH2yHu-gfLRx4-yyqXGLUpGjo5M6MhQ_NxmX4cTlwo&usqp=CAc", 2, "AirPods Pro", 8999m, 10 },
                    { 9, true, 1, "M10", "Celular económico", "https://ar.celulares.com/fotos/motorola-moto-e20-94506-g.jpg", 5, "Moto E20", 10999m, 10 },
                    { 10, true, 4, "S77", "Auriculares con cancelación de ruido", "https://encrypted-tbn1.gstatic.com/shopping?q=tbn:ANd9GcRwSPDvpeUVbL_SSTHEnUu9Wblp5hGqrjpY9gRjUplzy--_EbtfWpYKKi78wXxnCPR-bSBYx_gRI4LkRsCeyNNSpwpfbRPKNl0upmwAgdCX8wQzWJBkilJt2ID1c91RsijIkWngOk0&usqp=CAc", 3, "Sony WH-1000XM4", 29999m, 10 },
                    { 11, true, 3, "A99", "Tablet de alto rendimiento", "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS0roqcdNDvARcExaG2lSjzefvfrittrcbpbw&s", 2, "Apple iPad Pro", 150000m, 10 },
                    { 12, true, 3, "H55", "Notebook ligera", "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSilXc9Z-LL-TY3amhFEDmSbRGHMQx030hskg&s", 4, "Huawei MateBook", 95000m, 10 },
                    { 13, true, 4, "S88", "Auriculares inalámbricos", "https://encrypted-tbn0.gstatic.com/shopping?q=tbn:ANd9GcSHFJCqbFx9N3cSRQrooUZxZrG_0BHZJ_9mNkzAgsEE6aV4Bi8bDRgYX-lm0HI7DEjSaLxr_arVnDhd2-9CvM5jMcSW2aheoIOUwjEh706K-2cU8JrQLmxjxzVx8XxKQLRfGNh20Q&usqp=CAc", 1, "Samsung Galaxy Buds", 12999m, 10 },
                    { 14, true, 1, "M22", "Celular gama media", "https://http2.mlstatic.com/D_NQ_NP_2X_787732-MLU54979244644_042023-F.webp", 5, "Motorola Edge 30", 39999m, 10 },
                    { 15, true, 3, "S90", "Consola de última generación", "https://encrypted-tbn1.gstatic.com/shopping?q=tbn:ANd9GcQ5UaDAgLwtkEMiUEkVqydxn0tOezRPg87TI2KiIvzRLhiqlN-HsGu2MQSL-VCbJRa_wuwiS7-0Bto7_DuvgqIKYqGW2xutu_MmUmMt6RjUAqQcnjXZZlSbWB37-e84osdqKWPJZNX-1FNc&usqp=CAc", 3, "Sony PlayStation 5", 250000m, 10 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "CategoriaId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "CategoriaId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "CategoriaId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "CategoriaId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Marcas",
                keyColumn: "MarcaId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Marcas",
                keyColumn: "MarcaId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Marcas",
                keyColumn: "MarcaId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Marcas",
                keyColumn: "MarcaId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Marcas",
                keyColumn: "MarcaId",
                keyValue: 5);
        }
    }
}
