using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductcompanyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductGroupId",
                table: "Company",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_ProductGroupId",
                table: "Company",
                column: "ProductGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Company_ProductGroup_ProductGroupId",
                table: "Company",
                column: "ProductGroupId",
                principalTable: "ProductGroup",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Company_ProductGroup_ProductGroupId",
                table: "Company");

            migrationBuilder.DropIndex(
                name: "IX_Company_ProductGroupId",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "ProductGroupId",
                table: "Company");
        }
    }
}
