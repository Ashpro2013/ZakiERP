using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZakiSAASERP.SQlite.Migrations.TenantDB
{
    /// <inheritdoc />
    public partial class AddVoucherNoToInvoiceICV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VoucherNo",
                table: "InvoiceICV",
                type: "TEXT",
                nullable: true);


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoucherNo",
                table: "InvoiceICV");

        }
    }
}

