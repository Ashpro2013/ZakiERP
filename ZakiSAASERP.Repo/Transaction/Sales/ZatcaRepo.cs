using ZakiSAASERP.Interface.Transaction.Sales;
using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.ViewModel;
using AshproStringExtension;

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
        return await db.InvoiceICVs.CountAsync(x => x.ICVStatus == false && x.ReUploaded == false);
    }
    public async Task<PendingIcvData> GetPendingIcvDataAsync(ICVReqDTO request)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            int firstCount = 0;
            int secondCount = 0;
            int thirdCount = 0;

            // base query
            var query = db.InvoiceICVs.Where(x => x.InvICVDate >= request.FrmDate && x.InvICVDate <= request.ToDate).AsQueryable();
            if (request.BranchId != 0)
                query = query.Where(x => x.FK_BranchId == request.BranchId);

            switch (request.Type)
            {
                case 1:
                    firstCount = await query.CountAsync(x =>
                        x.ICVStatus == false &&
                        x.ReUploaded == false &&
                        (x.VoucherType == "Standard Invoice" || x.VoucherType == "Simplified Invoice"));

                    secondCount = await query.CountAsync(x =>
                        x.ICVStatus == false &&
                        x.ReUploaded == false &&
                        (x.VoucherType == "Standard Credit Note" || x.VoucherType == "Simplified Credit Note"));

                    thirdCount = await query.CountAsync(x =>
                        x.ICVStatus == false &&
                        x.ReUploaded == false &&
                        (x.VoucherType == "Standard Debit Note" || x.VoucherType == "Simplified Debit Note"));
                    break;

                case 2:
                    firstCount = await query.CountAsync(x =>
                        x.ICVStatus == false &&
                        x.ReUploaded == false &&
                        x.VoucherType == "Standard Invoice");

                    secondCount = await query.CountAsync(x =>
                        x.ICVStatus == false &&
                        x.ReUploaded == false &&
                        x.VoucherType == "Simplified Invoice");

                    thirdCount = 0;
                    break;

                case 3:
                    firstCount = await query.CountAsync(x =>
                        x.ICVStatus == false &&
                        x.ReUploaded == false &&
                        x.VoucherType == "Standard Credit Invoice");

                    secondCount = await query.CountAsync(x =>
                        x.ICVStatus == false &&
                        x.ReUploaded == false &&
                        x.VoucherType == "Simplified Credit Note");

                    thirdCount = 0;
                    break;

                case 4:
                    firstCount = await query.CountAsync(x =>
                        x.DetailedStatus == "PASS" &&
                        x.VoucherType == "Standard Invoice");

                    secondCount = await query.CountAsync(x =>
                        x.DetailedStatus == "WARNING" &&
                        x.VoucherType == "Standard Invoice");

                    thirdCount = await query.CountAsync(x =>
                        x.ICVStatus == false &&
                        x.ReUploaded == false &&
                        x.VoucherType == "Standard Invoice");
                    break;

                case 5:
                    firstCount = await query.CountAsync(x =>
                        x.DetailedStatus == "REPORTED" &&
                        x.VoucherType == "Simplified Invoice");

                    secondCount = await query.CountAsync(x =>
                        x.DetailedStatus == "WARNING" &&
                        x.VoucherType == "Simplified Invoice");

                    thirdCount = await query.CountAsync(x =>
                        x.ICVStatus == false &&
                        x.ReUploaded == false &&
                        x.VoucherType == "Simplified Invoice");
                    break;

                case 6:
                    firstCount = await query.CountAsync(x =>
                        x.DetailedStatus == "REPORTED" &&
                        x.VoucherType == "Simplified Credit Note");

                    secondCount = await query.CountAsync(x =>
                        x.DetailedStatus == "WARNING" &&
                        x.VoucherType == "Simplified Credit Note");

                    thirdCount = await query.CountAsync(x =>
                        x.ICVStatus == false &&
                        x.ReUploaded == false &&
                        x.VoucherType == "Simplified Credit Note");
                    break;

                case 7:
                    firstCount = await query.CountAsync(x =>
                        x.DetailedStatus == "PASS" &&
                        x.VoucherType == "Standard Credit Note");

                    secondCount = await query.CountAsync(x =>
                        x.DetailedStatus == "WARNING" &&
                        x.VoucherType == "Standard Credit Note");

                    thirdCount = await query.CountAsync(x =>
                        x.ICVStatus == false &&
                        x.ReUploaded == false &&
                        x.VoucherType == "Standard Credit Note");
                    break;
            }
            return new PendingIcvData(firstCount, secondCount, thirdCount);
        }
        catch (Exception)
        {

            throw;
        }
    }
    public async Task<IcvPendingPDTO> GetPendingIcvListAsync(IcvPendReqDTO req)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var query = db.InvoiceICVs.Where(x => x.InvICVDate >= req.reqParameter.FrmDate && x.InvICVDate <= req.reqParameter.ToDate).AsQueryable();

            if (req.PaginationDTO.BranchId != 0)
                query = query.Where(x => x.FK_BranchId == req.PaginationDTO.BranchId);


            IQueryable<ExInvoiceICV> result = Enumerable.Empty<ExInvoiceICV>().AsQueryable();

            switch (req.reqParameter.Type)
            {
                case 1:
                    result = query
                        .Where(x => x.DetailedStatus == "PASS" &&
                                    x.VoucherType == "Standard Invoice")
                        .Select(x => new ExInvoiceICV
                        {
                            InvoiceICVId = x.InvoiceICVId,
                            InvICVNo = x.InvICVNo,
                            VoucherId = x.VoucherId,
                            VoucherNo = x.VoucherNo ?? "",
                            VoucherType = x.VoucherType,
                            InvICVDate = x.InvICVDate,
                            DetailedStatus = x.DetailedStatus,
                            Message = x.ICVMessage
                        });
                    break;

                case 2:
                    result = query
                        .Where(x => x.DetailedStatus == "WARNING" &&
                                    x.VoucherType == "Standard Invoice")
                        .Select(x => new ExInvoiceICV
                        {
                            InvoiceICVId = x.InvoiceICVId,
                            InvICVNo = x.InvICVNo,
                            VoucherId = x.VoucherId,
                            VoucherNo = x.VoucherNo ?? "",
                            VoucherType = x.VoucherType,
                            InvICVDate = x.InvICVDate,
                            DetailedStatus = x.DetailedStatus,
                            Message = x.ICVMessage ?? (x.ICVInfoMessages)
                        });
                    break;

                case 3:
                    result = query
                        .Where(x => x.ICVStatus == false &&
                                    x.ReUploaded == false &&
                                    x.VoucherType == "Standard Invoice")
                        .Select(x => new ExInvoiceICV
                        {
                            InvoiceICVId = x.InvoiceICVId,
                            InvICVNo = x.InvICVNo,
                            VoucherId = x.VoucherId,
                            VoucherNo = x.VoucherNo ?? "",
                            VoucherType = x.VoucherType,
                            InvICVDate = x.InvICVDate,
                            DetailedStatus = x.DetailedStatus,
                            Message = x.ICVMessage ?? x.ICVInfoMessages
                        });
                    break;

                case 4:
                    result = query
                        .Where(x => x.DetailedStatus == "REPORTED" &&
                                    x.VoucherType == "Simplified Invoice")
                        .Select(x => new ExInvoiceICV
                        {
                            InvoiceICVId = x.InvoiceICVId,
                            InvICVNo = x.InvICVNo,
                            VoucherId = x.VoucherId,
                            VoucherNo = x.VoucherNo ?? "",
                            VoucherType = x.VoucherType,
                            InvICVDate = x.InvICVDate,
                            DetailedStatus = x.DetailedStatus,
                            Message = x.ICVMessage ?? x.ICVInfoMessages
                        });
                    break;

                case 5:
                    result = query
                        .Where(x => x.DetailedStatus == "WARNING" &&
                                    x.VoucherType == "Simplified Invoice")
                        .Select(x => new ExInvoiceICV
                        {
                            InvoiceICVId = x.InvoiceICVId,
                            InvICVNo = x.InvICVNo,
                            VoucherId = x.VoucherId,
                            VoucherNo = x.VoucherNo ?? "",
                            VoucherType = x.VoucherType,
                            InvICVDate = x.InvICVDate,
                            DetailedStatus = x.DetailedStatus,
                            Message = x.ICVMessage ?? x.ICVInfoMessages
                        });
                    break;

                case 6:
                    result = query
                        .Where(x => x.ICVStatus == false &&
                                    x.ReUploaded == false &&
                                    x.VoucherType == "Simplified Invoice")
                        .Select(x => new ExInvoiceICV
                        {
                            InvoiceICVId = x.InvoiceICVId,
                            InvICVNo = x.InvICVNo,
                            VoucherId = x.VoucherId,
                            VoucherNo = x.VoucherNo ?? "",
                            VoucherType = x.VoucherType,
                            InvICVDate = x.InvICVDate,
                            DetailedStatus = x.DetailedStatus,
                            Message = x.ICVMessage ?? x.ICVInfoMessages
                        });
                    break;

                case 7:
                    result = query
                        .Where(x => x.DetailedStatus == "PASS" &&
                                    x.VoucherType == "Standard Credit Invoice")
                        .Select(x => new ExInvoiceICV
                        {
                            InvoiceICVId = x.InvoiceICVId,
                            InvICVNo = x.InvICVNo,
                            VoucherId = x.VoucherId,
                            VoucherNo = x.VoucherNo ?? "",
                            VoucherType = x.VoucherType,
                            InvICVDate = x.InvICVDate,
                            DetailedStatus = x.DetailedStatus,
                            Message = x.ICVMessage ?? x.ICVInfoMessages
                        });
                    break;

                case 8:
                    result = query
                        .Where(x => x.DetailedStatus == "WARNING" &&
                                    x.VoucherType == "Standard Credit Invoice")
                        .Select(x => new ExInvoiceICV
                        {
                            InvoiceICVId = x.InvoiceICVId,
                            InvICVNo = x.InvICVNo,
                            VoucherId = x.VoucherId,
                            VoucherNo = x.VoucherNo ?? "",
                            VoucherType = x.VoucherType,
                            InvICVDate = x.InvICVDate,
                            DetailedStatus = x.DetailedStatus,
                            Message = x.ICVMessage ?? x.ICVInfoMessages
                        });
                    break;

                case 9:
                    result = query
                        .Where(x => x.ICVStatus == false &&
                                    x.ReUploaded == false &&
                                    x.VoucherType == "Standard Credit Note")
                        .Select(x => new ExInvoiceICV
                        {
                            InvoiceICVId = x.InvoiceICVId,
                            InvICVNo = x.InvICVNo,
                            VoucherId = x.VoucherId,
                            VoucherNo = x.VoucherNo ?? "",
                            VoucherType = x.VoucherType,
                            InvICVDate = x.InvICVDate,
                            DetailedStatus = x.DetailedStatus,
                            Message = x.ICVMessage ?? x.ICVInfoMessages
                        });
                    break;

                case 10:
                    result = query
                        .Where(x => x.DetailedStatus == "REPORTED" &&
                                    x.VoucherType == "Simplified Credit Note")
                        .Select(x => new ExInvoiceICV
                        {
                            InvoiceICVId = x.InvoiceICVId,
                            InvICVNo = x.InvICVNo,
                            VoucherId = x.VoucherId,
                            VoucherNo = x.VoucherNo ?? "",
                            VoucherType = x.VoucherType,
                            InvICVDate = x.InvICVDate,
                            DetailedStatus = x.DetailedStatus,
                            Message = x.ICVMessage ?? x.ICVInfoMessages
                        });
                    break;

                case 11:
                    result = query
                        .Where(x => x.DetailedStatus == "WARNING" &&
                                    x.VoucherType == "Simplified Credit Note")
                        .Select(x => new ExInvoiceICV
                        {
                            InvoiceICVId = x.InvoiceICVId,
                            InvICVNo = x.InvICVNo,
                            VoucherId = x.VoucherId,
                            VoucherNo = x.VoucherNo ?? "",
                            VoucherType = x.VoucherType,
                            InvICVDate = x.InvICVDate,
                            DetailedStatus = x.DetailedStatus,
                            Message = x.ICVMessage ?? x.ICVInfoMessages
                        });
                    break;

                case 12:
                    result = query
                        .Where(x => x.ICVStatus == false &&
                                    x.ReUploaded == false &&
                                    x.VoucherType == "Simplified Credit Note")
                        .Select(x => new ExInvoiceICV
                        {
                            InvoiceICVId = x.InvoiceICVId,
                            InvICVNo = x.InvICVNo,
                            VoucherId = x.VoucherId,
                            VoucherNo = x.VoucherNo ?? "",
                            VoucherType = x.VoucherType,
                            InvICVDate = x.InvICVDate,
                            DetailedStatus = x.DetailedStatus,
                            Message = x.ICVMessage ?? x.ICVInfoMessages
                        });
                    break;
            }
            if (!string.IsNullOrWhiteSpace(req.PaginationDTO.search))
            {
                var term = req.PaginationDTO.search.ToLower();
                result = result.Where(x => x.InvICVNo == term.ToInt32() || (x.VoucherNo != null && x.VoucherNo.ToLower().Contains(term)));
            }
            int count = result.Count();
            var list = result.Skip((req.PaginationDTO.page - 1) * req.PaginationDTO.pageSize).Take(req.PaginationDTO.pageSize).ToList();
            return new IcvPendingPDTO(list, count);
        }
        catch (Exception)
        {

            throw;
        }
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



