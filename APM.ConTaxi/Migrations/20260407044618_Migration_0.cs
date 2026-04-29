using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace APM.ConTaxi.Migrations
{
    /// <inheritdoc />
    public partial class Migration_0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RealName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "CreatedAt", "Description", "ModifiedAt", "RoleName" },
                values: new object[,]
                {
                    { new Guid("7035810c-ede3-4ffc-ab72-14cf85061a04"), new DateTime(2026, 3, 25, 6, 20, 21, 0, DateTimeKind.Unspecified), "系统管理员", new DateTime(2026, 3, 25, 6, 20, 21, 0, DateTimeKind.Unspecified), "系统管理员" },
                    { new Guid("a4ee65f8-ebd9-47aa-ad77-7ecad0cf9db6"), new DateTime(2026, 3, 25, 6, 20, 21, 0, DateTimeKind.Unspecified), "仓库管理员", new DateTime(2026, 3, 25, 6, 20, 21, 0, DateTimeKind.Unspecified), "仓库管理员" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "CreatedAt", "IsActive", "ModifiedAt", "PasswordHash", "Realname", "Username" },
                values: new object[] { new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"), new DateTime(2026, 3, 25, 6, 20, 21, 0, DateTimeKind.Unspecified), true, new DateTime(2026, 3, 25, 6, 20, 21, 0, DateTimeKind.Unspecified), "$2a$11$ToqAlthCo6lbu4j6kAb8m.7XIP9gCOUgQRCBsSorupnn88xK9S5ee", "Administrator", "Administrator" });

            migrationBuilder.InsertData(
                table: "UserRole",
                columns: new[] { "Id", "AssignedAt", "CreatedAt", "ModifiedAt", "RoleId", "UserId" },
                values: new object[] { new Guid("8b006bae-2330-4b7e-a6b5-e3defe6e92ef"), new DateTime(2026, 3, 25, 6, 20, 21, 0, DateTimeKind.Unspecified), new DateTime(2026, 3, 25, 6, 20, 21, 0, DateTimeKind.Unspecified), new DateTime(2026, 3, 25, 6, 20, 21, 0, DateTimeKind.Unspecified), new Guid("7035810c-ede3-4ffc-ab72-14cf85061a04"), new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4") });

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId_RoleId",
                table: "UserRole",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.Sql(@"CREATE VIEW vw_UserRoleView AS
SELECT 
    u.Id AS UserId,
    r.Id AS RoleId,
    u.Username, 
    u.Realname, 
    r.RoleName, 
    r.Description AS RoleDescription,
    ur.AssignedAt,
    ur.CreatedAt,
    ur.ModifiedAt
FROM [User] u
JOIN UserRole ur ON u.Id = ur.UserId
JOIN Role r ON r.Id = ur.RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_UserRoleView");
        }
    }
}
