using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZakiSAASERP.Interface.Services;
using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Services;

namespace ZakiSAASERP.Shared.Services;

/// <summary>
/// Multimodal AI invoice & receipt OCR service powered by Google Gemini API.
/// Extracts supplier information, tax numbers, and itemized billing rows.
/// </summary>
public class InvoiceOcrService : IInvoiceOcrService
{
    private readonly HttpClient _httpClient;

    public InvoiceOcrService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(90) };
    }

    public async Task<PurchaseInvoiceOcrResult> ParseInvoiceAsync(
        byte[] fileBytes,
        string fileName,
        string? customApiKey = null,
        string? modelName = null)
    {
        if (fileBytes == null || fileBytes.Length == 0)
        {
            throw new ArgumentException("The uploaded file is empty.", nameof(fileBytes));
        }

        string apiKey = ResolveApiKey(customApiKey);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Gemini API Key is not configured. Please enter your Gemini API Key in the AI Settings.");
        }

        string mimeType = GetMimeType(fileName);
        string model = !string.IsNullOrWhiteSpace(modelName)
            ? modelName
            : (!string.IsNullOrWhiteSpace(PermanentSettingsData.AiModelName) ? PermanentSettingsData.AiModelName : "gemini-2.5-flash");

        string requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        string base64Data = Convert.ToBase64String(fileBytes);

        var prompt = @"You are an expert AI ERP Accounting Assistant. Analyze this purchase invoice/receipt document (which may be in English, Arabic, or bilingual).
Extract all invoice header information and every line item into valid, strict JSON matching this structure:
{
  ""supplierName"": ""Supplier / Vendor business name"",
  ""supplierVatNo"": ""15-digit VAT / Tax Registration Number (e.g. 300xxxxxxxxxxxx), or empty if none"",
  ""invoiceNo"": ""Supplier's invoice or bill number"",
  ""invoiceDate"": ""YYYY-MM-DD"",
  ""billType"": ""Cash or Credit"",
  ""currency"": ""SAR or USD or local currency"",
  ""grossAmount"": 0.0,
  ""discountAmount"": 0.0,
  ""taxAmount"": 0.0,
  ""grandTotal"": 0.0,
  ""remarks"": ""Summary or payment terms if visible"",
  ""lineItems"": [
    {
      ""sl"": 1,
      ""itemName"": ""Product description in English"",
      ""itemArabicName"": ""Product description in Arabic (keep original Arabic from invoice or translate accurately)"",
      ""barcode"": ""Barcode / Item code if visible, otherwise empty"",
      ""quantity"": 1.0,
      ""unitName"": ""Unit (e.g. PCS, BOX, KG, L, CTN, etc.)"",
      ""unitPrice"": 0.0,
      ""discountPerc"": 0.0,
      ""discountAmount"": 0.0,
      ""taxPerc"": 15.0,
      ""taxAmount"": 0.0,
      ""lineTotal"": 0.0
    }
  ]
}
Rules:
1. Extract line items accurately from tables. Do not hallucinate or omit items.
2. If taxPerc is not explicitly stated on lines, default to 15.0 for Saudi Arabia VAT invoices unless marked zero-rated.
3. Compute unitPrice excluding VAT. If invoice only gives rate including VAT, set unitPrice = round(rate / (1 + (taxPerc / 100)), 2).
4. Return ONLY valid JSON.";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt },
                        new
                        {
                            inlineData = new
                            {
                                mimeType = mimeType,
                                data = base64Data
                            }
                        }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                temperature = 0.1
            }
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(requestUrl, jsonContent);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to communicate with AI endpoint: {ex.Message}", ex);
        }

        string responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            string errorMessage = ExtractGeminiError(responseJson) ?? $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
            throw new InvalidOperationException($"AI OCR failed: {errorMessage}");
        }

        return ParseGeminiResponse(responseJson);
    }

    public PurchaseInvoiceOcrResult GetSampleInvoiceData()
    {
        return new PurchaseInvoiceOcrResult
        {
            SupplierName = "Al-Madina Cleaning & Trading Supplies Co.",
            SupplierVatNo = "300123456789013",
            InvoiceNo = "INV-2026-9842",
            InvoiceDate = DateTime.Today,
            BillType = "Credit",
            Currency = "SAR",
            GrossAmount = 1273.00m,
            DiscountAmount = 0.00m,
            TaxAmount = 190.95m,
            GrandTotal = 1463.95m,
            Remarks = "Payment terms: Net 30 days. Auto-scanned sample invoice.",
            LineItems = new List<PurchaseInvoiceOcrLineItem>
            {
                new PurchaseInvoiceOcrLineItem
                {
                    SL = 1,
                    ItemName = "101 Toilet Cleaner-PCs",
                    ItemArabicName = "101 منظف المرحاض-PCs",
                    Barcode = "628100100101",
                    Quantity = 20,
                    UnitName = "PCS",
                    UnitPrice = 15.00m,
                    DiscountPerc = 0,
                    DiscountAmount = 0,
                    TaxPerc = 15.00m,
                    TaxAmount = 45.00m,
                    LineTotal = 345.00m,
                    IsSelected = true
                },
                new PurchaseInvoiceOcrLineItem
                {
                    SL = 2,
                    ItemName = "Floor Cleaner Liquid 5L",
                    ItemArabicName = "سائل تنظيف الأرضيات 5 لتر",
                    Barcode = "628100100102",
                    Quantity = 10,
                    UnitName = "PCS",
                    UnitPrice = 45.00m,
                    DiscountPerc = 0,
                    DiscountAmount = 0,
                    TaxPerc = 15.00m,
                    TaxAmount = 67.50m,
                    LineTotal = 517.50m,
                    IsSelected = true
                },
                new PurchaseInvoiceOcrLineItem
                {
                    SL = 3,
                    ItemName = "Microfiber Cleaning Cloths (Pack of 10)",
                    ItemArabicName = "أقمشة تنظيف مايكروفايبر (عبوة 10)",
                    Barcode = "628100100103",
                    Quantity = 15,
                    UnitName = "BOX",
                    UnitPrice = 25.00m,
                    DiscountPerc = 0,
                    DiscountAmount = 0,
                    TaxPerc = 15.00m,
                    TaxAmount = 56.25m,
                    LineTotal = 431.25m,
                    IsSelected = true
                },
                new PurchaseInvoiceOcrLineItem
                {
                    SL = 4,
                    ItemName = "Bleach Disinfectant 1 Gallon",
                    ItemArabicName = "مبيض ومطهر 1 جالون",
                    Barcode = "628100100104",
                    Quantity = 8,
                    UnitName = "PCS",
                    UnitPrice = 18.50m,
                    DiscountPerc = 0,
                    DiscountAmount = 0,
                    TaxPerc = 15.00m,
                    TaxAmount = 22.20m,
                    LineTotal = 170.20m,
                    IsSelected = true
                }
            }
        };
    }

    private static string ResolveApiKey(string? customApiKey)
    {
        if (!string.IsNullOrWhiteSpace(customApiKey)) return customApiKey.Trim();
        if (!string.IsNullOrWhiteSpace(PermanentSettingsData.AiApiKey)) return PermanentSettingsData.AiApiKey.Trim();

        string? envKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                      ?? Environment.GetEnvironmentVariable("AI_API_KEY");
        return envKey?.Trim() ?? string.Empty;
    }

    private static string GetMimeType(string fileName)
    {
        string ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/jpeg"
        };
    }

    private static string? ExtractGeminiError(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("error", out var errorEl))
            {
                if (errorEl.TryGetProperty("message", out var msgEl))
                {
                    return msgEl.GetString();
                }
            }
        }
        catch { }
        return null;
    }

    private static PurchaseInvoiceOcrResult ParseGeminiResponse(string geminiResponseJson)
    {
        using var doc = JsonDocument.Parse(geminiResponseJson);

        if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("AI response contains no candidates.");
        }

        var firstCandidate = candidates[0];
        if (!firstCandidate.TryGetProperty("content", out var content) ||
            !content.TryGetProperty("parts", out var parts) ||
            parts.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("AI response content structure was unexpected.");
        }

        var part = parts[0];
        if (!part.TryGetProperty("text", out var textEl))
        {
            throw new InvalidOperationException("AI response part has no text.");
        }

        string rawText = textEl.GetString()?.Trim() ?? "{}";

        // Remove potential markdown code fences if model enclosed JSON
        if (rawText.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            rawText = rawText.Substring(7);
        }
        else if (rawText.StartsWith("```"))
        {
            rawText = rawText.Substring(3);
        }
        if (rawText.EndsWith("```"))
        {
            rawText = rawText.Substring(0, rawText.Length - 3);
        }
        rawText = rawText.Trim();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        var parsed = JsonSerializer.Deserialize<RawOcrDto>(rawText, options)
                     ?? throw new InvalidOperationException("Failed to deserialize AI JSON response.");

        var result = new PurchaseInvoiceOcrResult
        {
            SupplierName = parsed.SupplierName,
            SupplierVatNo = CleanVatNo(parsed.SupplierVatNo),
            InvoiceNo = parsed.InvoiceNo,
            InvoiceDate = ParseDate(parsed.InvoiceDate),
            BillType = !string.IsNullOrWhiteSpace(parsed.BillType) ? parsed.BillType : "Credit",
            Currency = !string.IsNullOrWhiteSpace(parsed.Currency) ? parsed.Currency : "SAR",
            GrossAmount = parsed.GrossAmount,
            DiscountAmount = parsed.DiscountAmount,
            TaxAmount = parsed.TaxAmount,
            GrandTotal = parsed.GrandTotal,
            Remarks = parsed.Remarks
        };

        int sl = 1;
        if (parsed.LineItems != null)
        {
            foreach (var item in parsed.LineItems)
            {
                decimal qty = item.Quantity > 0 ? item.Quantity : 1;
                decimal unitPrice = item.UnitPrice >= 0 ? item.UnitPrice : 0;
                decimal discPerc = item.DiscountPerc;
                decimal discAmt = item.DiscountAmount > 0 ? item.DiscountAmount : (qty * unitPrice * discPerc / 100);
                decimal taxable = Math.Max(0, (qty * unitPrice) - discAmt);
                decimal taxPerc = item.TaxPerc >= 0 ? item.TaxPerc : 15.0m;
                decimal taxAmt = item.TaxAmount > 0 ? item.TaxAmount : (taxable * taxPerc / 100);
                decimal lineTotal = item.LineTotal > 0 ? item.LineTotal : (taxable + taxAmt);

                result.LineItems.Add(new PurchaseInvoiceOcrLineItem
                {
                    SL = sl++,
                    ItemName = item.ItemName?.Trim(),
                    ItemArabicName = item.ItemArabicName?.Trim(),
                    Barcode = item.Barcode?.Trim(),
                    Quantity = qty,
                    UnitName = !string.IsNullOrWhiteSpace(item.UnitName) ? item.UnitName.Trim().ToUpperInvariant() : "PCS",
                    UnitPrice = unitPrice,
                    DiscountPerc = discPerc,
                    DiscountAmount = discAmt,
                    TaxPerc = taxPerc,
                    TaxAmount = taxAmt,
                    LineTotal = lineTotal,
                    IsSelected = true
                });
            }
        }

        // Recalculate summary if model omitted totals
        if (result.GrandTotal == 0 && result.LineItems.Any())
        {
            result.GrossAmount = result.LineItems.Sum(x => x.Quantity * x.UnitPrice);
            result.DiscountAmount = result.LineItems.Sum(x => x.DiscountAmount);
            result.TaxAmount = result.LineItems.Sum(x => x.TaxAmount);
            result.GrandTotal = result.LineItems.Sum(x => x.LineTotal);
        }

        return result;
    }

    private static string? CleanVatNo(string? rawVat)
    {
        if (string.IsNullOrWhiteSpace(rawVat)) return null;
        var digitsOnly = new string(rawVat.Where(char.IsDigit).ToArray());
        return digitsOnly.Length >= 10 ? digitsOnly : rawVat.Trim();
    }

    private static DateTime? ParseDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return null;
        if (DateTime.TryParse(dateStr, out var dt)) return dt;
        return null;
    }

    private class RawOcrDto
    {
        public string? SupplierName { get; set; }
        public string? SupplierVatNo { get; set; }
        public string? InvoiceNo { get; set; }
        public string? InvoiceDate { get; set; }
        public string? BillType { get; set; }
        public string? Currency { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string? Remarks { get; set; }
        public List<RawOcrLineItemDto>? LineItems { get; set; }
    }

    private class RawOcrLineItemDto
    {
        public string? ItemName { get; set; }
        public string? ItemArabicName { get; set; }
        public string? Barcode { get; set; }
        public decimal Quantity { get; set; }
        public string? UnitName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPerc { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPerc { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
    }
}

