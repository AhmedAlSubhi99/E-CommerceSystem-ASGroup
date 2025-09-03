using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_CommerceSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_UID",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UID_PID",
                table: "Reviews",
                columns: new[] { "UID", "PID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_UID_PID",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UID",
                table: "Reviews",
                column: "UID");
        }
    }
}
