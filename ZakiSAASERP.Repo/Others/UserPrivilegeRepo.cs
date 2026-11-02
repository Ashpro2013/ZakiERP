using ZakiSAASERP.Models.User;
using ZakiSAASERP.Models.ViewModel;
using AshproStringExtension;

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
        if (Id > 0)
        {
            var hdr = db.UserPrivilegeHdrs.AsNoTracking().FirstOrDefault(x => x.Id == Id);
            var savedDtls = db.UserPrivilegeDtls.AsNoTracking().Include(y => y.MenuDetails).Where(x => x.FK_UserPrivilegeHdrId == Id).ToList();
            var funcDtls = db.FunctionalPrivileges.AsNoTracking().Include(y => y.FunctionDetails).Where(x => x.FK_UserPrivilegeHdrId == Id).ToList();

            // Fetch ALL active menu details
            var allMenus = await db.MenuDetails.AsNoTracking().Where(x => x.Active && x.ParentId != null).ToListAsync();

            List<ExUserPrivilege> exUserPrevilages = new List<ExUserPrivilege>();

            foreach (var menu in allMenus)
            {
                var existingPrivilege = savedDtls.FirstOrDefault(x => x.FK_MenuDetailsId == menu.Id);

                if (existingPrivilege != null)
                {
                    // Use saved privilege
                    var exitem = existingPrivilege.CopyTo<ExUserPrivilege>();
                    exitem.MenuId = existingPrivilege.FK_MenuDetailsId;
                    exitem.MenuCaption = existingPrivilege.MenuDetails?.MenuCaption!;
                    // exitem.MenuTitle = (await db.MenuDetails.FirstOrDefaultAsync(x => x.Id == existingPrivilege.MenuDetails!.ParentId))!.MenuCaption;
                    exitem.MenuTitle = (await db.MenuDetails.FirstOrDefaultAsync(x => x.Id == menu.ParentId))!.MenuCaption;
                    exUserPrevilages.Add(exitem);
                }
                else
                {
                    // Create default privilege for new/missing menu
                    ExUserPrivilege exitem = new ExUserPrivilege();
                    exitem.MenuId = menu.Id;
                    exitem.MenuCaption = menu.MenuCaption;
                    exitem.MenuTitle = (await db.MenuDetails.FirstOrDefaultAsync(x => x.Id == menu.ParentId))!.MenuCaption;
                    exitem.Visible = false;
                    exitem.SaveData = false;
                    exitem.EditData = false;
                    exitem.DeleteData = false;

                    // Important: Set FK relationships if needed for saving logic, though usually Handle in Save
                    exitem.FK_UserPrivilegeHdrId = Id;
                    exitem.FK_MenuDetailsId = menu.Id;

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
            var dtls = await db.MenuDetails.AsNoTracking().Where(x => x.Active && x.ParentId != null).ToListAsync();
            var funcDtls = await db.FunctionDetails.AsNoTracking().ToListAsync();
            List<ExUserPrivilege> exUserPrevilages = new List<ExUserPrivilege>();
            foreach (var item in dtls)
            {
                ExUserPrivilege exitem = new ExUserPrivilege();
                exitem.MenuId = item.Id;
                exitem.MenuCaption = item.MenuCaption;
                exitem.MenuTitle = (await db.MenuDetails.FirstOrDefaultAsync(x => x.Id == item.ParentId))!.MenuCaption;
                exitem.Visible = false;
                exitem.SaveData = false;
                exitem.EditData = false;
                exitem.DeleteData = false;
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

            var dtls = db.UserPrivilegeDtls.AsNoTracking().Include(x => x.MenuDetails).Where(x => x.FK_UserPrivilegeHdrId == privilegeId).ToList();
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
