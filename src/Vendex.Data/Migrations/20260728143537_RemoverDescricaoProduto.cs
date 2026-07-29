using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendex.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoverDescricaoProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Produtos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Produtos",
                type: "TEXT",
                nullable: true);
        }
    }
}
