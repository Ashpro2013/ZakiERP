using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ZakiSAASERP.PSQL.Migrations.TenantDB
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
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "ItemMaster",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "ItemMaster",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "ItemMaster",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoucherNo",
                table: "InvoiceICV",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShiftClosingRecords",
                columns: table => new
                {
                    ShiftClosingRecordId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShiftCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Terminal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FK_BranchId = table.Column<int>(type: "integer", nullable: true),
                    DateFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalSale = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalSalesReturn = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetSales = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalCashSales = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalCardSales = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalCreditSales = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalCashReceipt = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalCardReceipt = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ExpectedCash = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ActualCash = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Difference = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NewOpeningBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftClosingRecords", x => x.ShiftClosingRecordId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShiftClosingRecords");

            migrationBuilder.DropColumn(
                name: "VoucherNo",
                table: "InvoiceICV");

            migrationBuilder.AlterColumn<string>(
                name: "ItemNameArabic",
                table: "ItemMaster",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "ItemMaster",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "ItemMaster",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "ItemMaster",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
