using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CotizacionMVC.Migrations
{
    /// <inheritdoc />
    public partial class AgregarModuloSeguimientosV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Clientes_ClienteId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Comentarios",
                table: "Seguimientos");

            migrationBuilder.RenameColumn(
                name: "FechaProgramada",
                table: "Seguimientos",
                newName: "FechaContacto");

            migrationBuilder.RenameColumn(
                name: "FechaCompletado",
                table: "Seguimientos",
                newName: "ProximoContacto");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "Seguimientos",
                newName: "Resultado");

            migrationBuilder.AlterColumn<Guid>(
                name: "CotizacionId",
                table: "Seguimientos",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "LeadId",
                table: "Seguimientos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "Seguimientos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RecordatorioEnviado",
                table: "Seguimientos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ProductoBusca",
                table: "Leads",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ComentarioNoCotizable",
                table: "Leads",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrigenLead",
                table: "Leads",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoSeguimiento",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeadId",
                table: "Cotizaciones",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seguimientos_LeadId",
                table: "Seguimientos",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_LeadId",
                table: "Cotizaciones",
                column: "LeadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_Leads_LeadId",
                table: "Cotizaciones",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Clientes_ClienteId",
                table: "Leads",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Seguimientos_Leads_LeadId",
                table: "Seguimientos",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Leads_LeadId",
                table: "Cotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Clientes_ClienteId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Seguimientos_Leads_LeadId",
                table: "Seguimientos");

            migrationBuilder.DropIndex(
                name: "IX_Seguimientos_LeadId",
                table: "Seguimientos");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_LeadId",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "Seguimientos");

            migrationBuilder.DropColumn(
                name: "Notas",
                table: "Seguimientos");

            migrationBuilder.DropColumn(
                name: "RecordatorioEnviado",
                table: "Seguimientos");

            migrationBuilder.DropColumn(
                name: "OrigenLead",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "UltimoSeguimiento",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "Cotizaciones");

            migrationBuilder.RenameColumn(
                name: "Resultado",
                table: "Seguimientos",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "ProximoContacto",
                table: "Seguimientos",
                newName: "FechaCompletado");

            migrationBuilder.RenameColumn(
                name: "FechaContacto",
                table: "Seguimientos",
                newName: "FechaProgramada");

            migrationBuilder.AlterColumn<Guid>(
                name: "CotizacionId",
                table: "Seguimientos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comentarios",
                table: "Seguimientos",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductoBusca",
                table: "Leads",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ComentarioNoCotizable",
                table: "Leads",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Clientes_ClienteId",
                table: "Leads",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id");
        }
    }
}
