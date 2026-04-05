using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontelloImport.Data.PIMigrations
{
    /// <inheritdoc />
    public partial class AddNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DealerID = table.Column<int>(type: "INTEGER", nullable: true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    ActionUrl = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK_Notifications_Dealers_DealerID",
                        column: x => x.DealerID,
                        principalTable: "Dealers",
                        principalColumn: "DealerID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DealerID",
                table: "Notifications",
                column: "DealerID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
