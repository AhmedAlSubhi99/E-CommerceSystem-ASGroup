using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_CommerceSystem.Migrations
{
    /// <inheritdoc />
    public partial class editUserIDColumnAndPidInReviewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Products_productPID",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_userUID",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_productPID",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_userUID",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "productPID",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "userUID",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_PID",
                table: "Reviews",
                column: "PID");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UID",
                table: "Reviews",
                column: "UID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Products_PID",
                table: "Reviews",
                column: "PID",
                principalTable: "Products",
                principalColumn: "PID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_UID",
                table: "Reviews",
                column: "UID",
                principalTable: "Users",
                principalColumn: "UID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Products_PID",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_UID",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_PID",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UID",
                table: "Reviews");

            migrationBuilder.AddColumn<int>(
                name: "productPID",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "userUID",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_productPID",
                table: "Reviews",
                column: "productPID");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_userUID",
                table: "Reviews",
                column: "userUID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Products_productPID",
                table: "Reviews",
                column: "productPID",
                principalTable: "Products",
                principalColumn: "PID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_userUID",
                table: "Reviews",
                column: "userUID",
                principalTable: "Users",
                principalColumn: "UID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
