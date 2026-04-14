using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APM.ConTaxi.Migrations
{
    /// <inheritdoc />
    public partial class Migration_1_RenameBasicRoleName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("7035810c-ede3-4ffc-ab72-14cf85061a04"),
                column: "RoleName",
                value: "Administrator");

            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("a4ee65f8-ebd9-47aa-ad77-7ecad0cf9db6"),
                column: "RoleName",
                value: "Warehouse Manager");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("7035810c-ede3-4ffc-ab72-14cf85061a04"),
                column: "RoleName",
                value: "系统管理员");

            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("a4ee65f8-ebd9-47aa-ad77-7ecad0cf9db6"),
                column: "RoleName",
                value: "仓库管理员");
        }
    }
}
