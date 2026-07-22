using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CotizacionMVC.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarItemCotizacionFactores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UtilidadVendedorPorcentaje",
                table: "ItemsCotizacion",
                newName: "SubtotalUSD_Monto");

            migrationBuilder.RenameColumn(
                name: "UtilidadEmpresaPorcentaje",
                table: "ItemsCotizacion",
                newName: "PrecioUnitarioUSD_Monto");

            migrationBuilder.AddColumn<decimal>(
                name: "FactorPrecio",
                table: "ItemsCotizacion",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FactorUtilidad",
                table: "ItemsCotizacion",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PrecioUnitarioUSD_Moneda",
                table: "ItemsCotizacion",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioUnitarioUSD_TipoCambio",
                table: "ItemsCotizacion",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubtotalUSD_Moneda",
                table: "ItemsCotizacion",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SubtotalUSD_TipoCambio",
                table: "ItemsCotizacion",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FactorPrecio",
                table: "ItemsCotizacion");

            migrationBuilder.DropColumn(
                name: "FactorUtilidad",
                table: "ItemsCotizacion");

            migrationBuilder.DropColumn(
                name: "PrecioUnitarioUSD_Moneda",
                table: "ItemsCotizacion");

            migrationBuilder.DropColumn(
                name: "PrecioUnitarioUSD_TipoCambio",
                table: "ItemsCotizacion");

            migrationBuilder.DropColumn(
                name: "SubtotalUSD_Moneda",
                table: "ItemsCotizacion");

            migrationBuilder.DropColumn(
                name: "SubtotalUSD_TipoCambio",
                table: "ItemsCotizacion");

            migrationBuilder.RenameColumn(
                name: "SubtotalUSD_Monto",
                table: "ItemsCotizacion",
                newName: "UtilidadVendedorPorcentaje");

            migrationBuilder.RenameColumn(
                name: "PrecioUnitarioUSD_Monto",
                table: "ItemsCotizacion",
                newName: "UtilidadEmpresaPorcentaje");
        }
    }
}
