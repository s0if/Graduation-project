using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation.Data.Migrations
{
    /// <inheritdoc />
    public partial class addCurrentTokenIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentTokenId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentTokenId",
                table: "AspNetUsers");
        }
    }
}
