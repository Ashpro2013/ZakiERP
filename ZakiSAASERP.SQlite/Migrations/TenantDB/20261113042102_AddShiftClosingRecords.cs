using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZakiSAASERP.SQlite.Migrations.TenantDB
{
    /// <inheritdoc />
    public partial class AddShiftClosingRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""ShiftClosingRecords"" (
                    ""ShiftClosingRecordId"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""ShiftCode"" TEXT NOT NULL,
                    ""UserName"" TEXT NOT NULL,
                    ""Terminal"" TEXT NOT NULL,
                    ""FK_BranchId"" INTEGER NULL,
                    ""DateFrom"" TEXT NULL,
                    ""DateTo"" TEXT NULL,
                    ""OpeningBalance"" REAL NOT NULL,
                    ""TotalSale"" REAL NOT NULL,
                    ""TotalSalesReturn"" REAL NOT NULL,
                    ""NetSales"" REAL NOT NULL,
                    ""TotalCashSales"" REAL NOT NULL,
                    ""TotalCardSales"" REAL NOT NULL,
                    ""TotalCreditSales"" REAL NOT NULL,
                    ""TotalPayment"" REAL NOT NULL,
                    ""TotalCashReceipt"" REAL NOT NULL,
                    ""TotalCardReceipt"" REAL NOT NULL,
                    ""ExpectedCash"" REAL NOT NULL,
                    ""ActualCash"" REAL NOT NULL,
                    ""Difference"" REAL NOT NULL,
                    ""NewOpeningBalance"" REAL NOT NULL,
                    ""Status"" TEXT NOT NULL,
                    ""Remarks"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""ShiftClosingRecords"";");
        }
    }
}
