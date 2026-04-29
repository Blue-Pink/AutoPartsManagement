using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APM.ConTaxi.Migrations
{
    /// <inheritdoc />
    public partial class Migration_3_CreatePartView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
         CREATE VIEW vw_PartView AS
        SELECT 
            p.Id,
            p.PartName,
            p.OECode,
            p.Model,
            p.Brand,
            p.CostPrice,
            p.SellingPrice,
            p.MinStock,
            p.MaxStock,
            p.CreatedAt,
            p.ModifiedAt,
            p.CategoryId,
            p.UnitId,
            c.Name AS CategoryName,
            u.Name AS UnitName
        FROM Part p
        LEFT JOIN PartCategory c ON p.CategoryId = c.Id
        LEFT JOIN PartUnit u ON p.UnitId = u.Id
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW vw_PartView");
        }
    }
}
