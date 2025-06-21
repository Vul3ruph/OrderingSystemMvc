using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderingSystemMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUserProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdminPermissionsId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AdminPermissionsId",
                table: "AspNetUsers",
                column: "AdminPermissionsId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AdminPermissions_AdminPermissionsId",
                table: "AspNetUsers",
                column: "AdminPermissionsId",
                principalTable: "AdminPermissions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AdminPermissions_AdminPermissionsId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AdminPermissionsId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AdminPermissionsId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "AspNetUsers");
        }
    }
}
