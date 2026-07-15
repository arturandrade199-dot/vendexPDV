using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarConfiguracaoBackup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracoesBackup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Horario = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    CaminhoDestino = table.Column<string>(type: "TEXT", nullable: true),
                    UltimoBackupData = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UltimoBackupSucesso = table.Column<bool>(type: "INTEGER", nullable: false),
                    UltimaMensagemErro = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesBackup", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesBackup");
        }
    }
}
