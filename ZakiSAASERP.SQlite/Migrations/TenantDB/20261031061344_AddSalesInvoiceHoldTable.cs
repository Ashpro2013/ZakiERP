using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZakiSAASERP.SQlite.Migrations.TenantDB
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
                    SalesInvoiceHoldHdrId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SalesInvoiceNo = table.Column<string>(type: "TEXT", nullable: false),
                    CustomerName = table.Column<string>(type: "TEXT", nullable: true),
                    RefNo = table.Column<string>(type: "TEXT", nullable: true),
                    GrossAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    LineDsicount = table.Column<decimal>(type: "TEXT", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    NetAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AdditionalAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    Freight = table.Column<decimal>(type: "TEXT", nullable: true),
                    AdditionalDiscPertge = table.Column<decimal>(type: "TEXT", nullable: true),
                    AdditionalDiscAmnt = table.Column<decimal>(type: "TEXT", nullable: true),
                    GrandTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    AdditionalDiscountType = table.Column<int>(type: "INTEGER", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    FK_BranchId = table.Column<int>(type: "INTEGER", nullable: false),
                    FK_CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    FK_SalesManId = table.Column<int>(type: "INTEGER", nullable: false),
                    FK_UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "INTEGER", nullable: false),
                    FK_ProjectId = table.Column<int>(type: "INTEGER", nullable: true),
                    SalesInvoiceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeliveryNoteId = table.Column<int>(type: "INTEGER", nullable: true),
                    DeliveryNoteIds = table.Column<string>(type: "TEXT", nullable: true),
                    FK_StoreId = table.Column<int>(type: "INTEGER", nullable: false),
                    HijriDate = table.Column<string>(type: "TEXT", nullable: true),
                    InvoiceQRCode = table.Column<string>(type: "TEXT", nullable: true),
                    CurrencyId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "TEXT", nullable: true),
                    BillType = table.Column<string>(type: "TEXT", nullable: true),
                    QuotationId = table.Column<int>(type: "INTEGER", nullable: true),
                    SalesOrderId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProformaInvoiceId = table.Column<int>(type: "INTEGER", nullable: true)
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
                    SalesInvoiceHoldDtlsId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FK_SalesInvoiceHoldHdrId = table.Column<int>(type: "INTEGER", nullable: false),
                    FK_ItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemName = table.Column<string>(type: "TEXT", nullable: false),
                    Barcode = table.Column<string>(type: "TEXT", nullable: true),
                    FK_UnitId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "TEXT", nullable: false),
                    ExRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    InRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    DiscountPerc = table.Column<decimal>(type: "TEXT", nullable: false),
                    DiscountAmnt = table.Column<decimal>(type: "TEXT", nullable: false),
                    TaxableAmnt = table.Column<decimal>(type: "TEXT", nullable: false),
                    TaxPerc = table.Column<decimal>(type: "TEXT", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    LineTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    FK_StoreId = table.Column<int>(type: "INTEGER", nullable: true),
                    FK_DeliveryNoteDtlsId = table.Column<int>(type: "INTEGER", nullable: true),
                    DeliveredQty = table.Column<decimal>(type: "TEXT", nullable: false)
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
