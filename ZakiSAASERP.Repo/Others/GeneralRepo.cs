using ZakiSAASERP.Models.ViewModel;
using AshproStringExtension;
using System.Text;

namespace ZakiSAASERP.Repo.Others;

public class GeneralRepo(IDbContextProvider contextProvider) : IGeneralRepo
{
    public async Task<ChartEntity> GetChartValues(int branchId, string format)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            ChartEntity chartEntity = new ChartEntity();

            // 1. Cash In Hand: Default Cash Account balance from TblAccounting, or Cash Sales + Cash Receipts
            var cashSetting = await db.PermanentSettings.AsNoTracking().FirstOrDefaultAsync(x => x.SettingKey == "DefaultCashAccount");
            int defaultCashAccId = (cashSetting != null && int.TryParse(cashSetting.SettingValue, out int cId)) ? cId : 0;

            decimal cashInHand = 0m;
            if (defaultCashAccId > 0 && await db.TblAccountings.AnyAsync(x => x.FK_AccountId == defaultCashAccId && (branchId == 0 || x.FK_BranchId == branchId)))
            {
                cashInHand = await db.TblAccountings
                    .Where(x => x.FK_AccountId == defaultCashAccId && (branchId == 0 || x.FK_BranchId == branchId))
                    .SumAsync(x => (decimal?)x.Amount) ?? 0m;
            }
            else
            {
                var cashSales = await db.SalesInvoiceHdrs
                    .Where(x => x.BillType == "Cash" && !x.IsDeleted && x.FK_BranchId == branchId)
                    .SumAsync(x => (decimal?)x.GrandTotal) ?? 0m;

                var cashReceipts = await db.CustomerReceiptHdrs
                    .Where(x => x.PaymentMode == "Cash" && !x.IsDeleted && x.FK_BranchId == branchId)
                    .SumAsync(x => (decimal?)x.TotalAmount) ?? 0m;

                cashInHand = cashSales + cashReceipts;
            }

            chartEntity.CashInHand = cashInHand.ToString("F2");

            // 2. Cash At Bank: Default Bank Account balance from TblAccounting, or Bank Sales + Bank Receipts
            var bankSetting = await db.PermanentSettings.AsNoTracking().FirstOrDefaultAsync(x => x.SettingKey == "DefaultBankAccount");
            int defaultBankAccId = (bankSetting != null && int.TryParse(bankSetting.SettingValue, out int bId)) ? bId : 0;

            if (defaultBankAccId == 0)
            {
                var fallbackBank = await db.AccountMasters.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Code.StartsWith("11000102") || x.Name.Contains("Bank") || (x.NameArabic != null && x.NameArabic.Contains("بنك")));
                if (fallbackBank != null)
                {
                    defaultBankAccId = fallbackBank.AccountId;
                }
            }

            decimal cashAtBank = 0m;
            if (defaultBankAccId > 0 && await db.TblAccountings.AnyAsync(x => x.FK_AccountId == defaultBankAccId && (branchId == 0 || x.FK_BranchId == branchId)))
            {
                cashAtBank = await db.TblAccountings
                    .Where(x => x.FK_AccountId == defaultBankAccId && (branchId == 0 || x.FK_BranchId == branchId))
                    .SumAsync(x => (decimal?)x.Amount) ?? 0m;
            }
            else
            {
                var bankSales = await db.SalesInvoiceHdrs
                    .Where(x => (x.BillType == "Bank" || x.BillType == "Card") && !x.IsDeleted && x.FK_BranchId == branchId)
                    .SumAsync(x => (decimal?)x.GrandTotal) ?? 0m;

                var bankReceipts = await db.CustomerReceiptHdrs
                    .Where(x => (x.PaymentMode == "Bank" || x.PaymentMode == "Card" || x.PaymentMode == "Cheque" || (x.PaymentMode != "Cash" && x.PaymentMode != "Credit")) && !x.IsDeleted && x.FK_BranchId == branchId)
                    .SumAsync(x => (decimal?)x.TotalAmount) ?? 0m;

                cashAtBank = bankSales + bankReceipts;
            }

            chartEntity.CashAtBank = cashAtBank.ToString("F2");

            // 3. Payable: Credit Purchases - Supplier Payments
            var creditPurchases = await db.PurchaseHdrs
                .Where(x => x.BillType == "Credit" && !x.IsDeleted && x.FK_BranchId == branchId)
                .SumAsync(x => x.GrandTotal);

            var supplierPayments = await db.SupplierPaymentHdrs
                .Where(x => !x.IsDeleted && x.FK_BranchId == branchId)
                .SumAsync(x => x.Amount);

            chartEntity.Payable = (creditPurchases - supplierPayments).ToString("F2");

            // 4. Receivable: Credit Sales - Customer Receipts
            var creditSales = await db.SalesInvoiceHdrs
               .Where(x => x.BillType == "Credit" && !x.IsDeleted && x.FK_BranchId == branchId)
               .SumAsync(x => x.GrandTotal);

            var allCustomerReceipts = await db.CustomerReceiptHdrs
               .Where(x => !x.IsDeleted && x.FK_BranchId == branchId)
               .SumAsync(x => x.TotalAmount);

            chartEntity.Receivable = (creditSales - allCustomerReceipts).ToString("F2");

            // 5. Groupwise Sales List
            chartEntity.GroupwiseSalesList = await db.SalesInvoiceDtls
                .Include(x => x.SalesInvoiceHdr)
                .Include(x => x.Item).ThenInclude(x => x.Category)
                .Where(x => x.SalesInvoiceHdr.FK_BranchId == branchId && !x.IsDeleted && !x.SalesInvoiceHdr.IsDeleted)
                .GroupBy(x => x.Item.Category.Name ?? "Unknown")
                .Select(g => new ChartValue
                {
                    Head = g.Key ?? "Unknown",
                    Value = (double)g.Sum(x => x.LineTotal)
                })
                .ToListAsync();

            // 6. Groupwise Stock List
            // Note: Assuming Value = CurrentQty * SalesRate (from ItemMaster)
            chartEntity.GroupwiseStockList = await db.ItemStockBalances
                .Include(x => x.Item).ThenInclude(x => x.Category)
                .Where(x => x.FK_BranchId == branchId)
                .GroupBy(x => x.Item.Category.Name ?? "Unknown")
                .Select(g => new ChartValue
                {
                    Head = g.Key,
                    Value = (double)g.Sum(x => x.CurrentQty * (x.Item.SalesRate ?? 1))
                })
                .ToListAsync();

            // 7. Recent Sales List (Top 5)
            chartEntity.SalesList = await db.SalesInvoiceHdrs
                .Where(x => x.FK_BranchId == branchId && !x.IsDeleted)
                .OrderByDescending(x => x.SalesInvoiceId)
                .Take(10)
                .Select(x => new ChartListStatement
                {
                    InvoiceId = x.SalesInvoiceId,
                    InvType = "Sales",
                    InvNo = x.SalesInvoiceNo,
                    InvoiceDate = x.SalesInvoiceDate.HasValue ? x.SalesInvoiceDate.Value.ToString("yyyy-MM-dd") : "",
                    PartyName = x.CustomerName,
                    Amount = x.GrandTotal
                })
                .ToListAsync();

            // 8. Recent Purchase List (Top 5)
            chartEntity.PurchaseList = await db.PurchaseHdrs
                .Where(x => x.FK_BranchId == branchId && !x.IsDeleted)
                .OrderByDescending(x => x.PurchaseId)
                .Take(10)
                .Select(x => new ChartListStatement
                {
                    InvoiceId = x.PurchaseId,
                    InvType = "Purchase",
                    InvNo = x.PurchaseNo,
                    InvoiceDate = x.PurchaseDate.HasValue ? x.PurchaseDate.Value.ToString("yyyy-MM-dd") : "",
                    PartyName = x.SupplierName,
                    Amount = x.GrandTotal
                })
                .ToListAsync();

            // 9. Last 30 days — daily sales totals
            var fromDate = DateTime.Today.AddDays(-29);

            var rawDailySales = await db.SalesInvoiceHdrs
                .Where(x => x.FK_BranchId == branchId && !x.IsDeleted
                         && x.SalesInvoiceDate.HasValue
                         && x.SalesInvoiceDate >= fromDate)
                .GroupBy(x => x.SalesInvoiceDate!.Value.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(x => x.GrandTotal) })
                .OrderBy(x => x.Date)
                .ToListAsync();

            chartEntity.DailySalesList = rawDailySales
                .Select(x => new ChartValue { Head = x.Date.ToString("dd/MM"), Value = (double)x.Total })
                .ToList();

            // 10. Last 30 days — daily purchase totals
            var rawDailyPurchase = await db.PurchaseHdrs
                .Where(x => x.FK_BranchId == branchId && !x.IsDeleted
                         && x.PurchaseDate.HasValue
                         && x.PurchaseDate >= fromDate)
                .GroupBy(x => x.PurchaseDate!.Value.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(x => x.GrandTotal) })
                .OrderBy(x => x.Date)
                .ToListAsync();

            chartEntity.DailyPurchaseList = rawDailyPurchase
                .Select(x => new ChartValue { Head = x.Date.ToString("dd/MM"), Value = (double)x.Total })
                .ToList();

            return chartEntity;
        }
        catch (Exception) { return null; }
    }
    public async Task<List<CommonModel>> GetComboFill(string type, int? branchId)
    {
        await using var db = contextProvider.CreateDbContext();
        return type.ToLowerInvariant() switch
        {
            "customers" => await db.CustomerMasters.Select(c => new CommonModel { Id = c.CustomerId, Name = c.Name, Code = c.Code }).ToListAsync(),
            "suppliers" => await db.SupplierMasters.Select(s => new CommonModel { Id = s.SupplierId, Name = s.Name, Code = s.Code }).ToListAsync(),
            "unit" => await db.UnitMasters.Select(u => new CommonModel { Id = u.UnitId, Name = u.Name }).ToListAsync(),
            "product" => await db.ItemMasters.Select(p => new CommonModel { Id = p.ItemId, Name = p.ItemName, Code = p.ItemCode }).ToListAsync(),
            "munit" => await db.ItemMultiUnits.Where(x => x.FK_ItemId == branchId).Select(u => new CommonModel { Id = u.FK_UnitId, Name = u.Unit.Name }).ToListAsync(),
            "salesman" => await db.SalesManMasters.Select(s => new CommonModel { Id = s.SalesmanId, Name = s.Name }).ToListAsync(),
            "project" => await db.ProjectMasters.Select(p => new CommonModel { Id = p.ProjectId, Name = p.Name }).ToListAsync(),
            "purchaser" => await db.PurchaserMasters.Select(p => new CommonModel { Id = p.PurchaserId, Name = p.Name, Code = p.Code }).ToListAsync(),
            "account" => await db.AccountMasters.Where(x => x.AccountId != 0).Select(a => new CommonModel { Id = a.AccountId, Name = a.Name, Code = a.Code }).ToListAsync(),
            "branch" => (new List<CommonModel> { new CommonModel { Id = 0, Name = "All" } }).Concat(await db.BranchMasters.Select(b => new CommonModel { Id = b.BranchId, Name = b.Name }).ToListAsync()).ToList(),
            "store" => (new List<CommonModel> { new CommonModel { Id = 0, Name = "All" } }).Concat(await db.StoreMasters.Select(s => new CommonModel { Id = s.StoreId, Name = s.Name }).ToListAsync()).ToList(),
            "brand" => (new List<CommonModel> { new CommonModel { Id = 0, Name = "All" } }).Concat(await db.BrandMasters.Select(b => new CommonModel { Id = b.BrandId, Name = b.Name }).ToListAsync()).ToList(),
            "category" => (new List<CommonModel> { new CommonModel { Id = 0, Name = "All" } }).Concat(await db.CategoryMasters.Select(b => new CommonModel { Id = b.CategoryId, Name = b.Name }).ToListAsync()).ToList(),
            _ => new List<CommonModel>()
        };
    }
    public async Task<List<ExMasters>> GetMasters(string head, int? branchId)
    {
        await using var db = contextProvider.CreateDbContext();
        return head.ToLowerInvariant() switch
        {
            "product" => await db.ItemMasters.Select(e => new ExMasters
            {
                Id = e.ItemId,
                Name = e.ItemName,
                Code = e.ItemCode,
                IsSelected = false,
                Amount = e.SalesRate.toDecimal(),
                ExtraField = e.ItemNameArabic,
                Description = "", //pending for barcode from MultiBarcode
                CategoryId = e.FK_CategoryId,
                ItemImage = e.ItemImage
            }).ToListAsync(),
            "posproduct" => await db.ItemMasters.Select(e => new ExMasters
            {
                Id = e.ItemId,
                Name = e.ItemName,
                Code = e.ItemCode,
                IsSelected = false,
                Amount = e.InclusiveRate.toDecimal(),
                ExtraField = e.ItemNameArabic,
                Description = "", //pending for barcode from MultiBarcode
                CategoryId = e.FK_CategoryId,
                ItemImage = e.ItemImage
            }).ToListAsync(),
            "customer" => await db.CustomerMasters.Select(e => new ExMasters
            {
                Id = e.CustomerId,
                Name = e.Name,
                Code = e.Code,
                IsSelected = false,
                Amount = 0,
                ExtraField = e.NameArabic
            }).ToListAsync(),
            "purchaseorder" => await db.PurchaseOrderHdrs.Where(x => x.FK_BranchId == branchId).Select(e => new ExMasters
            {
                Id = e.PurchaseOrderId,
                Name = e.Supplier!.Name,
                Code = e.PurchaseOrderNo,
                IsSelected = false,
                Amount = e.GrandTotal,
                ExtraField = e.RefNo,
                Description = e.PurchaseOrderDate.Value.ToString("dd-MM-yyyy")
            }).OrderByDescending(x => x.Id).ToListAsync(),
            "purchase" => await db.PurchaseHdrs.Where(x => x.FK_BranchId == branchId).Select(e => new ExMasters
            {
                Id = e.PurchaseId,
                Name = e.Supplier!.Name,
                Code = e.PurchaseNo,
                IsSelected = false,
                Amount = e.GrandTotal,
                ExtraField = e.RefNo,
                Description = e.PurchaseDate.HasValue ? e.PurchaseDate.Value.ToString("dd-MM-yyyy") : ""
            }).OrderByDescending(x => x.Id).ToListAsync(),
            "salesquotation" => await db.SalesQuotationHdrs.Where(x => x.FK_BranchId == branchId).Select(e => new ExMasters
            {
                Id = e.SalesQuotationId,
                Name = e.Customer!.Name,
                Code = e.SalesQuotationNo,
                IsSelected = false,
                Amount = e.GrandTotal,
                ExtraField = e.Customer!.Code,
                Description = e.SalesQuotationDate.HasValue ? e.SalesQuotationDate.Value.ToString("dd-MM-yyyy") : ""
            }).OrderByDescending(x => x.Id).ToListAsync(),
            "salesman" => await db.SalesManMasters.Where(e => e.SalesmanId != 0).Select(e => new ExMasters
            {
                Id = e.SalesmanId,
                Name = e.Name,
                Code = "",
                IsSelected = false,
                Amount = 0,
                ExtraField = "",
                Description = ""
            }).ToListAsync(),
            _ => new List<ExMasters>()
        };
    }
    public async Task<int> GetNewCode(string head, int? branchId)
    {
        await using var db = contextProvider.CreateDbContext();
        return head.ToLowerInvariant() switch
        {
            "customer" => await db.CustomerMasters.AnyAsync() ? await db.CustomerMasters.MaxAsync(c => c.CustomerId) + 1 : 1,
            "product" => await db.ItemMasters.AnyAsync() ? await db.ItemMasters.MaxAsync(i => i.ItemId) + 1 : 1,
            "icv" => await db.InvoiceICVs.AnyAsync() ? await db.InvoiceICVs.MaxAsync(i => i.InvICVNo) + 1 : 1,
            _ => 0
        };
    }
    public async Task<decimal> GetValueMethod(string head, int? val)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var data = head switch
            {
                "vatFromItemId" => await db.ItemMasters.Where(x => x.ItemId == val).Select(x => x.Vat.Percentage).FirstOrDefaultAsync(),
                _ => 0.00m
            };
            return data ?? 0;
        }
        catch (Exception ex)
        {
            string message = ex.Message;
            return 0M;
        }
    }
    public string GetZatcaQRCodeValFirstPhase(TransactionHdr Invoice, CompanyDetails CompanyData, DateTime? invoiceDate)
    {
        TimeSpan currentTime = DateTime.Now.TimeOfDay;
        DateTime? updatedInvoiceDate = invoiceDate.Value.Date.Add(currentTime);
        Invoice.InvoiceDate = updatedInvoiceDate.ToString();
        string sellername = CompanyData.Name;
        string vatregistration = CompanyData.VatNo ?? "312345678100003";
        string timestamp = updatedInvoiceDate.Value.ToString("yyyy-MM-dd") + "T" + updatedInvoiceDate.Value.ToString("HH:mm:ss") + "Z";
        string invoiceamount = Invoice.NetAmount.ToString();
        string vatamount = Invoice.TaxTotal.ToString();
        return GetBase64(sellername, vatregistration, timestamp, invoiceamount, vatamount);
    }
    private byte[] GetTlvValue(string tagnum, string tagvalue)
    {
        byte[] tagnumBytes = { byte.Parse(tagnum) };
        byte[] tagvalueBytes = Encoding.UTF8.GetBytes(tagvalue);
        byte[] tagvalueLength = { (byte)tagvalueBytes.Length };

        byte[] tlvValue = tagnumBytes.Concat(tagvalueLength).Concat(tagvalueBytes).ToArray();
        return tlvValue;
    }
    private string GetBase64(string sellername, string vatregistration, string timestamp, string invoiceamount, string vatamount)
    {
        string ltr = ((char)0x200E).ToString();
        byte[] seller = GetTlvValue("1", sellername);
        byte[] vatno = GetTlvValue("2", vatregistration);
        byte[] time = GetTlvValue("3", timestamp);
        byte[] invamt = GetTlvValue("4", invoiceamount);
        byte[] vatamt = GetTlvValue("5", vatamount);
        byte[] result = seller.Concat(vatno).Concat(time).Concat(invamt).Concat(vatamt).ToArray();
        string output = Convert.ToBase64String(result);
        return output;
    }

}
