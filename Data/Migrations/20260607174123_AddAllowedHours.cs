using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeyManagment.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowedHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllowedHours",
                table: "KeyHandovers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedHours",
                table: "KeyHandovers");
        }
    }
}
