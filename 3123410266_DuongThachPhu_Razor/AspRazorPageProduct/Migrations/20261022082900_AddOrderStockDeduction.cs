using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspRazorPageProduct.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderStockDeduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StockDeducted",
                table: "Orders",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockDeducted",
                table: "Orders");
        }
    }
}
