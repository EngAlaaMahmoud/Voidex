using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewTaxRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MainCode",
                table: "Tax",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubCode",
                table: "Tax",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeCode",
                table: "Tax",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeName",
                table: "Tax",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tax_MainCode",
                table: "Tax",
                column: "MainCode");

            migrationBuilder.CreateIndex(
                name: "IX_Tax_SubCode",
                table: "Tax",
                column: "SubCode");

            migrationBuilder.CreateIndex(
                name: "IX_Tax_TypeCode",
                table: "Tax",
                column: "TypeCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tax_MainCode",
                table: "Tax");

            migrationBuilder.DropIndex(
                name: "IX_Tax_SubCode",
                table: "Tax");

            migrationBuilder.DropIndex(
                name: "IX_Tax_TypeCode",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "MainCode",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "SubCode",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "TypeCode",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "TypeName",
                table: "Tax");
        }
    }
}
