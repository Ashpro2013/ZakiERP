using ZakiSAASERP.Interface.Masters;
using ZakiSAASERP.Interface.Others;
using ZakiSAASERP.Interface.Transaction.Sales;
using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Models.Zatca;
using ZakiSAASERP.ZatcaEngine.Abstractions;
using ZakiSAASERP.ZatcaEngine.Models;
using AshproStringExtension;
using System.IO;
using ZatcaParty = ZakiSAASERP.ZatcaEngine.Models.Party;

namespace ZakiSAASERP.POS.Services;

public class ZatcaNewUploadService(
    IItemMasterRepo itemrepo,
    IGeneralRepo general,
    IMasterValueRepo masterValue,
    IICVRepo icvrepo,
    IZatcaEngine zatcaEngine,
    IZatcaInvoiceRepo zatcaInvoiceRepo) : IZatcaUploadService
{
    private ZakiSAASERP.ZatcaEngine.Enums.EnvironmentType _environment = ZakiSAASERP.ZatcaEngine.Enums.EnvironmentType.Simulation;

    public async Task<ZatcaUploadResponse> UploadZatcaInvoice(TransactionHdr master, List<TransactionDtls> transactions, CompanyDetails company, CustomerMaster customer, string InvType, bool blnUploadXML, bool isCash)
    {
        MasterValue csrDetails = await masterValue.GetAsync("CSRDetails", master.FK_BranchId);
        string sMode = (await masterValue.GetAsync("ZatcaMode", master.FK_BranchId)).Description!;
        return await GenerateZatcaInvoice(master, transactions, company, customer, InvType, sMode!, blnUploadXML, isCash, csrDetails);
    }

    public async Task<CSRResponse> GenerateCSR(CSRRequest request)
    {
        var company = request.company;
        var zatcaRequest = new CsrRequest
        {
            CommonName = request.CommonName,
            OrganizationName = request.Organization,
            OrganizationUnitName = request.UnitName,
            OrganizationIdentifier = request.OrganizationIdentifier,
            CountryCode = request.CountryCode,
            InvoiceType = request.InvoiceType,
            Location = request.Location,
            BusinessCategory = request.BusinessCategory,
            OTP = request.OTP,
            Environment = MapEnvironmentType(request.CSRGenerationType),
            SerialNumber = request.SerialNumber,
            Seller = company != null ? MapCompanyToParty(company) : null
        };

        var response = await zatcaEngine.GenerateCsrAsync(zatcaRequest);

        if (response.IsSuccess)
        {
            var previoushash = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
            int branchId = company != null && company.FK_BranchId > 0 ? company.FK_BranchId : 1;
            var mval = new MasterValue { BranchId = branchId, MasterType = "CSRDetails", Name = response.CsrPem, Description = response.PrivateKeyPem, FieldOne = response.BinarySecurityToken, FieldTwo = response.Secret, FieldThree = request.OTP, FieldFour = previoushash };
            await masterValue.AddAsync(mval);
            mval = new MasterValue { BranchId = branchId, MasterType = "ZatcaMode", Name = "ZatcaMode", Description = request.CSRGenerationType };
            await masterValue.AddAsync(mval);

            return new CSRResponse { ZatcaCSR = response.CsrPem, CSRSecret = response.PrivateKeyPem, CSIDToken = response.BinarySecurityToken, CSIDSecret = response.Secret, OTP = request.OTP, PreviousHash = previoushash, BranchId = branchId };
        }
        else
        {
            throw new InvalidOperationException(response.ErrorMessage ?? "ZATCA CSR / CSID Onboarding failed.");
        }
    }

    private async Task<ZatcaUploadResponse> GenerateZatcaInvoice(TransactionHdr master, List<TransactionDtls> transactions, CompanyDetails company, CustomerMaster customer, string InvType, string sMode, bool blnUploadXML, bool isCash, MasterValue csrDetails)
    {
        try
        {
            if (csrDetails == null)
            {
                await SaveErrorResponse("CSR Details Not Found - 308", "CSR Details Not Found", master.HdrId, InvType, master.HdrDate, master.HdrNo);
                return new ZatcaUploadResponse(false, "CSR Details Not Found", null);
            }

            string ZatcaCsr = csrDetails.Name!.Trim();
            string CsrSecret = csrDetails.Description!.Trim();
            string ZatcaTocken = csrDetails.FieldOne!.Trim();
            string CsidSecret = csrDetails.FieldTwo!.Trim();
            string ZatcaOTP = csrDetails.FieldThree!.Trim();
            string sPreviousHash = csrDetails.FieldFour!.Trim();

            var invoice = new Invoice();
            invoice.InvoiceNumber = master.HdrNo.ToString();
            invoice.IssueDate = Convert.ToDateTime(master.InvoiceDate);

            var vResult = GetInvoiceTypeName(InvType);
            string invoiceTypeName = vResult.Item1;
            bool updateHash = vResult.Item2;
            int invoiceTypeCode = GetInvoiceType(InvType);

            invoice.InvoiceTypeCode = invoiceTypeCode;
            invoice.InvoiceSubType = invoiceTypeName;
            invoice.PreviousInvoiceHash = sPreviousHash;

            invoice.Icv = await general.GetNewCode("icv", master.FK_BranchId);

            invoice.PaymentMeansCode = GetPaymentMethod(isCash ? "Cash" : "Credit");
            invoice.PaymentInstruction = "Payment Notes";

            decimal invoiceDiscount = Convert.ToDecimal(master.AdditionalDiscAmnt);
            if (invoiceDiscount > 0)
            {
                invoice.Allowances.Add(new Allowance
                {
                    ChargeIndicator = false,
                    Reason = "Discount",
                    Amount = invoiceDiscount,
                    TaxCategory = "S",
                    TaxRate = 15
                });
            }

            invoice.Seller = MapCompanyToParty(company);
            invoice.Buyer = MapCustomerToParty(customer);

            decimal totalAmount = 0;
            decimal totalVat = 0;

            foreach (var item in transactions)
            {
                var line = new InvoiceLine();
                line.Description = item.ItemName!;
                line.Quantity = Convert.ToDecimal(item.Quantity);
                line.UnitPrice = Convert.ToDecimal(item.ExRate);

                decimal discountVal = Convert.ToDecimal(item.DiscountAmnt);
                line.DiscountAmount = discountVal;

                line.VatRate = Convert.ToDecimal(item.TaxPerc);
                line.VatAmount = Convert.ToDecimal(item.TaxAmount);
                line.TaxCategory = item.TaxAmount == 0 ? "Z" : "S";

                if (item.TaxAmount == 0)
                {
                    var reason = await itemrepo.GetZeroTaxReason(item.FK_ItemId);
                    if (reason != null && !string.IsNullOrEmpty(reason.ExemptionCode))
                    {
                        line.TaxExemptionReasonCode = reason.ExemptionCode;
                        line.TaxExemptionReason = reason.ExemptionTextEN!;
                        if (!string.IsNullOrEmpty(reason.ZatcaTypeCode))
                        {
                            line.TaxCategory = reason.ZatcaTypeCode;
                        }
                    }
                }

                line.LineTotal = (line.UnitPrice * line.Quantity) - line.DiscountAmount;

                totalAmount += line.LineTotal + line.VatAmount;
                totalVat += line.VatAmount;

                invoice.Lines.Add(line);
            }

            invoice.TotalAmount = totalAmount;
            invoice.VatAmount = totalVat;

            var request = new ZatcaInvoiceRequest
            {
                Invoice = invoice,
                PrivateKeyPem = CsrSecret,
                CertificateBase64 = ZatcaTocken,
                BinarySecurityToken = ZatcaTocken,
                Secret = CsidSecret,
                Environment = MapEnvironmentType(sMode),
                InvoiceTypeName = invoiceTypeName,
                blnUploadXML = blnUploadXML
            };

            var result = await zatcaEngine.SubmitInvoiceAsync(request);

            if (result.IsSuccess)
            {
                try
                {
                    string locationpath;
                    if (OperatingSystem.IsWindows())
                    {
                        var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        locationpath = Path.Combine(myDocuments, "Zaki ERP");
                    }
                    else
                    {
                        locationpath = Path.Combine(Directory.GetCurrentDirectory(), company?.Name ?? "Company", "InvoiceXML");
                    }

                    Directory.CreateDirectory(locationpath);
                    string safeFileName = string.Join("_", invoice.InvoiceNumber.Split(Path.GetInvalidFileNameChars()));
                    if (string.IsNullOrWhiteSpace(safeFileName))
                    {
                        safeFileName = $"Invoice_{DateTime.Now:yyyyMMddHHmmss}";
                    }
                    var filePath = Path.Combine(locationpath, $"{safeFileName}.xml");
                    await File.WriteAllTextAsync(filePath, result.SignedXml);

                    if (OperatingSystem.IsWindows())
                    {
                        try
                        {
                            var xmlSubDir = Path.Combine(locationpath, "InvoiceXML");
                            Directory.CreateDirectory(xmlSubDir);
                            await File.WriteAllTextAsync(Path.Combine(xmlSubDir, $"{safeFileName}.xml"), result.SignedXml);
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    await SaveErrorResponse($"Generate XML Save Error", ex.Message, master.HdrId, InvType, master.HdrDate, master.HdrNo);
                }

                if (blnUploadXML)
                {
                    var saveResult = await SaveResponse(result, master.HdrId, InvType, master.HdrDate, master.HdrNo);

                    if (saveResult.Item1)
                    {
                        if (updateHash && result.IsSuccess)
                        {
                            await masterValue.UpdatePreviousHashAsync(result.InvoiceHash!, master.FK_BranchId);
                        }
                        return new ZatcaUploadResponse(true, null, result.QrCode!);
                    }
                    else
                    {
                        return new ZatcaUploadResponse(false, saveResult.Item2, null);
                    }
                }
                else
                {
                    if (updateHash)
                    {
                        await masterValue.UpdatePreviousHashAsync(result.InvoiceHash!, master.FK_BranchId);
                    }
                    await SavePendingInvoice(result, invoiceTypeName, master.HdrNo.ToInt32(), invoiceTypeCode.ToString(), master.HdrId, CsrSecret, CsidSecret, sMode, master.HdrDate, invoice.Icv, master.FK_BranchId);
                    return new ZatcaUploadResponse(true, null!, result.QrCode);
                }
            }
            else
            {
                await SaveErrorResponse("Zatca Integration Error", result.ErrorMessage!, master.HdrId, InvType, master.HdrDate, master.HdrNo);
                return new ZatcaUploadResponse(false, result.ErrorMessage, null);
            }

        }
        catch (Exception ex)
        {
            await SaveErrorResponse($"Zatca Integration Try Catch Error", ex.Message, master.HdrId, InvType, master.HdrDate, master.HdrNo);
            return new ZatcaUploadResponse(false, $"{ex.Message} - ZatcaNewUploadService", null);
        }
    }

    private ZatcaParty MapCompanyToParty(CompanyDetails company)
    {
        if (company == null) return null!;
        return new ZatcaParty
        {
            Name = !string.IsNullOrWhiteSpace(company.Name) ? company.Name : "Establishment",
            VatNumber = company.VatNo?.Trim() ?? "",
            IdentificationNumber = company.IdentificationNo?.Trim() ?? "",
            IdentificationScheme = !string.IsNullOrWhiteSpace(company.IdentificationType) ? company.IdentificationType.Trim() : "CRN",
            Street = !string.IsNullOrWhiteSpace(company.StreetName) ? company.StreetName : "King Fahd Road",
            BuildingNumber = !string.IsNullOrWhiteSpace(company.BuildingNo) ? company.BuildingNo : "1234",
            City = !string.IsNullOrWhiteSpace(company.City) ? company.City : "Riyadh",
            PostalCode = !string.IsNullOrWhiteSpace(company.PostalCode) ? company.PostalCode : "12345",
            District = !string.IsNullOrWhiteSpace(company.District) ? company.District : "Al Olaya",
            Subdivision = company.CountrySubentity ?? "",
            CountryCode = company.Country?.Code ?? "SA"
        };
    }

    private ZatcaParty MapCustomerToParty(CustomerMaster customer)
    {
        if (customer != null && !string.IsNullOrEmpty(customer.BuildingNo))
        {
            return new ZatcaParty
            {
                Name = customer.Name!,
                VatNumber = customer.VatNo!.Trim(),
                IdentificationNumber = customer.IdentificationNo!.Trim(),
                IdentificationScheme = customer.Identification?.Trim() ?? "CRN",
                Street = customer.StreetName!,
                BuildingNumber = customer.BuildingNo,
                City = customer.City!,
                PostalCode = customer.PostalCode!.Trim(),
                District = customer.District!,
                Subdivision = customer.CountrySubentity!,
                CountryCode = customer.Country?.Code ?? "SA"
            };
        }
        else
        {
            var defaultParty = new ZatcaParty
            {
                Street = "شارع صلاح الدين | Salah Al-Din",
                BuildingNumber = "1111",
                City = "الرياض | Riyadh",
                PostalCode = "12222",
                District = "المروج | Al-Murooj",
                CountryCode = "SA",
                Name = customer?.Name ?? "مستهلك نهائي | Consumer",
                VatNumber = customer?.VatNo?.Trim() ?? "399999999800003",
                IdentificationNumber = string.IsNullOrEmpty(customer?.IdentificationNo) ? "3104244153" : customer.IdentificationNo.Trim(),
                IdentificationScheme = customer?.Identification?.Trim() ?? "CRN"
            };
            return defaultParty;
        }
    }

    private int GetInvoiceType(string InvType)
    {
        return InvType switch
        {
            "Simplified Invoice" => 388,
            "Standard Invoice" => 388,
            "Simplified Debit Note" => 383,
            "Standard Debit Note" => 383,
            "Simplified Credit Note" => 381,
            "Standard Credit Note" => 381,
            _ => 388
        };
    }

    private string GetPaymentMethod(string PaymentMode)
    {
        return PaymentMode switch
        {
            "Cash" => "10",
            "Credit" => "30",
            "Bank Deposit" => "42",
            "Card" => "48",
            "FreeCharge" => "1",
            _ => "10"
        };
    }

    private string MapEnvironmentType(string environmentString)
    {
        return environmentString?.ToLower() switch
        {
            "production" => "Production",
            "core" => "Production",
            "simulation" => "Simulation",
            "developer" => "Developer",
            "sandbox" => "Sandbox",
            _ => "Developer"
        };
    }

    private (string, bool) GetInvoiceTypeName(string InvType)
    {
        string? InvoiceTypeName = null;
        bool updateHash = true;
        switch (InvType)
        {
            case "Simplified Invoice":
                InvoiceTypeName = "0200000";
                break;
            case "Standard Invoice":
                InvoiceTypeName = "0100000";
                break;
            case "Simplified Debit Note":
                InvoiceTypeName = "0200000";
                break;
            case "Standard Debit Note":
                InvoiceTypeName = "0100000";
                break;
            case "Simplified Credit Note":
                InvoiceTypeName = "0200000";
                break;
            case "Standard Credit Note":
                InvoiceTypeName = "0100000";
                break;
            default:
                InvoiceTypeName = "0100000";
                break;
        }
        return (InvoiceTypeName, updateHash);
    }

    private async Task<(bool, string)> SaveResponse(ZatcaSubmissionResult result, int invId, string invType, DateTime? date, string? voucherNo = null)
    {
        try
        {
            var icv = new InvoiceICV();
            icv.VoucherId = invId;
            icv.VoucherNo = voucherNo;
            icv.VoucherType = invType;
            icv.InvICVDate = date;

            icv.ICVStatus = result.IsSuccess;
            icv.DetailedStatus = result.ClearanceStatus ?? result.ReportingStatus ?? (result.IsSuccess ? "Success" : "Failed");
            icv.ICVMessage = result.ErrorMessage;

            icv.ICVInfoMessages = result.ErrorMessage;
            icv.InvoiceHash = result.InvoiceHash;

            return await icvrepo.SaveInvoiceICV(icv);
        }
        catch (Exception ex) { return (false, $"{ex.Message} - SaveResponse"); }
    }

    private async Task<(bool, string)> SaveErrorResponse(string ErrorType, string ErrorMessage, int id, string invType, DateTime? date, string? voucherNo = null)
    {
        try
        {
            var icv = new InvoiceICV();
            icv.VoucherId = id;
            icv.VoucherNo = voucherNo;
            icv.VoucherType = invType;
            icv.InvICVDate = date;
            icv.ICVStatus = false;
            icv.DetailedStatus = ErrorType;
            icv.ICVMessage = ErrorMessage;
            return await icvrepo.SaveInvoiceICV(icv);
        }
        catch (Exception ex) { return (false, $"{ex.Message} - SaveErrorResponse"); }
    }

    public async Task<(bool, string)> SavePendingInvoice(ZatcaSubmissionResult res, string InvoiceType, int InvNo, string sCode, int? InvoiceId, string certificate, string secret, string sMode, DateTime date, int InvoiceIcv, int iBranchId)
    {
        try
        {
            ZatcaInvoices invoices = new ZatcaInvoices
            {
                Id = 1,
                invoiceHash = res.InvoiceHash!,
                Code = sCode,
                EncodedInvoice = res.SignedXml,
                InvNo = InvNo.ToString(),
                InvoiceType = InvoiceType,
                FK_BranchId = iBranchId,
                InvoiceId = InvoiceId.ToInt32(),
                ZatcaCertificate = certificate,
                ZatcaSecret = secret,
                InvoiceDate = date,
                SMode = sMode,
                InvoiceIcv = InvoiceIcv
            };
            return await zatcaInvoiceRepo.SaveZatcaInvoice(invoices);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public Task UploadPendingSimplifiedInvoices(int BranchId)
    {
        return Task.CompletedTask;
    }
}

