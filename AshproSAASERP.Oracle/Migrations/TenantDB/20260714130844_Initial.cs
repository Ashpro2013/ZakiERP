using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AshproSAASERP.Oracle.Migrations.TenantDB
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountMaster",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    NameArabic = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    FK_AccountHeadId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_AcGroupId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Opening = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DbCr = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    FK_AcTypeId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_AccountSubGroupId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Flag = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    IsReserved = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountMaster", x => x.AccountId);
                });

            migrationBuilder.CreateTable(
                name: "AccountOpeningEntryHdr",
                columns: table => new
                {
                    AccountOpeningEntryId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    VoucherNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    VoucherDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    DebitTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    CreditTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountOpeningEntryHdr", x => x.AccountOpeningEntryId);
                });

            migrationBuilder.CreateTable(
                name: "AccountType",
                columns: table => new
                {
                    AcTypeId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    AcTypeName = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountType", x => x.AcTypeId);
                });

            migrationBuilder.CreateTable(
                name: "BarcodeData",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ItemCode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ItemName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ItemNameArabic = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SalesRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    PurchaseRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    WholeSaleRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    MinSaleRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Location = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    StandardCost = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    StockMode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Notes = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Model = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Fitted = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ItemImageName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ItemExpiry = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ItemType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Barcode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    WeightPerCarton = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    LandingCost = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    InclusiveRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    BrandName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    UnitName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CategoryName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SubCategoryName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SupplierName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "BarcodeDesignDtls",
                columns: table => new
                {
                    BarcodeDesignDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_BarcodeDesignHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Text = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Position_x = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Position_y = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Width = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Height = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Alignment = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RotationPoint = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    fontFamily = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    fontSize = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    fStyle = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Format = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    bAlignment = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Type = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Prefix = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarcodeDesignDtls", x => x.BarcodeDesignDtlsId);
                });

            migrationBuilder.CreateTable(
                name: "BarcodeDesignHdr",
                columns: table => new
                {
                    BarcodeDesignHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DesignName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    FirstMargin = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TopMargin = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    RightMargin = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    BottomMargin = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Width = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Height = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Accross = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Down = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TotalPerPage = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PrinterName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PaperName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PSWidth = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PSHieght = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    IsBorderIncluded = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    IsBarcode = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    GenerateLabel = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    IsDefault = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarcodeDesignHdr", x => x.BarcodeDesignHdrId);
                });

            migrationBuilder.CreateTable(
                name: "BranchMaster",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    AccountId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ShowInLogin = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchMaster", x => x.BranchId);
                });

            migrationBuilder.CreateTable(
                name: "BrandMaster",
                columns: table => new
                {
                    BrandId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    NameArabic = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandMaster", x => x.BrandId);
                });

            migrationBuilder.CreateTable(
                name: "CategoryMaster",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: false),
                    NameArabic = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    ItemImage = table.Column<byte[]>(type: "RAW(2000)", nullable: true),
                    IsFixedAsset = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryMaster", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "ClientERPSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ERPTypeId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientERPSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CodeSequences",
                columns: table => new
                {
                    CodeKey = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    LastNumber = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeSequences", x => x.CodeKey);
                });

            migrationBuilder.CreateTable(
                name: "CountryMaster",
                columns: table => new
                {
                    CountryId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    NameArabic = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryMaster", x => x.CountryId);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyMaster",
                columns: table => new
                {
                    CurrencyId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyMaster", x => x.CurrencyId);
                });

            migrationBuilder.CreateTable(
                name: "CustomerGroupMaster",
                columns: table => new
                {
                    CustomerGroupId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    NameArabic = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerGroupMaster", x => x.CustomerGroupId);
                });

            migrationBuilder.CreateTable(
                name: "CustomerLocationDetails",
                columns: table => new
                {
                    CustomerLocationDetailsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_CustomerLocationId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerLocationDetails", x => x.CustomerLocationDetailsId);
                });

            migrationBuilder.CreateTable(
                name: "CustomerLocationMaster",
                columns: table => new
                {
                    LocationId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerLocationMaster", x => x.LocationId);
                });

            migrationBuilder.CreateTable(
                name: "ERPType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TypeName = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    IsCustom = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ERPType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ERPTypePreset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ERPTypeId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MenuDetailId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsVisible = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ERPTypePreset", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialYear",
                columns: table => new
                {
                    FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    RefNo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FromDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ToDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsClosed = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialYear", x => x.FinancialYearId);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceICV",
                columns: table => new
                {
                    InvoiceICVId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    InvICVNo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    VoucherType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    VoucherId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    InvICVDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    ICVStatus = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    DetailedStatus = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ICVMessage = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ICVInfoCode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ICVInfoMessages = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ICVInfoStatus = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ReUploaded = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    InvoiceHash = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    InvoicePreviousHash = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceICV", x => x.InvoiceICVId);
                });

            migrationBuilder.CreateTable(
                name: "MasterValue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    MasterType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldOne = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldTwo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldThree = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FieldFour = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    BranchId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterValue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MenuCaption = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ArabicCaption = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ParentId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    AssemblyName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ClassName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Active = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    Position = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsFavorite = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    IsFavorite2 = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FilterOneOrder = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FilterTwoOrder = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuVisibility",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MenuDetailId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsVisible = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuVisibility", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermanentSettings",
                columns: table => new
                {
                    SettingId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    SettingKey = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    SettingValue = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermanentSettings", x => x.SettingId);
                });

            migrationBuilder.CreateTable(
                name: "PrintDesignDtls",
                columns: table => new
                {
                    PrintDesignDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_PrintDesignHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ControlName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ControlType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    FieldText = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PositionX = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PositionY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FontSize = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Font = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsBold = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    Width = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Alignment = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Location = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PIndex = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Height = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Spec = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FStyle = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintDesignDtls", x => x.PrintDesignDtlsId);
                });

            migrationBuilder.CreateTable(
                name: "PrintDesignHdr",
                columns: table => new
                {
                    PrintDesignHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DesignName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    VoucherType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsDefault = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    PageHeight = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PageWidth = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LinesPerPage = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SpaceBtwnLines = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    BagroundImagePath = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PrinterName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PrintType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    FormType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintDesignHdr", x => x.PrintDesignHdrId);
                });

            migrationBuilder.CreateTable(
                name: "PurchaserMaster",
                columns: table => new
                {
                    PurchaserId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    NameArabic = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaserMaster", x => x.PurchaserId);
                });

            migrationBuilder.CreateTable(
                name: "RepairMaster",
                columns: table => new
                {
                    RepairId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairMaster", x => x.RepairId);
                });

            migrationBuilder.CreateTable(
                name: "SalesManMaster",
                columns: table => new
                {
                    SalesmanId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(15)", maxLength: 15, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    NameArabic = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    Designation = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    Mobile = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    SignatureImage = table.Column<byte[]>(type: "RAW(2000)", nullable: true),
                    SealImage = table.Column<byte[]>(type: "RAW(2000)", nullable: true),
                    SalesmanImage = table.Column<byte[]>(type: "RAW(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesManMaster", x => x.SalesmanId);
                });

            migrationBuilder.CreateTable(
                name: "SupplierOPEntryUpdated",
                columns: table => new
                {
                    SupplierOPEntryUpdatedId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_SupplierOPEntryId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_SupplierId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    DocNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PurchaseNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    PurchaseAmt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    ReceivedAmt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    DiscountAmt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    ReturnedAmt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    BalanceAmt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierOPEntryUpdated", x => x.SupplierOPEntryUpdatedId);
                });

            migrationBuilder.CreateTable(
                name: "SupplierPaymentDtls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_PaymentId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_PurchaseId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    InvoiceAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    PayingAmount = table.Column<decimal>(type: "decimal(24,4)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(24,4)", nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Narration = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierPaymentDtls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplierPaymentHdr",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    VoucherNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    FK_SupplierId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RVNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_CashBankId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PayMode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ChequeNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Bank = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_SalesmanId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Amount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    AdvanceAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    FK_ProjectId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Narration = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierPaymentHdr", x => x.PaymentId);
                });

            migrationBuilder.CreateTable(
                name: "TechnicianMaster",
                columns: table => new
                {
                    TechnicianId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Position = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Mobile = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicianMaster", x => x.TechnicianId);
                });

            migrationBuilder.CreateTable(
                name: "UnitMaster",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitMaster", x => x.UnitId);
                });

            migrationBuilder.CreateTable(
                name: "UserBranch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBranch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserDetails",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PrevilageId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    UserName = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    PassWord = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDetails", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "UserPrivilegeHdr",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPrivilegeHdr", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VATCategoryDetails",
                columns: table => new
                {
                    VATCategoryId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DescriptionEN = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DescriptionAR = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ExemptionCode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ExemptionTextEN = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ExemptionTextAR = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VATCategoryDetails", x => x.VATCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "VatMaster",
                columns: table => new
                {
                    VatId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Percentage = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    IsDefault = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VatMaster", x => x.VatId);
                });

            migrationBuilder.CreateTable(
                name: "VehicleMaster",
                columns: table => new
                {
                    VehicleId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PlateNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    ChassisNo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CustomerName = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    ItemImageName = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    ImagePath = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    StorageType = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleMaster", x => x.VehicleId);
                });

            migrationBuilder.CreateTable(
                name: "ZatcaInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    invoiceHash = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    uuid = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    EncodedInvoice = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    InvoiceType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    InvNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    InvoiceId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Code = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ZatcaCertificate = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ZatcaSecret = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    SMode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    InvoiceIcv = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZatcaInvoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountOpeningEntryDetails",
                columns: table => new
                {
                    AccountOpeningEntryDetailsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_AccountOpeningEntryId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_AccountId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Debit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Credit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Narration = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountOpeningEntryDetails", x => x.AccountOpeningEntryDetailsId);
                    table.ForeignKey(
                        name: "FK_AccountOpeningEntryDetails_AccountMaster_FK_AccountId",
                        column: x => x.FK_AccountId,
                        principalTable: "AccountMaster",
                        principalColumn: "AccountId");
                });

            migrationBuilder.CreateTable(
                name: "AccountHeadMaster",
                columns: table => new
                {
                    AccountHeadId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    NameArabic = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_AcTypeId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsReserved = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountHeadMaster", x => x.AccountHeadId);
                    table.ForeignKey(
                        name: "FK_AccountHeadMaster_AccountType_FK_AcTypeId",
                        column: x => x.FK_AcTypeId,
                        principalTable: "AccountType",
                        principalColumn: "AcTypeId");
                });

            migrationBuilder.CreateTable(
                name: "StoreMaster",
                columns: table => new
                {
                    StoreId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    IsDefaultStore = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreMaster", x => x.StoreId);
                    table.ForeignKey(
                        name: "FK_StoreMaster_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                });

            migrationBuilder.CreateTable(
                name: "SubCategoryMaster",
                columns: table => new
                {
                    SubCategoryId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    NameArabic = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    FK_CategoryId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    ItemImageName = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    ItemImage = table.Column<byte[]>(type: "RAW(2000)", nullable: true),
                    IsFixedAsset = table.Column<bool>(type: "BOOLEAN", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategoryMaster", x => x.SubCategoryId);
                    table.ForeignKey(
                        name: "FK_SubCategoryMaster_CategoryMaster_FK_CategoryId",
                        column: x => x.FK_CategoryId,
                        principalTable: "CategoryMaster",
                        principalColumn: "CategoryId");
                });

            migrationBuilder.CreateTable(
                name: "CompanyDetails",
                columns: table => new
                {
                    CompanyDetailId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: false),
                    NameArabic = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    Contact = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    ContactArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Telephone = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    TelephoneArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Fax = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    FaxArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Mobile = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    MobileArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    VatNo = table.Column<string>(type: "NVARCHAR2(15)", maxLength: 15, nullable: true),
                    VatNoArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    BuildingNo = table.Column<string>(type: "NVARCHAR2(4)", maxLength: 4, nullable: true),
                    BuildingNoArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    StreetName = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    StreetNameArabic = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    District = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    DistrictArabic = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    CityArabic = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    PostalCode = table.Column<string>(type: "NVARCHAR2(5)", maxLength: 5, nullable: true),
                    PostalCodeArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    AdditionalNo = table.Column<string>(type: "NVARCHAR2(4)", maxLength: 4, nullable: true),
                    AdditionalNoArabic = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    CountrySubentity = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    CountrySubentityArabic = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    FK_CountryId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    OtherSellerId = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: true),
                    OtherSellerIdArabic = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    IdentificationType = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    IdentificationNo = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    IdentificationNoArabic = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    GroupVat = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    HeaderImage = table.Column<byte[]>(type: "RAW(2000)", nullable: true),
                    FooterImage = table.Column<byte[]>(type: "RAW(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyDetails", x => x.CompanyDetailId);
                    table.ForeignKey(
                        name: "FK_CompanyDetails_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_CompanyDetails_CountryMaster_FK_CountryId",
                        column: x => x.FK_CountryId,
                        principalTable: "CountryMaster",
                        principalColumn: "CountryId");
                });

            migrationBuilder.CreateTable(
                name: "FunctionDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_MenuDetailsId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EventName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EventCaption = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FunctionDetails_MenuDetails_FK_MenuDetailsId",
                        column: x => x.FK_MenuDetailsId,
                        principalTable: "MenuDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplierMaster",
                columns: table => new
                {
                    SupplierId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    NameArabic = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    Address = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    AddressArabic = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    Contact = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    ContactArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Telephone = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    Mobile = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    PhoneArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    MobileArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    VatNo = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: true),
                    VatNoArabic = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    NeedsFill = table.Column<bool>(type: "BOOLEAN", nullable: true),
                    SupStatus = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    CreditDays = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    BuildingNo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    StreetName = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    District = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    PostalCode = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    AdditionalNo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    OtherSellerNo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    BuildingNoArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    StreetNameArabic = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    DistrictArabic = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    CityArabic = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    PostalCodeArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    AdditionalNoArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    OtherSellerNoArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    FK_CountryId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_PurchaserId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_AccountId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    IdentificationNo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Identification = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    IdentificationNoArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    GroupVat = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: true),
                    Neighbourhood = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    NeighbourhoodArabic = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    SuppBillType = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: true),
                    SuppType = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    IsCustomer = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    IsActive = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierMaster", x => x.SupplierId);
                    table.ForeignKey(
                        name: "FK_SupplierMaster_AccountMaster_FK_AccountId",
                        column: x => x.FK_AccountId,
                        principalTable: "AccountMaster",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_SupplierMaster_CountryMaster_FK_CountryId",
                        column: x => x.FK_CountryId,
                        principalTable: "CountryMaster",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_SupplierMaster_PurchaserMaster_FK_PurchaserId",
                        column: x => x.FK_PurchaserId,
                        principalTable: "PurchaserMaster",
                        principalColumn: "PurchaserId");
                });

            migrationBuilder.CreateTable(
                name: "CustomerMaster",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: false),
                    NameArabic = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    Address = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    AddressArabic = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    CreditDays = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Contact = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    ContactArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Telephone = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    PhoneArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Mobile = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    MobileArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    VatNo = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    VatNoArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    BuildingNo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    BuildingNoArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    StreetName = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    StreetNameArabic = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    District = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    DistrictArabic = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    CityArabic = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    PostalCode = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    PostalCodeArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    AdditionalNo = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    AdditionalNoArabic = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    OtherSellerNo = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    OtherSellerNoArabic = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    CountrySubentity = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    CountrySubentityArabic = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    IdentificationNo = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    IdentificationNoArabic = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    Identification = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    FK_CustomerGroupId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_CountryId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_SalesmanId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_AccountId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    CusBillType = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    NeedsFill = table.Column<bool>(type: "BOOLEAN", nullable: true),
                    IsSupplier = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    IsActive = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Neighbourhood = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    NeighbourhoodArabic = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    GroupVat = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerMaster", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_CustomerMaster_AccountMaster_FK_AccountId",
                        column: x => x.FK_AccountId,
                        principalTable: "AccountMaster",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_CustomerMaster_CountryMaster_FK_CountryId",
                        column: x => x.FK_CountryId,
                        principalTable: "CountryMaster",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_CustomerMaster_CustomerGroupMaster_FK_CustomerGroupId",
                        column: x => x.FK_CustomerGroupId,
                        principalTable: "CustomerGroupMaster",
                        principalColumn: "CustomerGroupId");
                    table.ForeignKey(
                        name: "FK_CustomerMaster_SalesManMaster_FK_SalesmanId",
                        column: x => x.FK_SalesmanId,
                        principalTable: "SalesManMaster",
                        principalColumn: "SalesmanId");
                });

            migrationBuilder.CreateTable(
                name: "CustomerOPEntry",
                columns: table => new
                {
                    CustomerOPEntryId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DocNo = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    EnteredBy = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerOPEntry", x => x.CustomerOPEntryId);
                    table.ForeignKey(
                        name: "FK_CustomerOPEntry_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_CustomerOPEntry_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryHdr",
                columns: table => new
                {
                    JournalId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    JournalNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    JournalDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Narration = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    ReferenceNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    FK_ProjectId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_SalesmanId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TotalDebit = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalCredit = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryHdr", x => x.JournalId);
                    table.ForeignKey(
                        name: "FK_JournalEntryHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_JournalEntryHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "MaterialTransferInHdr",
                columns: table => new
                {
                    MatTransInId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    VoucherNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ReferenceNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_FromBranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ToBranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EnteredBy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialTransferInHdr", x => x.MatTransInId);
                    table.ForeignKey(
                        name: "FK_MaterialTransferInHdr_BranchMaster_FK_FromBranchId",
                        column: x => x.FK_FromBranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferInHdr_BranchMaster_FK_ToBranchId",
                        column: x => x.FK_ToBranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferInHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "MaterialTransferOutHdr",
                columns: table => new
                {
                    MatTransOutId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    VoucherNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ReferenceNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_FromBranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ToBranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EnteredBy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialTransferOutHdr", x => x.MatTransOutId);
                    table.ForeignKey(
                        name: "FK_MaterialTransferOutHdr_BranchMaster_FK_FromBranchId",
                        column: x => x.FK_FromBranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferOutHdr_BranchMaster_FK_ToBranchId",
                        column: x => x.FK_ToBranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferOutHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "MaterialTransferRequestHdr",
                columns: table => new
                {
                    MatTransReqId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    VoucherNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ReferenceNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_FromBranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ToBranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EnteredBy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_ApprovedBy = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    FK_RejectedBy = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    RejectionReason = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialTransferRequestHdr", x => x.MatTransReqId);
                    table.ForeignKey(
                        name: "FK_MaterialTransferRequestHdr_BranchMaster_FK_FromBranchId",
                        column: x => x.FK_FromBranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferRequestHdr_BranchMaster_FK_ToBranchId",
                        column: x => x.FK_ToBranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferRequestHdr_UserDetails_FK_ApprovedBy",
                        column: x => x.FK_ApprovedBy,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferRequestHdr_UserDetails_FK_RejectedBy",
                        column: x => x.FK_RejectedBy,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferRequestHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "OpeningStockEntry",
                columns: table => new
                {
                    OpeningStockEntryId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DocumentNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    EnteredBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpeningStockEntry", x => x.OpeningStockEntryId);
                    table.ForeignKey(
                        name: "FK_OpeningStockEntry_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_OpeningStockEntry_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustmentHdr",
                columns: table => new
                {
                    StockAdjId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    VoucherNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ReferenceNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EnteredBy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustmentHdr", x => x.StockAdjId);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_StockAdjustmentHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "SupplierOPEntry",
                columns: table => new
                {
                    SupplierOPEntryId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DocNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    EnteredBy = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierOPEntry", x => x.SupplierOPEntryId);
                    table.ForeignKey(
                        name: "FK_SupplierOPEntry_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_SupplierOPEntry_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TblAccounting",
                columns: table => new
                {
                    TblAccountId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    VoucherId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    VoucherNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    FK_AccountId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    Narration = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Type = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    BillType = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_SupplierId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblAccounting", x => x.TblAccountId);
                    table.ForeignKey(
                        name: "FK_TblAccounting_AccountMaster_FK_AccountId",
                        column: x => x.FK_AccountId,
                        principalTable: "AccountMaster",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_TblAccounting_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_TblAccounting_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserPrivilegeDtls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_UserPrivilegeHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_MenuDetailsId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Visible = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    SaveData = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    EditData = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    DeleteData = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPrivilegeDtls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPrivilegeDtls_MenuDetails_FK_MenuDetailsId",
                        column: x => x.FK_MenuDetailsId,
                        principalTable: "MenuDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPrivilegeDtls_UserPrivilegeHdr_FK_UserPrivilegeHdrId",
                        column: x => x.FK_UserPrivilegeHdrId,
                        principalTable: "UserPrivilegeHdr",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AccountGroup",
                columns: table => new
                {
                    AccountGroupId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    NameArabic = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    FK_AcHeadId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsReserved = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountGroup", x => x.AccountGroupId);
                    table.ForeignKey(
                        name: "FK_AccountGroup_AccountHeadMaster_FK_AcHeadId",
                        column: x => x.FK_AcHeadId,
                        principalTable: "AccountHeadMaster",
                        principalColumn: "AccountHeadId");
                });

            migrationBuilder.CreateTable(
                name: "InternalMaterialTransferHdr",
                columns: table => new
                {
                    IntMatTransId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    VoucherNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ReferenceNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FromStoreId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ToStoreId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EnteredBy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalMaterialTransferHdr", x => x.IntMatTransId);
                    table.ForeignKey(
                        name: "FK_InternalMaterialTransferHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_InternalMaterialTransferHdr_StoreMaster_FK_FromStoreId",
                        column: x => x.FK_FromStoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_InternalMaterialTransferHdr_StoreMaster_FK_ToStoreId",
                        column: x => x.FK_ToStoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_InternalMaterialTransferHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "FunctionalPrivilege",
                columns: table => new
                {
                    id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_UserPrivilegeHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FunctionDetailsId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Enable = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionalPrivilege", x => x.id);
                    table.ForeignKey(
                        name: "FK_FunctionalPrivilege_FunctionDetails_FK_FunctionDetailsId",
                        column: x => x.FK_FunctionDetailsId,
                        principalTable: "FunctionDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FunctionalPrivilege_UserPrivilegeHdr_FK_UserPrivilegeHdrId",
                        column: x => x.FK_UserPrivilegeHdrId,
                        principalTable: "UserPrivilegeHdr",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceiptHdr",
                columns: table => new
                {
                    GoodsReceiptId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    GoodsReceiptNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    RefNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    OurPORefNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineDsicount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    AdditionalAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Freight = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscPertge = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_SupplierId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_PurchaserId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_CurrencyId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    GoodsReceiptDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceiptHdr", x => x.GoodsReceiptId);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_GoodsReceiptHdr_FinancialYear_FK_FinancialYearId",
                        column: x => x.FK_FinancialYearId,
                        principalTable: "FinancialYear",
                        principalColumn: "FinancialYearId");
                    table.ForeignKey(
                        name: "FK_GoodsReceiptHdr_PurchaserMaster_FK_PurchaserId",
                        column: x => x.FK_PurchaserId,
                        principalTable: "PurchaserMaster",
                        principalColumn: "PurchaserId");
                    table.ForeignKey(
                        name: "FK_GoodsReceiptHdr_SupplierMaster_FK_SupplierId",
                        column: x => x.FK_SupplierId,
                        principalTable: "SupplierMaster",
                        principalColumn: "SupplierId");
                    table.ForeignKey(
                        name: "FK_GoodsReceiptHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ItemMaster",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ItemCode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ItemName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ItemNameArabic = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SalesRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    PurchaseRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    WholeSaleRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    MinSaleRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    FK_BrandId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_CategoryId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Location = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    StandardCost = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    StockMode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    VatCategory = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    VatExcCode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    VatExcName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Notes = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Model = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Fitted = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ItemImageName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ItemImage = table.Column<byte[]>(type: "RAW(2000)", nullable: true),
                    ItemExpiry = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ItemType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Barcode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    WeightPerCarton = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    LandingCost = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    IsActive = table.Column<bool>(type: "BOOLEAN", nullable: true),
                    InclusiveRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    FK_SubCategoryId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_SupplierId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_VatId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    IsSaleable = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemMaster", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_ItemMaster_BrandMaster_FK_BrandId",
                        column: x => x.FK_BrandId,
                        principalTable: "BrandMaster",
                        principalColumn: "BrandId");
                    table.ForeignKey(
                        name: "FK_ItemMaster_CategoryMaster_FK_CategoryId",
                        column: x => x.FK_CategoryId,
                        principalTable: "CategoryMaster",
                        principalColumn: "CategoryId");
                    table.ForeignKey(
                        name: "FK_ItemMaster_SubCategoryMaster_FK_SubCategoryId",
                        column: x => x.FK_SubCategoryId,
                        principalTable: "SubCategoryMaster",
                        principalColumn: "SubCategoryId");
                    table.ForeignKey(
                        name: "FK_ItemMaster_SupplierMaster_FK_SupplierId",
                        column: x => x.FK_SupplierId,
                        principalTable: "SupplierMaster",
                        principalColumn: "SupplierId");
                    table.ForeignKey(
                        name: "FK_ItemMaster_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                    table.ForeignKey(
                        name: "FK_ItemMaster_VatMaster_FK_VatId",
                        column: x => x.FK_VatId,
                        principalTable: "VatMaster",
                        principalColumn: "VatId");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderHdr",
                columns: table => new
                {
                    PurchaseOrderId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PurchaseOrderNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    RefNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineDsicount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DeliveryTerms = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PaymentTerms = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DeliveryTo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_SupplierId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_PurchaserId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PurchaseOrderDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderHdr", x => x.PurchaseOrderId);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderHdr_FinancialYear_FK_FinancialYearId",
                        column: x => x.FK_FinancialYearId,
                        principalTable: "FinancialYear",
                        principalColumn: "FinancialYearId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderHdr_PurchaserMaster_FK_PurchaserId",
                        column: x => x.FK_PurchaserId,
                        principalTable: "PurchaserMaster",
                        principalColumn: "PurchaserId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderHdr_SupplierMaster_FK_SupplierId",
                        column: x => x.FK_SupplierId,
                        principalTable: "SupplierMaster",
                        principalColumn: "SupplierId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "SupplierOPEntryDetails",
                columns: table => new
                {
                    SupplierOPEntryDetailsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_SupplierOPEntryId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_SupplierId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    DocNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PurchaseNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    PurchaseAmt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    ReceivedAmt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    DiscountAmt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    ReturnedAmt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    BalanceAmt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierOPEntryDetails", x => x.SupplierOPEntryDetailsId);
                    table.ForeignKey(
                        name: "FK_SupplierOPEntryDetails_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_SupplierOPEntryDetails_SupplierMaster_FK_SupplierId",
                        column: x => x.FK_SupplierId,
                        principalTable: "SupplierMaster",
                        principalColumn: "SupplierId");
                });

            migrationBuilder.CreateTable(
                name: "JobOrderHdr",
                columns: table => new
                {
                    JobOrderId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    VoucherNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PromisedDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    FK_VehicleId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Kilometer = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ContactNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    DeliveredBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    TimeIn = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    ReferenceNo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    FK_SalesManId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    VehicleInfo = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    Comments = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IstimaraPath = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalVat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EnteredBy = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOrderHdr", x => x.JobOrderId);
                    table.ForeignKey(
                        name: "FK_JobOrderHdr_CustomerMaster_FK_CustomerId",
                        column: x => x.FK_CustomerId,
                        principalTable: "CustomerMaster",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_JobOrderHdr_SalesManMaster_FK_SalesManId",
                        column: x => x.FK_SalesManId,
                        principalTable: "SalesManMaster",
                        principalColumn: "SalesmanId");
                    table.ForeignKey(
                        name: "FK_JobOrderHdr_VehicleMaster_FK_VehicleId",
                        column: x => x.FK_VehicleId,
                        principalTable: "VehicleMaster",
                        principalColumn: "VehicleId");
                });

            migrationBuilder.CreateTable(
                name: "ProjectMaster",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_SalesmanId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_PurchaserId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Amount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    ContractValue = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Description = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    ExpectedStrtDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    ExpectedEndDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    ActualStrtDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Status = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMaster", x => x.ProjectId);
                    table.ForeignKey(
                        name: "FK_ProjectMaster_CustomerMaster_FK_CustomerId",
                        column: x => x.FK_CustomerId,
                        principalTable: "CustomerMaster",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_ProjectMaster_PurchaserMaster_FK_PurchaserId",
                        column: x => x.FK_PurchaserId,
                        principalTable: "PurchaserMaster",
                        principalColumn: "PurchaserId");
                    table.ForeignKey(
                        name: "FK_ProjectMaster_SalesManMaster_FK_SalesmanId",
                        column: x => x.FK_SalesmanId,
                        principalTable: "SalesManMaster",
                        principalColumn: "SalesmanId");
                });

            migrationBuilder.CreateTable(
                name: "RFQHdr",
                columns: table => new
                {
                    RFQId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    RFQNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    RefNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CustomerRefNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CustomerRFQDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_SalesManId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFQHdr", x => x.RFQId);
                    table.ForeignKey(
                        name: "FK_RFQHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_RFQHdr_CustomerMaster_FK_CustomerId",
                        column: x => x.FK_CustomerId,
                        principalTable: "CustomerMaster",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_RFQHdr_FinancialYear_FK_FinancialYearId",
                        column: x => x.FK_FinancialYearId,
                        principalTable: "FinancialYear",
                        principalColumn: "FinancialYearId");
                    table.ForeignKey(
                        name: "FK_RFQHdr_SalesManMaster_FK_SalesManId",
                        column: x => x.FK_SalesManId,
                        principalTable: "SalesManMaster",
                        principalColumn: "SalesmanId");
                    table.ForeignKey(
                        name: "FK_RFQHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "CustomerOPEntryDetails",
                columns: table => new
                {
                    CustomerOPEntryDetailsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_CustomerOPEntryId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    DocNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    InvoiceNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    InvoiceAmt = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ReceivedAmt = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    DiscountAmt = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ReturnedAmt = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    BalanceAmt = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PONo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerOPEntryDetails", x => x.CustomerOPEntryDetailsId);
                    table.ForeignKey(
                        name: "FK_CustomerOPEntryDetails_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_CustomerOPEntryDetails_CustomerMaster_FK_CustomerId",
                        column: x => x.FK_CustomerId,
                        principalTable: "CustomerMaster",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_CustomerOPEntryDetails_CustomerOPEntry_FK_CustomerOPEntryId",
                        column: x => x.FK_CustomerOPEntryId,
                        principalTable: "CustomerOPEntry",
                        principalColumn: "CustomerOPEntryId");
                });

            migrationBuilder.CreateTable(
                name: "CustomerOPEntryUpdated",
                columns: table => new
                {
                    CustomerOPEntryUpdatedId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_CustomerOPEntryId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    DocNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    InvoiceNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    InvoiceAmt = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ReceivedAmt = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    DiscountAmt = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ReturnedAmt = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    BalanceAmt = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PONo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerOPEntryUpdated", x => x.CustomerOPEntryUpdatedId);
                    table.ForeignKey(
                        name: "FK_CustomerOPEntryUpdated_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_CustomerOPEntryUpdated_CustomerMaster_FK_CustomerId",
                        column: x => x.FK_CustomerId,
                        principalTable: "CustomerMaster",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_CustomerOPEntryUpdated_CustomerOPEntry_FK_CustomerOPEntryId",
                        column: x => x.FK_CustomerOPEntryId,
                        principalTable: "CustomerOPEntry",
                        principalColumn: "CustomerOPEntryId");
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryDtls",
                columns: table => new
                {
                    JournalDtlId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_JournalId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_AccountId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DebitAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    CreditAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Narration = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    LineNo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    InvoiceNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    InvoiceAmt = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    DebitVatper = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    CreditVatper = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    DebitVat = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    CreditVat = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    VatNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    VatPer = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalDebit = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalCredit = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    VatId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    AccType = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryDtls", x => x.JournalDtlId);
                    table.ForeignKey(
                        name: "FK_JournalEntryDtls_AccountMaster_FK_AccountId",
                        column: x => x.FK_AccountId,
                        principalTable: "AccountMaster",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_JournalEntryDtls_JournalEntryHdr_FK_JournalId",
                        column: x => x.FK_JournalId,
                        principalTable: "JournalEntryHdr",
                        principalColumn: "JournalId");
                });

            migrationBuilder.CreateTable(
                name: "AccountSubGroup",
                columns: table => new
                {
                    SubGroupId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_AcGroupId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Code = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    NameArabic = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: true),
                    FK_AcHeadId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsReserved = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSubGroup", x => x.SubGroupId);
                    table.ForeignKey(
                        name: "FK_AccountSubGroup_AccountGroup_FK_AcGroupId",
                        column: x => x.FK_AcGroupId,
                        principalTable: "AccountGroup",
                        principalColumn: "AccountGroupId");
                    table.ForeignKey(
                        name: "FK_AccountSubGroup_AccountHeadMaster_FK_AcHeadId",
                        column: x => x.FK_AcHeadId,
                        principalTable: "AccountHeadMaster",
                        principalColumn: "AccountHeadId");
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceiptDtls",
                columns: table => new
                {
                    GoodsReceiptDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_GoodsReceiptHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ItemName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ItemArabicName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Barcode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    ExRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    InRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceiptDtls", x => x.GoodsReceiptDtlsId);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptDtls_GoodsReceiptHdr_FK_GoodsReceiptHdrId",
                        column: x => x.FK_GoodsReceiptHdrId,
                        principalTable: "GoodsReceiptHdr",
                        principalColumn: "GoodsReceiptId");
                    table.ForeignKey(
                        name: "FK_GoodsReceiptDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_GoodsReceiptDtls_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_GoodsReceiptDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "InternalMaterialTransferDtls",
                columns: table => new
                {
                    IntMatTransDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_IntMatTransId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    QtyPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalMaterialTransferDtls", x => x.IntMatTransDtlsId);
                    table.ForeignKey(
                        name: "FK_InternalMaterialTransferDtls_InternalMaterialTransferHdr_FK_IntMatTransId",
                        column: x => x.FK_IntMatTransId,
                        principalTable: "InternalMaterialTransferHdr",
                        principalColumn: "IntMatTransId");
                    table.ForeignKey(
                        name: "FK_InternalMaterialTransferDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_InternalMaterialTransferDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "ItemMultiUnit",
                columns: table => new
                {
                    MultiUnitId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    QTYPerUnit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Rate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    IncRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Barcode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Cost = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    MultiUnitName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    MultiUnitArabicName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemMultiUnit", x => x.MultiUnitId);
                    table.ForeignKey(
                        name: "FK_ItemMultiUnit_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_ItemMultiUnit_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "ItemStockBalance",
                columns: table => new
                {
                    ItemStockBalanceId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CurrentQty = table.Column<decimal>(type: "decimal(24,4)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemStockBalance", x => x.ItemStockBalanceId);
                    table.ForeignKey(
                        name: "FK_ItemStockBalance_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_ItemStockBalance_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateTable(
                name: "MaterialTransferInDtls",
                columns: table => new
                {
                    MatTransInDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_MatTransInId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FromStoreId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ToStoreId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Rate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Amount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    QtyPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialTransferInDtls", x => x.MatTransInDtlsId);
                    table.ForeignKey(
                        name: "FK_MaterialTransferInDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferInDtls_MaterialTransferInHdr_FK_MatTransInId",
                        column: x => x.FK_MatTransInId,
                        principalTable: "MaterialTransferInHdr",
                        principalColumn: "MatTransInId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferInDtls_StoreMaster_FK_FromStoreId",
                        column: x => x.FK_FromStoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferInDtls_StoreMaster_FK_ToStoreId",
                        column: x => x.FK_ToStoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferInDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "MaterialTransferOutDtls",
                columns: table => new
                {
                    MatTransOutDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_MatTransOutId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FromStoreId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ToStoreId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialTransferOutDtls", x => x.MatTransOutDtlsId);
                    table.ForeignKey(
                        name: "FK_MaterialTransferOutDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferOutDtls_MaterialTransferOutHdr_FK_MatTransOutId",
                        column: x => x.FK_MatTransOutId,
                        principalTable: "MaterialTransferOutHdr",
                        principalColumn: "MatTransOutId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferOutDtls_StoreMaster_FK_FromStoreId",
                        column: x => x.FK_FromStoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferOutDtls_StoreMaster_FK_ToStoreId",
                        column: x => x.FK_ToStoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferOutDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "MaterialTransferRequestDtls",
                columns: table => new
                {
                    MatTransReqDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_MatTransReqId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FromStoreId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ToStoreId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    ApprovedQuantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Amount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ApproverComment = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    QtyPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialTransferRequestDtls", x => x.MatTransReqDtlsId);
                    table.ForeignKey(
                        name: "FK_MaterialTransferRequestDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferRequestDtls_MaterialTransferRequestHdr_FK_MatTransReqId",
                        column: x => x.FK_MatTransReqId,
                        principalTable: "MaterialTransferRequestHdr",
                        principalColumn: "MatTransReqId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferRequestDtls_StoreMaster_FK_FromStoreId",
                        column: x => x.FK_FromStoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferRequestDtls_StoreMaster_FK_ToStoreId",
                        column: x => x.FK_ToStoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_MaterialTransferRequestDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "OpeningStockEntryItem",
                columns: table => new
                {
                    OpeningStockEntryItemId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DocumentNo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    FK_OpeningStockEntryId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SlNo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    LocName = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    VatPer = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    VatAmt = table.Column<decimal>(type: "decimal(18,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpeningStockEntryItem", x => x.OpeningStockEntryItemId);
                    table.ForeignKey(
                        name: "FK_OpeningStockEntryItem_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_OpeningStockEntryItem_OpeningStockEntry_FK_OpeningStockEntryId",
                        column: x => x.FK_OpeningStockEntryId,
                        principalTable: "OpeningStockEntry",
                        principalColumn: "OpeningStockEntryId");
                    table.ForeignKey(
                        name: "FK_OpeningStockEntryItem_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_OpeningStockEntryItem_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "OpeningStockEntryItemForUpdate",
                columns: table => new
                {
                    OpeningStockEntryItemForUpdateId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_OpeningStockEntryId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SlNo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    VatPer = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    VatAmt = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    LocName = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpeningStockEntryItemForUpdate", x => x.OpeningStockEntryItemForUpdateId);
                    table.ForeignKey(
                        name: "FK_OpeningStockEntryItemForUpdate_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_OpeningStockEntryItemForUpdate_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_OpeningStockEntryItemForUpdate_OpeningStockEntry_FK_OpeningStockEntryId",
                        column: x => x.FK_OpeningStockEntryId,
                        principalTable: "OpeningStockEntry",
                        principalColumn: "OpeningStockEntryId");
                    table.ForeignKey(
                        name: "FK_OpeningStockEntryItemForUpdate_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_OpeningStockEntryItemForUpdate_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustmentDtls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_StockAdjId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Rate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Amount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    QtyPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentQty = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    NewQty = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustmentDtls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDtls_StockAdjustmentHdr_FK_StockAdjId",
                        column: x => x.FK_StockAdjId,
                        principalTable: "StockAdjustmentHdr",
                        principalColumn: "StockAdjId");
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "StockTranLog",
                columns: table => new
                {
                    STranId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    VchrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    VchrNo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    VchrDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    VchrType = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    VchrFormType = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    AccountId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    InQuantity = table.Column<decimal>(type: "decimal(24,4)", nullable: true),
                    OutQuantity = table.Column<decimal>(type: "decimal(24,4)", nullable: true),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Narration = table.Column<string>(type: "NVARCHAR2(400)", maxLength: 400, nullable: true),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TranDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    InRate = table.Column<decimal>(type: "decimal(24,4)", nullable: true),
                    OutRate = table.Column<decimal>(type: "decimal(24,4)", nullable: true),
                    FK_ActualUnitId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ActualUnitQty = table.Column<decimal>(type: "decimal(24,4)", nullable: true),
                    ActualRate = table.Column<decimal>(type: "decimal(24,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTranLog", x => x.STranId);
                    table.ForeignKey(
                        name: "FK_StockTranLog_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_StockTranLog_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_StockTranLog_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_StockTranLog_UnitMaster_FK_ActualUnitId",
                        column: x => x.FK_ActualUnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                    table.ForeignKey(
                        name: "FK_StockTranLog_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                    table.ForeignKey(
                        name: "FK_StockTranLog_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderDtls",
                columns: table => new
                {
                    PurchaseOrderDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_PurchaseOrderHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ItemName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Barcode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    ExRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    InRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderDtls", x => x.PurchaseOrderDtlsId);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDtls_PurchaseOrderHdr_FK_PurchaseOrderHdrId",
                        column: x => x.FK_PurchaseOrderHdrId,
                        principalTable: "PurchaseOrderHdr",
                        principalColumn: "PurchaseOrderId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDtls_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "JobCardHdr",
                columns: table => new
                {
                    JobCardId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    VoucherNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PromisedDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    FK_JobOrderId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    JobOrderNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_VehicleId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PlateNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    ChassisNo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Year = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Kilometer = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    VehicleInfo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CustomerName = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    ContactNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    DeliveredBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Comments = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    Reference = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    FK_SalesManId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TimeIn = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    QuotationNo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalVat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EnteredBy = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobCardHdr", x => x.JobCardId);
                    table.ForeignKey(
                        name: "FK_JobCardHdr_JobOrderHdr_FK_JobOrderId",
                        column: x => x.FK_JobOrderId,
                        principalTable: "JobOrderHdr",
                        principalColumn: "JobOrderId");
                    table.ForeignKey(
                        name: "FK_JobCardHdr_SalesManMaster_FK_SalesManId",
                        column: x => x.FK_SalesManId,
                        principalTable: "SalesManMaster",
                        principalColumn: "SalesmanId");
                });

            migrationBuilder.CreateTable(
                name: "JobOrderDtls",
                columns: table => new
                {
                    JobOrderDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_JobOrderId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_RepairId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_TechnicianId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VatPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOrderDtls", x => x.JobOrderDtlsId);
                    table.ForeignKey(
                        name: "FK_JobOrderDtls_JobOrderHdr_FK_JobOrderId",
                        column: x => x.FK_JobOrderId,
                        principalTable: "JobOrderHdr",
                        principalColumn: "JobOrderId");
                    table.ForeignKey(
                        name: "FK_JobOrderDtls_RepairMaster_FK_RepairId",
                        column: x => x.FK_RepairId,
                        principalTable: "RepairMaster",
                        principalColumn: "RepairId");
                    table.ForeignKey(
                        name: "FK_JobOrderDtls_TechnicianMaster_FK_TechnicianId",
                        column: x => x.FK_TechnicianId,
                        principalTable: "TechnicianMaster",
                        principalColumn: "TechnicianId");
                });

            migrationBuilder.CreateTable(
                name: "CustomerReceiptHdr",
                columns: table => new
                {
                    CustomerReceiptId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ReceiptNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    ReceiptDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_ProjectId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_SalesManId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PaymentMode = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    BankName = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    ChequeNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    BankAccountNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    RvNumber = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    AdvanceAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    FK_AccountId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerReceiptHdr", x => x.CustomerReceiptId);
                    table.ForeignKey(
                        name: "FK_CustomerReceiptHdr_AccountMaster_FK_AccountId",
                        column: x => x.FK_AccountId,
                        principalTable: "AccountMaster",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_CustomerReceiptHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_CustomerReceiptHdr_CustomerMaster_FK_CustomerId",
                        column: x => x.FK_CustomerId,
                        principalTable: "CustomerMaster",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_CustomerReceiptHdr_ProjectMaster_FK_ProjectId",
                        column: x => x.FK_ProjectId,
                        principalTable: "ProjectMaster",
                        principalColumn: "ProjectId");
                    table.ForeignKey(
                        name: "FK_CustomerReceiptHdr_SalesManMaster_FK_SalesManId",
                        column: x => x.FK_SalesManId,
                        principalTable: "SalesManMaster",
                        principalColumn: "SalesmanId");
                    table.ForeignKey(
                        name: "FK_CustomerReceiptHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "DamageStockHdr",
                columns: table => new
                {
                    DamageStockId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DamageStockNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_ProjectId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_AccountId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TotalValue = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamageStockHdr", x => x.DamageStockId);
                    table.ForeignKey(
                        name: "FK_DamageStockHdr_AccountMaster_FK_AccountId",
                        column: x => x.FK_AccountId,
                        principalTable: "AccountMaster",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_DamageStockHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_DamageStockHdr_FinancialYear_FK_FinancialYearId",
                        column: x => x.FK_FinancialYearId,
                        principalTable: "FinancialYear",
                        principalColumn: "FinancialYearId");
                    table.ForeignKey(
                        name: "FK_DamageStockHdr_ProjectMaster_FK_ProjectId",
                        column: x => x.FK_ProjectId,
                        principalTable: "ProjectMaster",
                        principalColumn: "ProjectId");
                    table.ForeignKey(
                        name: "FK_DamageStockHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "DeliveryNoteHdr",
                columns: table => new
                {
                    DeliveryNoteId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DeliveryNoteNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    RefNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineDsicount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    AdditionalAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Freight = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscPertge = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscountType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CustomerName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_SalesManId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ProjectId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DeliveryNoteDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    HijriDate = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SalesInvoiceId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryNoteHdr", x => x.DeliveryNoteId);
                    table.ForeignKey(
                        name: "FK_DeliveryNoteHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteHdr_CustomerMaster_FK_CustomerId",
                        column: x => x.FK_CustomerId,
                        principalTable: "CustomerMaster",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteHdr_FinancialYear_FK_FinancialYearId",
                        column: x => x.FK_FinancialYearId,
                        principalTable: "FinancialYear",
                        principalColumn: "FinancialYearId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteHdr_ProjectMaster_FK_ProjectId",
                        column: x => x.FK_ProjectId,
                        principalTable: "ProjectMaster",
                        principalColumn: "ProjectId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteHdr_SalesManMaster_FK_SalesManId",
                        column: x => x.FK_SalesManId,
                        principalTable: "SalesManMaster",
                        principalColumn: "SalesmanId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "DeliveryNoteRtnHdr",
                columns: table => new
                {
                    DeliveryNoteRtnId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DeliveryNoteRtnNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    RefNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineDsicount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    AdditionalAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Freight = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscPertge = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscountType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CustomerName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_SalesManId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ProjectId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DeliveryNoteRtnDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    HijriDate = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DeliveryNoteId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryNoteRtnHdr", x => x.DeliveryNoteRtnId);
                    table.ForeignKey(
                        name: "FK_DeliveryNoteRtnHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteRtnHdr_CustomerMaster_FK_CustomerId",
                        column: x => x.FK_CustomerId,
                        principalTable: "CustomerMaster",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteRtnHdr_FinancialYear_FK_FinancialYearId",
                        column: x => x.FK_FinancialYearId,
                        principalTable: "FinancialYear",
                        principalColumn: "FinancialYearId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteRtnHdr_ProjectMaster_FK_ProjectId",
                        column: x => x.FK_ProjectId,
                        principalTable: "ProjectMaster",
                        principalColumn: "ProjectId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteRtnHdr_SalesManMaster_FK_SalesManId",
                        column: x => x.FK_SalesManId,
                        principalTable: "SalesManMaster",
                        principalColumn: "SalesmanId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteRtnHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ExpenseEntryHdr",
                columns: table => new
                {
                    ExpenseEntryId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ExpenseNo = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    FK_AccountId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_ProjectId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_SalesmanId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_SupplierId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Gross = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    VatTotal = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    NetTotal = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Narration = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    ReferenceNo = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    BillType = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    VatNo = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseEntryHdr", x => x.ExpenseEntryId);
                    table.ForeignKey(
                        name: "FK_ExpenseEntryHdr_AccountMaster_FK_AccountId",
                        column: x => x.FK_AccountId,
                        principalTable: "AccountMaster",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_ExpenseEntryHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_ExpenseEntryHdr_ProjectMaster_FK_ProjectId",
                        column: x => x.FK_ProjectId,
                        principalTable: "ProjectMaster",
                        principalColumn: "ProjectId");
                    table.ForeignKey(
                        name: "FK_ExpenseEntryHdr_SalesManMaster_FK_SalesmanId",
                        column: x => x.FK_SalesmanId,
                        principalTable: "SalesManMaster",
                        principalColumn: "SalesmanId");
                    table.ForeignKey(
                        name: "FK_ExpenseEntryHdr_SupplierMaster_FK_SupplierId",
                        column: x => x.FK_SupplierId,
                        principalTable: "SupplierMaster",
                        principalColumn: "SupplierId");
                    table.ForeignKey(
                        name: "FK_ExpenseEntryHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ProformaInvoiceHdr",
                columns: table => new
                {
                    ProformaInvoiceId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ProformaInvoiceNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CustomerName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RefNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineDsicount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    AdditionalAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscPertge = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    InclusiveDiscAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    GrandTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    AdditionalDiscountType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_SalesManId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ProjectId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ProformaInvoiceDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    BillType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CustomerVatNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DueDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    CreditDays = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    HijriDate = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_AccountId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Attention = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DeliveryTo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SectionNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SourceDocType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SourceDocNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    QuotationId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProformaInvoiceHdr", x => x.ProformaInvoiceId);
                    table.ForeignKey(
                        name: "FK_ProformaInvoiceHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_ProformaInvoiceHdr_CustomerMaster_FK_CustomerId",
                        column: x => x.FK_CustomerId,
                        principalTable: "CustomerMaster",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_ProformaInvoiceHdr_FinancialYear_FK_FinancialYearId",
                        column: x => x.FK_FinancialYearId,
                        principalTable: "FinancialYear",
                        principalColumn: "FinancialYearId");
                    table.ForeignKey(
                        name: "FK_ProformaInvoiceHdr_ProjectMaster_FK_ProjectId",
                        column: x => x.FK_ProjectId,
                        principalTable: "ProjectMaster",
                        principalColumn: "ProjectId");
                    table.ForeignKey(
                        name: "FK_ProformaInvoiceHdr_SalesManMaster_FK_SalesManId",
                        column: x => x.FK_SalesManId,
                        principalTable: "SalesManMaster",
                        principalColumn: "SalesmanId");
                    table.ForeignKey(
                        name: "FK_ProformaInvoiceHdr_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_ProformaInvoiceHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseHdr",
                columns: table => new
                {
                    PurchaseId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PurchaseNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RefNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    InvoiceNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PONo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PODate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    GoodsRecNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RFQNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineDsicount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    AdditionalAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscPertge = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscountType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    BillType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ReceivedAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    PaymentAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    FK_SupplierId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SupplierName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SupplierVatNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Narration = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Purchaser = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_PurchaserId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_ProjectId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_CurrencyId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_AccountId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseHdr", x => x.PurchaseId);
                    table.ForeignKey(
                        name: "FK_PurchaseHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_PurchaseHdr_FinancialYear_FK_FinancialYearId",
                        column: x => x.FK_FinancialYearId,
                        principalTable: "FinancialYear",
                        principalColumn: "FinancialYearId");
                    table.ForeignKey(
                        name: "FK_PurchaseHdr_ProjectMaster_FK_ProjectId",
                        column: x => x.FK_ProjectId,
                        principalTable: "ProjectMaster",
                        principalColumn: "ProjectId");
                    table.ForeignKey(
                        name: "FK_PurchaseHdr_PurchaserMaster_FK_PurchaserId",
                        column: x => x.FK_PurchaserId,
                        principalTable: "PurchaserMaster",
                        principalColumn: "PurchaserId");
                    table.ForeignKey(
                        name: "FK_PurchaseHdr_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_PurchaseHdr_SupplierMaster_FK_SupplierId",
                        column: x => x.FK_SupplierId,
                        principalTable: "SupplierMaster",
                        principalColumn: "SupplierId");
                    table.ForeignKey(
                        name: "FK_PurchaseHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceHdr",
                columns: table => new
                {
                    SalesInvoiceId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    SalesInvoiceNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CustomerName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RefNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineDsicount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    AdditionalAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Freight = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscPertge = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    GrandTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    AdditionalDiscountType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_SalesManId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ProjectId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    SalesInvoiceDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    DeliveryNoteId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    DeliveryNoteIds = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    HijriDate = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CurrencyId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    BillType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    QuotationId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    SalesOrderId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ProformaInvoiceId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceHdr", x => x.SalesInvoiceId);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHdr_CustomerMaster_FK_CustomerId",
                        column: x => x.FK_CustomerId,
                        principalTable: "CustomerMaster",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHdr_FinancialYear_FK_FinancialYearId",
                        column: x => x.FK_FinancialYearId,
                        principalTable: "FinancialYear",
                        principalColumn: "FinancialYearId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHdr_ProjectMaster_FK_ProjectId",
                        column: x => x.FK_ProjectId,
                        principalTable: "ProjectMaster",
                        principalColumn: "ProjectId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHdr_SalesManMaster_FK_SalesManId",
                        column: x => x.FK_SalesManId,
                        principalTable: "SalesManMaster",
                        principalColumn: "SalesmanId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceRtnHdr",
                columns: table => new
                {
                    SalesInvoiceRtnId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    SalesInvoiceRtnNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CustomerName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RefNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineDsicount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    AdditionalAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Freight = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscPertge = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    GrandTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    AdditionalDiscountType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_SalesManId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ProjectId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    SalesInvoiceRtnDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    HijriDate = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SalesInvoiceId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    BillType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceRtnHdr", x => x.SalesInvoiceRtnId);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceRtnHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceRtnHdr_CustomerMaster_FK_CustomerId",
                        column: x => x.FK_CustomerId,
                        principalTable: "CustomerMaster",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceRtnHdr_FinancialYear_FK_FinancialYearId",
                        column: x => x.FK_FinancialYearId,
                        principalTable: "FinancialYear",
                        principalColumn: "FinancialYearId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceRtnHdr_ProjectMaster_FK_ProjectId",
                        column: x => x.FK_ProjectId,
                        principalTable: "ProjectMaster",
                        principalColumn: "ProjectId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceRtnHdr_SalesManMaster_FK_SalesManId",
                        column: x => x.FK_SalesManId,
                        principalTable: "SalesManMaster",
                        principalColumn: "SalesmanId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceRtnHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderHdr",
                columns: table => new
                {
                    SalesOrderId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    SalesOrderNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    RefNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineDsicount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    AdditionalAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Freight = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscPertge = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscountType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CustomerName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    FK_SalesManId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ProjectId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    SalesOrderDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    SalesQuotationId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderHdr", x => x.SalesOrderId);
                    table.ForeignKey(
                        name: "FK_SalesOrderHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_SalesOrderHdr_CustomerMaster_FK_CustomerId",
                        column: x => x.FK_CustomerId,
                        principalTable: "CustomerMaster",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_SalesOrderHdr_FinancialYear_FK_FinancialYearId",
                        column: x => x.FK_FinancialYearId,
                        principalTable: "FinancialYear",
                        principalColumn: "FinancialYearId");
                    table.ForeignKey(
                        name: "FK_SalesOrderHdr_ProjectMaster_FK_ProjectId",
                        column: x => x.FK_ProjectId,
                        principalTable: "ProjectMaster",
                        principalColumn: "ProjectId");
                    table.ForeignKey(
                        name: "FK_SalesOrderHdr_SalesManMaster_FK_SalesManId",
                        column: x => x.FK_SalesManId,
                        principalTable: "SalesManMaster",
                        principalColumn: "SalesmanId");
                    table.ForeignKey(
                        name: "FK_SalesOrderHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "SalesQuotationHdr",
                columns: table => new
                {
                    SalesQuotationId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    SalesQuotationNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CustomerName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RefNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineDsicount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    AdditionalAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Freight = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscPertge = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    GrandTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    AdditionalDiscountType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_SalesManId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ProjectId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    SalesQuotationDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    DeliveryNoteId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesQuotationHdr", x => x.SalesQuotationId);
                    table.ForeignKey(
                        name: "FK_SalesQuotationHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_SalesQuotationHdr_CustomerMaster_FK_CustomerId",
                        column: x => x.FK_CustomerId,
                        principalTable: "CustomerMaster",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_SalesQuotationHdr_FinancialYear_FK_FinancialYearId",
                        column: x => x.FK_FinancialYearId,
                        principalTable: "FinancialYear",
                        principalColumn: "FinancialYearId");
                    table.ForeignKey(
                        name: "FK_SalesQuotationHdr_ProjectMaster_FK_ProjectId",
                        column: x => x.FK_ProjectId,
                        principalTable: "ProjectMaster",
                        principalColumn: "ProjectId");
                    table.ForeignKey(
                        name: "FK_SalesQuotationHdr_SalesManMaster_FK_SalesManId",
                        column: x => x.FK_SalesManId,
                        principalTable: "SalesManMaster",
                        principalColumn: "SalesmanId");
                    table.ForeignKey(
                        name: "FK_SalesQuotationHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "RFQDtls",
                columns: table => new
                {
                    RFQDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_RFQId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFQDtls", x => x.RFQDtlsId);
                    table.ForeignKey(
                        name: "FK_RFQDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_RFQDtls_RFQHdr_FK_RFQId",
                        column: x => x.FK_RFQId,
                        principalTable: "RFQHdr",
                        principalColumn: "RFQId");
                    table.ForeignKey(
                        name: "FK_RFQDtls_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_RFQDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "JobCardDtls",
                columns: table => new
                {
                    JobCardDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_JobCardId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ItemCode = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    ItemName = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    Barcode = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    QtyPerUnit = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncVat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VatPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobCardDtls", x => x.JobCardDtlsId);
                    table.ForeignKey(
                        name: "FK_JobCardDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_JobCardDtls_JobCardHdr_FK_JobCardId",
                        column: x => x.FK_JobCardId,
                        principalTable: "JobCardHdr",
                        principalColumn: "JobCardId");
                    table.ForeignKey(
                        name: "FK_JobCardDtls_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_JobCardDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "DamageStockDtls",
                columns: table => new
                {
                    DamageStockDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_DamageStockHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ItemName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ItemArabicName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Barcode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Cost = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Rate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Value = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamageStockDtls", x => x.DamageStockDtlsId);
                    table.ForeignKey(
                        name: "FK_DamageStockDtls_DamageStockHdr_FK_DamageStockHdrId",
                        column: x => x.FK_DamageStockHdrId,
                        principalTable: "DamageStockHdr",
                        principalColumn: "DamageStockId");
                    table.ForeignKey(
                        name: "FK_DamageStockDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_DamageStockDtls_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_DamageStockDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "DeliveryNoteDtls",
                columns: table => new
                {
                    DeliveryNoteDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_DeliveryNoteHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ItemName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Barcode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    ExRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    InRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_SalesInvoiceDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    InvoicedQty = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryNoteDtls", x => x.DeliveryNoteDtlsId);
                    table.ForeignKey(
                        name: "FK_DeliveryNoteDtls_DeliveryNoteHdr_FK_DeliveryNoteHdrId",
                        column: x => x.FK_DeliveryNoteHdrId,
                        principalTable: "DeliveryNoteHdr",
                        principalColumn: "DeliveryNoteId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteDtls_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "DeliveryNoteRtnDtls",
                columns: table => new
                {
                    DeliveryNoteRtnDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_DeliveryNoteRtnHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ItemName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Barcode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    ExRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    InRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryNoteRtnDtls", x => x.DeliveryNoteRtnDtlsId);
                    table.ForeignKey(
                        name: "FK_DeliveryNoteRtnDtls_DeliveryNoteRtnHdr_FK_DeliveryNoteRtnHdrId",
                        column: x => x.FK_DeliveryNoteRtnHdrId,
                        principalTable: "DeliveryNoteRtnHdr",
                        principalColumn: "DeliveryNoteRtnId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteRtnDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteRtnDtls_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteRtnDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "ExpenseEntryDtls",
                columns: table => new
                {
                    ExpenseDtlId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_ExpenseEntryId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_AccountId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_SupplierId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    VatPer = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    VatAmt = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    NetAmt = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    InvoiceNo = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    Narration = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    VatNo = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    LineNo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseEntryDtls", x => x.ExpenseDtlId);
                    table.ForeignKey(
                        name: "FK_ExpenseEntryDtls_AccountMaster_FK_AccountId",
                        column: x => x.FK_AccountId,
                        principalTable: "AccountMaster",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_ExpenseEntryDtls_ExpenseEntryHdr_FK_ExpenseEntryId",
                        column: x => x.FK_ExpenseEntryId,
                        principalTable: "ExpenseEntryHdr",
                        principalColumn: "ExpenseEntryId");
                    table.ForeignKey(
                        name: "FK_ExpenseEntryDtls_SupplierMaster_FK_SupplierId",
                        column: x => x.FK_SupplierId,
                        principalTable: "SupplierMaster",
                        principalColumn: "SupplierId");
                });

            migrationBuilder.CreateTable(
                name: "ProformaInvoiceDtls",
                columns: table => new
                {
                    ProformaInvoiceDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_ProformaInvoiceHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ItemName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Barcode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    ExRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    InRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_SourceDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProformaInvoiceDtls", x => x.ProformaInvoiceDtlsId);
                    table.ForeignKey(
                        name: "FK_ProformaInvoiceDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_ProformaInvoiceDtls_ProformaInvoiceHdr_FK_ProformaInvoiceHdrId",
                        column: x => x.FK_ProformaInvoiceHdrId,
                        principalTable: "ProformaInvoiceHdr",
                        principalColumn: "ProformaInvoiceId");
                    table.ForeignKey(
                        name: "FK_ProformaInvoiceDtls_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_ProformaInvoiceDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseDtls",
                columns: table => new
                {
                    PurchaseDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_PurchaseHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ItemName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Barcode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Damage = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    QtyPerUnit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    ExRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    InRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LandingCost = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_SourceDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseDtls", x => x.PurchaseDtlsId);
                    table.ForeignKey(
                        name: "FK_PurchaseDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_PurchaseDtls_PurchaseHdr_FK_PurchaseHdrId",
                        column: x => x.FK_PurchaseHdrId,
                        principalTable: "PurchaseHdr",
                        principalColumn: "PurchaseId");
                    table.ForeignKey(
                        name: "FK_PurchaseDtls_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_PurchaseDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnHdr",
                columns: table => new
                {
                    PurchaseReturnId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PurchaseReturnNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    RefNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_PurchaseId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    InvoiceNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    FK_SupplierId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    SupplierName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    BillType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_AccountId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    AccountNo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CurrencyId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    CurrencyRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    FK_PurchaserId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_ProjectId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Remarks = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineDsicount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    AdditionalAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    Freight = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscPertge = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    AdditionalDiscountType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_BranchId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_FinancialYearId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PurchaseReturnDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturnHdr", x => x.PurchaseReturnId);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnHdr_BranchMaster_FK_BranchId",
                        column: x => x.FK_BranchId,
                        principalTable: "BranchMaster",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnHdr_CurrencyMaster_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "CurrencyMaster",
                        principalColumn: "CurrencyId");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnHdr_FinancialYear_FK_FinancialYearId",
                        column: x => x.FK_FinancialYearId,
                        principalTable: "FinancialYear",
                        principalColumn: "FinancialYearId");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnHdr_ProjectMaster_FK_ProjectId",
                        column: x => x.FK_ProjectId,
                        principalTable: "ProjectMaster",
                        principalColumn: "ProjectId");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnHdr_PurchaseHdr_FK_PurchaseId",
                        column: x => x.FK_PurchaseId,
                        principalTable: "PurchaseHdr",
                        principalColumn: "PurchaseId");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnHdr_PurchaserMaster_FK_PurchaserId",
                        column: x => x.FK_PurchaserId,
                        principalTable: "PurchaserMaster",
                        principalColumn: "PurchaserId");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnHdr_SupplierMaster_FK_SupplierId",
                        column: x => x.FK_SupplierId,
                        principalTable: "SupplierMaster",
                        principalColumn: "SupplierId");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnHdr_UserDetails_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "UserDetails",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "CustomerReceiptDtls",
                columns: table => new
                {
                    CustomerReceiptDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_CustomerReceiptId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_InvoiceId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    InvoiceNo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    InvoiceAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ReturnAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Narration = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    RecordNo = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerReceiptDtls", x => x.CustomerReceiptDtlsId);
                    table.ForeignKey(
                        name: "FK_CustomerReceiptDtls_CustomerReceiptHdr_FK_CustomerReceiptId",
                        column: x => x.FK_CustomerReceiptId,
                        principalTable: "CustomerReceiptHdr",
                        principalColumn: "CustomerReceiptId");
                    table.ForeignKey(
                        name: "FK_CustomerReceiptDtls_SalesInvoiceHdr_FK_InvoiceId",
                        column: x => x.FK_InvoiceId,
                        principalTable: "SalesInvoiceHdr",
                        principalColumn: "SalesInvoiceId");
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceDtls",
                columns: table => new
                {
                    SalesInvoiceDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_SalesInvoiceHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ItemName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Barcode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    ExRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    InRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    FK_DeliveryNoteDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    DeliveredQty = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceDtls", x => x.SalesInvoiceDtlsId);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceDtls_SalesInvoiceHdr_FK_SalesInvoiceHdrId",
                        column: x => x.FK_SalesInvoiceHdrId,
                        principalTable: "SalesInvoiceHdr",
                        principalColumn: "SalesInvoiceId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceDtls_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceRtnDtls",
                columns: table => new
                {
                    SalesInvoiceRtnDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_SalesInvoiceRtnHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ItemName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Barcode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    ExRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    InRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceRtnDtls", x => x.SalesInvoiceRtnDtlsId);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceRtnDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceRtnDtls_SalesInvoiceRtnHdr_FK_SalesInvoiceRtnHdrId",
                        column: x => x.FK_SalesInvoiceRtnHdrId,
                        principalTable: "SalesInvoiceRtnHdr",
                        principalColumn: "SalesInvoiceRtnId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceRtnDtls_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceRtnDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderDtls",
                columns: table => new
                {
                    SalesOrderDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_SalesOrderHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ItemName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Barcode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    ExRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    InRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderDtls", x => x.SalesOrderDtlsId);
                    table.ForeignKey(
                        name: "FK_SalesOrderDtls_SalesOrderHdr_FK_SalesOrderHdrId",
                        column: x => x.FK_SalesOrderHdrId,
                        principalTable: "SalesOrderHdr",
                        principalColumn: "SalesOrderId");
                    table.ForeignKey(
                        name: "FK_SalesOrderDtls_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_SalesOrderDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "SalesQuotationDtls",
                columns: table => new
                {
                    SalesQuotationDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_SalesQuotationHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ItemName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Barcode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    ExRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    InRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesQuotationDtls", x => x.SalesQuotationDtlsId);
                    table.ForeignKey(
                        name: "FK_SalesQuotationDtls_SalesQuotationHdr_FK_SalesQuotationHdrId",
                        column: x => x.FK_SalesQuotationHdrId,
                        principalTable: "SalesQuotationHdr",
                        principalColumn: "SalesQuotationId");
                    table.ForeignKey(
                        name: "FK_SalesQuotationDtls_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_SalesQuotationDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnDtls",
                columns: table => new
                {
                    PurchaseReturnDtlsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FK_PurchaseReturnHdrId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FK_ItemId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ItemName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ItemArabicName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Barcode = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FK_UnitId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    QtyPerUnit = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    ExRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    InRate = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DiscountAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxableAmnt = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxPerc = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    FK_StoreId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturnDtls", x => x.PurchaseReturnDtlsId);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDtls_ItemMaster_FK_ItemId",
                        column: x => x.FK_ItemId,
                        principalTable: "ItemMaster",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDtls_PurchaseReturnHdr_FK_PurchaseReturnHdrId",
                        column: x => x.FK_PurchaseReturnHdrId,
                        principalTable: "PurchaseReturnHdr",
                        principalColumn: "PurchaseReturnId");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDtls_StoreMaster_FK_StoreId",
                        column: x => x.FK_StoreId,
                        principalTable: "StoreMaster",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDtls_UnitMaster_FK_UnitId",
                        column: x => x.FK_UnitId,
                        principalTable: "UnitMaster",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountGroup_FK_AcHeadId",
                table: "AccountGroup",
                column: "FK_AcHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountHeadMaster_FK_AcTypeId",
                table: "AccountHeadMaster",
                column: "FK_AcTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountMaster_Code",
                table: "AccountMaster",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountOpeningEntryDetails_FK_AccountId",
                table: "AccountOpeningEntryDetails",
                column: "FK_AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountSubGroup_Code",
                table: "AccountSubGroup",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountSubGroup_FK_AcGroupId",
                table: "AccountSubGroup",
                column: "FK_AcGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountSubGroup_FK_AcHeadId",
                table: "AccountSubGroup",
                column: "FK_AcHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDetails_FK_BranchId",
                table: "CompanyDetails",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDetails_FK_CountryId",
                table: "CompanyDetails",
                column: "FK_CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerMaster_FK_AccountId",
                table: "CustomerMaster",
                column: "FK_AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerMaster_FK_CountryId",
                table: "CustomerMaster",
                column: "FK_CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerMaster_FK_CustomerGroupId",
                table: "CustomerMaster",
                column: "FK_CustomerGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerMaster_FK_SalesmanId",
                table: "CustomerMaster",
                column: "FK_SalesmanId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOPEntry_FK_BranchId",
                table: "CustomerOPEntry",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOPEntry_FK_UserId",
                table: "CustomerOPEntry",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOPEntryDetails_FK_BranchId",
                table: "CustomerOPEntryDetails",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOPEntryDetails_FK_CustomerId",
                table: "CustomerOPEntryDetails",
                column: "FK_CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOPEntryDetails_FK_CustomerOPEntryId",
                table: "CustomerOPEntryDetails",
                column: "FK_CustomerOPEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOPEntryUpdated_FK_BranchId",
                table: "CustomerOPEntryUpdated",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOPEntryUpdated_FK_CustomerId",
                table: "CustomerOPEntryUpdated",
                column: "FK_CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOPEntryUpdated_FK_CustomerOPEntryId",
                table: "CustomerOPEntryUpdated",
                column: "FK_CustomerOPEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReceiptDtls_FK_CustomerReceiptId",
                table: "CustomerReceiptDtls",
                column: "FK_CustomerReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReceiptDtls_FK_InvoiceId",
                table: "CustomerReceiptDtls",
                column: "FK_InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReceiptHdr_FK_AccountId",
                table: "CustomerReceiptHdr",
                column: "FK_AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReceiptHdr_FK_BranchId",
                table: "CustomerReceiptHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReceiptHdr_FK_CustomerId",
                table: "CustomerReceiptHdr",
                column: "FK_CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReceiptHdr_FK_ProjectId",
                table: "CustomerReceiptHdr",
                column: "FK_ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReceiptHdr_FK_SalesManId",
                table: "CustomerReceiptHdr",
                column: "FK_SalesManId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReceiptHdr_FK_UserId",
                table: "CustomerReceiptHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageStockDtls_FK_DamageStockHdrId",
                table: "DamageStockDtls",
                column: "FK_DamageStockHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageStockDtls_FK_ItemId",
                table: "DamageStockDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageStockDtls_FK_StoreId",
                table: "DamageStockDtls",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageStockDtls_FK_UnitId",
                table: "DamageStockDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageStockHdr_FK_AccountId",
                table: "DamageStockHdr",
                column: "FK_AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageStockHdr_FK_BranchId",
                table: "DamageStockHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageStockHdr_FK_FinancialYearId",
                table: "DamageStockHdr",
                column: "FK_FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageStockHdr_FK_ProjectId",
                table: "DamageStockHdr",
                column: "FK_ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageStockHdr_FK_UserId",
                table: "DamageStockHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteDtls_FK_DeliveryNoteHdrId",
                table: "DeliveryNoteDtls",
                column: "FK_DeliveryNoteHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteDtls_FK_StoreId",
                table: "DeliveryNoteDtls",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteDtls_FK_UnitId",
                table: "DeliveryNoteDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteHdr_FK_BranchId",
                table: "DeliveryNoteHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteHdr_FK_CustomerId",
                table: "DeliveryNoteHdr",
                column: "FK_CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteHdr_FK_FinancialYearId",
                table: "DeliveryNoteHdr",
                column: "FK_FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteHdr_FK_ProjectId",
                table: "DeliveryNoteHdr",
                column: "FK_ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteHdr_FK_SalesManId",
                table: "DeliveryNoteHdr",
                column: "FK_SalesManId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteHdr_FK_UserId",
                table: "DeliveryNoteHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteRtnDtls_FK_DeliveryNoteRtnHdrId",
                table: "DeliveryNoteRtnDtls",
                column: "FK_DeliveryNoteRtnHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteRtnDtls_FK_ItemId",
                table: "DeliveryNoteRtnDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteRtnDtls_FK_StoreId",
                table: "DeliveryNoteRtnDtls",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteRtnDtls_FK_UnitId",
                table: "DeliveryNoteRtnDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteRtnHdr_FK_BranchId",
                table: "DeliveryNoteRtnHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteRtnHdr_FK_CustomerId",
                table: "DeliveryNoteRtnHdr",
                column: "FK_CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteRtnHdr_FK_FinancialYearId",
                table: "DeliveryNoteRtnHdr",
                column: "FK_FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteRtnHdr_FK_ProjectId",
                table: "DeliveryNoteRtnHdr",
                column: "FK_ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteRtnHdr_FK_SalesManId",
                table: "DeliveryNoteRtnHdr",
                column: "FK_SalesManId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteRtnHdr_FK_UserId",
                table: "DeliveryNoteRtnHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseEntryDtls_FK_AccountId",
                table: "ExpenseEntryDtls",
                column: "FK_AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseEntryDtls_FK_ExpenseEntryId",
                table: "ExpenseEntryDtls",
                column: "FK_ExpenseEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseEntryDtls_FK_SupplierId",
                table: "ExpenseEntryDtls",
                column: "FK_SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseEntryHdr_FK_AccountId",
                table: "ExpenseEntryHdr",
                column: "FK_AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseEntryHdr_FK_BranchId",
                table: "ExpenseEntryHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseEntryHdr_FK_ProjectId",
                table: "ExpenseEntryHdr",
                column: "FK_ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseEntryHdr_FK_SalesmanId",
                table: "ExpenseEntryHdr",
                column: "FK_SalesmanId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseEntryHdr_FK_SupplierId",
                table: "ExpenseEntryHdr",
                column: "FK_SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseEntryHdr_FK_UserId",
                table: "ExpenseEntryHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FunctionalPrivilege_FK_FunctionDetailsId",
                table: "FunctionalPrivilege",
                column: "FK_FunctionDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_FunctionalPrivilege_FK_UserPrivilegeHdrId",
                table: "FunctionalPrivilege",
                column: "FK_UserPrivilegeHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_FunctionDetails_FK_MenuDetailsId",
                table: "FunctionDetails",
                column: "FK_MenuDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptDtls_FK_GoodsReceiptHdrId",
                table: "GoodsReceiptDtls",
                column: "FK_GoodsReceiptHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptDtls_FK_ItemId",
                table: "GoodsReceiptDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptDtls_FK_StoreId",
                table: "GoodsReceiptDtls",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptDtls_FK_UnitId",
                table: "GoodsReceiptDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptHdr_FK_BranchId",
                table: "GoodsReceiptHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptHdr_FK_FinancialYearId",
                table: "GoodsReceiptHdr",
                column: "FK_FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptHdr_FK_PurchaserId",
                table: "GoodsReceiptHdr",
                column: "FK_PurchaserId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptHdr_FK_SupplierId",
                table: "GoodsReceiptHdr",
                column: "FK_SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptHdr_FK_UserId",
                table: "GoodsReceiptHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalMaterialTransferDtls_FK_IntMatTransId",
                table: "InternalMaterialTransferDtls",
                column: "FK_IntMatTransId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalMaterialTransferDtls_FK_ItemId",
                table: "InternalMaterialTransferDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalMaterialTransferDtls_FK_UnitId",
                table: "InternalMaterialTransferDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalMaterialTransferHdr_FK_BranchId",
                table: "InternalMaterialTransferHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalMaterialTransferHdr_FK_FromStoreId",
                table: "InternalMaterialTransferHdr",
                column: "FK_FromStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalMaterialTransferHdr_FK_ToStoreId",
                table: "InternalMaterialTransferHdr",
                column: "FK_ToStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalMaterialTransferHdr_FK_UserId",
                table: "InternalMaterialTransferHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_FK_BrandId",
                table: "ItemMaster",
                column: "FK_BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_FK_CategoryId",
                table: "ItemMaster",
                column: "FK_CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_FK_SubCategoryId",
                table: "ItemMaster",
                column: "FK_SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_FK_SupplierId",
                table: "ItemMaster",
                column: "FK_SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_FK_UnitId",
                table: "ItemMaster",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_FK_VatId",
                table: "ItemMaster",
                column: "FK_VatId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMultiUnit_FK_ItemId",
                table: "ItemMultiUnit",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMultiUnit_FK_UnitId",
                table: "ItemMultiUnit",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemStockBalance_FK_BranchId",
                table: "ItemStockBalance",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemStockBalance_FK_ItemId",
                table: "ItemStockBalance",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_JobCardDtls_FK_ItemId",
                table: "JobCardDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_JobCardDtls_FK_JobCardId",
                table: "JobCardDtls",
                column: "FK_JobCardId");

            migrationBuilder.CreateIndex(
                name: "IX_JobCardDtls_FK_StoreId",
                table: "JobCardDtls",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_JobCardDtls_FK_UnitId",
                table: "JobCardDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_JobCardHdr_FK_JobOrderId",
                table: "JobCardHdr",
                column: "FK_JobOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_JobCardHdr_FK_SalesManId",
                table: "JobCardHdr",
                column: "FK_SalesManId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrderDtls_FK_JobOrderId",
                table: "JobOrderDtls",
                column: "FK_JobOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrderDtls_FK_RepairId",
                table: "JobOrderDtls",
                column: "FK_RepairId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrderDtls_FK_TechnicianId",
                table: "JobOrderDtls",
                column: "FK_TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrderHdr_FK_CustomerId",
                table: "JobOrderHdr",
                column: "FK_CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrderHdr_FK_SalesManId",
                table: "JobOrderHdr",
                column: "FK_SalesManId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrderHdr_FK_VehicleId",
                table: "JobOrderHdr",
                column: "FK_VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryDtls_FK_AccountId",
                table: "JournalEntryDtls",
                column: "FK_AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryDtls_FK_JournalId",
                table: "JournalEntryDtls",
                column: "FK_JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryHdr_FK_BranchId",
                table: "JournalEntryHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryHdr_FK_UserId",
                table: "JournalEntryHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferInDtls_FK_FromStoreId",
                table: "MaterialTransferInDtls",
                column: "FK_FromStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferInDtls_FK_ItemId",
                table: "MaterialTransferInDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferInDtls_FK_MatTransInId",
                table: "MaterialTransferInDtls",
                column: "FK_MatTransInId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferInDtls_FK_ToStoreId",
                table: "MaterialTransferInDtls",
                column: "FK_ToStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferInDtls_FK_UnitId",
                table: "MaterialTransferInDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferInHdr_FK_FromBranchId",
                table: "MaterialTransferInHdr",
                column: "FK_FromBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferInHdr_FK_ToBranchId",
                table: "MaterialTransferInHdr",
                column: "FK_ToBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferInHdr_FK_UserId",
                table: "MaterialTransferInHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferOutDtls_FK_FromStoreId",
                table: "MaterialTransferOutDtls",
                column: "FK_FromStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferOutDtls_FK_ItemId",
                table: "MaterialTransferOutDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferOutDtls_FK_MatTransOutId",
                table: "MaterialTransferOutDtls",
                column: "FK_MatTransOutId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferOutDtls_FK_ToStoreId",
                table: "MaterialTransferOutDtls",
                column: "FK_ToStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferOutDtls_FK_UnitId",
                table: "MaterialTransferOutDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferOutHdr_FK_FromBranchId",
                table: "MaterialTransferOutHdr",
                column: "FK_FromBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferOutHdr_FK_ToBranchId",
                table: "MaterialTransferOutHdr",
                column: "FK_ToBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferOutHdr_FK_UserId",
                table: "MaterialTransferOutHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferRequestDtls_FK_FromStoreId",
                table: "MaterialTransferRequestDtls",
                column: "FK_FromStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferRequestDtls_FK_ItemId",
                table: "MaterialTransferRequestDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferRequestDtls_FK_MatTransReqId",
                table: "MaterialTransferRequestDtls",
                column: "FK_MatTransReqId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferRequestDtls_FK_ToStoreId",
                table: "MaterialTransferRequestDtls",
                column: "FK_ToStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferRequestDtls_FK_UnitId",
                table: "MaterialTransferRequestDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferRequestHdr_FK_ApprovedBy",
                table: "MaterialTransferRequestHdr",
                column: "FK_ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferRequestHdr_FK_FromBranchId",
                table: "MaterialTransferRequestHdr",
                column: "FK_FromBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferRequestHdr_FK_RejectedBy",
                table: "MaterialTransferRequestHdr",
                column: "FK_RejectedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferRequestHdr_FK_ToBranchId",
                table: "MaterialTransferRequestHdr",
                column: "FK_ToBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialTransferRequestHdr_FK_UserId",
                table: "MaterialTransferRequestHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningStockEntry_FK_BranchId",
                table: "OpeningStockEntry",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningStockEntry_FK_UserId",
                table: "OpeningStockEntry",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningStockEntryItem_FK_ItemId",
                table: "OpeningStockEntryItem",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningStockEntryItem_FK_OpeningStockEntryId",
                table: "OpeningStockEntryItem",
                column: "FK_OpeningStockEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningStockEntryItem_FK_StoreId",
                table: "OpeningStockEntryItem",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningStockEntryItem_FK_UnitId",
                table: "OpeningStockEntryItem",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningStockEntryItemForUpdate_FK_BranchId",
                table: "OpeningStockEntryItemForUpdate",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningStockEntryItemForUpdate_FK_ItemId",
                table: "OpeningStockEntryItemForUpdate",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningStockEntryItemForUpdate_FK_OpeningStockEntryId",
                table: "OpeningStockEntryItemForUpdate",
                column: "FK_OpeningStockEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningStockEntryItemForUpdate_FK_StoreId",
                table: "OpeningStockEntryItemForUpdate",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningStockEntryItemForUpdate_FK_UnitId",
                table: "OpeningStockEntryItemForUpdate",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoiceDtls_FK_ItemId",
                table: "ProformaInvoiceDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoiceDtls_FK_ProformaInvoiceHdrId",
                table: "ProformaInvoiceDtls",
                column: "FK_ProformaInvoiceHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoiceDtls_FK_StoreId",
                table: "ProformaInvoiceDtls",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoiceDtls_FK_UnitId",
                table: "ProformaInvoiceDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoiceHdr_FK_BranchId",
                table: "ProformaInvoiceHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoiceHdr_FK_CustomerId",
                table: "ProformaInvoiceHdr",
                column: "FK_CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoiceHdr_FK_FinancialYearId",
                table: "ProformaInvoiceHdr",
                column: "FK_FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoiceHdr_FK_ProjectId",
                table: "ProformaInvoiceHdr",
                column: "FK_ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoiceHdr_FK_SalesManId",
                table: "ProformaInvoiceHdr",
                column: "FK_SalesManId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoiceHdr_FK_StoreId",
                table: "ProformaInvoiceHdr",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoiceHdr_FK_UserId",
                table: "ProformaInvoiceHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMaster_FK_CustomerId",
                table: "ProjectMaster",
                column: "FK_CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMaster_FK_PurchaserId",
                table: "ProjectMaster",
                column: "FK_PurchaserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMaster_FK_SalesmanId",
                table: "ProjectMaster",
                column: "FK_SalesmanId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDtls_FK_ItemId",
                table: "PurchaseDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDtls_FK_PurchaseHdrId",
                table: "PurchaseDtls",
                column: "FK_PurchaseHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDtls_FK_StoreId",
                table: "PurchaseDtls",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDtls_FK_UnitId",
                table: "PurchaseDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseHdr_FK_BranchId",
                table: "PurchaseHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseHdr_FK_FinancialYearId",
                table: "PurchaseHdr",
                column: "FK_FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseHdr_FK_ProjectId",
                table: "PurchaseHdr",
                column: "FK_ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseHdr_FK_PurchaserId",
                table: "PurchaseHdr",
                column: "FK_PurchaserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseHdr_FK_StoreId",
                table: "PurchaseHdr",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseHdr_FK_SupplierId",
                table: "PurchaseHdr",
                column: "FK_SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseHdr_FK_UserId",
                table: "PurchaseHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDtls_FK_PurchaseOrderHdrId",
                table: "PurchaseOrderDtls",
                column: "FK_PurchaseOrderHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDtls_FK_StoreId",
                table: "PurchaseOrderDtls",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDtls_FK_UnitId",
                table: "PurchaseOrderDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderHdr_FK_BranchId",
                table: "PurchaseOrderHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderHdr_FK_FinancialYearId",
                table: "PurchaseOrderHdr",
                column: "FK_FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderHdr_FK_PurchaserId",
                table: "PurchaseOrderHdr",
                column: "FK_PurchaserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderHdr_FK_SupplierId",
                table: "PurchaseOrderHdr",
                column: "FK_SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderHdr_FK_UserId",
                table: "PurchaseOrderHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDtls_FK_ItemId",
                table: "PurchaseReturnDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDtls_FK_PurchaseReturnHdrId",
                table: "PurchaseReturnDtls",
                column: "FK_PurchaseReturnHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDtls_FK_StoreId",
                table: "PurchaseReturnDtls",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDtls_FK_UnitId",
                table: "PurchaseReturnDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHdr_CurrencyId",
                table: "PurchaseReturnHdr",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHdr_FK_BranchId",
                table: "PurchaseReturnHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHdr_FK_FinancialYearId",
                table: "PurchaseReturnHdr",
                column: "FK_FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHdr_FK_ProjectId",
                table: "PurchaseReturnHdr",
                column: "FK_ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHdr_FK_PurchaseId",
                table: "PurchaseReturnHdr",
                column: "FK_PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHdr_FK_PurchaserId",
                table: "PurchaseReturnHdr",
                column: "FK_PurchaserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHdr_FK_SupplierId",
                table: "PurchaseReturnHdr",
                column: "FK_SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHdr_FK_UserId",
                table: "PurchaseReturnHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RFQDtls_FK_ItemId",
                table: "RFQDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RFQDtls_FK_RFQId",
                table: "RFQDtls",
                column: "FK_RFQId");

            migrationBuilder.CreateIndex(
                name: "IX_RFQDtls_FK_StoreId",
                table: "RFQDtls",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_RFQDtls_FK_UnitId",
                table: "RFQDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_RFQHdr_FK_BranchId",
                table: "RFQHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RFQHdr_FK_CustomerId",
                table: "RFQHdr",
                column: "FK_CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RFQHdr_FK_FinancialYearId",
                table: "RFQHdr",
                column: "FK_FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_RFQHdr_FK_SalesManId",
                table: "RFQHdr",
                column: "FK_SalesManId");

            migrationBuilder.CreateIndex(
                name: "IX_RFQHdr_FK_UserId",
                table: "RFQHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceDtls_FK_ItemId",
                table: "SalesInvoiceDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceDtls_FK_SalesInvoiceHdrId",
                table: "SalesInvoiceDtls",
                column: "FK_SalesInvoiceHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceDtls_FK_StoreId",
                table: "SalesInvoiceDtls",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceDtls_FK_UnitId",
                table: "SalesInvoiceDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHdr_FK_BranchId",
                table: "SalesInvoiceHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHdr_FK_CustomerId",
                table: "SalesInvoiceHdr",
                column: "FK_CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHdr_FK_FinancialYearId",
                table: "SalesInvoiceHdr",
                column: "FK_FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHdr_FK_ProjectId",
                table: "SalesInvoiceHdr",
                column: "FK_ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHdr_FK_SalesManId",
                table: "SalesInvoiceHdr",
                column: "FK_SalesManId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHdr_FK_UserId",
                table: "SalesInvoiceHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceRtnDtls_FK_ItemId",
                table: "SalesInvoiceRtnDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceRtnDtls_FK_SalesInvoiceRtnHdrId",
                table: "SalesInvoiceRtnDtls",
                column: "FK_SalesInvoiceRtnHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceRtnDtls_FK_StoreId",
                table: "SalesInvoiceRtnDtls",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceRtnDtls_FK_UnitId",
                table: "SalesInvoiceRtnDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceRtnHdr_FK_BranchId",
                table: "SalesInvoiceRtnHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceRtnHdr_FK_CustomerId",
                table: "SalesInvoiceRtnHdr",
                column: "FK_CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceRtnHdr_FK_FinancialYearId",
                table: "SalesInvoiceRtnHdr",
                column: "FK_FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceRtnHdr_FK_ProjectId",
                table: "SalesInvoiceRtnHdr",
                column: "FK_ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceRtnHdr_FK_SalesManId",
                table: "SalesInvoiceRtnHdr",
                column: "FK_SalesManId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceRtnHdr_FK_UserId",
                table: "SalesInvoiceRtnHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDtls_FK_SalesOrderHdrId",
                table: "SalesOrderDtls",
                column: "FK_SalesOrderHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDtls_FK_StoreId",
                table: "SalesOrderDtls",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDtls_FK_UnitId",
                table: "SalesOrderDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHdr_FK_BranchId",
                table: "SalesOrderHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHdr_FK_CustomerId",
                table: "SalesOrderHdr",
                column: "FK_CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHdr_FK_FinancialYearId",
                table: "SalesOrderHdr",
                column: "FK_FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHdr_FK_ProjectId",
                table: "SalesOrderHdr",
                column: "FK_ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHdr_FK_SalesManId",
                table: "SalesOrderHdr",
                column: "FK_SalesManId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHdr_FK_UserId",
                table: "SalesOrderHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationDtls_FK_SalesQuotationHdrId",
                table: "SalesQuotationDtls",
                column: "FK_SalesQuotationHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationDtls_FK_StoreId",
                table: "SalesQuotationDtls",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationDtls_FK_UnitId",
                table: "SalesQuotationDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationHdr_FK_BranchId",
                table: "SalesQuotationHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationHdr_FK_CustomerId",
                table: "SalesQuotationHdr",
                column: "FK_CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationHdr_FK_FinancialYearId",
                table: "SalesQuotationHdr",
                column: "FK_FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationHdr_FK_ProjectId",
                table: "SalesQuotationHdr",
                column: "FK_ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationHdr_FK_SalesManId",
                table: "SalesQuotationHdr",
                column: "FK_SalesManId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationHdr_FK_UserId",
                table: "SalesQuotationHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentDtls_FK_ItemId",
                table: "StockAdjustmentDtls",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentDtls_FK_StockAdjId",
                table: "StockAdjustmentDtls",
                column: "FK_StockAdjId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentDtls_FK_UnitId",
                table: "StockAdjustmentDtls",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentHdr_FK_BranchId",
                table: "StockAdjustmentHdr",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentHdr_FK_UserId",
                table: "StockAdjustmentHdr",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTranLog_FK_ActualUnitId",
                table: "StockTranLog",
                column: "FK_ActualUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTranLog_FK_BranchId",
                table: "StockTranLog",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTranLog_FK_ItemId",
                table: "StockTranLog",
                column: "FK_ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTranLog_FK_StoreId",
                table: "StockTranLog",
                column: "FK_StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTranLog_FK_UnitId",
                table: "StockTranLog",
                column: "FK_UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTranLog_FK_UserId",
                table: "StockTranLog",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreMaster_FK_BranchId",
                table: "StoreMaster",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryMaster_FK_CategoryId",
                table: "SubCategoryMaster",
                column: "FK_CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierMaster_FK_AccountId",
                table: "SupplierMaster",
                column: "FK_AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierMaster_FK_CountryId",
                table: "SupplierMaster",
                column: "FK_CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierMaster_FK_PurchaserId",
                table: "SupplierMaster",
                column: "FK_PurchaserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierOPEntry_FK_BranchId",
                table: "SupplierOPEntry",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierOPEntry_FK_UserId",
                table: "SupplierOPEntry",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierOPEntryDetails_FK_BranchId",
                table: "SupplierOPEntryDetails",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierOPEntryDetails_FK_SupplierId",
                table: "SupplierOPEntryDetails",
                column: "FK_SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_TblAccounting_FK_AccountId",
                table: "TblAccounting",
                column: "FK_AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TblAccounting_FK_BranchId",
                table: "TblAccounting",
                column: "FK_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TblAccounting_FK_UserId",
                table: "TblAccounting",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrivilegeDtls_FK_MenuDetailsId",
                table: "UserPrivilegeDtls",
                column: "FK_MenuDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrivilegeDtls_FK_UserPrivilegeHdrId",
                table: "UserPrivilegeDtls",
                column: "FK_UserPrivilegeHdrId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountOpeningEntryDetails");

            migrationBuilder.DropTable(
                name: "AccountOpeningEntryHdr");

            migrationBuilder.DropTable(
                name: "AccountSubGroup");

            migrationBuilder.DropTable(
                name: "BarcodeData");

            migrationBuilder.DropTable(
                name: "BarcodeDesignDtls");

            migrationBuilder.DropTable(
                name: "BarcodeDesignHdr");

            migrationBuilder.DropTable(
                name: "ClientERPSettings");

            migrationBuilder.DropTable(
                name: "CodeSequences");

            migrationBuilder.DropTable(
                name: "CompanyDetails");

            migrationBuilder.DropTable(
                name: "CustomerLocationDetails");

            migrationBuilder.DropTable(
                name: "CustomerLocationMaster");

            migrationBuilder.DropTable(
                name: "CustomerOPEntryDetails");

            migrationBuilder.DropTable(
                name: "CustomerOPEntryUpdated");

            migrationBuilder.DropTable(
                name: "CustomerReceiptDtls");

            migrationBuilder.DropTable(
                name: "DamageStockDtls");

            migrationBuilder.DropTable(
                name: "DeliveryNoteDtls");

            migrationBuilder.DropTable(
                name: "DeliveryNoteRtnDtls");

            migrationBuilder.DropTable(
                name: "ERPType");

            migrationBuilder.DropTable(
                name: "ERPTypePreset");

            migrationBuilder.DropTable(
                name: "ExpenseEntryDtls");

            migrationBuilder.DropTable(
                name: "FunctionalPrivilege");

            migrationBuilder.DropTable(
                name: "GoodsReceiptDtls");

            migrationBuilder.DropTable(
                name: "InternalMaterialTransferDtls");

            migrationBuilder.DropTable(
                name: "InvoiceICV");

            migrationBuilder.DropTable(
                name: "ItemMultiUnit");

            migrationBuilder.DropTable(
                name: "ItemStockBalance");

            migrationBuilder.DropTable(
                name: "JobCardDtls");

            migrationBuilder.DropTable(
                name: "JobOrderDtls");

            migrationBuilder.DropTable(
                name: "JournalEntryDtls");

            migrationBuilder.DropTable(
                name: "MasterValue");

            migrationBuilder.DropTable(
                name: "MaterialTransferInDtls");

            migrationBuilder.DropTable(
                name: "MaterialTransferOutDtls");

            migrationBuilder.DropTable(
                name: "MaterialTransferRequestDtls");

            migrationBuilder.DropTable(
                name: "MenuVisibility");

            migrationBuilder.DropTable(
                name: "OpeningStockEntryItem");

            migrationBuilder.DropTable(
                name: "OpeningStockEntryItemForUpdate");

            migrationBuilder.DropTable(
                name: "PermanentSettings");

            migrationBuilder.DropTable(
                name: "PrintDesignDtls");

            migrationBuilder.DropTable(
                name: "PrintDesignHdr");

            migrationBuilder.DropTable(
                name: "ProformaInvoiceDtls");

            migrationBuilder.DropTable(
                name: "PurchaseDtls");

            migrationBuilder.DropTable(
                name: "PurchaseOrderDtls");

            migrationBuilder.DropTable(
                name: "PurchaseReturnDtls");

            migrationBuilder.DropTable(
                name: "RFQDtls");

            migrationBuilder.DropTable(
                name: "SalesInvoiceDtls");

            migrationBuilder.DropTable(
                name: "SalesInvoiceRtnDtls");

            migrationBuilder.DropTable(
                name: "SalesOrderDtls");

            migrationBuilder.DropTable(
                name: "SalesQuotationDtls");

            migrationBuilder.DropTable(
                name: "StockAdjustmentDtls");

            migrationBuilder.DropTable(
                name: "StockTranLog");

            migrationBuilder.DropTable(
                name: "SupplierOPEntry");

            migrationBuilder.DropTable(
                name: "SupplierOPEntryDetails");

            migrationBuilder.DropTable(
                name: "SupplierOPEntryUpdated");

            migrationBuilder.DropTable(
                name: "SupplierPaymentDtls");

            migrationBuilder.DropTable(
                name: "SupplierPaymentHdr");

            migrationBuilder.DropTable(
                name: "TblAccounting");

            migrationBuilder.DropTable(
                name: "UserBranch");

            migrationBuilder.DropTable(
                name: "UserPrivilegeDtls");

            migrationBuilder.DropTable(
                name: "VATCategoryDetails");

            migrationBuilder.DropTable(
                name: "ZatcaInvoices");

            migrationBuilder.DropTable(
                name: "AccountGroup");

            migrationBuilder.DropTable(
                name: "CustomerOPEntry");

            migrationBuilder.DropTable(
                name: "CustomerReceiptHdr");

            migrationBuilder.DropTable(
                name: "DamageStockHdr");

            migrationBuilder.DropTable(
                name: "DeliveryNoteHdr");

            migrationBuilder.DropTable(
                name: "DeliveryNoteRtnHdr");

            migrationBuilder.DropTable(
                name: "ExpenseEntryHdr");

            migrationBuilder.DropTable(
                name: "FunctionDetails");

            migrationBuilder.DropTable(
                name: "GoodsReceiptHdr");

            migrationBuilder.DropTable(
                name: "InternalMaterialTransferHdr");

            migrationBuilder.DropTable(
                name: "JobCardHdr");

            migrationBuilder.DropTable(
                name: "RepairMaster");

            migrationBuilder.DropTable(
                name: "TechnicianMaster");

            migrationBuilder.DropTable(
                name: "JournalEntryHdr");

            migrationBuilder.DropTable(
                name: "MaterialTransferInHdr");

            migrationBuilder.DropTable(
                name: "MaterialTransferOutHdr");

            migrationBuilder.DropTable(
                name: "MaterialTransferRequestHdr");

            migrationBuilder.DropTable(
                name: "OpeningStockEntry");

            migrationBuilder.DropTable(
                name: "ProformaInvoiceHdr");

            migrationBuilder.DropTable(
                name: "PurchaseOrderHdr");

            migrationBuilder.DropTable(
                name: "PurchaseReturnHdr");

            migrationBuilder.DropTable(
                name: "RFQHdr");

            migrationBuilder.DropTable(
                name: "SalesInvoiceHdr");

            migrationBuilder.DropTable(
                name: "SalesInvoiceRtnHdr");

            migrationBuilder.DropTable(
                name: "SalesOrderHdr");

            migrationBuilder.DropTable(
                name: "SalesQuotationHdr");

            migrationBuilder.DropTable(
                name: "StockAdjustmentHdr");

            migrationBuilder.DropTable(
                name: "ItemMaster");

            migrationBuilder.DropTable(
                name: "UserPrivilegeHdr");

            migrationBuilder.DropTable(
                name: "AccountHeadMaster");

            migrationBuilder.DropTable(
                name: "MenuDetails");

            migrationBuilder.DropTable(
                name: "JobOrderHdr");

            migrationBuilder.DropTable(
                name: "CurrencyMaster");

            migrationBuilder.DropTable(
                name: "PurchaseHdr");

            migrationBuilder.DropTable(
                name: "BrandMaster");

            migrationBuilder.DropTable(
                name: "SubCategoryMaster");

            migrationBuilder.DropTable(
                name: "UnitMaster");

            migrationBuilder.DropTable(
                name: "VatMaster");

            migrationBuilder.DropTable(
                name: "AccountType");

            migrationBuilder.DropTable(
                name: "VehicleMaster");

            migrationBuilder.DropTable(
                name: "FinancialYear");

            migrationBuilder.DropTable(
                name: "ProjectMaster");

            migrationBuilder.DropTable(
                name: "StoreMaster");

            migrationBuilder.DropTable(
                name: "SupplierMaster");

            migrationBuilder.DropTable(
                name: "UserDetails");

            migrationBuilder.DropTable(
                name: "CategoryMaster");

            migrationBuilder.DropTable(
                name: "CustomerMaster");

            migrationBuilder.DropTable(
                name: "BranchMaster");

            migrationBuilder.DropTable(
                name: "PurchaserMaster");

            migrationBuilder.DropTable(
                name: "AccountMaster");

            migrationBuilder.DropTable(
                name: "CountryMaster");

            migrationBuilder.DropTable(
                name: "CustomerGroupMaster");

            migrationBuilder.DropTable(
                name: "SalesManMaster");
        }
    }
}
