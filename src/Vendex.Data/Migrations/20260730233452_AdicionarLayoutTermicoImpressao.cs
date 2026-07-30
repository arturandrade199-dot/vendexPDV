using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarLayoutTermicoImpressao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CnpjLoja",
                table: "ConfiguracoesImpressao",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoLoja",
                table: "ConfiguracoesImpressao",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeLoja",
                table: "ConfiguracoesImpressao",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UsarLayoutTermico",
                table: "ConfiguracoesImpressao",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CnpjLoja",
                table: "ConfiguracoesImpressao");

            migrationBuilder.DropColumn(
                name: "EnderecoLoja",
                table: "ConfiguracoesImpressao");

            migrationBuilder.DropColumn(
                name: "NomeLoja",
                table: "ConfiguracoesImpressao");

            migrationBuilder.DropColumn(
                name: "UsarLayoutTermico",
                table: "ConfiguracoesImpressao");
        }
    }
}
