using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.ViewModel;

namespace ZakiSAASERP.Repo.Others;

public class PrintDesignRepo(IDbContextProvider contextProvider, IServerRepo serverRepo) : IPrintDesignRepo
{
    public async Task<OutputResponse> CreateAsync(PrintDesignDTO entity)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.hdr;
            hdr.PrintDesignHdrId = 0;
            var dtls = entity.dtls;
            if (hdr.IsDefault)
            {
                await db.PrintDesignHdrs.Where(x => x.FormType == hdr.FormType && x.VoucherType == hdr.VoucherType).
                     ExecuteUpdateAsync(x => x.SetProperty(e => e.IsDefault, false));
            }
            var exists = await db.PrintDesignHdrs.AnyAsync(x => x.DesignName == hdr.DesignName);
            if (exists) return new OutputResponse(0, "Print Designer already exists.");
            await db.PrintDesignHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();
            dtls.ForEach(x => { x.FK_PrintDesignHdrId = hdr.PrintDesignHdrId; x.PrintDesignDtlsId = 0; });
            await db.PrintDesignDtls.AddRangeAsync(dtls);
            await db.SaveChangesAsync(); await transaction.CommitAsync();
            return new OutputResponse(hdr.PrintDesignHdrId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }
    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            if (id == 0) return new OutputResponse(0, "Invalid identifier.");
            await db.PrintDesignDtls.Where(x => x.FK_PrintDesignHdrId == id).ExecuteDeleteAsync();
            await db.PrintDesignHdrs.Where(x => x.PrintDesignHdrId == id).ExecuteDeleteAsync();
            await transaction.CommitAsync();
            return new OutputResponse(id, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }
    public async Task<PrintDesignDTO> GetAsync(PrintDesignReq req)
    {
        await using var db = contextProvider.CreateDbContext();
        PrintDesignHdr? hdr = null;
        List<PrintDesignDtls> printDesignDtls = new();
        if (req.VoucherType == "pdesign")
        {
            hdr = await db.PrintDesignHdrs.FirstOrDefaultAsync(x => x.PrintDesignHdrId == req.id);
        }
        else
        {
            string ty = req.isThermal ? "Thermal" : "A4";
            hdr = await db.PrintDesignHdrs.FirstOrDefaultAsync(x => x.VoucherType == req.VoucherType && x.IsDefault == true && x.PrintType == ty);
        }
        if (hdr != null)
        {
            printDesignDtls = await db.PrintDesignDtls.Where(x => x.FK_PrintDesignHdrId == hdr.PrintDesignHdrId).ToListAsync();
        }
        else
        {
            var pdFromFile = await serverRepo.GetPDesignFromFileAsync(req.VoucherType);
            if (pdFromFile != null)
            {
                var data = await CreateAsync(pdFromFile);
                if (string.IsNullOrEmpty(data.Message))
                {
                    hdr = pdFromFile.hdr;
                    printDesignDtls = pdFromFile.dtls;
                }
            }
        }
        if (hdr == null || printDesignDtls.Any() == false) return null;
        var result = new PrintDesignDTO(hdr, printDesignDtls);
        await serverRepo.SavePDesignToFileAsync(result);
        return result;
    }
    public async Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.PrintDesignHdrs.AsNoTracking().AsQueryable();
        if (!query.Any())
        {
            var pdFromFile = await serverRepo.GetFullPDesignFromFileAsync();
            if (pdFromFile != null && pdFromFile.Any())
            {
                foreach (var item in pdFromFile)
                {
                    await CreateAsync(item);
                }
               await  GetAllWithPagination(pagination);
            }
        }
        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.DesignName.ToLower().Contains(term) || x.FormType.ToLower().Contains(term)
            || x.VoucherType.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            bool desc = pagination.sortDirection == "desc";
            query = pagination.sortColumn switch
            {
                "Code" => desc ? query.OrderByDescending(x => x.DesignName) : query.OrderBy(x => x.DesignName),
                "Name" => desc ? query.OrderByDescending(x => x.VoucherType) : query.OrderBy(x => x.VoucherType),
                "Description" => desc ? query.OrderByDescending(x => x.FormType) : query.OrderBy(x => x.FormType),
                _ => desc ? query.OrderByDescending(x => x.PrintDesignHdrId) : query.OrderBy(x => x.PrintDesignHdrId)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.PrintDesignHdrId);
        }

        int count = await query.CountAsync();
        
        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();

        var finalList = list.Where(x => x.FK_BranchId == pagination.BranchId).Select(e => new ExMasters
        {
            Id = e.PrintDesignHdrId,
            Name = e.VoucherType,
            Code = e.DesignName,
            IsSelected = true,
            Amount = 0M,
            ExtraField = e.PrintType,
            Description = e.FormType
        }).ToList();
        return new TransactionPDTO(finalList, count);
    }

    public async Task<OutputResponse> UpdateAsync(PrintDesignDTO entity)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.hdr;
            if (hdr.IsDefault)
            {
                await db.PrintDesignHdrs.Where(x => x.FormType == hdr.FormType && x.VoucherType == hdr.VoucherType).
                     ExecuteUpdateAsync(x => x.SetProperty(e => e.IsDefault, false));
            }
            // 1. Ensure header exists (important)
            db.PrintDesignHdrs.Attach(entity.hdr);
            db.Entry(entity.hdr).State = EntityState.Modified;

            // 2. Delete existing details
            await db.PrintDesignDtls
                .Where(x => x.FK_PrintDesignHdrId == entity.hdr.PrintDesignHdrId)
                .ExecuteDeleteAsync();

            // 3. Ensure FK is correctly assigned in each detail
            foreach (var d in entity.dtls)
            {
                d.PrintDesignDtlsId = 0;
                d.FK_PrintDesignHdrId = entity.hdr.PrintDesignHdrId;
            }
            // 4. Insert details
            await db.PrintDesignDtls.AddRangeAsync(entity.dtls);

            // 5. Save
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OutputResponse(entity.hdr.PrintDesignHdrId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }

}



