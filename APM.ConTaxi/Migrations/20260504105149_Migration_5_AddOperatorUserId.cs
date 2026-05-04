using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APM.ConTaxi.Migrations
{
    /// <inheritdoc />
    public partial class Migration_5_AddOperatorUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OperatorUserId",
                table: "UserRole",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"));

            migrationBuilder.AddColumn<Guid>(
                name: "OperatorUserId",
                table: "User",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"));

            migrationBuilder.AddColumn<Guid>(
                name: "OperatorUserId",
                table: "Supplier",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"));

            migrationBuilder.AddColumn<Guid>(
                name: "OperatorUserId",
                table: "RolePermission",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"));

            migrationBuilder.AddColumn<Guid>(
                name: "OperatorUserId",
                table: "Role",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"));

            migrationBuilder.AddColumn<Guid>(
                name: "OperatorUserId",
                table: "PartUnit",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"));

            migrationBuilder.AddColumn<Guid>(
                name: "OperatorUserId",
                table: "PartCategory",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"));

            migrationBuilder.AddColumn<Guid>(
                name: "OperatorUserId",
                table: "Part",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"));

            migrationBuilder.AddColumn<Guid>(
                name: "OperatorUserId",
                table: "InboundItem",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"));

            migrationBuilder.AddColumn<Guid>(
                name: "OperatorUserId",
                table: "EntityRecord",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"));

            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("7035810c-ede3-4ffc-ab72-14cf85061a04"),
                column: "OperatorUserId",
                value: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"));

            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("a4ee65f8-ebd9-47aa-ad77-7ecad0cf9db6"),
                column: "OperatorUserId",
                value: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"),
                column: "OperatorUserId",
                value: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"));

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "Id",
                keyValue: new Guid("8b006bae-2330-4b7e-a6b5-e3defe6e92ef"),
                column: "OperatorUserId",
                value: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"));

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_OperatorUserId",
                table: "UserRole",
                column: "OperatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_OperatorUserId",
                table: "User",
                column: "OperatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_OperatorUserId",
                table: "Supplier",
                column: "OperatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_OperatorUserId",
                table: "RolePermission",
                column: "OperatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_OperatorUserId",
                table: "Role",
                column: "OperatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartUnit_OperatorUserId",
                table: "PartUnit",
                column: "OperatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartCategory_OperatorUserId",
                table: "PartCategory",
                column: "OperatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Part_OperatorUserId",
                table: "Part",
                column: "OperatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InboundItem_OperatorUserId",
                table: "InboundItem",
                column: "OperatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityRecord_OperatorUserId",
                table: "EntityRecord",
                column: "OperatorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EntityRecord_User_OperatorUserId",
                table: "EntityRecord",
                column: "OperatorUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InboundItem_User_OperatorUserId",
                table: "InboundItem",
                column: "OperatorUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Part_User_OperatorUserId",
                table: "Part",
                column: "OperatorUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartCategory_User_OperatorUserId",
                table: "PartCategory",
                column: "OperatorUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartUnit_User_OperatorUserId",
                table: "PartUnit",
                column: "OperatorUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Role_User_OperatorUserId",
                table: "Role",
                column: "OperatorUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_User_OperatorUserId",
                table: "RolePermission",
                column: "OperatorUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Supplier_User_OperatorUserId",
                table: "Supplier",
                column: "OperatorUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_User_User_OperatorUserId",
                table: "User",
                column: "OperatorUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_User_OperatorUserId",
                table: "UserRole",
                column: "OperatorUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntityRecord_User_OperatorUserId",
                table: "EntityRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_InboundItem_User_OperatorUserId",
                table: "InboundItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Part_User_OperatorUserId",
                table: "Part");

            migrationBuilder.DropForeignKey(
                name: "FK_PartCategory_User_OperatorUserId",
                table: "PartCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_PartUnit_User_OperatorUserId",
                table: "PartUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_Role_User_OperatorUserId",
                table: "Role");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_User_OperatorUserId",
                table: "RolePermission");

            migrationBuilder.DropForeignKey(
                name: "FK_Supplier_User_OperatorUserId",
                table: "Supplier");

            migrationBuilder.DropForeignKey(
                name: "FK_User_User_OperatorUserId",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_User_OperatorUserId",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_OperatorUserId",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_User_OperatorUserId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_Supplier_OperatorUserId",
                table: "Supplier");

            migrationBuilder.DropIndex(
                name: "IX_RolePermission_OperatorUserId",
                table: "RolePermission");

            migrationBuilder.DropIndex(
                name: "IX_Role_OperatorUserId",
                table: "Role");

            migrationBuilder.DropIndex(
                name: "IX_PartUnit_OperatorUserId",
                table: "PartUnit");

            migrationBuilder.DropIndex(
                name: "IX_PartCategory_OperatorUserId",
                table: "PartCategory");

            migrationBuilder.DropIndex(
                name: "IX_Part_OperatorUserId",
                table: "Part");

            migrationBuilder.DropIndex(
                name: "IX_InboundItem_OperatorUserId",
                table: "InboundItem");

            migrationBuilder.DropIndex(
                name: "IX_EntityRecord_OperatorUserId",
                table: "EntityRecord");

            migrationBuilder.DropColumn(
                name: "OperatorUserId",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "OperatorUserId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "OperatorUserId",
                table: "Supplier");

            migrationBuilder.DropColumn(
                name: "OperatorUserId",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "OperatorUserId",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "OperatorUserId",
                table: "PartUnit");

            migrationBuilder.DropColumn(
                name: "OperatorUserId",
                table: "PartCategory");

            migrationBuilder.DropColumn(
                name: "OperatorUserId",
                table: "Part");

            migrationBuilder.DropColumn(
                name: "OperatorUserId",
                table: "InboundItem");

            migrationBuilder.DropColumn(
                name: "OperatorUserId",
                table: "EntityRecord");
        }
    }
}
