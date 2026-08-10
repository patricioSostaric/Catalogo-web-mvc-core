using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace catalogo_web_mvc.Migrations
{
    /// <inheritdoc />
    public partial class AddArticuloActivoStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Articulos",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Articulos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Activo", table: "Articulos");
            migrationBuilder.DropColumn(name: "Stock",  table: "Articulos");
        }
    }
}
