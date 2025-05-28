using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation.Data.Migrations
{
    /// <inheritdoc />
    public partial class editAdvirtaismant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "propertyId",
                table: "advertisements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "serviceId",
                table: "advertisements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_advertisements_propertyId",
                table: "advertisements",
                column: "propertyId");

            migrationBuilder.CreateIndex(
                name: "IX_advertisements_serviceId",
                table: "advertisements",
                column: "serviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_advertisements_properties_propertyId",
                table: "advertisements",
                column: "propertyId",
                principalTable: "properties",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_advertisements_services_serviceId",
                table: "advertisements",
                column: "serviceId",
                principalTable: "services",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_advertisements_properties_propertyId",
                table: "advertisements");

            migrationBuilder.DropForeignKey(
                name: "FK_advertisements_services_serviceId",
                table: "advertisements");

            migrationBuilder.DropIndex(
                name: "IX_advertisements_propertyId",
                table: "advertisements");

            migrationBuilder.DropIndex(
                name: "IX_advertisements_serviceId",
                table: "advertisements");

            migrationBuilder.DropColumn(
                name: "propertyId",
                table: "advertisements");

            migrationBuilder.DropColumn(
                name: "serviceId",
                table: "advertisements");
        }
    }
}
