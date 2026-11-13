using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZakiSAASERP.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftClosingRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ItemNameArabic",
                table: "ItemMaster",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "ItemMaster",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "ItemMaster",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "ItemMaster",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ShiftClosingRecord') AND NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ShiftClosingRecords')
BEGIN
    EXEC sp_rename 'ShiftClosingRecord', 'ShiftClosingRecords';
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ShiftClosingRecords')
BEGIN
    CREATE TABLE [ShiftClosingRecords] (
        [ShiftClosingRecordId] int NOT NULL IDENTITY(1, 1),
        [ShiftCode] nvarchar(50) NOT NULL,
        [UserName] nvarchar(100) NOT NULL,
        [Terminal] nvarchar(100) NOT NULL,
        [FK_BranchId] int NULL,
        [DateFrom] datetime2 NULL,
        [DateTo] datetime2 NULL,
        [OpeningBalance] decimal(18,2) NOT NULL,
        [TotalSale] decimal(18,2) NOT NULL,
        [TotalSalesReturn] decimal(18,2) NOT NULL,
        [NetSales] decimal(18,2) NOT NULL,
        [TotalCashSales] decimal(18,2) NOT NULL,
        [TotalCardSales] decimal(18,2) NOT NULL,
        [TotalCreditSales] decimal(18,2) NOT NULL,
        [TotalPayment] decimal(18,2) NOT NULL,
        [TotalCashReceipt] decimal(18,2) NOT NULL,
        [TotalCardReceipt] decimal(18,2) NOT NULL,
        [ExpectedCash] decimal(18,2) NOT NULL,
        [ActualCash] decimal(18,2) NOT NULL,
        [Difference] decimal(18,2) NOT NULL,
        [NewOpeningBalance] decimal(18,2) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [Remarks] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ShiftClosingRecords] PRIMARY KEY ([ShiftClosingRecordId])
    );
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ShiftClosingRecords')
BEGIN
    DROP TABLE [ShiftClosingRecords];
END
");

            migrationBuilder.AlterColumn<string>(
                name: "ItemNameArabic",
                table: "ItemMaster",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "ItemMaster",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "ItemMaster",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "ItemMaster",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
