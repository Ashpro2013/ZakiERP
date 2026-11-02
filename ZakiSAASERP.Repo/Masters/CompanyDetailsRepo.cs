namespace ZakiSAASERP.Repo.Masters;

public class CompanyDetailsRepo(IDbContextProvider _contextProvider) : ICompanyDetailsRepo
{
    public async Task<CompanyDetails?> GetByBranchIdAsync(int branchId)
    {

        await using var db = _contextProvider.CreateDbContext();
        try
        {
            return await db.CompanyDetails
                .FirstOrDefaultAsync(x => x.FK_BranchId == branchId);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving company details: {ex.Message}", ex);
        }
    }

    public async Task<OutputResponse> CreateAsync(CompanyDetails details)
    {
        await using var db = _contextProvider.CreateDbContext();
        try
        {
            // Check if company details already exist for this branch
            var exists = await ExistsForBranchAsync(details.FK_BranchId);
            if (exists)
            {
                return new OutputResponse(0, "Company details already exist for this branch. Please use update instead.");
            }

            await db.CompanyDetails.AddAsync(details);
            await db.SaveChangesAsync();

            return new OutputResponse(1, "Company details saved successfully.");
        }
        catch (Exception ex)
        {
            return new OutputResponse(0, $"Error saving company details: {ex.Message}");
        }
    }

    public async Task<OutputResponse> UpdateAsync(CompanyDetails details)
    {
        await using var db = _contextProvider.CreateDbContext();
        try
        {
            var existing = await db.CompanyDetails
                .FirstOrDefaultAsync(x => x.CompanyDetailId == details.CompanyDetailId);

            if (existing == null)
            {
                return new OutputResponse(0, "Company details not found.");
            }

            // Update all fields
            existing.Name = details.Name;
            existing.NameArabic = details.NameArabic;
            existing.Contact = details.Contact;
            existing.ContactArabic = details.ContactArabic;
            existing.Telephone = details.Telephone;
            existing.TelephoneArabic = details.TelephoneArabic;
            existing.Fax = details.Fax;
            existing.FaxArabic = details.FaxArabic;
            existing.Mobile = details.Mobile;
            existing.MobileArabic = details.MobileArabic;
            existing.Email = details.Email;
            existing.VatNo = details.VatNo;
            existing.VatNoArabic = details.VatNoArabic;
            existing.BuildingNo = details.BuildingNo;
            existing.BuildingNoArabic = details.BuildingNoArabic;
            existing.StreetName = details.StreetName;
            existing.StreetNameArabic = details.StreetNameArabic;
            existing.District = details.District;
            existing.DistrictArabic = details.DistrictArabic;
            existing.City = details.City;
            existing.CityArabic = details.CityArabic;
            existing.PostalCode = details.PostalCode;
            existing.PostalCodeArabic = details.PostalCodeArabic;
            existing.AdditionalNo = details.AdditionalNo;
            existing.AdditionalNoArabic = details.AdditionalNoArabic;
            existing.FK_CountryId = details.FK_CountryId;
            existing.OtherSellerId = details.OtherSellerId;
            existing.OtherSellerIdArabic = details.OtherSellerIdArabic;
            existing.IdentificationType = details.IdentificationType;
            existing.IdentificationNo = details.IdentificationNo;
            existing.IdentificationNoArabic = details.IdentificationNoArabic;
            existing.GroupVat = details.GroupVat;
            existing.HeaderImage = details.HeaderImage;
            existing.FooterImage = details.FooterImage;
            existing.CountrySubentity = details.CountrySubentity;
            existing.CountrySubentityArabic = details.CountrySubentityArabic;
            db.CompanyDetails.Update(existing);
            await db.SaveChangesAsync();

            return new OutputResponse(1, "Company details updated successfully.");
        }
        catch (Exception ex)
        {
            return new OutputResponse(0, $"Error updating company details: {ex.Message}");
        }
    }

    public async Task<bool> ExistsForBranchAsync(int branchId)
    {
        await using var db = _contextProvider.CreateDbContext();
        try
        {
            return await db.CompanyDetails.AnyAsync(x => x.FK_BranchId == branchId);
        }
        catch
        {
            return false;
        }
    }
}
