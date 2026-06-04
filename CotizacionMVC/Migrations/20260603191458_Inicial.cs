using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CotizacionMVC.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Contacto_Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Contacto_TelefonoMovil = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Contacto_Correo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Contacto_NombreContacto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Direccion_Calle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Direccion_NumeroExterior = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Direccion_NumeroInterior = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Direccion_Colonia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Direccion_Ciudad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Direccion_Estado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Direccion_CodigoPostal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreComercial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NombreLegal = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EsExclusivaTrane = table.Column<bool>(type: "boolean", nullable: false),
                    MonedaBase = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    UtilidadEmpresaPorcentaje = table.Column<decimal>(type: "numeric", nullable: false),
                    UtilidadVendedorPorcentaje = table.Column<decimal>(type: "numeric", nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ColorPrimario = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ColorSecundario = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PlantillaPdfNombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TelefonoContacto = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CorreoContacto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SitioWeb = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Eslogan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Equipos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Marca = table.Column<int>(type: "integer", nullable: false),
                    Modelo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CapacidadToneladas = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Tension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Tecnologia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrecioBase = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MonedaOriginal = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Instalaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CostoUnitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instalaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CorreoElectronico = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContraseniaHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Rol = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UltimoAcceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cotizaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroCotizacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendedorId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    AreaMetrosCuadrados = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CondicionesPago = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Subtotal_Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal_Moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Subtotal_TipoCambio = table.Column<decimal>(type: "numeric", nullable: true),
                    Iva_Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Iva_Moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Iva_TipoCambio = table.Column<decimal>(type: "numeric", nullable: true),
                    Total_Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total_Moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Total_TipoCambio = table.Column<decimal>(type: "numeric", nullable: true),
                    RequiereAutorizacion = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cotizaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Usuarios_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendedorAsignadoId = table.Column<Guid>(type: "uuid", nullable: true),
                    NombreContacto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CorreoElectronico = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EmpresaCliente = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Categoria = table.Column<int>(type: "integer", nullable: false),
                    Origen = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ComentariosInternos = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leads_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leads_Usuarios_VendedorAsignadoId",
                        column: x => x.VendedorAsignadoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ItemsCotizacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CotizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario_Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioUnitario_Moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PrecioUnitario_TipoCambio = table.Column<decimal>(type: "numeric", nullable: true),
                    Subtotal_Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal_Moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Subtotal_TipoCambio = table.Column<decimal>(type: "numeric", nullable: true),
                    UtilidadEmpresaPorcentaje = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UtilidadVendedorPorcentaje = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DescripcionPersonalizada = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsCotizacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsCotizacion_Cotizaciones_CotizacionId",
                        column: x => x.CotizacionId,
                        principalTable: "Cotizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemsCotizacion_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemsInstalacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CotizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstalacionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    CostoUnitario_Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoUnitario_Moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CostoUnitario_TipoCambio = table.Column<decimal>(type: "numeric", nullable: true),
                    Subtotal_Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal_Moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Subtotal_TipoCambio = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsInstalacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsInstalacion_Cotizaciones_CotizacionId",
                        column: x => x.CotizacionId,
                        principalTable: "Cotizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemsInstalacion_Instalaciones_InstalacionId",
                        column: x => x.InstalacionId,
                        principalTable: "Instalaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Seguimientos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CotizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendedorId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaProgramada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCompletado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Comentarios = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MedioContacto = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seguimientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seguimientos_Cotizaciones_CotizacionId",
                        column: x => x.CotizacionId,
                        principalTable: "Cotizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Seguimientos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Seguimientos_Usuarios_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_ClienteId",
                table: "Cotizaciones",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_EmpresaId",
                table: "Cotizaciones",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_NumeroCotizacion",
                table: "Cotizaciones",
                column: "NumeroCotizacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_VendedorId",
                table: "Cotizaciones",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsCotizacion_CotizacionId",
                table: "ItemsCotizacion",
                column: "CotizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsCotizacion_EquipoId",
                table: "ItemsCotizacion",
                column: "EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsInstalacion_CotizacionId",
                table: "ItemsInstalacion",
                column: "CotizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsInstalacion_InstalacionId",
                table: "ItemsInstalacion",
                column: "InstalacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_EmpresaId",
                table: "Leads",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_VendedorAsignadoId",
                table: "Leads",
                column: "VendedorAsignadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Seguimientos_CotizacionId",
                table: "Seguimientos",
                column: "CotizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Seguimientos_EmpresaId",
                table: "Seguimientos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Seguimientos_VendedorId",
                table: "Seguimientos",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_CorreoElectronico",
                table: "Usuarios",
                column: "CorreoElectronico",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemsCotizacion");

            migrationBuilder.DropTable(
                name: "ItemsInstalacion");

            migrationBuilder.DropTable(
                name: "Leads");

            migrationBuilder.DropTable(
                name: "Seguimientos");

            migrationBuilder.DropTable(
                name: "Equipos");

            migrationBuilder.DropTable(
                name: "Instalaciones");

            migrationBuilder.DropTable(
                name: "Cotizaciones");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Empresas");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
