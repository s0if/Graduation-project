using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation.Data.Migrations
{
    /// <inheritdoc />
    public partial class addlatAndlngToAddressPropertyAndservice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "lat",
                table: "services",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "lng",
                table: "services",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "lat",
                table: "properties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "lng",
                table: "properties",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lat",
                table: "services");

            migrationBuilder.DropColumn(
                name: "lng",
                table: "services");

            migrationBuilder.DropColumn(
                name: "lat",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "lng",
                table: "properties");
        }
    }
}
