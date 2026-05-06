using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APM.ConTaxi.Migrations
{
    /// <inheritdoc />
    public partial class Migration_6_CreateCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OperatorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customer_User_OperatorUserId",
                        column: x => x.OperatorUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("7035810c-ede3-4ffc-ab72-14cf85061a04"),
                column: "OperatorUserId",
                value: new Guid("F1A89D52-1C0F-4070-A6DD-761A04FCF7F4"));

            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("a4ee65f8-ebd9-47aa-ad77-7ecad0cf9db6"),
                column: "OperatorUserId",
                value: new Guid("F1A89D52-1C0F-4070-A6DD-761A04FCF7F4"));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"),
                column: "OperatorUserId",
                value: new Guid("F1A89D52-1C0F-4070-A6DD-761A04FCF7F4"));

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "Id",
                keyValue: new Guid("8b006bae-2330-4b7e-a6b5-e3defe6e92ef"),
                column: "OperatorUserId",
                value: new Guid("F1A89D52-1C0F-4070-A6DD-761A04FCF7F4"));

            migrationBuilder.CreateIndex(
                name: "IX_Customer_OperatorUserId",
                table: "Customer",
                column: "OperatorUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customer");

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
        }
    }
}
