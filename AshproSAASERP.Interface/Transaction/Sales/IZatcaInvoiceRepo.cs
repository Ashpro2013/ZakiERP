using AshproSAASERP.Models.Zatca;

namespace AshproSAASERP.Interface.Transaction.Sales;

public interface IZatcaInvoiceRepo
{
    Task<(bool, string)> SaveZatcaInvoice(ZatcaInvoices zatcaInvoices);
    Task<(bool, string)> DeleteZatcaInvoice(int id);
    Task<ZatcaInvoiceDTO> GetAllZatcaInvoiceWithPagination(PaginationModel pagination);
    Task<List<ZatcaInvoices>> GetAllZatcaInvoiceByBranchId(int BranchId);
}
