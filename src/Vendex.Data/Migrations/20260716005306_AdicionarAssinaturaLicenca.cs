using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarAssinaturaLicenca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataValidaAte",
                table: "Licencas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Licencas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaDataVista",
                table: "Licencas",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaVerificacaoOnline",
                table: "Licencas",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataValidaAte",
                table: "Licencas");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Licencas");

            migrationBuilder.DropColumn(
                name: "UltimaDataVista",
                table: "Licencas");

            migrationBuilder.DropColumn(
                name: "UltimaVerificacaoOnline",
                table: "Licencas");
        }
    }
}
