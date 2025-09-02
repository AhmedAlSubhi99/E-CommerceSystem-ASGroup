using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_CommerceSystem.Migrations
{
    /// <inheritdoc />
    public partial class editUserIDColumninOrderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_userUID",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_userUID",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "userUID",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UID",
                table: "Orders",
                column: "UID");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_UID",
                table: "Orders",
                column: "UID",
                principalTable: "Users",
                principalColumn: "UID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_UID",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UID",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "userUID",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_userUID",
                table: "Orders",
                column: "userUID");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_userUID",
                table: "Orders",
                column: "userUID",
                principalTable: "Users",
                principalColumn: "UID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
