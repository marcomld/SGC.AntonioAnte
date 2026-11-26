using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGC.AntonioAnte.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RelacionesAvaluosCatastro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LiquidacionesPrediales_PredioId",
                schema: "Avaluos",
                table: "LiquidacionesPrediales",
                column: "PredioId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoresDepreciacion_EstadoConservacionId",
                schema: "Avaluos",
                table: "FactoresDepreciacion",
                column: "EstadoConservacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CostosReposicion_TipoEstructuraId",
                schema: "Avaluos",
                table: "CostosReposicion",
                column: "TipoEstructuraId");

            migrationBuilder.AddForeignKey(
                name: "FK_CostosReposicion_TiposEstructura_TipoEstructuraId",
                schema: "Avaluos",
                table: "CostosReposicion",
                column: "TipoEstructuraId",
                principalSchema: "Catastro",
                principalTable: "TiposEstructura",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FactoresDepreciacion_EstadosConservacion_EstadoConservacionId",
                schema: "Avaluos",
                table: "FactoresDepreciacion",
                column: "EstadoConservacionId",
                principalSchema: "Catastro",
                principalTable: "EstadosConservacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LiquidacionesPrediales_Predios_PredioId",
                schema: "Avaluos",
                table: "LiquidacionesPrediales",
                column: "PredioId",
                principalSchema: "Catastro",
                principalTable: "Predios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CostosReposicion_TiposEstructura_TipoEstructuraId",
                schema: "Avaluos",
                table: "CostosReposicion");

            migrationBuilder.DropForeignKey(
                name: "FK_FactoresDepreciacion_EstadosConservacion_EstadoConservacionId",
                schema: "Avaluos",
                table: "FactoresDepreciacion");

            migrationBuilder.DropForeignKey(
                name: "FK_LiquidacionesPrediales_Predios_PredioId",
                schema: "Avaluos",
                table: "LiquidacionesPrediales");

            migrationBuilder.DropIndex(
                name: "IX_LiquidacionesPrediales_PredioId",
                schema: "Avaluos",
                table: "LiquidacionesPrediales");

            migrationBuilder.DropIndex(
                name: "IX_FactoresDepreciacion_EstadoConservacionId",
                schema: "Avaluos",
                table: "FactoresDepreciacion");

            migrationBuilder.DropIndex(
                name: "IX_CostosReposicion_TipoEstructuraId",
                schema: "Avaluos",
                table: "CostosReposicion");
        }
    }
}
