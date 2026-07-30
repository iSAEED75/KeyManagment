using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeyManagment.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReturnNotes",
                table: "KeyHandovers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnNotes",
                table: "KeyHandovers");
        }
    }
}
