using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontelloImport.Data.PIMigrations
{
    /// <inheritdoc />
    public partial class RepairAddressDealerID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder mb)
        {
            // Backfill DealerID for BillingAddress rows that were created before
            // the DealerID column existed (those rows have DealerID = NULL).
            mb.Sql(@"
                UPDATE Addresses
                SET DealerID = (
                    SELECT DealerID FROM Dealers
                    WHERE Dealers.BillingAddressID = Addresses.AddressID
                )
                WHERE DealerID IS NULL
                AND AddressID IN (
                    SELECT BillingAddressID FROM Dealers
                    WHERE BillingAddressID IS NOT NULL
                )
            ");

            // Backfill DealerID for ShippingAddress rows (if different from billing).
            mb.Sql(@"
                UPDATE Addresses
                SET DealerID = (
                    SELECT DealerID FROM Dealers
                    WHERE Dealers.ShippingAddressID = Addresses.AddressID
                )
                WHERE DealerID IS NULL
                AND AddressID IN (
                    SELECT ShippingAddressID FROM Dealers
                    WHERE ShippingAddressID IS NOT NULL
                )
            ");

            // Set IsDefault = true for BillingAddress rows so they appear as
            // default in the UI after the backfill.
            mb.Sql(@"
                UPDATE Addresses
                SET IsDefault = 1
                WHERE AddressID IN (
                    SELECT BillingAddressID FROM Dealers
                    WHERE BillingAddressID IS NOT NULL
                )
                AND IsDefault = 0
            ");

            // Set AddressType = 'Both' for billing addresses that have no type.
            mb.Sql(@"
                UPDATE Addresses
                SET AddressType = 'Both'
                WHERE AddressID IN (
                    SELECT BillingAddressID FROM Dealers
                    WHERE BillingAddressID IS NOT NULL
                )
                AND (AddressType IS NULL OR AddressType = '')
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder mb)
        {
            // Revert: clear DealerID on addresses that were backfilled.
            mb.Sql(@"
                UPDATE Addresses
                SET DealerID = NULL
                WHERE AddressID IN (
                    SELECT BillingAddressID FROM Dealers
                    WHERE BillingAddressID IS NOT NULL
                    UNION
                    SELECT ShippingAddressID FROM Dealers
                    WHERE ShippingAddressID IS NOT NULL
                )
            ");
        }
    }
}
