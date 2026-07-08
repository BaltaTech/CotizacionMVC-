using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CotizacionMVC.Migrations
{
    /// <inheritdoc />
    public partial class CamposRecepcionCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComentarioNoCotizable",
                table: "Clientes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Clientes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAsignacion",
                table: "Clientes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCotizacion",
                table: "Clientes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MotivoNoCotizable",
                table: "Clientes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Origen",
                table: "Clientes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "RegistradoPorId",
                table: "Clientes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "VendedorAsignadoId",
                table: "Clientes",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComentarioNoCotizable",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaAsignacion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaCotizacion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "MotivoNoCotizable",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Origen",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "RegistradoPorId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "VendedorAsignadoId",
                table: "Clientes");
        }
    }
}
