using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CotizacionMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddEtapaNegociacionYAlcanceVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EtapaNegociacion",
                table: "Leads",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AlcanceVenta",
                table: "Cotizaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EtapaNegociacion",
                table: "Cotizaciones",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EtapaNegociacion",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "AlcanceVenta",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "EtapaNegociacion",
                table: "Cotizaciones");
        }
    }
}
