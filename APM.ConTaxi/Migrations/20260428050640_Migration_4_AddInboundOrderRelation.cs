using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APM.ConTaxi.Migrations
{
    /// <inheritdoc />
    public partial class Migration_4_AddInboundOrderRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InboundItem_Part_PartId",
                table: "InboundItem");

            migrationBuilder.DropForeignKey(
                name: "FK_InboundOrder_Supplier_SupplierId",
                table: "InboundOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_InboundOrder_User_OperatorUserId",
                table: "InboundOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_Part_Category_CategoryId",
                table: "Part");

            migrationBuilder.DropForeignKey(
                name: "FK_Part_Unit_UnitId",
                table: "Part");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Unit",
                table: "Unit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Category",
                table: "Category");

            migrationBuilder.RenameTable(
                name: "Unit",
                newName: "PartUnit");

            migrationBuilder.RenameTable(
                name: "Category",
                newName: "PartCategory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartUnit",
                table: "PartUnit",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartCategory",
                table: "PartCategory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InboundItem_Part_PartId",
                table: "InboundItem",
                column: "PartId",
                principalTable: "Part",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InboundOrder_Supplier_SupplierId",
                table: "InboundOrder",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InboundOrder_User_OperatorUserId",
                table: "InboundOrder",
                column: "OperatorUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Part_PartCategory_CategoryId",
                table: "Part",
                column: "CategoryId",
                principalTable: "PartCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Part_PartUnit_UnitId",
                table: "Part",
                column: "UnitId",
                principalTable: "PartUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InboundItem_Part_PartId",
                table: "InboundItem");

            migrationBuilder.DropForeignKey(
                name: "FK_InboundOrder_Supplier_SupplierId",
                table: "InboundOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_InboundOrder_User_OperatorUserId",
                table: "InboundOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_Part_PartCategory_CategoryId",
                table: "Part");

            migrationBuilder.DropForeignKey(
                name: "FK_Part_PartUnit_UnitId",
                table: "Part");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartUnit",
                table: "PartUnit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartCategory",
                table: "PartCategory");

            migrationBuilder.RenameTable(
                name: "PartUnit",
                newName: "Unit");

            migrationBuilder.RenameTable(
                name: "PartCategory",
                newName: "Category");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Unit",
                table: "Unit",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Category",
                table: "Category",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InboundItem_Part_PartId",
                table: "InboundItem",
                column: "PartId",
                principalTable: "Part",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InboundOrder_Supplier_SupplierId",
                table: "InboundOrder",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InboundOrder_User_OperatorUserId",
                table: "InboundOrder",
                column: "OperatorUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Part_Category_CategoryId",
                table: "Part",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Part_Unit_UnitId",
                table: "Part",
                column: "UnitId",
                principalTable: "Unit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
