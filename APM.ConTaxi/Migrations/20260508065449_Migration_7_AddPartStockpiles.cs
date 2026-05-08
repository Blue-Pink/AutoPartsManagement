using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APM.ConTaxi.Migrations
{
    /// <inheritdoc />
    public partial class Migration_7_AddPartStockpiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stockpiles",
                table: "Part",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stockpiles",
                table: "Part");
        }
    }
}
