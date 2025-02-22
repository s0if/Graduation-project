using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation.Data.Migrations
{
    /// <inheritdoc />
    public partial class addAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "services",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_services_AddressId",
                table: "services",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_properties_AddressId",
                table: "properties",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AddressId",
                table: "AspNetUsers",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_addresses_AddressId",
                table: "AspNetUsers",
                column: "AddressId",
                principalTable: "addresses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_properties_addresses_AddressId",
                table: "properties",
                column: "AddressId",
                principalTable: "addresses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_services_addresses_AddressId",
                table: "services",
                column: "AddressId",
                principalTable: "addresses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_addresses_AddressId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_properties_addresses_AddressId",
                table: "properties");

            migrationBuilder.DropForeignKey(
                name: "FK_services_addresses_AddressId",
                table: "services");

            migrationBuilder.DropTable(
                name: "addresses");

            migrationBuilder.DropIndex(
                name: "IX_services_AddressId",
                table: "services");

            migrationBuilder.DropIndex(
                name: "IX_properties_AddressId",
                table: "properties");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AddressId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "services");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
