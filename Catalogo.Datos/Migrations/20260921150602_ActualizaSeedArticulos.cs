using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace catalogo_web_mvc.Migrations
{
    /// <inheritdoc />
    public partial class ActualizaSeedArticulos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Descripcion", "Precio" },
                values: new object[] { "Celular con pantalla infinita y triple cámara", 239000m });

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Descripcion", "Precio" },
                values: new object[] { "Celular de gama media con batería de larga duración", 120000m });

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Descripcion", "Precio" },
                values: new object[] { "Consola de videojuegos con lector Blu-ray", 350000m });

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Descripcion", "Precio" },
                values: new object[] { "Televisor LED de 55 pulgadas", 700000m });

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Descripcion", "Precio" },
                values: new object[] { "Reproductor multimedia para streaming", 250000m });

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 6,
                column: "Precio",
                value: 280000m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 7,
                column: "Precio",
                value: 1400000m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 8,
                column: "Precio",
                value: 350000m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 9,
                column: "Precio",
                value: 90000m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 10,
                column: "Precio",
                value: 450000m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 11,
                column: "Precio",
                value: 1800000m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 12,
                column: "Precio",
                value: 1200000m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 13,
                column: "Precio",
                value: 150000m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 14,
                column: "Precio",
                value: 400000m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 15,
                column: "Precio",
                value: 900000m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Descripcion", "Precio" },
                values: new object[] { "Una canoa cara", 69999m });

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Descripcion", "Precio" },
                values: new object[] { "Ya siete de estos?", 15699m });

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Descripcion", "Precio" },
                values: new object[] { "Ya no se cuantas versiones hay", 35000m });

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Descripcion", "Precio" },
                values: new object[] { "Alta tele", 49500m });

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Descripcion", "Precio" },
                values: new object[] { "lindo loro", 7850m });

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 6,
                column: "Precio",
                value: 45999m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 7,
                column: "Precio",
                value: 120000m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 8,
                column: "Precio",
                value: 8999m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 9,
                column: "Precio",
                value: 10999m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 10,
                column: "Precio",
                value: 29999m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 11,
                column: "Precio",
                value: 150000m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 12,
                column: "Precio",
                value: 95000m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 13,
                column: "Precio",
                value: 12999m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 14,
                column: "Precio",
                value: 39999m);

            migrationBuilder.UpdateData(
                table: "Articulos",
                keyColumn: "Id",
                keyValue: 15,
                column: "Precio",
                value: 250000m);
        }
    }
}
