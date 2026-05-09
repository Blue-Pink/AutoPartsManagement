using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APM.ConTaxi.Migrations
{
    /// <inheritdoc />
    public partial class Migration_8_CreateVWAllBoundItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE VIEW vw_AllBoundItemView AS
SELECT Id, PartId, Quantity, TotalAmount, Convert(int, '1') as Type, CreatedAt, ModifiedAt, OperatorUserId FROM InboundItem
UNION ALL
SELECT Id, PartId, -Quantity, TotalAmount, Convert(int, '0') as Type, CreatedAt, ModifiedAt, OperatorUserId FROM OutboundItem
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW vw_AllBoundItemView");
        }
    }
}
