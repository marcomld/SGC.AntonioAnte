using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace SGC.AntonioAnte.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Modulo2_FichaCatastral_Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Catastro");

            migrationBuilder.CreateTable(
                name: "EstadosConservacion",
                schema: "Catastro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoActivo = table.Column<bool>(type: "bit", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosConservacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Predios",
                schema: "Catastro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaveCatastral = table.Column<string>(type: "nvarchar(28)", maxLength: 28, nullable: false),
                    ClaveAnterior = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TipoPredio = table.Column<int>(type: "int", nullable: false),
                    AreaTerrenoEscritura = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AreaTerrenoGrafica = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PoligonoEspacial = table.Column<Geometry>(type: "geometry", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Propietarios",
                schema: "Catastro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoPropietario = table.Column<int>(type: "int", nullable: false),
                    Identificacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RazonSocial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EstadoCivil = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Propietarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposEstructura",
                schema: "Catastro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoActivo = table.Column<bool>(type: "bit", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposEstructura", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposTenencia",
                schema: "Catastro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoActivo = table.Column<bool>(type: "bit", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposTenencia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BloquesConstruccion",
                schema: "Catastro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PredioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroBloque = table.Column<int>(type: "int", nullable: false),
                    TipoEstructuraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstadoConservacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroPisos = table.Column<int>(type: "int", nullable: false),
                    AreaConstruccion = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AnioConstruccion = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloquesConstruccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloquesConstruccion_EstadosConservacion_EstadoConservacionId",
                        column: x => x.EstadoConservacionId,
                        principalSchema: "Catastro",
                        principalTable: "EstadosConservacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BloquesConstruccion_Predios_PredioId",
                        column: x => x.PredioId,
                        principalSchema: "Catastro",
                        principalTable: "Predios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BloquesConstruccion_TiposEstructura_TipoEstructuraId",
                        column: x => x.TipoEstructuraId,
                        principalSchema: "Catastro",
                        principalTable: "TiposEstructura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Dominios",
                schema: "Catastro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PredioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropietarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoTenenciaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PorcentajePropiedad = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notaria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dominios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dominios_Predios_PredioId",
                        column: x => x.PredioId,
                        principalSchema: "Catastro",
                        principalTable: "Predios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dominios_Propietarios_PropietarioId",
                        column: x => x.PropietarioId,
                        principalSchema: "Catastro",
                        principalTable: "Propietarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dominios_TiposTenencia_TipoTenenciaId",
                        column: x => x.TipoTenenciaId,
                        principalSchema: "Catastro",
                        principalTable: "TiposTenencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BloquesConstruccion_EstadoConservacionId",
                schema: "Catastro",
                table: "BloquesConstruccion",
                column: "EstadoConservacionId");

            migrationBuilder.CreateIndex(
                name: "IX_BloquesConstruccion_PredioId",
                schema: "Catastro",
                table: "BloquesConstruccion",
                column: "PredioId");

            migrationBuilder.CreateIndex(
                name: "IX_BloquesConstruccion_TipoEstructuraId",
                schema: "Catastro",
                table: "BloquesConstruccion",
                column: "TipoEstructuraId");

            migrationBuilder.CreateIndex(
                name: "IX_Dominios_PredioId",
                schema: "Catastro",
                table: "Dominios",
                column: "PredioId");

            migrationBuilder.CreateIndex(
                name: "IX_Dominios_PropietarioId",
                schema: "Catastro",
                table: "Dominios",
                column: "PropietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Dominios_TipoTenenciaId",
                schema: "Catastro",
                table: "Dominios",
                column: "TipoTenenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_Predios_ClaveCatastral",
                schema: "Catastro",
                table: "Predios",
                column: "ClaveCatastral",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Propietarios_Identificacion",
                schema: "Catastro",
                table: "Propietarios",
                column: "Identificacion",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BloquesConstruccion",
                schema: "Catastro");

            migrationBuilder.DropTable(
                name: "Dominios",
                schema: "Catastro");

            migrationBuilder.DropTable(
                name: "EstadosConservacion",
                schema: "Catastro");

            migrationBuilder.DropTable(
                name: "TiposEstructura",
                schema: "Catastro");

            migrationBuilder.DropTable(
                name: "Predios",
                schema: "Catastro");

            migrationBuilder.DropTable(
                name: "Propietarios",
                schema: "Catastro");

            migrationBuilder.DropTable(
                name: "TiposTenencia",
                schema: "Catastro");
        }
    }
}
