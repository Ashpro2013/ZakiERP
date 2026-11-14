using ZakiSAASERP.Models.User;
using ZakiSAASERP.Models.ViewModel;
using AshproStringExtension;
using ZakiSAASERP.Interface.Others;

namespace ZakiSAASERP.Repo.Others;

public class UserPrivilegeRepo(IDbContextProvider contextProvider) : IUserPrivilegeRepo
{
    #region Models
    private async Task SaveSubMethod(TenantDBContext db, int iHdrId, List<ExUserPrivilege> dtls, List<ExFuncDetails> funcDtls)
    {
        foreach (var item in dtls)
        {
            item.Id = 0;
            item.FK_UserPrivilegeHdrId = iHdrId;
        }
        await db.UserPrivilegeDtls.AddRangeAsync(dtls);
        var functionalPrivileges = new List<FunctionalPrivilege>();
        foreach (var item in funcDtls)
        {
            FunctionalPrivilege fp = new FunctionalPrivilege();
            fp.id = 0;
            fp.FK_UserPrivilegeHdrId = iHdrId;
            fp.Enable = item.Enable;
            fp.FK_FunctionDetailsId = item.Id;
            functionalPrivileges.Add(fp);
        }
        await db.FunctionalPrivileges.AddRangeAsync(functionalPrivileges);
        await db.SaveChangesAsync();
    }
    #endregion

    #region Main
    public async Task<OutputResponse> CreateAsync(UserPrivilegeDTO entity)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.entUserPrivilegeHdr;
            var dtls = entity.entUserPrivilegeDtls;
            var funcDtls = entity.entFunctionalPrivilege;
            var exists = await db.UserPrivilegeHdrs.AnyAsync(x => x.Name == hdr.Name);
            if (exists) return new OutputResponse(0, "User Privilege already exists.");
            await db.UserPrivilegeHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();
            int iHdrId = hdr.Id;
            await SaveSubMethod(db, iHdrId, dtls, funcDtls);
            await transaction.CommitAsync();
            return new OutputResponse(iHdrId, null);
        }
        catch (DbUpdateException ex) { await transaction.RollbackAsync(); return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { await transaction.RollbackAsync(); return new OutputResponse(0, ex.Message); }
    }
    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            if (id is 0) return new OutputResponse(0, "Invalid identifier.");
            var existing = await db.UserPrivilegeHdrs.FindAsync(id);
            if (existing is null) return new OutputResponse(0, "Privilege not found.");
            await db.UserPrivilegeDtls.Where(x => x.FK_UserPrivilegeHdrId == id).ExecuteDeleteAsync();
            await db.FunctionalPrivileges.Where(x => x.FK_UserPrivilegeHdrId == id).ExecuteDeleteAsync();
            db.UserPrivilegeHdrs.Remove(existing);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(id, null);
        }
        catch (DbUpdateException ex) { await transaction.RollbackAsync(); return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { await transaction.RollbackAsync(); return new OutputResponse(0, ex.Message); }
    }
    public async Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.UserPrivilegeHdrs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            bool desc = pagination.sortDirection == "desc";
            query = pagination.sortColumn switch
            {
                "Name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                _ => desc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.Id);
        }

        int count = await query.CountAsync();
        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();
        var finalList = list.Select(e => new ExMasters
        {
            Id = e.Id,
            Name = e.Name,
            Code = e.Id.ToString(), // Map Id to Code
            IsSelected = true,
            Amount = 0,
            ExtraField = "",
            Description = ""
        }).ToList();
        return new TransactionPDTO(finalList, count);
    }
    public async Task<OutputResponse> UpdateAsync(UserPrivilegeDTO entity)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.entUserPrivilegeHdr;
            var dtls = entity.entUserPrivilegeDtls;
            var funcDtls = entity.entFunctionalPrivilege;
            var existing = await db.UserPrivilegeHdrs.FirstOrDefaultAsync(x => x.Id == hdr.Id);
            if (existing is null) return new OutputResponse(0, "User Privilege Header not found.");
            await db.UserPrivilegeDtls.Where(x => x.FK_UserPrivilegeHdrId == hdr.Id).ExecuteDeleteAsync();
            await db.FunctionalPrivileges.Where(x => x.FK_UserPrivilegeHdrId == hdr.Id).ExecuteDeleteAsync();
            existing.Name = hdr.Name;
            existing.FK_BranchId = hdr.FK_BranchId;
            await db.SaveChangesAsync();
            int iHdrId = existing.Id;
            await SaveSubMethod(db, iHdrId, dtls, funcDtls);
            await transaction.CommitAsync();
            return new OutputResponse(existing.Id, null);
        }
        catch (DbUpdateException ex) { await transaction.RollbackAsync(); return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { await transaction.RollbackAsync(); return new OutputResponse(0, ex.Message); }
    }
    public async Task<UserPrivilegeDTO> GetAsync(int Id)
    {
        await using var db = contextProvider.CreateDbContext();
        await MenuSeedHelper.EnsureSeedDataAsync(db);

        // Pre-fetch all menus for fast in-memory lookup and parent resolution
        var allDbMenus = await db.MenuDetails.AsNoTracking().ToListAsync();
        var menusById = allDbMenus.ToDictionary(m => m.Id);

        // Filter active menus that belong to a parent (exclude top-level root groupings like Masters, Transactions, etc.)
        var allMenus = allDbMenus
            .Where(x => x.Active && x.ParentId != null)
            .OrderBy(x => menusById.TryGetValue(x.ParentId ?? 0, out var p) ? p.Position : 999)
            .ThenBy(x => x.Position)
            .ToList();

        if (Id > 0)
        {
            var hdr = await db.UserPrivilegeHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
            var savedDtls = await db.UserPrivilegeDtls.AsNoTracking().Include(y => y.MenuDetails).Where(x => x.FK_UserPrivilegeHdrId == Id).ToListAsync();
            var funcDtls = await db.FunctionalPrivileges.AsNoTracking().Include(y => y.FunctionDetails).Where(x => x.FK_UserPrivilegeHdrId == Id).ToListAsync();

            List<ExUserPrivilege> exUserPrevilages = new List<ExUserPrivilege>();

            foreach (var menu in allMenus)
            {
                var existingPrivilege = savedDtls.FirstOrDefault(x => x.FK_MenuDetailsId == menu.Id);
                var parentCaption = (menu.ParentId != null && menusById.TryGetValue(menu.ParentId.Value, out var parent))
                    ? parent.MenuCaption
                    : string.Empty;
                var parentArabic = (menu.ParentId != null && menusById.TryGetValue(menu.ParentId.Value, out var pAr))
                    ? pAr.ArabicCaption
                    : string.Empty;

                if (existingPrivilege != null)
                {
                    // Use saved privilege
                    var exitem = existingPrivilege.CopyTo<ExUserPrivilege>();
                    exitem.MenuId = existingPrivilege.FK_MenuDetailsId;
                    exitem.MenuCaption = existingPrivilege.MenuDetails?.MenuCaption ?? menu.MenuCaption;
                    exitem.MenuTitle = parentCaption;
                    exitem.ArabicCaption = menu.ArabicCaption;
                    exitem.ArabicTitle = parentArabic;
                    exUserPrevilages.Add(exitem);
                }
                else
                {
                    // Create default privilege for new/missing menu
                    ExUserPrivilege exitem = new ExUserPrivilege
                    {
                        MenuId = menu.Id,
                        MenuCaption = menu.MenuCaption,
                        MenuTitle = parentCaption,
                        ArabicCaption = menu.ArabicCaption,
                        ArabicTitle = parentArabic,
                        Visible = false,
                        SaveData = false,
                        EditData = false,
                        DeleteData = false,
                        FK_UserPrivilegeHdrId = Id,
                        FK_MenuDetailsId = menu.Id
                    };
                    exUserPrevilages.Add(exitem);
                }
            }

            List<ExFuncDetails> exFuncDetails = new List<ExFuncDetails>();
            foreach (var item in funcDtls)
            {
                var exitem = item.FunctionDetails.CopyTo<ExFuncDetails>();
                exitem.EventCaption = item.FunctionDetails?.EventCaption;
                exitem.Enable = item.Enable;
                exFuncDetails.Add(exitem);
            }
            return new UserPrivilegeDTO(hdr!, exUserPrevilages, exFuncDetails);
        }
        else
        {
            var hdr = new UserPrivilegeHdr();
            var funcDtls = await db.FunctionDetails.AsNoTracking().ToListAsync();

            List<ExUserPrivilege> exUserPrevilages = new List<ExUserPrivilege>();
            foreach (var item in allMenus)
            {
                var parentCaption = (item.ParentId != null && menusById.TryGetValue(item.ParentId.Value, out var parent))
                    ? parent.MenuCaption
                    : string.Empty;
                var parentArabic = (item.ParentId != null && menusById.TryGetValue(item.ParentId.Value, out var pAr))
                    ? pAr.ArabicCaption
                    : string.Empty;

                ExUserPrivilege exitem = new ExUserPrivilege
                {
                    MenuId = item.Id,
                    MenuCaption = item.MenuCaption,
                    MenuTitle = parentCaption,
                    ArabicCaption = item.ArabicCaption,
                    ArabicTitle = parentArabic,
                    Visible = false,
                    SaveData = false,
                    EditData = false,
                    DeleteData = false
                };
                exUserPrevilages.Add(exitem);
            }

            List<ExFuncDetails> exFuncDetails = new List<ExFuncDetails>();
            int isl = 1;
            foreach (var item in funcDtls)
            {
                var exitem = item.CopyTo<ExFuncDetails>();
                exitem.SL = isl;
                exitem.EventCaption = item.EventCaption;
                exitem.Enable = false;
                exFuncDetails.Add(exitem);
                isl++;
            }
            return new UserPrivilegeDTO(hdr, exUserPrevilages, exFuncDetails);
        }
    }
    public Task<int> GetNextVchrNo(int BranchId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<WebUserPrivilege>> GetWebUserPrivileges(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var user = await db.UserDetails
    .AsNoTracking()
    .FirstOrDefaultAsync(x => x.UserId == id);

            if (user == null || user.PrevilageId == null || user.PrevilageId == 0)
                return new List<WebUserPrivilege>();

            var privilegeId = user.PrevilageId;

            var dtls = await db.UserPrivilegeDtls.AsNoTracking().Include(x => x.MenuDetails).Where(x => x.FK_UserPrivilegeHdrId == privilegeId).ToListAsync();
            List<WebUserPrivilege> webUserPrivileges = new List<WebUserPrivilege>();
            foreach (var item in dtls)
            {
                var webItem = item.CopyTo<WebUserPrivilege>();
                webItem.Caption = item.MenuDetails?.MenuCaption;
                webUserPrivileges.Add(webItem);
            }
            return webUserPrivileges;
        }
        catch (Exception)
        {
            return new List<WebUserPrivilege>();
        }
    }
    public async Task<List<UserPrivilegeHdr>> GetAllAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.UserPrivilegeHdrs.AsNoTracking().ToListAsync();
    }
    #endregion
}
