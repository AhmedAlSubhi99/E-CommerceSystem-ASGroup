using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_CommerceSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UID",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CreatedByIp",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "ReplacedByToken",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "RevokedByIp",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "UID",
                table: "RefreshTokens",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_UID",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "RefreshTokens",
                newName: "UID");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_UID");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByIp",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReplacedByToken",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RevokedByIp",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UID",
                table: "RefreshTokens",
                column: "UID",
                principalTable: "Users",
                principalColumn: "UID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
