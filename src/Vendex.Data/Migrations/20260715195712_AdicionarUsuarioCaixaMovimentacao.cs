using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarUsuarioCaixaMovimentacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "CaixaMovimentacoes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CaixaMovimentacoes_UsuarioId",
                table: "CaixaMovimentacoes",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaixaMovimentacoes_Usuarios_UsuarioId",
                table: "CaixaMovimentacoes",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaixaMovimentacoes_Usuarios_UsuarioId",
                table: "CaixaMovimentacoes");

            migrationBuilder.DropIndex(
                name: "IX_CaixaMovimentacoes_UsuarioId",
                table: "CaixaMovimentacoes");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "CaixaMovimentacoes");
        }
    }
}
