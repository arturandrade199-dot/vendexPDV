using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendex.Data.Migrations
{
    /// <inheritdoc />
    public partial class PermissoesPorAcao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PodeCriar",
                table: "UsuarioPermissoes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PodeEditar",
                table: "UsuarioPermissoes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PodeExcluir",
                table: "UsuarioPermissoes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // Instalações existentes: quem já podia acessar um módulo herda acesso total
            // (criar/editar/excluir) nele, preservando o comportamento anterior a esta
            // migration — sem essa herança, todo Funcionário perderia essas capacidades
            // até o Administrador reabrir e reconfigurar cada usuário manualmente.
            migrationBuilder.Sql(
                "UPDATE \"UsuarioPermissoes\" SET \"PodeCriar\" = 1, \"PodeEditar\" = 1, \"PodeExcluir\" = 1 WHERE \"PodeAcessar\" = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PodeCriar",
                table: "UsuarioPermissoes");

            migrationBuilder.DropColumn(
                name: "PodeEditar",
                table: "UsuarioPermissoes");

            migrationBuilder.DropColumn(
                name: "PodeExcluir",
                table: "UsuarioPermissoes");
        }
    }
}
