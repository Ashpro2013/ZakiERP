using ZakiSAASERP.Interface.Transaction.Sales;
using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.ViewModel;
using AshproStringExtension;
using System.Linq.Expressions;

namespace ZakiSAASERP.Repo.Transaction.Sales;

public class ZatcaRepo : IZatcaRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly IZatcaUploadService _uploadService;
    private readonly IMasterValueRepo _masterValue;

    public ZatcaRepo(IDbContextProvider contextProvider, IZatcaUploadService uploadService, IMasterValueRepo masterValue)
    {
        _contextProvider = contextProvider;
        _uploadService = uploadService;
        _masterValue = masterValue;
    }

    public async Task<OutputResponse> AddNewCSR(CSRResponse dto)
    {
        var mVal = new MasterValue
        {
            Name = dto.ZatcaCSR,
            Description = dto.CSRSecret,
            MasterType = "CSRDetails",
            FieldOne = dto.CSIDToken,
            FieldTwo = dto.CSIDSecret,
            FieldThree = dto.OTP,
            FieldFour = dto.PreviousHash,
            BranchId = dto.BranchId
        };
        return await _masterValue.AddAsync(mVal);
    }
    public async Task<CSRResponse> GenerateCSR(CSRRequest request) => await _uploadService.GenerateCSR(request);
    public async Task<CSRResponse> GetCSR(int BranchId)
    {
        var mValue = await _masterValue.GetAsync("CSRDetails", BranchId);
        if (mValue == null)
        {
            return new CSRResponse { BranchId = BranchId };
        }
        return new CSRResponse
        {
            BranchId = BranchId,
            ZatcaCSR = mValue.Name,
            CSRSecret = mValue.Description,
            CSIDToken = mValue.FieldOne,
            CSIDSecret = mValue.FieldTwo,
            OTP = mValue.FieldThree,
            PreviousHash = mValue.FieldFour
        };
    }
    public async Task<int> GetFailedCount()
    {
        await using var db = _contextProvider.CreateDbContext();
        return await db.InvoiceICVs
     .Where(x => x.ICVStatus == false && x.ReUploaded == false)
     .Select(x => new
     {
         x.VoucherId,
         x.VoucherType
     })
     .Distinct()
     .CountAsync();
    }

public async Task<PendingIcvData> GetPendingIcvDataAsync(ICVReqDTO request)
{
    await using var db = _contextProvider.CreateDbContext();

    // Inner method: Count distinct Invoice Numbers
    async Task<int> GetDistinctInvoiceCountAsync(
        IQueryable<InvoiceICV> source,
        Expression<Func<InvoiceICV, bool>> predicate)
    {
        return await source
            .Where(predicate)
            .Select(x => new
            {
                x.VoucherId,
                x.VoucherType
            })
            .Distinct()
            .CountAsync();
    }

    int firstCount = 0;
    int secondCount = 0;
    int thirdCount = 0;

    // Base query
    var query = db.InvoiceICVs
        .AsNoTracking()
        .Where(x =>
            x.InvICVDate >= request.FrmDate &&
            x.InvICVDate <= request.ToDate);

    // Branch filter
    if (request.BranchId != 0)
    {
        query = query.Where(x => x.FK_BranchId == request.BranchId);
    }

    switch (request.Type)
    {
        case 1:

            firstCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.ICVStatus == false &&
                x.ReUploaded == false &&
                (x.VoucherType == "Standard Invoice" ||
                 x.VoucherType == "Simplified Invoice"));

            secondCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.ICVStatus == false &&
                x.ReUploaded == false &&
                (x.VoucherType == "Standard Credit Note" ||
                 x.VoucherType == "Simplified Credit Note"));

            thirdCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.ICVStatus == false &&
                x.ReUploaded == false &&
                (x.VoucherType == "Standard Debit Note" ||
                 x.VoucherType == "Simplified Debit Note"));

            break;

        case 2:

            firstCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.ICVStatus == false &&
                x.ReUploaded == false &&
                x.VoucherType == "Standard Invoice");

            secondCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.ICVStatus == false &&
                x.ReUploaded == false &&
                x.VoucherType == "Simplified Invoice");

            break;

        case 3:

            firstCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.ICVStatus == false &&
                x.ReUploaded == false &&
                x.VoucherType == "Standard Credit Invoice");

            secondCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.ICVStatus == false &&
                x.ReUploaded == false &&
                x.VoucherType == "Simplified Credit Note");

            break;

        case 4:

            firstCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.DetailedStatus == "PASS" &&
                x.VoucherType == "Standard Invoice");

            secondCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.DetailedStatus == "WARNING" &&
                x.VoucherType == "Standard Invoice");

            thirdCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.ICVStatus == false &&
                x.ReUploaded == false &&
                x.VoucherType == "Standard Invoice");

            break;

        case 5:

            firstCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.DetailedStatus == "REPORTED" &&
                x.VoucherType == "Simplified Invoice");

            secondCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.DetailedStatus == "WARNING" &&
                x.VoucherType == "Simplified Invoice");

            thirdCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.ICVStatus == false &&
                x.ReUploaded == false &&
                x.VoucherType == "Simplified Invoice");

            break;

        case 6:

            firstCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.DetailedStatus == "REPORTED" &&
                x.VoucherType == "Simplified Credit Note");

            secondCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.DetailedStatus == "WARNING" &&
                x.VoucherType == "Simplified Credit Note");

            thirdCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.ICVStatus == false &&
                x.ReUploaded == false &&
                x.VoucherType == "Simplified Credit Note");

            break;

        case 7:

            firstCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.DetailedStatus == "PASS" &&
                x.VoucherType == "Standard Credit Note");

            secondCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.DetailedStatus == "WARNING" &&
                x.VoucherType == "Standard Credit Note");

            thirdCount = await GetDistinctInvoiceCountAsync(query, x =>
                x.ICVStatus == false &&
                x.ReUploaded == false &&
                x.VoucherType == "Standard Credit Note");

            break;
    }

    return new PendingIcvData(firstCount, secondCount, thirdCount);
}

    public async Task<IcvPendingPDTO> GetPendingIcvListAsync(IcvPendReqDTO req)
    {
        await using var db = _contextProvider.CreateDbContext();

        // Base query
        var query = db.InvoiceICVs
            .AsNoTracking()
            .Where(x =>
                x.InvICVDate >= req.reqParameter.FrmDate &&
                x.InvICVDate <= req.reqParameter.ToDate);

        // Branch filter
        if (req.PaginationDTO.BranchId != 0)
        {
            query = query.Where(x =>
                x.FK_BranchId == req.PaginationDTO.BranchId);
        }

        // Filter by request type
        IQueryable<InvoiceICV> filteredQuery =
            query.Where(x => false);

        switch (req.reqParameter.Type)
        {
            case 1:
                filteredQuery = query.Where(x =>
                    x.DetailedStatus == "PASS" &&
                    x.VoucherType == "Standard Invoice");
                break;

            case 2:
                filteredQuery = query.Where(x =>
                    x.DetailedStatus == "WARNING" &&
                    x.VoucherType == "Standard Invoice");
                break;

            case 3:
                filteredQuery = query.Where(x =>
                    x.ICVStatus == false &&
                    x.ReUploaded == false &&
                    x.VoucherType == "Standard Invoice");
                break;

            case 4:
                filteredQuery = query.Where(x =>
                    x.DetailedStatus == "REPORTED" &&
                    x.VoucherType == "Simplified Invoice");
                break;

            case 5:
                filteredQuery = query.Where(x =>
                    x.DetailedStatus == "WARNING" &&
                    x.VoucherType == "Simplified Invoice");
                break;

            case 6:
                filteredQuery = query.Where(x =>
                    x.ICVStatus == false &&
                    x.ReUploaded == false &&
                    x.VoucherType == "Simplified Invoice");
                break;

            case 7:
                filteredQuery = query.Where(x =>
                    x.DetailedStatus == "PASS" &&
                    x.VoucherType == "Standard Credit Invoice");
                break;

            case 8:
                filteredQuery = query.Where(x =>
                    x.DetailedStatus == "WARNING" &&
                    x.VoucherType == "Standard Credit Invoice");
                break;

            case 9:
                filteredQuery = query.Where(x =>
                    x.ICVStatus == false &&
                    x.ReUploaded == false &&
                    x.VoucherType == "Standard Credit Note");
                break;

            case 10:
                filteredQuery = query.Where(x =>
                    x.DetailedStatus == "REPORTED" &&
                    x.VoucherType == "Simplified Credit Note");
                break;

            case 11:
                filteredQuery = query.Where(x =>
                    x.DetailedStatus == "WARNING" &&
                    x.VoucherType == "Simplified Credit Note");
                break;

            case 12:
                filteredQuery = query.Where(x =>
                    x.ICVStatus == false &&
                    x.ReUploaded == false &&
                    x.VoucherType == "Simplified Credit Note");
                break;
        }

        // Search filter
        if (!string.IsNullOrWhiteSpace(req.PaginationDTO.search))
        {
            var term = req.PaginationDTO.search.ToLower();
            var invoiceNo = term.ToInt32();

            filteredQuery = filteredQuery.Where(x =>
                x.InvICVNo == invoiceNo ||
                (x.VoucherNo != null &&
                 x.VoucherNo.ToLower().Contains(term)));
        }

        // Distinct by VoucherId using NOT EXISTS
        // Retain the record with the highest InvoiceICVId
        var candidates = filteredQuery;

        var distinctQuery = candidates.Where(x =>
            !candidates.Any(y =>
                y.VoucherId == x.VoucherId &&
                y.InvoiceICVId > x.InvoiceICVId));

        // Total distinct count
        int count = await distinctQuery.CountAsync();

        // Project and paginate
        var list = await distinctQuery
            .OrderByDescending(x => x.InvoiceICVId)
            .Skip((req.PaginationDTO.page - 1) *
                  req.PaginationDTO.pageSize)
            .Take(req.PaginationDTO.pageSize)
            .Select(x => new ExInvoiceICV
            {
                InvoiceICVId = x.InvoiceICVId,
                InvICVNo = x.InvICVNo,
                VoucherId = x.VoucherId,
                VoucherNo = x.VoucherNo ?? "",
                VoucherType = x.VoucherType,
                InvICVDate = x.InvICVDate,
                DetailedStatus = x.DetailedStatus,

                Message = req.reqParameter.Type == 1
                    ? x.ICVMessage
                    : (x.ICVMessage ?? x.ICVInfoMessages)
            })
            .ToListAsync();

        return new IcvPendingPDTO(list, count);
    }

    public async Task ReUploadAll(List<ExInvoiceICV> entInvoices,  CompanyDetails company = null)
    {
        foreach (var item in entInvoices)
        {
            await ReUploadSelected(item, company);
        }
    }
    public async Task<OutputResponse> ReUploadSelected(ExInvoiceICV invoiceICV,  CompanyDetails company = null)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            if (invoiceICV.VoucherType.Contains("Invoice"))
            {
                var entsaleshdr = await db.SalesInvoiceHdrs.FirstOrDefaultAsync(x => 
                    (invoiceICV.VoucherId != 0 && x.SalesInvoiceId == invoiceICV.VoucherId) ||
                    (!string.IsNullOrEmpty(invoiceICV.VoucherNo) && x.SalesInvoiceNo == invoiceICV.VoucherNo) ||
                    x.SalesInvoiceId == invoiceICV.InvoiceICVId);
                var entTransactions = await db.SalesInvoiceDtls.Where(x => x.FK_SalesInvoiceHdrId == entsaleshdr.SalesInvoiceId).ToListAsync();
                var lineDiscount = entTransactions.Sum(x => x.DiscountAmnt);
                if (lineDiscount > 0) { entsaleshdr.AdditionalDiscAmnt = 0; }
                return await UploadSales(db, entsaleshdr, entTransactions, company);
            }
            else
            {
                var entSalesRtnMaster = await db.SalesInvoiceRtnHdrs.FirstOrDefaultAsync(x => 
                    (invoiceICV.VoucherId != 0 && x.SalesInvoiceRtnId == invoiceICV.VoucherId) ||
                    (!string.IsNullOrEmpty(invoiceICV.VoucherNo) && x.SalesInvoiceRtnNo == invoiceICV.VoucherNo) ||
                    x.SalesInvoiceRtnId == invoiceICV.InvoiceICVId);
                var entTransactions = await db.SalesInvoiceRtnDtls.Where(x => x.FK_SalesInvoiceRtnHdrId == entSalesRtnMaster.SalesInvoiceRtnId).ToListAsync();
                return await UploadSalesReturn(db, entSalesRtnMaster, entTransactions, company);
            }
        }
        catch (Exception)
        {

            throw;
        }
    }
    private List<TransactionDtls> GetTransactionsFromSalesInvoiceDtls(List<SalesInvoiceDtls> dtls)
    {
        var transactions = new List<TransactionDtls>();
        foreach (var item in dtls)
        {
            var transaction = new TransactionDtls
            {
                Barcode = item.Barcode,
                DeliveredQty = item.DeliveredQty,
                DiscountAmnt = item.DiscountAmnt,
                 DiscountPerc = item.DiscountPerc,
                DtlsId = item.SalesInvoiceDtlsId,
                ExRate = item.ExRate,
                FK_DeliveryNoteDtlsId = item.FK_DeliveryNoteDtlsId,
                 FK_ItemId = item.FK_ItemId,
                FK_SalesInvoiceDtlsId = item.SalesInvoiceDtlsId,
                ItemName = item.ItemName,
                LineTotal = item.LineTotal,
                TaxPerc = item.TaxPerc,
                FK_UnitId = item.FK_UnitId,
                GrossAmount = item.GrossAmount,
                 HdrId = item.FK_SalesInvoiceHdrId,
                InRate = item.InRate,
                IsDeleted = item.IsDeleted,
                QtyPerUnit = item.QtyPerUnit,
                Quantity = item.Quantity,
                TaxableAmnt = item.TaxableAmnt,
                TaxAmount = item.TaxAmount
            };
            transactions.Add(transaction);
        }
        return transactions;
    }
    private async Task<OutputResponse> UploadSales(TenantDBContext db, SalesInvoiceHdr entsaleshdr, List<SalesInvoiceDtls> dtls,  CompanyDetails company = null)
    {
        try
        {
            string message = string.Empty;
            TransactionHdr tmaster = entsaleshdr.CopyTo<TransactionHdr>();
            tmaster.HdrNo = entsaleshdr.SalesInvoiceNo;
            tmaster.HdrId=entsaleshdr.SalesInvoiceId;
            tmaster.InvoiceDate = entsaleshdr.SalesInvoiceDate.ToString();
            tmaster.AdditionalDiscAmnt = entsaleshdr.AdditionalDiscAmnt;
            var entTransactions = GetTransactionsFromSalesInvoiceDtls(dtls);
            CustomerMaster account = await db.CustomerMasters.FirstOrDefaultAsync(x => x.CustomerId == entsaleshdr.FK_CustomerId);
            if (!string.IsNullOrEmpty(account.VatNo))
            {
                var result = await _uploadService.UploadZatcaInvoice(tmaster, entTransactions, company, account, "Standard Invoice", true, false);
                if (result.IsUploaded)
                {
                    await db.SalesInvoiceHdrs.Where(x => x.SalesInvoiceId == entsaleshdr.SalesInvoiceId).ExecuteUpdateAsync(x => x.SetProperty(e => e.InvoiceQRCode, result.QrCodeString));
                    await db.InvoiceICVs.Where(x => x.VoucherType == "Standard Invoice" && x.VoucherId == entsaleshdr.SalesInvoiceId).ExecuteUpdateAsync(x => x.SetProperty(e => e.ReUploaded, true));
                }
                else
                    message = result.ErrorMessage;
            }
            else
            {
                var result = await _uploadService.UploadZatcaInvoice(tmaster, entTransactions, company, null, "Simplified Invoice", true, true);
                if (result.IsUploaded)
                {
                    await db.SalesInvoiceHdrs.Where(x => x.SalesInvoiceId == entsaleshdr.SalesInvoiceId).ExecuteUpdateAsync(x => x.SetProperty(e => e.InvoiceQRCode, result.QrCodeString));
                    await db.InvoiceICVs.Where(x => x.VoucherType == "Simplified Invoice" && x.VoucherId == entsaleshdr.SalesInvoiceId).ExecuteUpdateAsync(x => x.SetProperty(e => e.ReUploaded, true));
                }
                else
                {
                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        message = result.ErrorMessage;
                    }
                }
            }
            return new OutputResponse(0, message);
        }
        catch (Exception ex)
        {
            return new OutputResponse(0, ex.Message);
        }
    }
    private async Task<OutputResponse> UploadSalesReturn(TenantDBContext db, SalesInvoiceRtnHdr entSalesRtnMaster, List<SalesInvoiceRtnDtls> dtls,  CompanyDetails company = null)
    {
        string Message = string.Empty;
        TransactionHdr tmaster = entSalesRtnMaster.CopyTo<TransactionHdr>();
        var entTransactions = dtls.Cast<TransactionDtls>().ToList();
        var mVal = await _masterValue.GetAsync("CSRDetails", entSalesRtnMaster.FK_BranchId);
        CustomerMaster account = await db.CustomerMasters.FirstOrDefaultAsync(x => x.CustomerId == entSalesRtnMaster.FK_CustomerId);
        if (!string.IsNullOrEmpty(account.VatNo))
        {
            var result = await _uploadService.UploadZatcaInvoice(tmaster, entTransactions, company, account, "Standard Credit Note", true, false);
            if (result.IsUploaded)
            {
                await db.InvoiceICVs.Where(x => x.VoucherType == "Standard Credit Note" && x.VoucherId == entSalesRtnMaster.SalesInvoiceRtnId).ExecuteUpdateAsync(x => x.SetProperty(e => e.ReUploaded, true));
            }
            else
                Message = result.ErrorMessage;
        }
        else
        {
            var result = await _uploadService.UploadZatcaInvoice(tmaster, entTransactions, company, null, "Simplified Credit Note", true, true);
            if (result.IsUploaded)
            {
                await db.InvoiceICVs.Where(x => x.VoucherType == "Simplified Credit Note" && x.VoucherId == entSalesRtnMaster.SalesInvoiceRtnId).ExecuteUpdateAsync(x => x.SetProperty(e => e.ReUploaded, true));
            }
            else
                Message = result.ErrorMessage;
        }
        return new OutputResponse(0, Message);
    }
}



