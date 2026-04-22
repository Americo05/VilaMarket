using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lab3Ano.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMotorCarro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =================================================================
            // PARTE 1: IGNORAR MUDANÇAS NA TABELA FAVORITO (ESTÁ A DAR ERRO)
            // =================================================================

            /* migrationBuilder.DropForeignKey(
                name: "FK_Favorito_Anuncio_IdAnuncioNavigationId",
                table: "Favorito");

            migrationBuilder.DropForeignKey(
                name: "FK_Favorito_Comprador_IdCompradorNavigationId",
                table: "Favorito");

            migrationBuilder.DropIndex(
                name: "IX_Favorito_IdAnuncioNavigationId",
                table: "Favorito");

            migrationBuilder.DropIndex(
                name: "IX_Favorito_IdCompradorNavigationId",
                table: "Favorito");

            migrationBuilder.DropColumn(
                name: "IdAnuncioNavigationId",
                table: "Favorito");

            migrationBuilder.DropColumn(
                name: "IdCompradorNavigationId",
                table: "Favorito");
            */

            // =================================================================
            // PARTE 2: NOVIDADES (ESTAS QUEREMOS MANTER!)
            // =================================================================

            // Adicionar IsBloqueado ao Vendedor (Útil para o teu painel Admin)
            //migrationBuilder.AddColumn<bool>(
            //    name: "IsBloqueado",
            //    table: "Vendedor",
            //    type: "bit",
            //    nullable: false,
            //    defaultValue: false);

            /* // Comenta isto também do Favorito para não dar conflito de tipos
            migrationBuilder.AlterColumn<string>(
                name: "IdComprador",
                table: "Favorito",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            */

            // Adicionar IsBloqueado ao Comprador
            //migrationBuilder.AddColumn<bool>(
            //    name: "IsBloqueado",
            //    table: "Comprador",
            //    type: "bit",
            //    nullable: false,
            //    defaultValue: false);

            // --- AQUI ESTÁ O QUE TU QUERES MESMO ---
            migrationBuilder.AddColumn<int>(
                name: "Cilindrada",
                table: "Carro",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Potencia",
                table: "Carro",
                type: "int",
                nullable: false,
                defaultValue: 0);

            /*
            // Mais coisas do Favorito para ignorar agora
            migrationBuilder.CreateIndex(
                name: "IX_Favorito_IdAnuncio",
                table: "Favorito",
                column: "IdAnuncio");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorito_Anuncio_IdAnuncio",
                table: "Favorito",
                column: "IdAnuncio",
                principalTable: "Anuncio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorito_Anuncio_IdAnuncio",
                table: "Favorito");

            migrationBuilder.DropIndex(
                name: "IX_Favorito_IdAnuncio",
                table: "Favorito");

            migrationBuilder.DropColumn(
                name: "IsBloqueado",
                table: "Vendedor");

            migrationBuilder.DropColumn(
                name: "IsBloqueado",
                table: "Comprador");

            migrationBuilder.DropColumn(
                name: "Cilindrada",
                table: "Carro");

            migrationBuilder.DropColumn(
                name: "Potencia",
                table: "Carro");

            migrationBuilder.AlterColumn<int>(
                name: "IdComprador",
                table: "Favorito",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "IdAnuncioNavigationId",
                table: "Favorito",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdCompradorNavigationId",
                table: "Favorito",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Favorito_IdAnuncioNavigationId",
                table: "Favorito",
                column: "IdAnuncioNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorito_IdCompradorNavigationId",
                table: "Favorito",
                column: "IdCompradorNavigationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorito_Anuncio_IdAnuncioNavigationId",
                table: "Favorito",
                column: "IdAnuncioNavigationId",
                principalTable: "Anuncio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Favorito_Comprador_IdCompradorNavigationId",
                table: "Favorito",
                column: "IdCompradorNavigationId",
                principalTable: "Comprador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
