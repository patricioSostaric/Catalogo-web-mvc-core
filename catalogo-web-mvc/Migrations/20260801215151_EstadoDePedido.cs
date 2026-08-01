using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace catalogo_web_mvc.Migrations
{
    /// <inheritdoc />
    public partial class EstadoDePedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Pedidos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimoEstado",
                table: "Pedidos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "FechaUltimoEstado",
                table: "Pedidos");
        }
    }
}
