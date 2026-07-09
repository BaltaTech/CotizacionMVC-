using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CotizacionMVC.Migrations
{
    /// <inheritdoc />
    public partial class ExpandirLeadAOportunidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClienteId",
                table: "Leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComentarioNoCotizable",
                table: "Leads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Leads",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaContacto",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCotizacion",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MotivoNoCotizable",
                table: "Leads",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductoBusca",
                table: "Leads",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Folio",
                table: "Clientes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ClienteId",
                table: "Leads",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Folio",
                table: "Clientes",
                column: "Folio",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Clientes_ClienteId",
                table: "Leads",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Clientes_ClienteId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_ClienteId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Folio",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ComentarioNoCotizable",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "FechaContacto",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "FechaCotizacion",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "MotivoNoCotizable",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ProductoBusca",
                table: "Leads");

            migrationBuilder.AlterColumn<string>(
                name: "Folio",
                table: "Clientes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
