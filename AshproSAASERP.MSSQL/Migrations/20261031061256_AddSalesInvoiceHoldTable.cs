using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AshproSAASERP.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesInvoiceHoldTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesInvoiceHoldHdr",
                columns: table => new
                {
                    SalesInvoiceHoldHdrId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesInvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineDsicount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdditionalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Freight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AdditionalDiscPertge = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AdditionalDiscAmnt = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdditionalDiscountType = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FK_BranchId = table.Column<int>(type: "int", nullable: false),
                    FK_CustomerId = table.Column<int>(type: "int", nullable: false),
                    FK_SalesManId = table.Column<int>(type: "int", nullable: false),
                    FK_UserId = table.Column<int>(type: "int", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    FK_ProjectId = table.Column<int>(type: "int", nullable: true),
                    SalesInvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryNoteId = table.Column<int>(type: "int", nullable: true),
                    DeliveryNoteIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FK_StoreId = table.Column<int>(type: "int", nullable: false),
                    HijriDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceQRCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BillType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuotationId = table.Column<int>(type: "int", nullable: true),
                    SalesOrderId = table.Column<int>(type: "int", nullable: true),
                    ProformaInvoiceId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceHoldHdr", x => x.SalesInvoiceHoldHdrId);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHoldHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHoldHdr_CustomerMaster_FK_CustomerId",
                        column: x => x.FK_CustomerId,
                        principalTable: "CustomerMaster",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHoldHdr_FinancialYear_FK_FinancialYearId",
                        column: x => x.FK_FinancialYearId,
                        principalTable: "FinancialYear",
                        principalColumn: "FinancialYearId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHoldHdr_ProjectMaster_FK_ProjectId",
                        column: x => x.FK_ProjectId,
                        principalTable: "ProjectMaster",
                        principalColumn: "ProjectId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHoldHdr_SalesManMaster_FK_SalesManId",
                        column: x => x.FK_SalesManId,
                        principalTable: "SalesManMaster",
                        principalColumn: "SalesmanId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHoldHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceHoldDtls",
                columns: table => new
                {
                    SalesInvoiceHoldDtlsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_SalesInvoiceHoldHdrId = table.Column<int>(type: "int", nullable: false),
                    FK_ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FK_UnitId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPerc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmnt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxableAmnt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxPerc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FK_StoreId = table.Column<int>(type: "int", nullable: true),
                    FK_DeliveryNoteDtlsId = table.Column<int>(type: "int", nullable: true),
                    DeliveredQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceHoldDtls", x => x.SalesInvoiceHoldDtlsId);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHoldDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHoldDtls_SalesInvoiceHoldHdr_FK_SalesInvoiceHoldHdrId",
                        column: x => x.FK_SalesInvoiceHoldHdrId,
                        principalTable: "SalesInvoiceHoldHdr",
                        principalColumn: "SalesInvoiceHoldHdrId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHoldDtls_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHoldDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHoldDtls_FK_ItemId",
                table: "SalesInvoiceHoldDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHoldDtls_FK_SalesInvoiceHoldHdrId",
                table: "SalesInvoiceHoldDtls",
                column: "FK_SalesInvoiceHoldHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHoldDtls_FK_StoreId",
                table: "SalesInvoiceHoldDtls",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHoldDtls_FK_UnitId",
                table: "SalesInvoiceHoldDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHoldHdr_FK_BranchId",
                table: "SalesInvoiceHoldHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHoldHdr_FK_CustomerId",
                table: "SalesInvoiceHoldHdr",
                column: "FK_CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHoldHdr_FK_FinancialYearId",
                table: "SalesInvoiceHoldHdr",
                column: "FK_FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHoldHdr_FK_ProjectId",
                table: "SalesInvoiceHoldHdr",
                column: "FK_ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHoldHdr_FK_SalesManId",
                table: "SalesInvoiceHoldHdr",
                column: "FK_SalesManId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHoldHdr_FK_UserId",
                table: "SalesInvoiceHoldHdr",
                column: "FK_UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesInvoiceHoldDtls");

            migrationBuilder.DropTable(
                name: "SalesInvoiceHoldHdr");
        }
    }
}
