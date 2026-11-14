using ZakiSAASERP.Repo.CodeGenerationServices;

namespace ZakiSAASERP.Repo.Masters;

public class CustomerMasterRepo(IDbContextProvider contextProvider, CodeSequenceService codeService) : ICustomerMasterRepo
{
    public async Task<string> GetNextCustomerCodeAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await codeService.GetNextUniqueCodeAsync(
            CodeKeys.Customer,
            async (code) => await db.CustomerMasters.AnyAsync(x => x.Code == code)
        );
    }

    public async Task<OutputResponse> CreateAsync(CustomerMaster entity)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var exists = await db.CustomerMasters.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
            if (exists) return new OutputResponse(0, "Customer already exists.");
            var acctSubGrp = (await db.AccountSubGroups.Include(x => x.AccountHead).FirstOrDefaultAsync(x => x.Name == "Sundry Debtors"));
            var accountMaster = new AccountMaster
            {
                Code = entity.Code,
                Name = entity.Name,
                FK_AcTypeId = acctSubGrp.AccountHead.FK_AcTypeId,
                FK_AccountHeadId = acctSubGrp.FK_AcHeadId,
                FK_AcGroupId = acctSubGrp.FK_AcGroupId ?? 0,
                FK_AccountSubGroupId = acctSubGrp.SubGroupId,
                NameArabic = entity.NameArabic,
                DbCr = "Debit",
                Opening = 0,
                Flag = "Cus",
                IsReserved = false
            };
            await db.AccountMasters.AddAsync(accountMaster);
            await db.SaveChangesAsync();
            entity.FK_AccountId = accountMaster.AccountId;
            await db.CustomerMasters.AddAsync(entity);
            await db.SaveChangesAsync();

            if (await codeService.IsAutoCodeEnabledAsync(CodeKeys.Customer, db))
            {
                await codeService.ConsumeCodeAsync(CodeKeys.Customer, db);
            }

            if (entity.LocationDetails != null && entity.LocationDetails.Any())
            {
                foreach (var item in entity.LocationDetails)
                {
                    item.FK_CustomerId = entity.CustomerId;
                    await db.CustomerLocationDetails.AddAsync(item);
                }
                await db.SaveChangesAsync();

            }

            // Integration: Save Account in Account Master
            // Find Sundry Debtors SubGroup (code: 11000201) from built-in accounts
            var sundryDebtorsSubGroup = await db.AccountSubGroups.FirstOrDefaultAsync(x => x.Code == "11000201");

            if (sundryDebtorsSubGroup != null)
            {
                var accountExists = await db.AccountMasters.AnyAsync(x => x.Code == entity.Code);
                if (!accountExists)
                {
                    // Get Asset Type ID
                    var assetType = await db.AccountTypes.FirstOrDefaultAsync(x => x.AcTypeName == "Asset");

                    var newAccount = new AccountMaster
                    {
                        Code = entity.Code,
                        Name = entity.Name,
                        NameArabic = entity.NameArabic,
                        FK_AccountHeadId = sundryDebtorsSubGroup.FK_AcHeadId,
                        FK_AcGroupId = sundryDebtorsSubGroup.FK_AcGroupId ?? 2,
                        FK_AccountSubGroupId = sundryDebtorsSubGroup.SubGroupId,
                        FK_AcTypeId = assetType?.AcTypeId ?? 1,
                        DbCr = "Debit",
                        Opening = 0,
                        Flag = "Cus",
                        IsReserved = false
                    };
                    await db.AccountMasters.AddAsync(newAccount);
                    await db.SaveChangesAsync();

                    entity.FK_AccountId = newAccount.AccountId;
                }
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OutputResponse(entity.CustomerId, null);
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
            var existing = await db.CustomerMasters.FindAsync(id);

            if (existing is null)
                return new OutputResponse(0, "Customer not found.");
            var existinAcctMaster = await db.AccountMasters.FirstOrDefaultAsync(x => x.AccountId == existing.FK_AccountId);
            if (existinAcctMaster is not null)
                db.AccountMasters.Remove(existinAcctMaster);
            db.CustomerMasters.Remove(existing);
            await db.SaveChangesAsync();

            return new OutputResponse(id, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }

    public async Task<List<CustomerMaster>> GetAllAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.CustomerMasters.AsNoTracking().ToListAsync();
    }

    public async Task<(List<CustomerMaster>, int)> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.CustomerMasters.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            query = query.Where(x => x.Name!.Contains(pagination.search) 
            || x.Code.Contains(pagination.search) 
            || x.VatNo!.Contains(pagination.search)
            || x.VatNoArabic!.Contains(pagination.search)
            || x.Mobile!.Contains(pagination.search)
            || x.MobileArabic!.Contains(pagination.search)
            || x.PhoneArabic!.Contains(pagination.search)
            || x.NameArabic!.Contains(pagination.search)
            || x.Telephone!.Contains(pagination.search));
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
                    _ => query.OrderByDescending(x => x.CustomerId)
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
                    _ => query.OrderBy(x => x.CustomerId)
                };
            }
        }
        else
        {
            query = query.OrderByDescending(x => x.CustomerId);
        }

        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();

        return (list, count);
    }

    public async Task<CustomerMaster?> GetAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();

        var customer = await db.CustomerMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CustomerId == id);

        if (customer != null)
        {
            customer.LocationDetails = await db.CustomerLocationDetails
               .AsNoTracking()
               .Where(x => x.FK_CustomerId == id)
               .Join(db.CustomerLocationMasters,
                     d => d.FK_CustomerLocationId,
                     m => m.LocationId,
                     (d, m) => new CustomerLocationDetails
                     {
                         CustomerLocationDetailsId = d.CustomerLocationDetailsId,
                         FK_CustomerId = d.FK_CustomerId,
                         FK_CustomerLocationId = d.FK_CustomerLocationId,
                         Remarks = d.Remarks,
                         LocationName = m.Name
                     })
               .ToListAsync();
        }

        return customer;
    }

    public async Task<OutputResponse> UpdateAsync(CustomerMaster entity)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var existing = await db.CustomerMasters
                .FirstOrDefaultAsync(x => x.CustomerId == entity.CustomerId);

            if (existing is null)
                return new OutputResponse(0, "Customer not found.");
            if (entity.FK_AccountId is null)
            {
                var acctSubGrp = (await db.AccountSubGroups.Include(x => x.AccountHead).FirstOrDefaultAsync(x => x.Name == "Sundry Debtors"));
                var accountMaster = new AccountMaster
                {
                    Code = entity.Code,
                    Name = entity.Name,
                    FK_AcTypeId = acctSubGrp.AccountHead.FK_AcTypeId,
                    FK_AccountHeadId = acctSubGrp.FK_AcHeadId,
                    FK_AcGroupId = acctSubGrp.FK_AcGroupId ?? 0,
                    FK_AccountSubGroupId = acctSubGrp.SubGroupId,
                    NameArabic = entity.NameArabic,
                    DbCr = "Debit",
                    Opening = 0,
                    Flag = "Cus",
                    IsReserved = false
                };
                await db.AccountMasters.AddAsync(accountMaster);
                await db.SaveChangesAsync();
                entity.FK_AccountId = accountMaster.AccountId;
            }
            // Update fields
            // Basic Info
            // Basic Info
            var oldCode = existing.Code;
            existing.Code = entity.Code;
            existing.Name = entity.Name;
            existing.NameArabic = entity.NameArabic;

            // Address
            existing.Address = entity.Address;
            existing.AddressArabic = entity.AddressArabic;

            // Financials
            existing.CreditLimit = entity.CreditLimit;
            existing.CreditDays = entity.CreditDays;

            // Contact
            existing.Contact = entity.Contact;
            existing.ContactArabic = entity.ContactArabic;
            existing.Telephone = entity.Telephone;
            existing.PhoneArabic = entity.PhoneArabic;
            existing.Mobile = entity.Mobile;
            existing.MobileArabic = entity.MobileArabic;
            existing.Email = entity.Email;

            // VAT
            existing.VatNo = entity.VatNo;
            existing.VatNoArabic = entity.VatNoArabic;

            // ZATCA / Address Details
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

            // Identification
            existing.IdentificationNo = entity.IdentificationNo;
            existing.IdentificationNoArabic = entity.IdentificationNoArabic;
            existing.Identification = entity.Identification;

            // Foreign Keys
            existing.FK_CustomerGroupId = entity.FK_CustomerGroupId;
            existing.FK_CountryId = entity.FK_CountryId;
            existing.FK_SalesmanId = entity.FK_SalesmanId;
            existing.FK_AccountId = entity.FK_AccountId;

            // Billing / Misc
            existing.CusBillType = entity.CusBillType;
            existing.NeedsFill = entity.NeedsFill;

            existing.IsActive = entity.IsActive;
            existing.IsSupplier = entity.IsSupplier;
            existing.DiscountPercentage = entity.DiscountPercentage;

            // ZATCA Extra
            existing.Neighbourhood = entity.Neighbourhood;
            existing.NeighbourhoodArabic = entity.NeighbourhoodArabic;
            existing.GroupVat = entity.GroupVat;

            // Update Location Details
            var existingDetails = await db.CustomerLocationDetails.Where(x => x.FK_CustomerId == entity.CustomerId).ToListAsync();
            db.CustomerLocationDetails.RemoveRange(existingDetails);

            if (entity.LocationDetails != null && entity.LocationDetails.Any())
            {
                foreach (var item in entity.LocationDetails)
                {
                    item.FK_CustomerId = entity.CustomerId;
                    // Reset ID to 0 to ensure insert
                    item.CustomerLocationDetailsId = 0;
                    await db.CustomerLocationDetails.AddAsync(item);
                }
            }

            // Integration: Update Account in Account Master
            var existingAccount = await db.AccountMasters.FirstOrDefaultAsync(x => x.Code == oldCode);
            if (existingAccount != null)
            {
                existingAccount.Code = entity.Code;
                existingAccount.Name = entity.Name;
                existingAccount.NameArabic = entity.NameArabic;

                // Ensure FK_AccountId is set
                existing.FK_AccountId = existingAccount.AccountId;
            }
            else
            {
                // Create if missing - use Sundry Debtors SubGroup from built-in accounts
                var sundryDebtorsSubGroup = await db.AccountSubGroups.FirstOrDefaultAsync(x => x.Code == "11000201");

                if (sundryDebtorsSubGroup != null)
                {
                    // Get Asset Type ID
                    var assetType = await db.AccountTypes.FirstOrDefaultAsync(x => x.AcTypeName == "Asset");

                    var newAccount = new AccountMaster
                    {
                        Code = entity.Code,
                        Name = entity.Name,
                        NameArabic = entity.NameArabic,
                        FK_AccountHeadId = sundryDebtorsSubGroup.FK_AcHeadId,
                        FK_AcGroupId = sundryDebtorsSubGroup.FK_AcGroupId ?? 2,
                        FK_AccountSubGroupId = sundryDebtorsSubGroup.SubGroupId,
                        FK_AcTypeId = assetType?.AcTypeId ?? 1, // Default to 1 (Asset) if not found
                        DbCr = "Debit",
                        Opening = 0,
                        Flag = "Cus",
                        IsReserved = false // User-created accounts are not reserved
                    };
                    await db.AccountMasters.AddAsync(newAccount);
                    await db.SaveChangesAsync();

                    // Store AccountId on CustomerMaster
                    existing.FK_AccountId = newAccount.AccountId;
                }
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OutputResponse(existing.CustomerId, null);
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

    public async Task<decimal> GetCustomerOutstandingBalanceAsync(int customerId)
    {
        await using var db = contextProvider.CreateDbContext();
        var customer = await db.CustomerMasters.AsNoTracking().FirstOrDefaultAsync(x => x.CustomerId == customerId);
        if (customer == null) return 0;

        int? accId = customer.FK_AccountId;
        if (accId == null || accId == 0)
        {
            var acc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == customer.Code);
            accId = acc?.AccountId;
        }

        if (accId == null || accId == 0) return 0;

        var transactions = await db.TblAccountings.AsNoTracking()
            .Where(x => x.FK_AccountId == accId)
            .ToListAsync();

        decimal balance = 0;
        foreach (var t in transactions)
        {
            decimal dr = 0, cr = 0;
            decimal amt = t.Amount ?? 0;
            if (t.Type == "Sales") dr = amt;
            else if (t.Type == "CustomerReceipt") cr = Math.Abs(amt);
            else if (t.Type == "JournalEntry" || t.Type == "ExpenseEntry" || t.Type == "OpeningBalance" || t.Type == "CustomerOPEntry")
            {
                if (t.BillType == "Debit") dr = amt;
                else if (t.BillType == "Credit") cr = Math.Abs(amt);
                else { if (amt >= 0) dr = amt; else cr = Math.Abs(amt); }
            }
            else { if (amt >= 0) dr = amt; else cr = Math.Abs(amt); }
            balance += (dr - cr);
        }
        return balance;
    }
}



