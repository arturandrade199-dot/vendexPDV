using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarControleDeValidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoVarianteId = table.Column<int>(type: "INTEGER", nullable: true),
                    DataFabricacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataValidade = table.Column<DateTime>(type: "TEXT", nullable: false),
                    QuantidadeInicial = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    QuantidadeAtual = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lotes_ProdutoVariantes_ProdutoVarianteId",
                        column: x => x.ProdutoVarianteId,
                        principalTable: "ProdutoVariantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lotes_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LotePerdas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoteId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Quantidade = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    Motivo = table.Column<string>(type: "TEXT", nullable: true),
                    PrecoCustoUnitarioNaData = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ValorPerdido = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotePerdas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotePerdas_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LotePerdas_LoteId",
                table: "LotePerdas",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_ProdutoId",
                table: "Lotes",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_ProdutoVarianteId",
                table: "Lotes",
                column: "ProdutoVarianteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LotePerdas");

            migrationBuilder.DropTable(
                name: "Lotes");
        }
    }
}
