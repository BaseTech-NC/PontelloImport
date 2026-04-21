using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontelloImport.Data.PIMigrations
{
    /// <inheritdoc />
    public partial class AddThreeTrackOrderStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingStatus",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: "NotInvoiced");

            migrationBuilder.AddColumn<string>(
                name: "FulfillmentStatus",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: "NotShipped");

            migrationBuilder.AddColumn<DateTime>(
                name: "ShipDate",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            // Backfill — default everything first
            migrationBuilder.Sql(@"
                UPDATE Orders
                SET FulfillmentStatus = 'NotShipped',
                    BillingStatus = 'NotInvoiced'
            ");

            // Old Shipped orders → FulfillmentStatus, Status → Confirmed
            migrationBuilder.Sql(@"
                UPDATE Orders
                SET FulfillmentStatus = 'Shipped',
                    ShipDate = datetime('now'),
                    Status = 'Confirmed'
                WHERE Status = 'Shipped'
            ");

            // Old Invoiced orders → BillingStatus, Status → Confirmed
            migrationBuilder.Sql(@"
                UPDATE Orders
                SET BillingStatus = 'Invoiced',
                    InvoicedAt = datetime('now'),
                    InvoiceNumber = 'INV-' || OrderNumber,
                    Status = 'Confirmed'
                WHERE Status = 'Invoiced'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FulfillmentStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShipDate",
                table: "Orders");
        }
    }
}
