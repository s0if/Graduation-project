using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation.Data.Migrations
{
    /// <inheritdoc />
    public partial class addRelationBetwenComplaintAndImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "complaintId",
                table: "images",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_images_complaintId",
                table: "images",
                column: "complaintId");

            migrationBuilder.AddForeignKey(
                name: "FK_images_complaints_complaintId",
                table: "images",
                column: "complaintId",
                principalTable: "complaints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_images_complaints_complaintId",
                table: "images");

            migrationBuilder.DropIndex(
                name: "IX_images_complaintId",
                table: "images");

            migrationBuilder.DropColumn(
                name: "complaintId",
                table: "images");
        }
    }
}
