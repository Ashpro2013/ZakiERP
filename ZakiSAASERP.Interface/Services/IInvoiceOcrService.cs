namespace ZakiSAASERP.Interface.Services;

using ZakiSAASERP.Models.DTO;

/// <summary>
/// Service for processing vendor purchase invoices and receipts using multimodal AI models (e.g. Gemini).
/// </summary>
public interface IInvoiceOcrService
{
    /// <summary>
    /// Parses an invoice from image or PDF bytes and returns structured ERP invoice data.
    /// </summary>
    Task<PurchaseInvoiceOcrResult> ParseInvoiceAsync(byte[] fileBytes, string fileName, string? customApiKey = null, string? modelName = null);

    /// <summary>
    /// Returns realistic sample invoice data for testing or offline demonstration.
    /// </summary>
    PurchaseInvoiceOcrResult GetSampleInvoiceData();
}

