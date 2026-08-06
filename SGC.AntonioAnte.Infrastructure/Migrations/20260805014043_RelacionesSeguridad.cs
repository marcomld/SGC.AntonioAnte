using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGC.AntonioAnte.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RelacionesSeguridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Departamento",
                schema: "Seguridad",
                table: "Usuarios");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartamentoId",
                schema: "Seguridad",
                table: "Usuarios",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_DepartamentoId",
                schema: "Seguridad",
                table: "Usuarios",
                column: "DepartamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_UsuarioId",
                schema: "Seguridad",
                table: "Auditorias",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auditorias_Usuarios_UsuarioId",
                schema: "Seguridad",
                table: "Auditorias",
                column: "UsuarioId",
                principalSchema: "Seguridad",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Departamentos_DepartamentoId",
                schema: "Seguridad",
                table: "Usuarios",
                column: "DepartamentoId",
                principalSchema: "Seguridad",
                principalTable: "Departamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auditorias_Usuarios_UsuarioId",
                schema: "Seguridad",
                table: "Auditorias");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Departamentos_DepartamentoId",
                schema: "Seguridad",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_DepartamentoId",
                schema: "Seguridad",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Auditorias_UsuarioId",
                schema: "Seguridad",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "DepartamentoId",
                schema: "Seguridad",
                table: "Usuarios");

            migrationBuilder.AddColumn<string>(
                name: "Departamento",
                schema: "Seguridad",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
