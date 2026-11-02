using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AshproSAASERP.Oracle.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    UserName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Password = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsActive = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "LoginCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CompanyName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CompanyCode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    SubscriptionType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    SubscriptionStartedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    SubscriptionEndingDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    IsExpired = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    ContactNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Email = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CompanyAddress = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ConnectionString = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DBMainVersion = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DBSubVersion = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginCompanies", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AdminUsers",
                columns: new[] { "UserId", "IsActive", "Password", "Remarks", "UserName" },
                values: new object[] { 1, true, "ig79891", "Default admin user", "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminUsers");

            migrationBuilder.DropTable(
                name: "LoginCompanies");
        }
    }
}
