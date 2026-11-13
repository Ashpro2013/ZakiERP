using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZakiSAASERP.MySQL.Migrations.TenantDB
{
    /// <inheritdoc />
    public partial class AddVoucherNoAndShiftClosingRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ItemNameArabic",
                table: "ItemMaster",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "ItemMaster",
                type: "varchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "ItemMaster",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "ItemMaster",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "VoucherNo",
                table: "InvoiceICV",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `ShiftClosingRecords` (
                    `ShiftClosingRecordId` int NOT NULL AUTO_INCREMENT,
                    `ShiftCode` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
                    `UserName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                    `Terminal` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                    `FK_BranchId` int NULL,
                    `DateFrom` datetime(6) NULL,
                    `DateTo` datetime(6) NULL,
                    `OpeningBalance` decimal(18,2) NOT NULL,
                    `TotalSale` decimal(18,2) NOT NULL,
                    `TotalSalesReturn` decimal(18,2) NOT NULL,
                    `NetSales` decimal(18,2) NOT NULL,
                    `TotalCashSales` decimal(18,2) NOT NULL,
                    `TotalCardSales` decimal(18,2) NOT NULL,
                    `TotalCreditSales` decimal(18,2) NOT NULL,
                    `TotalPayment` decimal(18,2) NOT NULL,
                    `TotalCashReceipt` decimal(18,2) NOT NULL,
                    `TotalCardReceipt` decimal(18,2) NOT NULL,
                    `ExpectedCash` decimal(18,2) NOT NULL,
                    `ActualCash` decimal(18,2) NOT NULL,
                    `Difference` decimal(18,2) NOT NULL,
                    `NewOpeningBalance` decimal(18,2) NOT NULL,
                    `Status` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
                    `Remarks` varchar(500) CHARACTER SET utf8mb4 NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    CONSTRAINT `PK_ShiftClosingRecords` PRIMARY KEY (`ShiftClosingRecordId`)
                ) CHARACTER SET=utf8mb4;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS `ShiftClosingRecords`;");

            migrationBuilder.DropColumn(
                name: "VoucherNo",
                table: "InvoiceICV");

            migrationBuilder.AlterColumn<string>(
                name: "ItemNameArabic",
                table: "ItemMaster",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldMaxLength: 250,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "ItemMaster",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldMaxLength: 250)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "ItemMaster",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "ItemMaster",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
