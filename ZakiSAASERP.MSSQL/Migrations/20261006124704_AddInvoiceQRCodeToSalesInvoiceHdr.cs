using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZakiSAASERP.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceQRCodeToSalesInvoiceHdr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvoiceQRCode",
                table: "SalesInvoiceHdr",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceQRCode",
                table: "SalesInvoiceHdr");
        }
    }
}
