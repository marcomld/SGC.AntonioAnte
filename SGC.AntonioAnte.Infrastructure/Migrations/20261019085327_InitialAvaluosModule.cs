using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGC.AntonioAnte.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialAvaluosModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Avaluos");

            migrationBuilder.CreateTable(
                name: "CostosReposicion",
                schema: "Avaluos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnioFiscal = table.Column<int>(type: "int", nullable: false),
                    TipoEstructuraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ValorMetroCuadrado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostosReposicion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FactoresAfectacion",
                schema: "Avaluos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnioFiscal = table.Column<int>(type: "int", nullable: false),
                    TipoAfectacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Coeficiente = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoresAfectacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FactoresDepreciacion",
                schema: "Avaluos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnioFiscal = table.Column<int>(type: "int", nullable: false),
                    EstadoConservacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EdadMinima = table.Column<int>(type: "int", nullable: false),
                    EdadMaxima = table.Column<int>(type: "int", nullable: false),
                    Coeficiente = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoresDepreciacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LiquidacionesPrediales",
                schema: "Avaluos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PredioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnioFiscal = table.Column<int>(type: "int", nullable: false),
                    EsSimulacion = table.Column<bool>(type: "bit", nullable: false),
                    ValorTerreno = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorConstruccion = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseImponible = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExoneracionesPrevias = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorEmision = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CemAplicada = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EstadoLiquidacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidacionesPrediales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreciosBaseSuelo",
                schema: "Avaluos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnioFiscal = table.Column<int>(type: "int", nullable: false),
                    TipoPredio = table.Column<int>(type: "int", nullable: false),
                    Zona = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Sector = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ManzanaPoligono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ValorMetroCuadrado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreciosBaseSuelo", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CostosReposicion",
                schema: "Avaluos");

            migrationBuilder.DropTable(
                name: "FactoresAfectacion",
                schema: "Avaluos");

            migrationBuilder.DropTable(
                name: "FactoresDepreciacion",
                schema: "Avaluos");

            migrationBuilder.DropTable(
                name: "LiquidacionesPrediales",
                schema: "Avaluos");

            migrationBuilder.DropTable(
                name: "PreciosBaseSuelo",
                schema: "Avaluos");
        }
    }
}
