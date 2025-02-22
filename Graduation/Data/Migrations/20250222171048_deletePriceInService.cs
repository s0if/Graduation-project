using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation.Data.Migrations
{
    /// <inheritdoc />
    public partial class deletePriceInService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "services");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "services",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
