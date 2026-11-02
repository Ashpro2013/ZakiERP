using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.Services;
using MudBlazor;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Text;
using System.Text.Json;

namespace ZakiSAASERP.Shared.Pages.Others;

public partial class BarcodeDesignerView : IDisposable, ITabAware
{
    #region Variables
    [Parameter] public int TabId { get; set; }
    BarcodeDesignHdr hdr = new();
    BarcodeDesignDtls dtls = new();
    List<BarcodeDesignDtls> Controls = new();
    List<string> Fields = new();
    List<string> AlignmentList = ["Left", "Center", "Right"];
    List<string> FontStyle = ["Bold", "Italic", "Regular", "Underline", "Strikeout"];
    string? selectedField;
    string barcodeImageBase64 = null;
    string searchQuery = "";
    string[] FontFamilies =
    {
        "Helvetica",
        "ZapfDingbats",
        "TimesRoman",
        "Symbol",
        "Courier"
    };
    // Drag helpers
    BarcodeDesignDtls? dragTarget;
    double startX, startY;
    double originX, originY;

    private DotNetObjectReference<BarcodeDesignerView>? objRef;
    #endregion

    protected override void OnInitialized()
    {
        NewClick();
        // Ensure default barcode control if none
        if (!Controls.Any())
        {
            AddDefaultBarcode();
        }
        BarcodeData item = new BarcodeData();
        Fields = item.GetProperties();
        Fields.RemoveAll(x => x.EndsWith("Id", StringComparison.OrdinalIgnoreCase));
      
    }

    private void AddDefaultBarcode()
    {
        var barcode = new BarcodeDesignDtls
        {
            Name = "Barcode",
            Text = "123456789",
            Position_x = 34,
            Position_y = 18,
            Width = 200,
            Height = 100,
            Alignment = "Center",
            Format = "Code128",
            fontFamily = "Arial",
            fontSize = 12,
            fStyle = "Regular",
            Type = "Barcode"
        };
        Controls.Add(barcode);
        dtls = barcode;
    }

    public void Dispose()
    {
        objRef?.Dispose();
    }

    private void OnFieldChanged(string field)
    {
        selectedField = field;
    }

    private void AddControl()
    {
        if (string.IsNullOrEmpty(selectedField)) return;

        var newField = new BarcodeDesignDtls
        {
            Name = selectedField,
            Text = selectedField,
            Position_x = 50,
            Position_y = 50 + (Controls.Count * 10),
            Width = 100,
            Height = 25,
            Alignment = "Left",
            Format = "",
            fontFamily = "TimesRoman",
            fontSize = 14,
            fStyle = "Regular",
            Type = "Text",
            Prefix = ""
        };

        // If user selected generic "Barcode" field from list, maybe treat as text unless they mean the image?
        // Assuming "Barcode" in Fields list just means the text representation.
        // The image one is likely unique.

        Controls.Add(newField);
        dtls = newField;
        StateHasChanged();
    }

    private void RemoveControl()
    {
        if (dtls == null || !Controls.Contains(dtls)) return;

        Controls.Remove(dtls);
        if (Controls.Any())
            dtls = Controls.Last();
        else
            dtls = new BarcodeDesignDtls();
        StateHasChanged();
    }

    // Drag Logic matching PrintDesignerView
    void StartDrag(MouseEventArgs e, BarcodeDesignDtls c)
    {
        dragTarget = c;
        startX = e.ClientX;
        startY = e.ClientY;
        originX = c.Position_x;
        originY = c.Position_y;

        if (objRef == null) objRef = DotNetObjectReference.Create(this);
        // Use existing window.designer from PrintDesignerView logic
        _ = JS.InvokeVoidAsync("designer.attach", objRef);
    }

    [JSInvokable]
    public void OnMouseMove(double x, double y)
    {
        if (dragTarget is null) return;
        dragTarget.Position_x = (int)(originX + (x - startX));
        dragTarget.Position_y = (int)(originY + (y - startY));
        dtls = dragTarget;
        StateHasChanged();
    }

    [JSInvokable]
    public void OnMouseUp()
    {
        dragTarget = null;
    }

    void SelectControl(BarcodeDesignDtls c)
    {
        dtls = c;
    }

    (string Justify, string TextAlign) GetAlignmentCss(string alignment)
    => alignment.ToLower() switch
    {
        "left" => ("flex-start", "left"),
        "right" => ("flex-end", "right"),
        "center" => ("center", "center"),
        _ => ("center", "center")   // default
    };
    string GetStyle(BarcodeDesignDtls c)
    {
        var alignment = GetAlignmentCss(c.Alignment ?? "center");

        string fontWeight = "normal";
        string fontStyle = "normal";
        string textDecoration = "none";

        var styles = (c.fStyle ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var s in styles)
        {
            switch (s.Trim())
            {
                case "Bold":
                    fontWeight = "bold";
                    break;
                case "Italic":
                    fontStyle = "italic";
                    break;
                case "Underline":
                    textDecoration = "underline";
                    break;
                case "Strikeout":
                    textDecoration = "line-through";
                    break;
            }
        }

        return
            $"position:absolute;" +
            $"left:{c.Position_x}px;" +
            $"top:{c.Position_y}px;" +
            $"width:{c.Width}px;" +
            $"height:{c.Height}px;" +
            $"border:1px dashed #ccc;" +
            $"font-family:{c.fontFamily};" +
            $"font-size:{c.fontSize}px;" +
            $"font-style:{fontStyle};" +
            $"font-weight:{fontWeight};" +
            $"text-decoration:{textDecoration};" +
            $"display:flex;" +
            $"align-items:center;" +
            $"justify-content:{alignment.Justify};" +
            $"text-align:{alignment.TextAlign};" +
            $"color:black;" +
            $"cursor:move;" +
            $"white-space:nowrap;" +
            $"overflow:hidden;";
    }

    private async Task BarcodeGenerateMethod(bool IsPreview = false)
    {
        try
        {
            var printHdr = hdr;
            if (string.IsNullOrWhiteSpace(printHdr.PrinterName))
            {
                printHdr.PrinterName = PermanentSettingsData.BarcodePrinterName;
            }
            var printDtls = Controls ?? new List<BarcodeDesignDtls>();
            foreach (var d in printDtls)
            {
                if (string.IsNullOrEmpty(d.Prefix))
                    d.Prefix = "";
            }

            BarcodeDesignDTO dto = new BarcodeDesignDTO(printHdr, printDtls);
            var data = IsPreview ? GetSampleData() : GetSampleData().Where(x => x.ItemId == 101).ToList();

            if (IsPreview)
            {
                // Preview of barcode is identical to the Web project (opens InvoicePreviewDialog with rendered PDF)
                await CrudHelper.PrintBarcodeMethodLocal(dto, data, null, Dialog, true);
            }
            else
            {
                // Generate button: load the generated barcode in the preview panel
                 var barcodePreview = await CrudHelper.PrintBarcodeMethodLocal(dto, data, null, Dialog, false);
                    if (barcodePreview is not null && barcodePreview.Length > 0)
                    {
                        barcodeImageBase64 = $"data:application/pdf;base64,{Convert.ToBase64String(barcodePreview)}";
                    }
            }
        }
        catch (Exception ex)
        {
            snackbar?.Add(AppData.IsArabic
                ? $"خطأ أثناء معالجة الباركود: {ex.Message}"
                : $"Error processing barcode: {ex.Message}", Severity.Error);
        }
        StateHasChanged();
    }
    private async Task GenerateBarcode() => await BarcodeGenerateMethod();
    private async Task PreviewBarcode() => await BarcodeGenerateMethod(true);
    private List<BarcodeData> GetSampleData()
    {
        var barcodeList = new List<BarcodeData>
{
    new BarcodeData
    {
        ItemId = 101,
        ItemCode = "ITM-SAMS-A15",
        ItemName = "Samsung Galaxy A15",
        ItemNameArabic = "سامسونج جالكسي A15",
        BrandName = "Samsung",
        CategoryName = "Mobile Phones",
        SubCategoryName = "Android Phones",
        UnitName = "PCS",
        SupplierName = "Al Fahad Traders",
        Barcode = "890123456789",
        SalesRate = 4500.00m,
        PurchaseRate = 4200.00m,
        WholeSaleRate = 4350.00m,
        MinSaleRate = 4400.00m,
        InclusiveRate = 4725.00m,
        StockMode = "Inventory",
        Location = "Rack-A1",
        WeightPerCarton = "5 Kg",
        Quantity = 1
    },
    new BarcodeData
    {
        ItemId = 102,
        ItemCode = "ITM-HP-M15",
        ItemName = "HP Laptop M15",
        ItemNameArabic = "اتش بي لابتوب M15",
        BrandName = "HP",
        CategoryName = "Computers",
        SubCategoryName = "Laptops",
        UnitName = "PCS",
        SupplierName = "Gulf Tech Supplies",
        Barcode = "890123456790",
        SalesRate = 185000.00m,
        PurchaseRate = 175000.00m,
        WholeSaleRate = 180000.00m,
        MinSaleRate = 182000.00m,
        InclusiveRate = 194250.00m,
        StockMode = "Inventory",
        Location = "Rack-B3",
        WeightPerCarton = "12 Kg",
        Quantity = 4
    },
    new BarcodeData
    {
        ItemId = 103,
        ItemCode = "ITM-SONY-BT",
        ItemName = "Sony Bluetooth Speaker",
        ItemNameArabic = "مكبر صوت سوني",
        BrandName = "Sony",
        CategoryName = "Electronics",
        SubCategoryName = "Audio Devices",
        UnitName = "PCS",
        SupplierName = "City Electronics",
        Barcode = "890123456791",
        SalesRate = 3500.00m,
        PurchaseRate = 3000.00m,
        WholeSaleRate = 3200.00m,
        MinSaleRate = 3300.00m,
        InclusiveRate = 3675.00m,
        StockMode = "Inventory",
        Location = "Rack-C2",
        WeightPerCarton = "2 Kg",
        Quantity = 10
    },
    new BarcodeData
    {
        ItemId = 104,
        ItemCode = "ITM-LG-TV55",
        ItemName = "LG 55\" Smart TV",
        ItemNameArabic = "تلفزيون ال جي 55 بوصة",
        BrandName = "LG",
        CategoryName = "Home Appliances",
        SubCategoryName = "Televisions",
        UnitName = "PCS",
        SupplierName = "Bright Vision LLC",
        Barcode = "890123456792",
        SalesRate = 260000.00m,
        PurchaseRate = 245000.00m,
        WholeSaleRate = 252000.00m,
        MinSaleRate = 255000.00m,
        InclusiveRate = 273000.00m,
        StockMode = "Inventory",
        Location = "Rack-D1",
        WeightPerCarton = "18 Kg",
        Quantity = 1
    },
    new BarcodeData
    {
        ItemId = 105,
        ItemCode = "ITM-DELL-KB",
        ItemName = "Dell Keyboard USB",
        ItemNameArabic = "لوحة مفاتيح ديل",
        BrandName = "Dell",
        CategoryName = "Computer Accessories",
        SubCategoryName = "Keyboards",
        UnitName = "PCS",
        SupplierName = "Tech World",
        Barcode = "890123456793",
        SalesRate = 850.00m,
        PurchaseRate = 600.00m,
        WholeSaleRate = 700.00m,
        MinSaleRate = 750.00m,
        InclusiveRate = 892.50m,
        StockMode = "Inventory",
        Location = "Rack-A3",
        WeightPerCarton = "1 Kg",
        Quantity = 4
    }
};
        return barcodeList;
    }
    async Task SearchDesign()
    {
        var result = await Dialog.TransactionSearchMethod("Barcode Design");
        if (result is not null)
        {
            try
            {
                var dto = await api.GetBarcodeDesignAsync(result.Id);
                hdr = dto.hdr;
                Controls = dto.dtls;
                if (Controls.Any()) dtls = Controls.First();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Dialog.DialogMethod(ex.Message);
            }
        }
    }

    async Task SaveOrUpdate()
    {
        try
        {
            if (string.IsNullOrEmpty(hdr.DesignName) || !Controls.Any())
            {
                Dialog.DialogMethod("Please provide a design name and add at least one field.");
                return;
            }

            hdr.FK_BranchId = AppData.BranchId;
            var dto = new BarcodeDesignDTO(hdr, Controls);

            if (hdr.BarcodeDesignHdrId == 0)
            {
                var output = await api.CreateBarcodeDesignAsync(dto);
                if (string.IsNullOrEmpty(output.Message))
                {
                    Dialog.DialogMethod(Localizer("SaveMessage"));
                    NewClick();
                }
                else Dialog.DialogMethod(output.Message);
            }
            else
            {
                var output = await api.UpdateBarcodeDesignAsync(dto);
                if (string.IsNullOrEmpty(output.Message))
                {
                    Dialog.DialogMethod(Localizer("UpdateMessage"));
                    NewClick();
                }
                else Dialog.DialogMethod(output.Message);
            }
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    async Task Delete()
    {
        if (hdr.BarcodeDesignHdrId == 0) return;

        if (await Dialog.DeleteDialogMethod())
        {
            var output = await api.DeleteBarcodeDesignAsync(hdr.BarcodeDesignHdrId);
            if (string.IsNullOrEmpty(output.Message))
            {
                Dialog.DialogMethod("Deleted successfully.");
                NewClick();
            }
            else Dialog.DialogMethod(output.Message);
        }
    }

    void NewClick()
    {
        barcodeImageBase64 = null;
        hdr = new BarcodeDesignHdr
        {
            FirstMargin = 0,
            RightMargin = 0,
            TopMargin = 0,
            BottomMargin = 0,
            IsBorderIncluded = false,
            GenerateLabel = true,
            IsDefault = false,
            DesignName = "",
            Width = 300,
            Height = 156
        };
        Controls.Clear();
        AddDefaultBarcode();
        StateHasChanged();
    }
    async Task UploadFiles(IBrowserFile file)
    {
        try
        {
            // basic validation
            if (file == null)
                throw new Exception("No file selected.");

            // ensure JSON file
            if (file.ContentType != "application/json" &&
                !file.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Please upload a valid JSON file.");
            }

            using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);

            var dto = await JsonSerializer.DeserializeAsync<BarcodeDesignDTO>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto is null)
                throw new Exception("Invalid JSON format.");

            hdr = dto.hdr;
            hdr.BarcodeDesignHdrId = 0;
            Controls = dto.dtls ?? new List<BarcodeDesignDtls>();
            Controls.ForEach(c => c.BarcodeDesignDtlsId = 0);
            StateHasChanged();
        }
        catch (JsonException)
        {
            Dialog.DialogMethod("The uploaded file is not valid JSON.");
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod($"{ex.Message}_427");
        }

    }

    async Task DownloadJson()
    {
        try
        {
            var obj = new BarcodeDesignDTO(hdr, Controls);
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var data = Encoding.UTF8.GetBytes(json);
            await downloader.DownloadFromStream(data, $"{hdr.DesignName}_barcode.json");
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
}
