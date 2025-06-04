using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation.Data.Migrations
{
    /// <inheritdoc />
    public partial class oneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_advertisements_propertyId",
                table: "advertisements");

            migrationBuilder.DropIndex(
                name: "IX_advertisements_serviceId",
                table: "advertisements");

            migrationBuilder.CreateIndex(
                name: "IX_advertisements_propertyId",
                table: "advertisements",
                column: "propertyId",
                unique: true,
                filter: "[propertyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_advertisements_serviceId",
                table: "advertisements",
                column: "serviceId",
                unique: true,
                filter: "[serviceId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_advertisements_propertyId",
                table: "advertisements");

            migrationBuilder.DropIndex(
                name: "IX_advertisements_serviceId",
                table: "advertisements");

            migrationBuilder.CreateIndex(
                name: "IX_advertisements_propertyId",
                table: "advertisements",
                column: "propertyId");

            migrationBuilder.CreateIndex(
                name: "IX_advertisements_serviceId",
                table: "advertisements",
                column: "serviceId");
        }
    }
}
