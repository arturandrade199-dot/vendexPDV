using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDevolucaoMercadoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devolucoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DataHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    VendaId = table.Column<int>(type: "INTEGER", nullable: true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Motivo = table.Column<string>(type: "TEXT", nullable: true),
                    ValorTotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    EstornouCaixa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devolucoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devolucoes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Devolucoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Devolucoes_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DevolucaoItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DevolucaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoVarianteId = table.Column<int>(type: "INTEGER", nullable: true),
                    Quantidade = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevolucaoItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevolucaoItens_Devolucoes_DevolucaoId",
                        column: x => x.DevolucaoId,
                        principalTable: "Devolucoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DevolucaoItens_ProdutoVariantes_ProdutoVarianteId",
                        column: x => x.ProdutoVarianteId,
                        principalTable: "ProdutoVariantes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DevolucaoItens_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DevolucaoItens_DevolucaoId",
                table: "DevolucaoItens",
                column: "DevolucaoId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucaoItens_ProdutoId",
                table: "DevolucaoItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucaoItens_ProdutoVarianteId",
                table: "DevolucaoItens",
                column: "ProdutoVarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_Devolucoes_ClienteId",
                table: "Devolucoes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Devolucoes_UsuarioId",
                table: "Devolucoes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Devolucoes_VendaId",
                table: "Devolucoes",
                column: "VendaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevolucaoItens");

            migrationBuilder.DropTable(
                name: "Devolucoes");
        }
    }
}
