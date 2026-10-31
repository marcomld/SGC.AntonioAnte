using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGC.AntonioAnte.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMotivoReliquidacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                schema: "Avaluos",
                table: "LiquidacionesPrediales",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                schema: "Avaluos",
                table: "LiquidacionesPrediales");
        }
    }
}
