using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace catalogo_web_mvc.Migrations
{
    /// <inheritdoc />
    public partial class UpdateArticulosSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImagenUrl",
                value: "https://i.blogs.es/baafde/motorola-moto-g7/1366_2000.webp");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImagenUrl",
                value: "https://images.fravega.com/f1000/b88b497b0887aa2110d09fe389a29054.jpg");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 4,
                column: "ImagenUrl",
                value: "https://fulltec.com.bo/medios/2021/08/KD-55X725E-4.jpg");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 5,
                column: "ImagenUrl",
                value: "https://www.apple.com/newsroom/images/2023/12/redesigned-apple-tv-app-simplifies-the-viewing-experience/article/Apple-TV-app-home-screen_big.jpg.large_2x.jpg");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 6,
                column: "ImagenUrl",
                value: "https://encrypted-tbn1.gstatic.com/shopping?q=tbn:ANd9GcRMFcq4Q7dPGPOLzhbIaeSTcZ7wuDSmHkWLmhlm0Nz-PsY6yWBwF5DnqdOfFx7UkKiCRDwPNOdtgK8SRX1mtGmYHZZnCwFlQJv1H4FpukAAJhbR2889GyFxaMswxa7o4gWZeZ7FvKm2Eg&usqp=CAc ");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 7,
                column: "ImagenUrl",
                value: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ9iqqSfTfIlR4odS9nlUOxO8wH1lu98ceyvA&s");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 8,
                column: "ImagenUrl",
                value: "https://encrypted-tbn1.gstatic.com/shopping?q=tbn:ANd9GcRJ9QKeD42BVe8PcDCAbL9fvLS2Jzwe94dXAqJ0oavHxv_j0czNzcjJ_GV5mb08uhWwYxgQ-jiG9OcKmrtnzto6LFQC8jj9_KH2yHu-gfLRx4-yyqXGLUpGjo5M6MhQ_NxmX4cTlwo&usqp=CAc");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 9,
                column: "ImagenUrl",
                value: "https://ar.celulares.com/fotos/motorola-moto-e20-94506-g.jpg");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 10,
                column: "ImagenUrl",
                value: "https://encrypted-tbn1.gstatic.com/shopping?q=tbn:ANd9GcRwSPDvpeUVbL_SSTHEnUu9Wblp5hGqrjpY9gRjUplzy--_EbtfWpYKKi78wXxnCPR-bSBYx_gRI4LkRsCeyNNSpwpfbRPKNl0upmwAgdCX8wQzWJBkilJt2ID1c91RsijIkWngOk0&usqp=CAc");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 11,
                column: "ImagenUrl",
                value: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS0roqcdNDvARcExaG2lSjzefvfrittrcbpbw&s");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 12,
                column: "ImagenUrl",
                value: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSilXc9Z-LL-TY3amhFEDmSbRGHMQx030hskg&s");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 13,
                column: "ImagenUrl",
                value: "https://encrypted-tbn0.gstatic.com/shopping?q=tbn:ANd9GcSHFJCqbFx9N3cSRQrooUZxZrG_0BHZJ_9mNkzAgsEE6aV4Bi8bDRgYX-lm0HI7DEjSaLxr_arVnDhd2-9CvM5jMcSW2aheoIOUwjEh706K-2cU8JrQLmxjxzVx8XxKQLRfGNh20Q&usqp=CAc");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 14,
                column: "ImagenUrl",
                value: "https://http2.mlstatic.com/D_NQ_NP_2X_787732-MLU54979244644_042023-F.webp");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 15,
                column: "ImagenUrl",
                value: "https://encrypted-tbn1.gstatic.com/shopping?q=tbn:ANd9GcQ5UaDAgLwtkEMiUEkVqydxn0tOezRPg87TI2KiIvzRLhiqlN-HsGu2MQSL-VCbJRa_wuwiS7-0Bto7_DuvgqIKYqGW2xutu_MmUmMt6RjUAqQcnjXZZlSbWB37-e84osdqKWPJZNX-1FNc&usqp=CAc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImagenUrl",
                value: "https://www.motorola.cl/...png");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImagenUrl",
                value: "sin_imagen_para_que_de_error....muejeje");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 4,
                column: "ImagenUrl",
                value: "https://intercompras.com/...jpg");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 5,
                column: "ImagenUrl",
                value: "https://store.storeimages.cdn-apple.com/...jpg");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 6,
                column: "ImagenUrl",
                value: "https://consumer.huawei.com/...jpg");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 7,
                column: "ImagenUrl",
                value: "https://images.samsung.com/...jpg");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 8,
                column: "ImagenUrl",
                value: "https://store.storeimages.cdn-apple.com/...jpg");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 9,
                column: "ImagenUrl",
                value: "https://motorola.com/...jpg");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 10,
                column: "ImagenUrl",
                value: "https://sony.com/...jpg");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 11,
                column: "ImagenUrl",
                value: "https://store.storeimages.cdn-apple.com/...jpg");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 12,
                column: "ImagenUrl",
                value: "https://consumer.huawei.com/...jpg");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 13,
                column: "ImagenUrl",
                value: "https://images.samsung.com/...jpg");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 14,
                column: "ImagenUrl",
                value: "https://motorola.com/...jpg");

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 15,
                column: "ImagenUrl",
                value: "https://sony.com/...jpg");
        }
    }
}
