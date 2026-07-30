using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeyManagment.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAllowedHoursToDouble : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "AllowedHours",
                table: "KeyHandovers",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AllowedHours",
                table: "KeyHandovers",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
