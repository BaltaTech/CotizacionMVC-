using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CotizacionMVC.Migrations
{
    /// <inheritdoc />
    public partial class QuitarIgnoreColecciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmpresaUsuario",
                columns: table => new
                {
                    EmpresasAccesoId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuariosAccesoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaUsuario", x => new { x.EmpresasAccesoId, x.UsuariosAccesoId });
                    table.ForeignKey(
                        name: "FK_EmpresaUsuario_Empresas_EmpresasAccesoId",
                        column: x => x.EmpresasAccesoId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmpresaUsuario_Usuarios_UsuariosAccesoId",
                        column: x => x.UsuariosAccesoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaUsuario_UsuariosAccesoId",
                table: "EmpresaUsuario",
                column: "UsuariosAccesoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpresaUsuario");
        }
    }
}
