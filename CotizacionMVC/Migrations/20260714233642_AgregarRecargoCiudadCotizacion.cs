using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CotizacionMVC.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRecargoCiudadCotizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RecargoCiudadPorcentaje",
                table: "Cotizaciones",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RecargoCiudad_Moneda",
                table: "Cotizaciones",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "RecargoCiudad_Monto",
                table: "Cotizaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RecargoCiudad_TipoCambio",
                table: "Cotizaciones",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TipoCambio",
                table: "Cotizaciones",
                type: "numeric(10,4)",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecargoCiudadPorcentaje",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "RecargoCiudad_Moneda",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "RecargoCiudad_Monto",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "RecargoCiudad_TipoCambio",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "TipoCambio",
                table: "Cotizaciones");
        }
    }
}
