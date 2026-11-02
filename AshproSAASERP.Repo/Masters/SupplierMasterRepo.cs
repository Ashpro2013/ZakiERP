using AshproSAASERP.Repo.CodeGenerationServices;

namespace AshproSAASERP.Repo.Masters;

public class SupplierMasterRepo(IDbContextProvider contextProvider, CodeSequenceService codeService) : ISupplierMasterRepo
{
    public async Task<string> GetNextSupplierCodeAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await codeService.GetNextUniqueCodeAsync(
            CodeKeys.Supplier,
            async (code) => await db.SupplierMasters.AnyAsync(x => x.Code == code)
        );
    }

    public async Task<OutputResponse> CreateAsync(SupplierMaster entity)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var exists = await db.SupplierMasters.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
            if (exists) return new OutputResponse(0, "Supplier already exists.");

            // Integration: Save Account in Account Master
            // Find Sundry Creditors SubGroup (code: 21000101) from built-in accounts
            var sundryCreditorsSubGroup = await db.AccountSubGroups.FirstOrDefaultAsync(x => x.Code == "21000101");

            if (sundryCreditorsSubGroup != null)
            {
                var accountExists = await db.AccountMasters.AnyAsync(x => x.Code == entity.Code);
                if (!accountExists)
                {
                    // Get Liability Type ID
                    var liabilityType = await db.AccountTypes.FirstOrDefaultAsync(x => x.AcTypeName == "Liability");

                    var newAccount = new AccountMaster
                    {
                        Code = entity.Code,
                        Name = entity.Name,
                        NameArabic = entity.NameArabic,
                        FK_AccountHeadId = sundryCreditorsSubGroup.FK_AcHeadId,
                        FK_AcGroupId = sundryCreditorsSubGroup.FK_AcGroupId ?? 4,
                        FK_AccountSubGroupId = sundryCreditorsSubGroup.SubGroupId,
                        FK_AcTypeId = liabilityType?.AcTypeId ?? 2, // Default to 2 (Liability) if not found
                        DbCr = "Credit",
                        Opening = 0,
                        Flag = "Sup",
                        IsReserved = false // User-created accounts are not reserved
                    };
                    await db.AccountMasters.AddAsync(newAccount);
                    await db.SaveChangesAsync();

                    // Link the new Account ID to the Supplier
                    entity.FK_AccountId = newAccount.AccountId;
                }
            }

            await db.SupplierMasters.AddAsync(entity);
            await db.SaveChangesAsync();

            if (await codeService.IsAutoCodeEnabledAsync(CodeKeys.Supplier, db))
            {
                await codeService.ConsumeCodeAsync(CodeKeys.Supplier, db);
            }

            await transaction.CommitAsync();

            return new OutputResponse(entity.SupplierId, null);
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.GetBaseException().Message);
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
        try
        {
            var existing = await db.SupplierMasters.FindAsync(id);

            if (existing is null)
                return new OutputResponse(0, "Supplier not found.");

            db.SupplierMasters.Remove(existing);
            await db.SaveChangesAsync();

            return new OutputResponse(id, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }

    public async Task<List<SupplierMaster>> GetAllAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.SupplierMasters.AsNoTracking().ToListAsync();
    }

    public async Task<(List<SupplierMaster>, int)> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.SupplierMasters.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            query = query.Where(x => x.Name.Contains(pagination.search) || x.Code.Contains(pagination.search));
        }

        int count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            if (pagination.sortDirection == "desc")
            {
                query = pagination.sortColumn switch
                {
                    "Code" => query.OrderByDescending(x => x.Code),
                    "Name" => query.OrderByDescending(x => x.Name),
                    "Mobile" => query.OrderByDescending(x => x.Mobile),
                    "VatNo" => query.OrderByDescending(x => x.VatNo),
                    _ => query.OrderByDescending(x => x.SupplierId)
                };
            }
            else
            {
                query = pagination.sortColumn switch
                {
                    "Code" => query.OrderBy(x => x.Code),
                    "Name" => query.OrderBy(x => x.Name),
                    "Mobile" => query.OrderBy(x => x.Mobile),
                    "VatNo" => query.OrderBy(x => x.VatNo),
                    _ => query.OrderBy(x => x.SupplierId)
                };
            }
        }
        else
        {
            query = query.OrderByDescending(x => x.SupplierId);
        }

        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();

        return (list, count);
    }

    public async Task<SupplierMaster?> GetAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();

        return await db.SupplierMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SupplierId == id);
    }

    public async Task<OutputResponse> UpdateAsync(SupplierMaster entity)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var existing = await db.SupplierMasters
                .FirstOrDefaultAsync(x => x.SupplierId == entity.SupplierId);

            if (existing is null)
                return new OutputResponse(0, "Supplier not found.");

            var oldCode = existing.Code;

            // Update fields

            // Basic
            existing.Code = entity.Code;
            existing.Name = entity.Name;
            existing.NameArabic = entity.NameArabic;
            existing.IsActive = entity.IsActive;

            // Address
            existing.Address = entity.Address;
            existing.AddressArabic = entity.AddressArabic;

            // Contact
            existing.Contact = entity.Contact;
            existing.ContactArabic = entity.ContactArabic;
            existing.Telephone = entity.Telephone;
            existing.PhoneArabic = entity.PhoneArabic;
            existing.Mobile = entity.Mobile;
            existing.MobileArabic = entity.MobileArabic;
            existing.Email = entity.Email;

            // Financials
            existing.CreditLimit = entity.CreditLimit;
            existing.CreditDays = entity.CreditDays;
            existing.NeedsFill = entity.NeedsFill;
            existing.SupStatus = entity.SupStatus;
            existing.SuppBillType = entity.SuppBillType;
            existing.SuppType = entity.SuppType;
            existing.IsCustomer = entity.IsCustomer;

            // VAT
            existing.VatNo = entity.VatNo;
            existing.VatNoArabic = entity.VatNoArabic;
            existing.GroupVat = entity.GroupVat;

            // ZATCA Address
            existing.BuildingNo = entity.BuildingNo;
            existing.BuildingNoArabic = entity.BuildingNoArabic;
            existing.StreetName = entity.StreetName;
            existing.StreetNameArabic = entity.StreetNameArabic;
            existing.District = entity.District;
            existing.DistrictArabic = entity.DistrictArabic;
            existing.City = entity.City;
            existing.CityArabic = entity.CityArabic;
            existing.PostalCode = entity.PostalCode;
            existing.PostalCodeArabic = entity.PostalCodeArabic;
            existing.AdditionalNo = entity.AdditionalNo;
            existing.AdditionalNoArabic = entity.AdditionalNoArabic;
            existing.OtherSellerNo = entity.OtherSellerNo;
            existing.OtherSellerNoArabic = entity.OtherSellerNoArabic;
            existing.Neighbourhood = entity.Neighbourhood;
            existing.NeighbourhoodArabic = entity.NeighbourhoodArabic;

            // Identification
            existing.IdentificationNo = entity.IdentificationNo;
            existing.IdentificationNoArabic = entity.IdentificationNoArabic;
            existing.Identification = entity.Identification;

            // Foreign Keys
            existing.FK_CountryId = entity.FK_CountryId;
            existing.FK_PurchaserId = entity.FK_PurchaserId;

            // Integration: Update Account in Account Master
            AccountMaster? linkedAccount = null;

            // 1. Try to find by ID first (Best Practice)
            if (existing.FK_AccountId.HasValue)
            {
                linkedAccount = await db.AccountMasters.FirstOrDefaultAsync(x => x.AccountId == existing.FK_AccountId.Value);
            }

            // 2. Fallback: If not linked yet, try to find by OLD Code (Legacy Data Support)
            if (linkedAccount == null)
            {
                linkedAccount = await db.AccountMasters.FirstOrDefaultAsync(x => x.Code == oldCode);

                // Self-healing: If found by code, link it now for future
                if (linkedAccount != null)
                {
                    existing.FK_AccountId = linkedAccount.AccountId;
                }
            }

            if (linkedAccount != null)
            {
                // Update existing account details
                linkedAccount.Code = entity.Code;
                linkedAccount.Name = entity.Name;
                linkedAccount.NameArabic = entity.NameArabic;
            }
            else
            {
                // Create if missing - use Sundry Creditors SubGroup from built-in accounts
                var sundryCreditorsSubGroup = await db.AccountSubGroups.FirstOrDefaultAsync(x => x.Code == "21000101");

                if (sundryCreditorsSubGroup != null)
                {
                    // Get Liability Type ID
                    var liabilityType = await db.AccountTypes.FirstOrDefaultAsync(x => x.AcTypeName == "Liability");

                    var newAccount = new AccountMaster
                    {
                        Code = entity.Code,
                        Name = entity.Name,
                        NameArabic = entity.NameArabic,
                        FK_AccountHeadId = sundryCreditorsSubGroup.FK_AcHeadId,
                        FK_AcGroupId = sundryCreditorsSubGroup.FK_AcGroupId ?? 4,
                        FK_AccountSubGroupId = sundryCreditorsSubGroup.SubGroupId,
                        FK_AcTypeId = liabilityType?.AcTypeId ?? 2, // Default to 2 (Liability) if not found
                        DbCr = "Credit",
                        Opening = 0,
                        Flag = "Sup",
                        IsReserved = false // User-created accounts are not reserved
                    };
                    await db.AccountMasters.AddAsync(newAccount);
                    await db.SaveChangesAsync(); // Save to generate ID

                    // Link the new record
                    existing.FK_AccountId = newAccount.AccountId;
                }
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OutputResponse(existing.SupplierId, null);
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.GetBaseException().Message);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }
}



