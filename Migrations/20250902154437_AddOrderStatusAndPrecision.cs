using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_CommerceSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderStatusAndPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Normalize existing data so the ALTER won't fail
            migrationBuilder.Sql(@"
        UPDATE [Orders]
        SET [Status] = 'Pending'
        WHERE [Status] IS NULL OR LTRIM(RTRIM([Status])) = '';
    ");

            migrationBuilder.Sql(@"
        UPDATE [Orders]
        SET [Status] = LEFT([Status], 50)
        WHERE LEN([Status]) > 50;
    ");

            // 2) Now safely alter the column to nvarchar(50) NOT NULL with default
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending",      // better default than empty string
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
