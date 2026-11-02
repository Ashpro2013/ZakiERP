using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZakiSAASERP.MySQL.Migrations.TenantDB
{
    /// <inheritdoc />
    public partial class UpdateTenantModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvoiceQRCode",
                table: "SalesInvoiceHdr",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AccountSubGroup_Code",
                table: "AccountSubGroup",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountMaster_Code",
                table: "AccountMaster",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccountSubGroup_Code",
                table: "AccountSubGroup");

            migrationBuilder.DropIndex(
                name: "IX_AccountMaster_Code",
                table: "AccountMaster");

            migrationBuilder.DropColumn(
                name: "InvoiceQRCode",
                table: "SalesInvoiceHdr");
        }
    }
}
