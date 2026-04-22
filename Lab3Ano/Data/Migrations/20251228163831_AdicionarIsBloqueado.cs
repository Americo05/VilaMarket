using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lab3Ano.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarIsBloqueado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.AddColumn<bool>(
                name: "IsBloqueado",
                table: "Comprador",
                type: "bit", // ou "boolean" dependendo do SQL
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBloqueado",
                table: "Vendedor",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
        }
    }
}
