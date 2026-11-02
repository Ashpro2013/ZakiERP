
using ZakiSAASERP.Interface.Transaction.Accounts;
using ZakiSAASERP.Models.Transactions.Accounts;
using ZakiSAASERP.Repo.CodeGenerationServices;
using Microsoft.Extensions.Logging;

namespace ZakiSAASERP.Repo.Accounts;

public class CustomerReceiptRepo : ICustomerReceiptRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly ILogger<CustomerReceiptRepo> _logger;
    private readonly CodeSequenceService _codeService;

    public CustomerReceiptRepo(IDbContextProvider contextProvider, ILogger<CustomerReceiptRepo> logger, CodeSequenceService codeService)
    {
        _contextProvider = contextProvider;
        _logger = logger;
        _codeService = codeService;
    }

    public async Task<string> GetNextReceiptNoAsync()
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            return await _codeService.GetNextUniqueCodeAsync(
                "CustomerReceipt",
                async (code) => await db.Set<CustomerReceiptHdr>().AnyAsync(x => x.ReceiptNo == code)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next receipt number");
            throw;
        }
    }

    public async Task<OutputResponse> CreateAsync(CustomerReceiptDTO dto)
    {
        await using var _db = _contextProvider.CreateDbContext();
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Check for duplicate receipt number and get a new one if exists
            var exists = await _db.Set<CustomerReceiptHdr>()
                .AnyAsync(r => r.ReceiptNo == dto.ReceiptNo
                              && r.FK_BranchId == dto.FK_BranchId);

            if (exists)
            {
                // Get a new unique receipt number recursively
                _logger.LogWarning($"Receipt number {dto.ReceiptNo} already exists, generating new one");
                dto.ReceiptNo = await GetNextReceiptNoAsync();

                // Recursively call CreateAsync with the new receipt number
                return await CreateAsync(dto);
            }

            // Validate total amount matches allocated + advance
            if (Math.Abs(dto.TotalAmount - (dto.AllocatedAmount + dto.AdvanceAmount)) > 0.01m)
            {
                return new OutputResponse(0, "Total amount must equal allocated amount plus advance amount.");
            }

            // Create header
            var header = new CustomerReceiptHdr
            {
                ReceiptNo = dto.ReceiptNo,
                ReceiptDate = dto.ReceiptDate,
                FK_CustomerId = dto.FK_CustomerId,
                FK_BranchId = dto.FK_BranchId,
                FK_UserId = dto.FK_UserId,
                FK_ProjectId = dto.FK_ProjectId,
                FK_AccountId = dto.FK_AccountId,
                PaymentMode = dto.PaymentMode,
                BankName = dto.BankName,
                ChequeNo = dto.ChequeNo,
                ChequeDate = dto.ChequeDate,
                BankAccountNo = dto.BankAccountNo,
                RvNumber = dto.RvNumber,
                TotalAmount = dto.TotalAmount,
                AdvanceAmount = dto.AdvanceAmount,
                AllocatedAmount = dto.AllocatedAmount,
                Remarks = dto.Remarks,
                CreatedDate = DateTime.Now,
                IsDeleted = false
            };

            _db.Set<CustomerReceiptHdr>().Add(header);
            await _db.SaveChangesAsync();

            // Add details if any invoices are being paid
            if (dto.Details != null && dto.Details.Any())
            {
                int recordNo = 1;
                foreach (var detail in dto.Details.Where(d => d.PaymentAmount > 0))
                {
                    detail.FK_CustomerReceiptId = header.CustomerReceiptId;
                    detail.RecordNo = recordNo++;
                    detail.RemainingBalance = detail.OutstandingAmount - detail.PaymentAmount - detail.Discount;

                    // Set FK_InvoiceId to null for opening entries
                    if (detail.IsOpeningEntry)
                    {
                        detail.FK_InvoiceId = null;
                    }

                    _db.Set<CustomerReceiptDtls>().Add(detail);
                }
                await _db.SaveChangesAsync();
            }

            // Create accounting entries
            await CreateAccountingEntriesAsync(_db, header);

            await transaction.CommitAsync();
            return new OutputResponse(header.CustomerReceiptId, "Customer receipt created successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating customer receipt");
            return new OutputResponse(0, $"Error: {ex.Message}");
        }
    }

    public async Task<OutputResponse> UpdateAsync(CustomerReceiptDTO dto)
    {
        await using var _db = _contextProvider.CreateDbContext();
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Validate total amount
            if (Math.Abs(dto.TotalAmount - (dto.AllocatedAmount + dto.AdvanceAmount)) > 0.01m)
            {
                return new OutputResponse(0, "Total amount must equal allocated amount plus advance amount.");
            }

            var existing = await _db.Set<CustomerReceiptHdr>()
                .Include(r => r.Details)
                .FirstOrDefaultAsync(r => r.CustomerReceiptId == dto.CustomerReceiptId);

            if (existing == null)
            {
                return new OutputResponse(0, "Receipt not found.");
            }

            // Update header
            existing.ReceiptDate = dto.ReceiptDate;
            existing.FK_CustomerId = dto.FK_CustomerId;
            existing.FK_ProjectId = dto.FK_ProjectId;
            existing.FK_AccountId = dto.FK_AccountId;
            existing.PaymentMode = dto.PaymentMode;
            existing.BankName = dto.BankName;
            existing.ChequeNo = dto.ChequeNo;
            existing.ChequeDate = dto.ChequeDate;
            existing.BankAccountNo = dto.BankAccountNo;
            existing.RvNumber = dto.RvNumber;
            existing.TotalAmount = dto.TotalAmount;
            existing.AdvanceAmount = dto.AdvanceAmount;
            existing.AllocatedAmount = dto.AllocatedAmount;
            existing.Remarks = dto.Remarks;
            existing.ModifiedDate = DateTime.Now;

            // Remove old details
            _db.Set<CustomerReceiptDtls>().RemoveRange(existing.Details);

            // Add updated details
            if (dto.Details != null && dto.Details.Any())
            {
                int recordNo = 1;
                foreach (var detail in dto.Details.Where(d => d.PaymentAmount > 0))
                {
                    var newDetail = new CustomerReceiptDtls
                    {
                        FK_CustomerReceiptId = existing.CustomerReceiptId,
                        FK_InvoiceId = detail.IsOpeningEntry ? null : detail.FK_InvoiceId,
                        InvoiceNo = detail.InvoiceNo,
                        InvoiceDate = detail.InvoiceDate,
                        InvoiceAmount = detail.InvoiceAmount,
                        ReturnAmount = detail.ReturnAmount,
                        OutstandingAmount = detail.OutstandingAmount,
                        PaidAmount = detail.PaidAmount,
                        PaymentAmount = detail.PaymentAmount,
                        Discount = detail.Discount,
                        Narration = detail.Narration,
                        RemainingBalance = detail.OutstandingAmount - detail.PaymentAmount - detail.Discount,
                        RecordNo = recordNo++
                    };
                    _db.Set<CustomerReceiptDtls>().Add(newDetail);
                }
            }

            // Delete old accounting entries
            var oldEntries = await _db.Set<TblAccounting>()
                .Where(a => a.VoucherId == existing.CustomerReceiptId)
                .ToListAsync();
            _db.Set<TblAccounting>().RemoveRange(oldEntries);

            // Create new accounting entries
            await CreateAccountingEntriesAsync(_db, existing);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(existing.CustomerReceiptId, "Customer receipt updated successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating customer receipt");
            return new OutputResponse(0, $"Error: {ex.Message}");
        }
    }

    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var _db = _contextProvider.CreateDbContext();
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var receipt = await _db.Set<CustomerReceiptHdr>()
                .FirstOrDefaultAsync(r => r.CustomerReceiptId == id);

            if (receipt == null)
            {
                return new OutputResponse(0, "Receipt not found.");
            }

            // Soft delete
            receipt.IsDeleted = true;
            receipt.ModifiedDate = DateTime.Now;

            // Delete accounting entries
            var accountingEntries = await _db.Set<TblAccounting>()
                .Where(a => a.VoucherId == receipt.CustomerReceiptId)
                .ToListAsync();
            _db.Set<TblAccounting>().RemoveRange(accountingEntries);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(1, "Customer receipt deleted successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting customer receipt");
            return new OutputResponse(0, $"Error: {ex.Message}");
        }
    }

    public async Task<List<CustomerReceiptHdr>> GetAllAsync()
    {
        try
        {
            await using var _db = _contextProvider.CreateDbContext();
            return await _db.Set<CustomerReceiptHdr>()
                .Include(r => r.Customer)
                .Include(r => r.Branch)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.ReceiptDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer receipts");
            throw;
        }
    }

    public async Task<CustomerReceiptDTO?> GetByIdAsync(int id)
    {
        try
        {
            await using var _db = _contextProvider.CreateDbContext();
            var receipt = await _db.Set<CustomerReceiptHdr>()
                .Include(r => r.Customer)
                .Include(r => r.Details)
                .FirstOrDefaultAsync(r => r.CustomerReceiptId == id);

            if (receipt == null) return null;

            var dto = new CustomerReceiptDTO
            {
                CustomerReceiptId = receipt.CustomerReceiptId,
                ReceiptNo = receipt.ReceiptNo,
                ReceiptDate = receipt.ReceiptDate,
                FK_CustomerId = receipt.FK_CustomerId,
                FK_BranchId = receipt.FK_BranchId,
                FK_UserId = receipt.FK_UserId,
                FK_ProjectId = receipt.FK_ProjectId,
                FK_AccountId = receipt.FK_AccountId,
                PaymentMode = receipt.PaymentMode,
                BankName = receipt.BankName,
                ChequeNo = receipt.ChequeNo,
                ChequeDate = receipt.ChequeDate,
                BankAccountNo = receipt.BankAccountNo,
                RvNumber = receipt.RvNumber,
                TotalAmount = receipt.TotalAmount,
                AdvanceAmount = receipt.AdvanceAmount,
                AllocatedAmount = receipt.AllocatedAmount,
                Remarks = receipt.Remarks,
                CustomerName = receipt.Customer?.Name,
                Details = receipt.Details.ToList()
            };

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer receipt by ID");
            throw;
        }
    }

    public async Task<List<OutstandingInvoiceDTO>> GetCustomerOutstandingInvoicesAsync(int customerId, int branchId)
    {
        try
        {
            await using var _db = _contextProvider.CreateDbContext();
            // Get all invoices for the customer
            var invoices = await _db.Set<ZakiSAASERP.Models.Transactions.Sales.SalesInvoiceHdr>()
                .Include(i => i.Customer)
                .Where(i => i.FK_CustomerId == customerId
                    && i.FK_BranchId == branchId
                    && !i.IsDeleted
                    && i.BillType.ToLower() == "credit")
                .Select(i => new
                {
                    i.SalesInvoiceId,
                    i.SalesInvoiceNo,
                    i.SalesInvoiceDate,
                    i.GrandTotal,
                    i.Customer!.Name
                })
                .ToListAsync();

            var result = new List<OutstandingInvoiceDTO>();

            foreach (var invoice in invoices)
            {
                var paymentData = await _db.Set<CustomerReceiptDtls>()
                    .Join(_db.Set<CustomerReceiptHdr>(),
                        dtl => dtl.FK_CustomerReceiptId,
                        hdr => hdr.CustomerReceiptId,
                        (dtl, hdr) => new { dtl, hdr })
                    .Where(x => x.dtl.FK_InvoiceId == invoice.SalesInvoiceId && !x.hdr.IsDeleted)
                    .GroupBy(x => 1)
                    .Select(g => new
                    {
                        TotalPayment = g.Sum(x => x.dtl.PaymentAmount),
                        TotalDiscount = g.Sum(x => x.dtl.Discount)
                    })
                    .FirstOrDefaultAsync();

                // Calculate return amount for this invoice
                var returnData = await _db.Set<ZakiSAASERP.Models.Transactions.Sales.SalesInvoiceRtnHdr>()
                    .Where(r => r.SalesInvoiceId == invoice.SalesInvoiceId
                        && !r.IsDeleted
                        && r.BillType.ToLower() == "credit") // Only credit returns affect outstanding
                    .GroupBy(x => 1)
                    .Select(g => new
                    {
                        TotalReturn = g.Sum(r => r.GrandTotal)
                    })
                    .FirstOrDefaultAsync();

                var totalPaid = (paymentData?.TotalPayment ?? 0m) + (paymentData?.TotalDiscount ?? 0m);
                var totalReturn = returnData?.TotalReturn ?? 0m;

                // Outstanding = Invoice Total - Paid - Returned
                var outstanding = invoice.GrandTotal - totalPaid - totalReturn;

                // Only include if there's outstanding amount
                if (outstanding > 0)
                {
                    result.Add(new OutstandingInvoiceDTO
                    {
                        InvoiceId = invoice.SalesInvoiceId,
                        InvoiceNo = invoice.SalesInvoiceNo ?? "",
                        InvoiceDate = invoice.SalesInvoiceDate,
                        TotalAmount = invoice.GrandTotal,
                        PaidAmount = totalPaid,
                        ReturnAmount = totalReturn,
                        OutstandingAmount = outstanding,
                        CustomerName = invoice.Name,
                        IsOpeningEntry = false
                    });
                }
            }

            // Get opening entries and their payments
            var openingEntries = await _db.Set<ZakiSAASERP.Models.Masters.CustomerOPEntryUpdated>()
                .Where(x => x.FK_CustomerId == customerId
                    && x.FK_BranchId == branchId)
                .Select(x => new
                {
                    x.CustomerOPEntryUpdatedId,
                    x.InvoiceNo,
                    x.InvoiceDate,
                    x.InvoiceAmt,
                    x.ReceivedAmt,
                    x.DiscountAmt,
                    x.ReturnedAmt,
                    x.BalanceAmt
                })
                .ToListAsync();

            foreach (var opening in openingEntries)
            {
                var receiptInvoiceNo = (opening.InvoiceNo ?? "OPENING") + " - OPENING";

                var receiptPaymentData = await _db.Set<CustomerReceiptDtls>()
                    .Join(_db.Set<CustomerReceiptHdr>(),
                        dtl => dtl.FK_CustomerReceiptId,
                        hdr => hdr.CustomerReceiptId,
                        (dtl, hdr) => new { dtl, hdr })
                    .Where(x => x.hdr.FK_CustomerId == customerId && x.dtl.InvoiceNo == receiptInvoiceNo && x.dtl.FK_InvoiceId == null && !x.hdr.IsDeleted)
                    .GroupBy(x => 1)
                    .Select(g => new
                    {
                        TotalPayment = g.Sum(x => x.dtl.PaymentAmount),
                        TotalDiscount = g.Sum(x => x.dtl.Discount)
                    })
                    .FirstOrDefaultAsync();

                var receiptPaid = (receiptPaymentData?.TotalPayment ?? 0m) + (receiptPaymentData?.TotalDiscount ?? 0m);

                var alreadyPaidInOpening = (opening.ReceivedAmt ?? 0) + (opening.DiscountAmt ?? 0);
                var totalPaid = alreadyPaidInOpening + receiptPaid;
                var totalReturn = opening.ReturnedAmt ?? 0;

                var totalInvoiceAmt = opening.InvoiceAmt ?? 0;
                var outstanding = totalInvoiceAmt - totalPaid - totalReturn;

                if (outstanding > 0.01m)
                {
                    result.Add(new OutstandingInvoiceDTO
                    {
                        InvoiceId = opening.CustomerOPEntryUpdatedId,
                        InvoiceNo = receiptInvoiceNo,
                        InvoiceDate = opening.InvoiceDate,
                        TotalAmount = totalInvoiceAmt,
                        PaidAmount = totalPaid,
                        ReturnAmount = totalReturn,
                        OutstandingAmount = outstanding,
                        CustomerName = "",
                        IsOpeningEntry = true
                    });
                }
            }

            return result.OrderBy(i => i.InvoiceDate).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer outstanding invoices and opening entries");
            throw;
        }
    }

    public async Task<decimal> GetCustomerBalanceAsync(int customerId, int branchId)
    {
        try
        {
            await using var _db = _contextProvider.CreateDbContext();
            var customer = await _db.Set<ZakiSAASERP.Models.Masters.CustomerMaster>()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null) return 0;

            int? accountId = customer.FK_AccountId;

            // Fallback strategy if FK_AccountId is missing, matching CustomerReportRepo
            if (accountId == null || accountId == 0)
            {
                var account = await _db.Set<ZakiSAASERP.Models.Masters.AccountMaster>()
                    .FirstOrDefaultAsync(a => a.Code == customer.Code);
                accountId = account?.AccountId;
            }

            if (accountId == null || accountId == 0) return 0;

            // Calculate balance from General Ledger (TblAccounting)
            // Summing all amounts for the account. Assuming Debit is positive and Credit is negative (or handled by sign in Amount).
            var balance = await _db.Set<ZakiSAASERP.Models.Transactions.Accounts.TblAccounting>()
                .Where(x => x.FK_AccountId == accountId && (branchId == 0 || x.FK_BranchId == branchId))
                .SumAsync(x => x.Amount ?? 0);

            return balance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating customer balance");
            throw;
        }
    }

    private async Task CreateAccountingEntriesAsync(TenantDBContext _db, CustomerReceiptHdr receipt)
    {
        try
        {
            // Get customer account ID (assuming customer account exists in AccountMaster)
            // Debit: Selected Cash/Bank Account (from FK_AccountId)
            // Credit: Customer Account

            var customer = await _db.Set<ZakiSAASERP.Models.Masters.CustomerMaster>()
                .FirstOrDefaultAsync(c => c.CustomerId == receipt.FK_CustomerId);

            if (customer == null) return;

            // Use the selected account from receipt (FK_AccountId)
            int cashAccountId = receipt.FK_AccountId ?? 0;

            // If no account selected, fall back to default cash/bank account (backward compatibility)
            if (cashAccountId == 0)
            {
                if (receipt.PaymentMode == "Cash")
                {
                    var cashAccount = await _db.Set<ZakiSAASERP.Models.Masters.AccountMaster>()
                        .FirstOrDefaultAsync(a => a.Code == "110001010001"); // Cash in Hand code
                    cashAccountId = cashAccount?.AccountId ?? 0;
                }
                else if (receipt.PaymentMode == "Bank" || receipt.PaymentMode == "Cheque")
                {
                    var bankAccount = await _db.Set<ZakiSAASERP.Models.Masters.AccountMaster>()
                        .FirstOrDefaultAsync(a => a.Code == "110001020001"); // Bank Account code
                    cashAccountId = bankAccount?.AccountId ?? 0;
                }
            }

            // Get customer's account
            var customerAccount = await _db.Set<ZakiSAASERP.Models.Masters.AccountMaster>()
                .FirstOrDefaultAsync(a => a.Code == customer.Code);

            if (cashAccountId > 0 && customerAccount != null)
            {
                // Debit entry (Selected Cash/Bank Account)
                _db.Set<TblAccounting>().Add(new TblAccounting
                {
                    EntryDate = receipt.ReceiptDate,
                    VoucherId = receipt.CustomerReceiptId,
                    VoucherNo = receipt.ReceiptNo,
                    FK_AccountId = cashAccountId,
                    Amount = receipt.TotalAmount,
                    Narration = $"Receipt from {customer.Name} - {receipt.Remarks}",
                    Type = "CustomerReceipt",
                    BillType = receipt.PaymentMode,
                    FK_BranchId = receipt.FK_BranchId,
                    FK_UserId = receipt.FK_UserId,
                    FK_CustomerId = receipt.FK_CustomerId
                });

                // Credit entry (Customer)
                _db.Set<TblAccounting>().Add(new TblAccounting
                {
                    EntryDate = receipt.ReceiptDate,
                    VoucherId = receipt.CustomerReceiptId,
                    VoucherNo = receipt.ReceiptNo,
                    FK_AccountId = customerAccount.AccountId,
                    Amount = -receipt.TotalAmount, // Credit (-ve)
                    Narration = $"Receipt from {customer.Name} - {receipt.Remarks}",
                    Type = "CustomerReceipt",
                    BillType = receipt.PaymentMode,
                    FK_BranchId = receipt.FK_BranchId,
                    FK_UserId = receipt.FK_UserId,
                    FK_CustomerId = receipt.FK_CustomerId
                });
            }

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating accounting entries");
            throw;
        }
    }
}
