using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation.Data.Migrations
{
    /// <inheritdoc />
    public partial class addModelTypeServce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_properties_types_TypeId",
                table: "properties");

            migrationBuilder.DropForeignKey(
                name: "FK_services_types_TypeId",
                table: "services");

            migrationBuilder.DropTable(
                name: "types");

            migrationBuilder.CreateTable(
                name: "typeProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_typeProperties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "typeServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_typeServices", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_properties_typeProperties_TypeId",
                table: "properties",
                column: "TypeId",
                principalTable: "typeProperties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_services_typeServices_TypeId",
                table: "services",
                column: "TypeId",
                principalTable: "typeServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_properties_typeProperties_TypeId",
                table: "properties");

            migrationBuilder.DropForeignKey(
                name: "FK_services_typeServices_TypeId",
                table: "services");

            migrationBuilder.DropTable(
                name: "typeProperties");

            migrationBuilder.DropTable(
                name: "typeServices");

            migrationBuilder.CreateTable(
                name: "types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_types", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_properties_types_TypeId",
                table: "properties",
                column: "TypeId",
                principalTable: "types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_services_types_TypeId",
                table: "services",
                column: "TypeId",
                principalTable: "types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
