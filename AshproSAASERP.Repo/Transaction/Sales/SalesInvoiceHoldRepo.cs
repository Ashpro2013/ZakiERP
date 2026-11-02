using AshproSAASERP.Interface;
using AshproSAASERP.Interface.Transaction.Sales;
using AshproSAASERP.Models.DTO;
using AshproSAASERP.Models.Transactions.Sales;
using AshproSAASERP.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace AshproSAASERP.Repo.Transaction.Sales;

public class SalesInvoiceHoldRepo : ISalesInvoiceHoldRepo
{
    private readonly IDbContextProvider _contextProvider;

    public SalesInvoiceHoldRepo(IDbContextProvider contextProvider)
    {
        _contextProvider = contextProvider;
    }

    public async Task<OutputResponse> CreateAsync(SalesInvoiceHoldDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.HoldHdr;
            var dtls = entity.HoldDtls ?? new List<TransactionDtls>();

            if (string.IsNullOrWhiteSpace(hdr.SalesInvoiceNo))
            {
                hdr.SalesInvoiceNo = "HOLD-" + DateTime.Now.ToString("yyMMddHHmmss");
            }

            if (hdr.FK_BranchId == 0)
            {
                var branch = await db.BranchMasters.FirstOrDefaultAsync();
                if (branch != null) hdr.FK_BranchId = branch.BranchId;
            }

            if (hdr.FK_CustomerId == 0)
            {
                var cust = await db.CustomerMasters.FirstOrDefaultAsync(x => x.Name.Contains("Cash")) ?? await db.CustomerMasters.FirstOrDefaultAsync();
                if (cust != null) hdr.FK_CustomerId = cust.CustomerId;
            }

            if (hdr.FK_StoreId == 0)
            {
                var store = await db.StoreMasters.FirstOrDefaultAsync();
                if (store != null) hdr.FK_StoreId = store.StoreId;
            }

            if (hdr.FK_SalesManId == 0)
            {
                var salesman = await db.SalesManMasters.FirstOrDefaultAsync();
                if (salesman != null) hdr.FK_SalesManId = salesman.SalesmanId;
            }

            if (hdr.FK_UserId == 0)
            {
                var user = await db.UserDetails.FirstOrDefaultAsync();
                if (user != null) hdr.FK_UserId = user.UserId;
            }

            if (hdr.FK_FinancialYearId == 0)
            {
                var fy = await db.FinancialYears.FirstOrDefaultAsync();
                if (fy != null) hdr.FK_FinancialYearId = fy.FinancialYearId;
            }

            if (!hdr.SalesInvoiceDate.HasValue) hdr.SalesInvoiceDate = DateTime.Now;
            if (!hdr.EntryDate.HasValue) hdr.EntryDate = DateTime.Now;

            hdr.SalesInvoiceHoldHdrId = 0;
            hdr.Branch = null;
            hdr.Customer = null;
            hdr.SalesMan = null;
            hdr.User = null;
            hdr.FinancialYear = null;
            hdr.Project = null;
            hdr.SalesInvoiceHoldDtls = new List<SalesInvoiceHoldDtls>();

            await db.SalesInvoiceHoldHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();

            int headerId = hdr.SalesInvoiceHoldHdrId;
            var holdDetails = new List<SalesInvoiceHoldDtls>();
            foreach (var item in dtls)
            {
                int unitId = item.FK_UnitId;
                if (unitId == 0)
                {
                    var itemMaster = await db.ItemMasters.AsNoTracking().FirstOrDefaultAsync(x => x.ItemId == item.FK_ItemId);
                    if (itemMaster != null) unitId = itemMaster.FK_UnitId??0;
                    if (unitId == 0)
                    {
                        var defaultUnit = await db.UnitMasters.FirstOrDefaultAsync();
                        if (defaultUnit != null) unitId = defaultUnit.UnitId;
                    }
                }

                var holdDtl = new SalesInvoiceHoldDtls
                {
                    FK_SalesInvoiceHoldHdrId = headerId,
                    FK_ItemId = item.FK_ItemId,
                    ItemName = item.ItemName ?? string.Empty,
                    Barcode = item.Barcode,
                    FK_UnitId = unitId,
                    Quantity = item.Quantity,
                    QtyPerUnit = item.QtyPerUnit,
                    ExRate = item.ExRate,
                    InRate = item.InRate,
                    GrossAmount = item.GrossAmount,
                    DiscountPerc = item.DiscountPerc,
                    DiscountAmnt = item.DiscountAmnt,
                    TaxableAmnt = item.TaxableAmnt,
                    TaxPerc = item.TaxPerc,
                    TaxAmount = item.TaxAmount,
                    LineTotal = item.LineTotal,
                    IsDeleted = false,
                    FK_StoreId = item.FK_StoreId > 0 ? item.FK_StoreId : hdr.FK_StoreId,
                    FK_DeliveryNoteDtlsId = item.FK_DeliveryNoteDtlsId,
                    DeliveredQty = item.DeliveredQty
                };
                holdDetails.Add(holdDtl);
            }

            if (holdDetails.Any())
            {
                await db.SalesInvoiceHoldDtls.AddRangeAsync(holdDetails);
                await db.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return new OutputResponse(hdr.SalesInvoiceHoldHdrId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.GetBaseException().Message);
        }
    }

    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            await db.SalesInvoiceHoldDtls.Where(x => x.FK_SalesInvoiceHoldHdrId == id).ExecuteDeleteAsync();
            await db.SalesInvoiceHoldHdrs.Where(x => x.SalesInvoiceHoldHdrId == id).ExecuteDeleteAsync();
            await transaction.CommitAsync();
            return new OutputResponse(id, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.GetBaseException().Message);
        }
    }

    public async Task<OutputResponse> DeleteAllAsync(int branchId, string? terminal = null)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var holdHdrQuery = db.SalesInvoiceHoldHdrs.AsQueryable();
            if (branchId > 0)
            {
                holdHdrQuery = holdHdrQuery.Where(x => x.FK_BranchId == branchId);
            }
            if (!string.IsNullOrWhiteSpace(terminal))
            {
                var term = terminal.Trim().ToLower();
                holdHdrQuery = holdHdrQuery.Where(x => (x.RefNo ?? string.Empty).ToLower() == term || string.IsNullOrEmpty(x.RefNo));
            }
            var holdHdrIds = await holdHdrQuery.Select(x => x.SalesInvoiceHoldHdrId).ToListAsync();

            if (holdHdrIds.Any())
            {
                await db.SalesInvoiceHoldDtls.Where(x => holdHdrIds.Contains(x.FK_SalesInvoiceHoldHdrId)).ExecuteDeleteAsync();
                await db.SalesInvoiceHoldHdrs.Where(x => holdHdrIds.Contains(x.SalesInvoiceHoldHdrId)).ExecuteDeleteAsync();
            }

            await transaction.CommitAsync();
            return new OutputResponse(1, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.GetBaseException().Message);
        }
    }

    public async Task<SalesInvoiceHoldDTO> GetAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.SalesInvoiceHoldHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.SalesInvoiceHoldHdrId == id);
        if (hdr == null) return null!;

        if ((hdr.QuotationId ?? 0) > 0)
        {
            hdr.QuotationNo = await db.SalesQuotationHdrs.AsNoTracking()
                .Where(x => x.SalesQuotationId == hdr.QuotationId)
                .Select(x => x.SalesQuotationNo)
                .FirstOrDefaultAsync();
        }

        var dtls = await db.SalesInvoiceHoldDtls.AsNoTracking()
            .Where(x => x.FK_SalesInvoiceHoldHdrId == id && !x.IsDeleted)
            .ToListAsync();

        var mappedDtls = dtls.Select((d, index) => new TransactionDtls
        {
            SL = index + 1,
            DtlsId = d.SalesInvoiceHoldDtlsId,
            HdrId = d.FK_SalesInvoiceHoldHdrId,
            FK_ItemId = d.FK_ItemId,
            ItemName = d.ItemName,
            Barcode = d.Barcode,
            FK_UnitId = d.FK_UnitId,
            Quantity = d.Quantity,
            QtyPerUnit = d.QtyPerUnit,
            ExRate = d.ExRate,
            InRate = d.InRate,
            GrossAmount = d.GrossAmount,
            DiscountPerc = d.DiscountPerc,
            DiscountAmnt = d.DiscountAmnt,
            TaxableAmnt = d.TaxableAmnt,
            TaxPerc = d.TaxPerc,
            TaxAmount = d.TaxAmount,
            LineTotal = d.LineTotal,
            IsDeleted = d.IsDeleted,
            FK_StoreId = d.FK_StoreId ?? hdr.FK_StoreId,
            RefDtlsId = d.FK_DeliveryNoteDtlsId,
            RefType = d.FK_DeliveryNoteDtlsId.HasValue ? "DeliveryNote" : null,
            FK_DeliveryNoteDtlsId = d.FK_DeliveryNoteDtlsId,
            DeliveredQty = d.DeliveredQty
        }).ToList();

        return new SalesInvoiceHoldDTO(hdr, mappedDtls);
    }

    public async Task<List<SalesInvoiceHoldHdr>> GetAllHeldAsync(int branchId, string? terminal = null)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.SalesInvoiceHoldHdrs.AsNoTracking();
        if (branchId > 0)
        {
            query = query.Where(x => x.FK_BranchId == branchId);
        }
        if (!string.IsNullOrWhiteSpace(terminal))
        {
            var term = terminal.Trim().ToLower();
            query = query.Where(x => (x.RefNo ?? string.Empty).ToLower() == term || string.IsNullOrEmpty(x.RefNo));
        }
        return await query.OrderByDescending(x => x.SalesInvoiceHoldHdrId).ToListAsync();
    }

    public async Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.SalesInvoiceHoldHdrs.AsNoTracking().AsQueryable();

        if (pagination.BranchId > 0)
        {
            query = query.Where(x => x.FK_BranchId == pagination.BranchId);
        }

        if (!string.IsNullOrWhiteSpace(pagination.Terminal))
        {
            var term = pagination.Terminal.Trim().ToLower();
            query = query.Where(x => (x.RefNo ?? string.Empty).ToLower() == term || string.IsNullOrEmpty(x.RefNo));
        }

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.SalesInvoiceNo.ToLower().Contains(term) ||
                                     (x.CustomerName ?? string.Empty).ToLower().Contains(term) ||
                                     (x.RefNo ?? string.Empty).ToLower().Contains(term));
        }

        int count = await query.CountAsync();
        var list = await query
            .OrderByDescending(x => x.SalesInvoiceHoldHdrId)
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();

        var finalList = list.Select(e => new ExMasters
        {
            Id = e.SalesInvoiceHoldHdrId,
            Name = e.CustomerName,
            Code = e.SalesInvoiceNo,
            IsSelected = true,
            Amount = e.GrandTotal,
            ExtraField = e.RefNo,
            ReferenceNo = e.BillType,
            Description = e.SalesInvoiceDate.HasValue ? e.SalesInvoiceDate.Value.ToString("dd-MM-yyyy HH:mm") : ""
        }).ToList();

        return new TransactionPDTO(finalList, count);
    }
}
