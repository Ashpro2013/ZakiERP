namespace AshproSAASERP.Repo.Reports;

public class TransactionReportRepo(IDbContextProvider contextProvider) : ITransactionReportRepo
{
    public async Task<TransactionMonitorResponse> GetTransactionMonitorReportAsync(TransactionMonitorRequest req)
    {
        return req.TransactionType switch
        {
            "Delivery Note" => await GetDeliveryNoteMonitorAsync(req),
            "Sales Invoice" => await GetSalesInvoiceMonitorAsync(req),
            "Sales Quotation" => await GetSalesQuotationMonitorAsync(req),
            "Proforma Invoice" => await GetProformaInvoiceMonitorAsync(req),
            _ => new TransactionMonitorResponse(new List<TransactionMonitorDtls>(), 0, 0)
        };
    }

    private async Task<TransactionMonitorResponse> GetDeliveryNoteMonitorAsync(TransactionMonitorRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.DeliveryNoteHdrs
            .Where(x => x.DeliveryNoteDate >= req.FromDate.Date && x.DeliveryNoteDate <= req.ToDate.Date.AddDays(1).AddTicks(-1) && !x.IsDeleted)
            .AsQueryable();

        if (req.BranchId > 0)
            query = query.Where(x => x.FK_BranchId == req.BranchId);

        if (req.PartyId.HasValue && req.PartyId > 0)
            query = query.Where(x => x.FK_CustomerId == req.PartyId);

        if (!string.IsNullOrEmpty(req.SearchTerm))
        {
            query = query.Where(x =>
                (x.DeliveryNoteNo != null && x.DeliveryNoteNo.Contains(req.SearchTerm)) ||
                (x.Customer != null && x.Customer.Name.Contains(req.SearchTerm)) ||
                (x.RefNo != null && x.RefNo.Contains(req.SearchTerm))
            );
        }

        var projectedQuery = query.Select(x => new
        {
            Hdr = x,
            // Try to find the associated Sales Invoice to get SalesNo and QuotNo
            SalesInvoice = db.SalesInvoiceHdrs.FirstOrDefault(s =>
                (x.SalesInvoiceId != null && s.SalesInvoiceId == x.SalesInvoiceId) ||
                (x.SalesInvoiceId == null && (s.DeliveryNoteId == x.DeliveryNoteId || s.DeliveryNoteIds.Contains(x.DeliveryNoteId.ToString())))),
            CustomerName = x.Customer != null ? x.Customer.Name : x.CustomerName
        });

        // Further project to get Quotation from Invoice and Determine Status
        var finalQuery = projectedQuery.Select(x => new
        {
            x.Hdr,
            x.CustomerName,
            x.SalesInvoice,
            Quotation = x.SalesInvoice != null && x.SalesInvoice.QuotationId.HasValue
                ? db.SalesQuotationHdrs.FirstOrDefault(q => q.SalesQuotationId == x.SalesInvoice.QuotationId)
                : null,
            Proforma = x.SalesInvoice != null && x.SalesInvoice.ProformaInvoiceId.HasValue
                ? db.ProformaInvoiceHdrs.FirstOrDefault(p => p.ProformaInvoiceId == x.SalesInvoice.ProformaInvoiceId)
                : null
        });



        var grandTotals = await finalQuery
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalAmount = g.Sum(x => x.Hdr.GrandTotal),
                Count = g.Count()
            })
            .FirstOrDefaultAsync();

        int totalCount = grandTotals?.Count ?? 0;
        decimal totalAmount = grandTotals?.TotalAmount ?? 0;

        // Ordering logic (defaults to Date Desc)
        var orderedQuery = finalQuery.OrderByDescending(x => x.Hdr.DeliveryNoteDate);

        var pagedData = await orderedQuery.ToListAsync();

        var reportItems = new List<TransactionMonitorDtls>();
        int sl = 1;

        foreach (var item in pagedData)
        {
            var hdr = item.Hdr;
            string salesNo = item.SalesInvoice?.SalesInvoiceNo ?? "";
            string quotNo = item.Quotation?.SalesQuotationNo ?? "";
            string proformaNo = item.Proforma?.ProformaInvoiceNo ?? "";

            reportItems.Add(new TransactionMonitorDtls(
                sl++,
                hdr.DeliveryNoteId,
                hdr.DeliveryNoteNo, // Inv No
                hdr.DeliveryNoteDate.HasValue ? hdr.DeliveryNoteDate.Value.ToString("dd/MM/yyyy") : "",
                item.CustomerName,
                hdr.GrandTotal, // Amount
                quotNo, // Quot No
                hdr.RefNo ?? "", // Ref No (was PO No)
                salesNo, // Sales No
                hdr.DeliveryNoteNo, // Delivery No
                proformaNo // Proforma No
            ));
        }

        return new TransactionMonitorResponse(reportItems, totalAmount, totalCount);
    }

    private async Task<TransactionMonitorResponse> GetSalesInvoiceMonitorAsync(TransactionMonitorRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.SalesInvoiceHdrs
            .Where(x => x.SalesInvoiceDate >= req.FromDate.Date && x.SalesInvoiceDate <= req.ToDate.Date.AddDays(1).AddTicks(-1) && !x.IsDeleted)
            .AsQueryable();

        if (req.BranchId > 0)
            query = query.Where(x => x.FK_BranchId == req.BranchId);

        if (req.PartyId.HasValue && req.PartyId > 0)
            query = query.Where(x => x.FK_CustomerId == req.PartyId);

        if (!string.IsNullOrEmpty(req.SearchTerm))
        {
            query = query.Where(x =>
                (x.SalesInvoiceNo != null && x.SalesInvoiceNo.Contains(req.SearchTerm)) ||
                (x.Customer != null && x.Customer.Name.Contains(req.SearchTerm)) ||
                (x.RefNo != null && x.RefNo.Contains(req.SearchTerm))
            );
        }

        var projectedQuery = query.Select(x => new
        {
            Hdr = x,
            Received = x.BillType == "Credit"
                       ? (db.CustomerReceiptDtls.Where(r => r.FK_InvoiceId == x.SalesInvoiceId).Sum(r => (decimal?)r.PaymentAmount) ?? 0)
                       : x.GrandTotal,
            Discount = x.BillType == "Credit"
                       ? (db.CustomerReceiptDtls.Where(r => r.FK_InvoiceId == x.SalesInvoiceId).Sum(r => (decimal?)r.Discount) ?? 0)
                       : 0,
            Return = db.SalesInvoiceRtnHdrs.Where(r => r.SalesInvoiceId == x.SalesInvoiceId && !r.IsDeleted).Sum(r => (decimal?)r.GrandTotal) ?? 0,
            Quotation = x.QuotationId.HasValue ? db.SalesQuotationHdrs.FirstOrDefault(q => q.SalesQuotationId == x.QuotationId) : null,
            Proforma = x.ProformaInvoiceId.HasValue ? db.ProformaInvoiceHdrs.FirstOrDefault(p => p.ProformaInvoiceId == x.ProformaInvoiceId) : null,
            CustomerName = x.Customer != null ? x.Customer.Name : x.CustomerName
        }).Select(x => new
        {
            x.Hdr,
            x.Received,
            x.Discount,
            x.Return,
            Balance = x.Hdr.BillType == "Credit"
                      ? (x.Hdr.GrandTotal - x.Received - x.Discount - x.Return)
                      : 0,
            x.Quotation,
            x.Proforma,
            x.CustomerName
        });



        var grandTotals = await projectedQuery
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalAmount = g.Sum(x => x.Hdr.GrandTotal),
                Count = g.Count()
            })
            .FirstOrDefaultAsync();

        int totalCount = grandTotals?.Count ?? 0;
        decimal totalAmount = grandTotals?.TotalAmount ?? 0;

        var orderedQuery = projectedQuery.OrderByDescending(x => x.Hdr.SalesInvoiceDate);

        var pagedData = await orderedQuery.ToListAsync();

        // Batch fetch Delivery Notes
        var allDnIds = new HashSet<int>();
        foreach (var item in pagedData)
        {
            var sale = (AshproSAASERP.Models.Transactions.Sales.SalesInvoiceHdr)item.Hdr;

            // Check formatted IDs string
            if (!string.IsNullOrEmpty(sale.DeliveryNoteIds))
            {
                var parts = sale.DeliveryNoteIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var p in parts)
                {
                    if (int.TryParse(p, out int id)) allDnIds.Add(id);
                }
            }
            // Fallback to legacy single ID
            if (sale.DeliveryNoteId.HasValue)
            {
                allDnIds.Add(sale.DeliveryNoteId.Value);
            }
        }

        Dictionary<int, string> dnLookup = new();
        if (allDnIds.Any())
        {
            dnLookup = await db.DeliveryNoteHdrs
                .Where(d => allDnIds.Contains(d.DeliveryNoteId))
                .ToDictionaryAsync(d => d.DeliveryNoteId, d => d.DeliveryNoteNo);
        }

        var reportItems = new List<TransactionMonitorDtls>();
        int sl = 1;

        foreach (var item in pagedData)
        {
            var sale = (AshproSAASERP.Models.Transactions.Sales.SalesInvoiceHdr)item.Hdr;

            // Resolve Delivery Note Numbers
            var dnNos = new List<string>();

            if (!string.IsNullOrEmpty(sale.DeliveryNoteIds))
            {
                var parts = sale.DeliveryNoteIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var p in parts)
                {
                    if (int.TryParse(p, out int id) && dnLookup.ContainsKey(id))
                    {
                        dnNos.Add(dnLookup[id]);
                    }
                }
            }

            if (sale.DeliveryNoteId.HasValue && !dnNos.Any() && dnLookup.ContainsKey(sale.DeliveryNoteId.Value))
            {
                dnNos.Add(dnLookup[sale.DeliveryNoteId.Value]);
            }

            string deliveryNoStr = string.Join(", ", dnNos.Distinct());

            reportItems.Add(new TransactionMonitorDtls(
                sl++,
                sale.SalesInvoiceId,
                sale.SalesInvoiceNo,
                sale.SalesInvoiceDate.HasValue ? sale.SalesInvoiceDate.Value.ToString("dd/MM/yyyy") : "",
                item.CustomerName,
                sale.GrandTotal,
                item.Quotation?.SalesQuotationNo ?? "", // Quot No
                sale.RefNo ?? "", // Ref No
                sale.SalesInvoiceNo, // Sales No
                deliveryNoStr, // Delivery No (Comma Separated)
                item.Proforma?.ProformaInvoiceNo ?? "" // Proforma No
            ));
        }

        return new TransactionMonitorResponse(reportItems, totalAmount, totalCount);
    }

    private async Task<TransactionMonitorResponse> GetSalesQuotationMonitorAsync(TransactionMonitorRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.SalesQuotationHdrs
            .Where(x => x.SalesQuotationDate >= req.FromDate.Date && x.SalesQuotationDate <= req.ToDate.Date.AddDays(1).AddTicks(-1) && !x.IsDeleted)
            .AsQueryable();

        if (req.BranchId > 0)
            query = query.Where(x => x.FK_BranchId == req.BranchId);

        if (req.PartyId.HasValue && req.PartyId > 0)
            query = query.Where(x => x.FK_CustomerId == req.PartyId);

        if (!string.IsNullOrEmpty(req.SearchTerm))
        {
            query = query.Where(x =>
                (x.SalesQuotationNo != null && x.SalesQuotationNo.Contains(req.SearchTerm)) ||
                (x.Customer != null && x.Customer.Name.Contains(req.SearchTerm)) ||
                (x.RefNo != null && x.RefNo.Contains(req.SearchTerm))
            );
        }

        var projectedQuery = query.Select(x => new
        {
            Hdr = x,
            CustomerName = x.Customer != null ? x.Customer.Name : x.CustomerName,
            // Find linked Sales Invoice
            SalesInvoice = db.SalesInvoiceHdrs.FirstOrDefault(i => i.QuotationId == x.SalesQuotationId),
            // Find linked Proforma (Quotation -> Proforma)
            Proforma = db.ProformaInvoiceHdrs.FirstOrDefault(p => p.QuotationId == x.SalesQuotationId)
        });

        // Get linked Delivery Note via Sales Invoice
        var finalQuery = projectedQuery.Select(x => new
        {
            x.Hdr,
            x.CustomerName,
            x.SalesInvoice,
            x.Proforma,
            DeliveryNote = x.SalesInvoice != null && x.SalesInvoice.DeliveryNoteId.HasValue
                ? db.DeliveryNoteHdrs.FirstOrDefault(d => d.DeliveryNoteId == x.SalesInvoice.DeliveryNoteId)
                : null
        });



        var grandTotals = await finalQuery
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalAmount = g.Sum(x => x.Hdr.GrandTotal),
                Count = g.Count()
            })
            .FirstOrDefaultAsync();

        int totalCount = grandTotals?.Count ?? 0;
        decimal totalAmount = grandTotals?.TotalAmount ?? 0;

        var orderedQuery = finalQuery.OrderByDescending(x => x.Hdr.SalesQuotationDate);

        var pagedData = await orderedQuery.ToListAsync();

        var reportItems = new List<TransactionMonitorDtls>();
        int sl = 1;

        foreach (var item in pagedData)
        {
            var hdr = item.Hdr;

            reportItems.Add(new TransactionMonitorDtls(
                sl++,
                hdr.SalesQuotationId,
                hdr.SalesQuotationNo, // Inv No
                hdr.SalesQuotationDate.HasValue ? hdr.SalesQuotationDate.Value.ToString("dd/MM/yyyy") : "",
                item.CustomerName,
                hdr.GrandTotal, // Amount
                hdr.SalesQuotationNo, // Quot No
                hdr.RefNo ?? "", // Ref No
                item.SalesInvoice?.SalesInvoiceNo ?? "", // Sales No
                item.DeliveryNote?.DeliveryNoteNo ?? "", // Delivery No
                item.Proforma?.ProformaInvoiceNo ?? "" // Proforma No
            ));
        }

        return new TransactionMonitorResponse(reportItems, totalAmount, totalCount);
    }

    private async Task<TransactionMonitorResponse> GetProformaInvoiceMonitorAsync(TransactionMonitorRequest req)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.ProformaInvoiceHdrs
            .Where(x => x.ProformaInvoiceDate >= req.FromDate.Date && x.ProformaInvoiceDate <= req.ToDate.Date.AddDays(1).AddTicks(-1) && !x.IsDeleted)
            .AsQueryable();

        if (req.BranchId > 0)
            query = query.Where(x => x.FK_BranchId == req.BranchId);

        if (req.PartyId.HasValue && req.PartyId > 0)
            query = query.Where(x => x.FK_CustomerId == req.PartyId);

        if (!string.IsNullOrEmpty(req.SearchTerm))
        {
            query = query.Where(x =>
                (x.ProformaInvoiceNo != null && x.ProformaInvoiceNo.Contains(req.SearchTerm)) ||
                (x.Customer != null && x.Customer.Name.Contains(req.SearchTerm)) ||
                (x.RefNo != null && x.RefNo.Contains(req.SearchTerm))
            );
        }

        var projectedQuery = query.Select(x => new
        {
            Hdr = x,
            CustomerName = x.Customer != null ? x.Customer.Name : x.CustomerName,
            // Find linked Sales Invoice
            SalesInvoice = db.SalesInvoiceHdrs.FirstOrDefault(i => i.ProformaInvoiceId == x.ProformaInvoiceId),
            Quotation = x.QuotationId.HasValue ? db.SalesQuotationHdrs.FirstOrDefault(q => q.SalesQuotationId == x.QuotationId) : null
        });

        // Get linked Delivery Note via Sales Invoice
        var finalQuery = projectedQuery.Select(x => new
        {
            x.Hdr,
            x.CustomerName,
            x.SalesInvoice,
            x.Quotation,
            DeliveryNote = x.SalesInvoice != null && x.SalesInvoice.DeliveryNoteId.HasValue
                ? db.DeliveryNoteHdrs.FirstOrDefault(d => d.DeliveryNoteId == x.SalesInvoice.DeliveryNoteId)
                : null
        });



        var grandTotals = await finalQuery
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalAmount = g.Sum(x => x.Hdr.GrandTotal),
                Count = g.Count()
            })
            .FirstOrDefaultAsync();

        int totalCount = grandTotals?.Count ?? 0;
        decimal totalAmount = grandTotals?.TotalAmount ?? 0;

        var orderedQuery = finalQuery.OrderByDescending(x => x.Hdr.ProformaInvoiceDate);

        var pagedData = await orderedQuery.ToListAsync();

        var reportItems = new List<TransactionMonitorDtls>();
        int sl = 1;

        foreach (var item in pagedData)
        {
            var hdr = item.Hdr;

            reportItems.Add(new TransactionMonitorDtls(
                sl++,
                hdr.ProformaInvoiceId,
                hdr.ProformaInvoiceNo, // Inv No
                hdr.ProformaInvoiceDate.HasValue ? hdr.ProformaInvoiceDate.Value.ToString("dd/MM/yyyy") : "",
                item.CustomerName,
                hdr.GrandTotal, // Amount
                item.Quotation?.SalesQuotationNo ?? "", // Quot No
                hdr.RefNo ?? "", // Ref No
                item.SalesInvoice?.SalesInvoiceNo ?? "", // Sales No
                item.DeliveryNote?.DeliveryNoteNo ?? "", // Delivery No
                hdr.ProformaInvoiceNo // Proforma No (Self)
            ));
        }

        return new TransactionMonitorResponse(reportItems, totalAmount, totalCount);
    }
}
