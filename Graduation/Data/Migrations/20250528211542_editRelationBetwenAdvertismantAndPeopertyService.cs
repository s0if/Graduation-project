using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation.Data.Migrations
{
    /// <inheritdoc />
    public partial class editRelationBetwenAdvertismantAndPeopertyService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_properties_advertisements_AdvertisementID",
                table: "properties");

            migrationBuilder.DropForeignKey(
                name: "FK_services_advertisements_AdvertisementID",
                table: "services");

            migrationBuilder.DropIndex(
                name: "IX_services_AdvertisementID",
                table: "services");

            migrationBuilder.DropIndex(
                name: "IX_properties_AdvertisementID",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "AdvertisementID",
                table: "services");

            migrationBuilder.DropColumn(
                name: "AdvertisementID",
                table: "properties");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdvertisementID",
                table: "services",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdvertisementID",
                table: "properties",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_services_AdvertisementID",
                table: "services",
                column: "AdvertisementID");

            migrationBuilder.CreateIndex(
                name: "IX_properties_AdvertisementID",
                table: "properties",
                column: "AdvertisementID");

            migrationBuilder.AddForeignKey(
                name: "FK_properties_advertisements_AdvertisementID",
                table: "properties",
                column: "AdvertisementID",
                principalTable: "advertisements",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_services_advertisements_AdvertisementID",
                table: "services",
                column: "AdvertisementID",
                principalTable: "advertisements",
                principalColumn: "Id");
        }
    }
}
