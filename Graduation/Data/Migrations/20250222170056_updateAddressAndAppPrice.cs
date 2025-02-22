using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateAddressAndAppPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_properties_addresses_AddressId",
                table: "properties");

            migrationBuilder.DropForeignKey(
                name: "FK_services_addresses_AddressId",
                table: "services");

            migrationBuilder.AlterColumn<int>(
                name: "AddressId",
                table: "services",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "services",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<int>(
                name: "AddressId",
                table: "properties",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "properties",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddForeignKey(
                name: "FK_properties_addresses_AddressId",
                table: "properties",
                column: "AddressId",
                principalTable: "addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_services_addresses_AddressId",
                table: "services",
                column: "AddressId",
                principalTable: "addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_properties_addresses_AddressId",
                table: "properties");

            migrationBuilder.DropForeignKey(
                name: "FK_services_addresses_AddressId",
                table: "services");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "services");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "properties");

            migrationBuilder.AlterColumn<int>(
                name: "AddressId",
                table: "services",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AddressId",
                table: "properties",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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
    }
}
