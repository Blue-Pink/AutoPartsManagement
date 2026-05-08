using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APM.ConTaxi.Migrations
{
    /// <inheritdoc />
    public partial class Migration_7_CreateOutbound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OrderNo",
                table: "InboundOrder",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "OutboundOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OutboundDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OperatorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboundOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutboundOrder_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OutboundOrder_User_OperatorUserId",
                        column: x => x.OperatorUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OutboundItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OutboundOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OperatorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboundItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutboundItem_OutboundOrder_OutboundOrderId",
                        column: x => x.OutboundOrderId,
                        principalTable: "OutboundOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OutboundItem_Part_PartId",
                        column: x => x.PartId,
                        principalTable: "Part",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OutboundItem_User_OperatorUserId",
                        column: x => x.OperatorUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboundOrder_OrderNo",
                table: "InboundOrder",
                column: "OrderNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboundItem_OperatorUserId",
                table: "OutboundItem",
                column: "OperatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboundItem_OutboundOrderId",
                table: "OutboundItem",
                column: "OutboundOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboundItem_PartId",
                table: "OutboundItem",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboundOrder_CustomerId",
                table: "OutboundOrder",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboundOrder_OperatorUserId",
                table: "OutboundOrder",
                column: "OperatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboundOrder_OrderNo",
                table: "OutboundOrder",
                column: "OrderNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboundItem");

            migrationBuilder.DropTable(
                name: "OutboundOrder");

            migrationBuilder.DropIndex(
                name: "IX_InboundOrder_OrderNo",
                table: "InboundOrder");

            migrationBuilder.AlterColumn<string>(
                name: "OrderNo",
                table: "InboundOrder",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
