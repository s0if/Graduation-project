using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation.Data.Migrations
{
    /// <inheritdoc />
    public partial class editTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lat",
                table: "services");

            migrationBuilder.DropColumn(
                name: "lng",
                table: "services");

            migrationBuilder.RenameColumn(
                name: "EndAt",
                table: "properties",
                newName: "updateAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateAt",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "updateAt",
                table: "properties",
                newName: "EndAt");

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
        }
    }
}
