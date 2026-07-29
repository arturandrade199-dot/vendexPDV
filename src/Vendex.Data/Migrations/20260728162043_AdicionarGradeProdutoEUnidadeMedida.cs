using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarGradeProdutoEUnidadeMedida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantidade",
                table: "VendaItens",
                type: "TEXT",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "ProdutoVarianteId",
                table: "VendaItens",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "EstoqueAtual",
                table: "Produtos",
                type: "TEXT",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<bool>(
                name: "TemGrade",
                table: "Produtos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UnidadeMedida",
                table: "Produtos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProdutoVariantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    CodigoBarras = table.Column<string>(type: "TEXT", nullable: true),
                    EstoqueAtual = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoVariantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProdutoVariantes_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VendaItens_ProdutoVarianteId",
                table: "VendaItens",
                column: "ProdutoVarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoVariantes_ProdutoId",
                table: "ProdutoVariantes",
                column: "ProdutoId");

            migrationBuilder.AddForeignKey(
                name: "FK_VendaItens_ProdutoVariantes_ProdutoVarianteId",
                table: "VendaItens",
                column: "ProdutoVarianteId",
                principalTable: "ProdutoVariantes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VendaItens_ProdutoVariantes_ProdutoVarianteId",
                table: "VendaItens");

            migrationBuilder.DropTable(
                name: "ProdutoVariantes");

            migrationBuilder.DropIndex(
                name: "IX_VendaItens_ProdutoVarianteId",
                table: "VendaItens");

            migrationBuilder.DropColumn(
                name: "ProdutoVarianteId",
                table: "VendaItens");

            migrationBuilder.DropColumn(
                name: "TemGrade",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "UnidadeMedida",
                table: "Produtos");

            migrationBuilder.AlterColumn<int>(
                name: "Quantidade",
                table: "VendaItens",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<int>(
                name: "EstoqueAtual",
                table: "Produtos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldPrecision: 18,
                oldScale: 3);
        }
    }
}
