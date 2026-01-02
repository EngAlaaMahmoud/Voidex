using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatetax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tax_Name",
                table: "Tax");

            migrationBuilder.DropIndex(
                name: "IX_Tax_TypeCode",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "TypeCode",
                table: "Tax");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Tax",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeCode",
                table: "Tax",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tax_Name",
                table: "Tax",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Tax_TypeCode",
                table: "Tax",
                column: "TypeCode");
        }
    }
}
