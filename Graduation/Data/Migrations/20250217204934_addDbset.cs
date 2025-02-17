using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation.Data.Migrations
{
    /// <inheritdoc />
    public partial class addDbset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaint_AspNetUsers_UsersID",
                table: "Complaint");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageDetails_Property_PropertyId",
                table: "ImageDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageDetails_Service_ServiceId",
                table: "ImageDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_AspNetUsers_UsersID",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Property_PropertyId",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Service_ServiceId",
                table: "Review");

            migrationBuilder.DropTable(
                name: "Property");

            migrationBuilder.DropTable(
                name: "Service");

            migrationBuilder.DropTable(
                name: "Type");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Review",
                table: "Review");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImageDetails",
                table: "ImageDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Complaint",
                table: "Complaint");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Advertisement",
                table: "Advertisement");

            migrationBuilder.RenameTable(
                name: "Review",
                newName: "reviews");

            migrationBuilder.RenameTable(
                name: "ImageDetails",
                newName: "images");

            migrationBuilder.RenameTable(
                name: "Complaint",
                newName: "complaints");

            migrationBuilder.RenameTable(
                name: "Advertisement",
                newName: "advertisements");

            migrationBuilder.RenameIndex(
                name: "IX_Review_UsersID",
                table: "reviews",
                newName: "IX_reviews_UsersID");

            migrationBuilder.RenameIndex(
                name: "IX_Review_ServiceId",
                table: "reviews",
                newName: "IX_reviews_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_Review_PropertyId",
                table: "reviews",
                newName: "IX_reviews_PropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageDetails_ServiceId",
                table: "images",
                newName: "IX_images_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageDetails_PropertyId",
                table: "images",
                newName: "IX_images_PropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_Complaint_UsersID",
                table: "complaints",
                newName: "IX_complaints_UsersID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_reviews",
                table: "reviews",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_images",
                table: "images",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_complaints",
                table: "complaints",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_advertisements",
                table: "advertisements",
                column: "Id");

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

            migrationBuilder.CreateTable(
                name: "properties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsersID = table.Column<int>(type: "int", nullable: false),
                    AdvertisementID = table.Column<int>(type: "int", nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_properties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_properties_AspNetUsers_UsersID",
                        column: x => x.UsersID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_properties_advertisements_AdvertisementID",
                        column: x => x.AdvertisementID,
                        principalTable: "advertisements",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_properties_types_TypeId",
                        column: x => x.TypeId,
                        principalTable: "types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<int>(type: "int", nullable: false),
                    PriceRange = table.Column<double>(type: "float", nullable: false),
                    UsersID = table.Column<int>(type: "int", nullable: false),
                    AdvertisementID = table.Column<int>(type: "int", nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_services_AspNetUsers_UsersID",
                        column: x => x.UsersID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_services_advertisements_AdvertisementID",
                        column: x => x.AdvertisementID,
                        principalTable: "advertisements",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_services_types_TypeId",
                        column: x => x.TypeId,
                        principalTable: "types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_properties_AdvertisementID",
                table: "properties",
                column: "AdvertisementID");

            migrationBuilder.CreateIndex(
                name: "IX_properties_TypeId",
                table: "properties",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_properties_UsersID",
                table: "properties",
                column: "UsersID");

            migrationBuilder.CreateIndex(
                name: "IX_services_AdvertisementID",
                table: "services",
                column: "AdvertisementID");

            migrationBuilder.CreateIndex(
                name: "IX_services_TypeId",
                table: "services",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_services_UsersID",
                table: "services",
                column: "UsersID");

            migrationBuilder.AddForeignKey(
                name: "FK_complaints_AspNetUsers_UsersID",
                table: "complaints",
                column: "UsersID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_images_properties_PropertyId",
                table: "images",
                column: "PropertyId",
                principalTable: "properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_images_services_ServiceId",
                table: "images",
                column: "ServiceId",
                principalTable: "services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_AspNetUsers_UsersID",
                table: "reviews",
                column: "UsersID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_properties_PropertyId",
                table: "reviews",
                column: "PropertyId",
                principalTable: "properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_services_ServiceId",
                table: "reviews",
                column: "ServiceId",
                principalTable: "services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_complaints_AspNetUsers_UsersID",
                table: "complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_images_properties_PropertyId",
                table: "images");

            migrationBuilder.DropForeignKey(
                name: "FK_images_services_ServiceId",
                table: "images");

            migrationBuilder.DropForeignKey(
                name: "FK_reviews_AspNetUsers_UsersID",
                table: "reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_reviews_properties_PropertyId",
                table: "reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_reviews_services_ServiceId",
                table: "reviews");

            migrationBuilder.DropTable(
                name: "properties");

            migrationBuilder.DropTable(
                name: "services");

            migrationBuilder.DropTable(
                name: "types");

            migrationBuilder.DropPrimaryKey(
                name: "PK_reviews",
                table: "reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_images",
                table: "images");

            migrationBuilder.DropPrimaryKey(
                name: "PK_complaints",
                table: "complaints");

            migrationBuilder.DropPrimaryKey(
                name: "PK_advertisements",
                table: "advertisements");

            migrationBuilder.RenameTable(
                name: "reviews",
                newName: "Review");

            migrationBuilder.RenameTable(
                name: "images",
                newName: "ImageDetails");

            migrationBuilder.RenameTable(
                name: "complaints",
                newName: "Complaint");

            migrationBuilder.RenameTable(
                name: "advertisements",
                newName: "Advertisement");

            migrationBuilder.RenameIndex(
                name: "IX_reviews_UsersID",
                table: "Review",
                newName: "IX_Review_UsersID");

            migrationBuilder.RenameIndex(
                name: "IX_reviews_ServiceId",
                table: "Review",
                newName: "IX_Review_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_reviews_PropertyId",
                table: "Review",
                newName: "IX_Review_PropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_images_ServiceId",
                table: "ImageDetails",
                newName: "IX_ImageDetails_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_images_PropertyId",
                table: "ImageDetails",
                newName: "IX_ImageDetails_PropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_complaints_UsersID",
                table: "Complaint",
                newName: "IX_Complaint_UsersID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Review",
                table: "Review",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImageDetails",
                table: "ImageDetails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Complaint",
                table: "Complaint",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Advertisement",
                table: "Advertisement",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Type",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Type", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Property",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdvertisementID = table.Column<int>(type: "int", nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    UsersID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Property", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Property_Advertisement_AdvertisementID",
                        column: x => x.AdvertisementID,
                        principalTable: "Advertisement",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Property_AspNetUsers_UsersID",
                        column: x => x.UsersID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Property_Type_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Type",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Service",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdvertisementID = table.Column<int>(type: "int", nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    UsersID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<int>(type: "int", nullable: false),
                    PriceRange = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Service_Advertisement_AdvertisementID",
                        column: x => x.AdvertisementID,
                        principalTable: "Advertisement",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Service_AspNetUsers_UsersID",
                        column: x => x.UsersID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Service_Type_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Type",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Property_AdvertisementID",
                table: "Property",
                column: "AdvertisementID");

            migrationBuilder.CreateIndex(
                name: "IX_Property_TypeId",
                table: "Property",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Property_UsersID",
                table: "Property",
                column: "UsersID");

            migrationBuilder.CreateIndex(
                name: "IX_Service_AdvertisementID",
                table: "Service",
                column: "AdvertisementID");

            migrationBuilder.CreateIndex(
                name: "IX_Service_TypeId",
                table: "Service",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Service_UsersID",
                table: "Service",
                column: "UsersID");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaint_AspNetUsers_UsersID",
                table: "Complaint",
                column: "UsersID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageDetails_Property_PropertyId",
                table: "ImageDetails",
                column: "PropertyId",
                principalTable: "Property",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageDetails_Service_ServiceId",
                table: "ImageDetails",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_AspNetUsers_UsersID",
                table: "Review",
                column: "UsersID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Property_PropertyId",
                table: "Review",
                column: "PropertyId",
                principalTable: "Property",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Service_ServiceId",
                table: "Review",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
