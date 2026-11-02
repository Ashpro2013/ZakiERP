using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.Services;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Shared.Helpers;
using ZakiSAASERP.Shared.Services;
using ZakiSAASERP.Shared.SubWindows;
using AshproStringExtension;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ZakiSAASERP.Shared.Pages.Settings;

public partial class PrintingBarcodeView : ITabAware
{
    [Parameter] public int TabId { get; set; }

    private bool isLoading = false;
    private string loadingText = "";

    private List<CategoryMaster> categories = new();
    private int? selectedCategoryId = null;

    private MudTable<BarcodeData>? table;
    private int totalCatalogCount = 0;
    private List<BarcodeData> currentCatalogPageItems = new();
    private List<BarcodeData> selectedBarcodes = new();

    private List<BarcodeDesignHdr> barcodeDesigns = new();
    private int selectedDesignId = 0;

    private string searchString = "";
    private string typeFilter = "All"; // "All", "Base", "Multi"
    private int defaultAddQty = 1;
    private int batchSetQty = 1;

    private int TotalLabelsCount => selectedBarcodes.Sum(x => x.Quantity > 0 ? x.Quantity : 1);

    protected override async Task OnInitializedAsync()
    {
        try
        {
            isLoading = true;
            loadingText = AppData.IsArabic ? "جاري تحميل البيانات..." : "Loading data...";
            StateHasChanged();

            // 1. Load Categories for Product Group Dropdown
            var cats = await api.GetAllCategory();
            categories = cats?.OrderBy(c => c.Name).ToList() ?? new List<CategoryMaster>();

            // 2. Load Barcode Designs
            try
            {
                var designs = await api.GetAllBarcodeDesigns();
                barcodeDesigns = designs?.ToList() ?? new List<BarcodeDesignHdr>();
            }
            catch
            {
                barcodeDesigns = new List<BarcodeDesignHdr>();
            }

            // 3. Set default design if available
            try
            {
                var defaultDto = await api.GetBarcodeDesignAsync();
                if (defaultDto?.hdr != null)
                {
                    selectedDesignId = defaultDto.hdr.BarcodeDesignHdrId;
                    if (!barcodeDesigns.Any(x => x.BarcodeDesignHdrId == selectedDesignId))
                    {
                        barcodeDesigns.Insert(0, defaultDto.hdr);
                    }
                }
                else if (barcodeDesigns.Any())
                {
                    selectedDesignId = barcodeDesigns.First().BarcodeDesignHdrId;
                }
            }
            catch
            {
                if (barcodeDesigns.Any())
                {
                    selectedDesignId = barcodeDesigns.First().BarcodeDesignHdrId;
                }
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error loading barcode printing view: {ex.Message}", Severity.Error);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task<TableData<BarcodeData>> ServerReload(TableState state, CancellationToken token)
    {
        try
        {
            var pagination = new PaginationModel(
                page: state.Page + 1,
                pageSize: state.PageSize,
                sortColumn: typeFilter != "All" ? typeFilter : null,
                sortDirection: state.SortDirection == SortDirection.Descending ? "desc" : "asc",
                search: searchString,
                FilterId: string.IsNullOrWhiteSpace(searchString) ? selectedCategoryId : null
            );

            var result = await api.GetItemMultiUnitPDetails(pagination);
            if (result?.PItemMultiUnits != null)
            {
                totalCatalogCount = result.TotalCount;
                var list = result.PItemMultiUnits.Select(item => new BarcodeData
                {
                    ItemId = item.FK_ItemId,
                    ItemCode = item.ItemCode ?? item.ItemMaster?.ItemCode,
                    ItemName = !string.IsNullOrWhiteSpace(item.ItemName) ? item.ItemName : (item.ItemMaster?.ItemName ?? item.MultiUnitName),
                    ItemNameArabic = !string.IsNullOrWhiteSpace(item.ItemNameArabic) ? item.ItemNameArabic : (item.ItemMaster?.ItemNameArabic ?? item.MultiUnitArabicName),
                    Barcode = item.Barcode?.Trim(),
                    SalesRate = item.Rate,
                    InclusiveRate = item.IncRate,
                    PurchaseRate = item.Cost,
                    CategoryName = item.CategoryName ?? item.ItemMaster?.Category?.Name,
                    UnitName = !string.IsNullOrWhiteSpace(item.UnitName) ? item.UnitName : item.MultiUnitName,
                    Quantity = 1,
                    Label = (item.QTYPerUnit == 1 || item.QTYPerUnit == null) ? "Base Barcode" : "MultiBarcode"
                }).ToList();

                currentCatalogPageItems = list;
                StateHasChanged();

                return new TableData<BarcodeData>
                {
                    TotalItems = result.TotalCount,
                    Items = list
                };
            }

            totalCatalogCount = 0;
            currentCatalogPageItems = new();
            StateHasChanged();
            return new TableData<BarcodeData> { TotalItems = 0, Items = new List<BarcodeData>() };
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error loading barcodes: {ex.Message}", Severity.Error);
            totalCatalogCount = 0;
            currentCatalogPageItems = new();
            StateHasChanged();
            return new TableData<BarcodeData> { TotalItems = 0, Items = new List<BarcodeData>() };
        }
    }

    private async Task OnSearchChanged(string text)
    {
        searchString = text;
        if (table != null)
        {
            await table.ReloadServerData();
        }
    }

    private async Task OnCategoryChanged(int? catId)
    {
        selectedCategoryId = catId;
        if (table != null)
        {
            await table.ReloadServerData();
        }
    }

    private async Task OnTypeFilterChanged(string filter)
    {
        typeFilter = filter;
        if (table != null)
        {
            await table.ReloadServerData();
        }
    }

    private void OnDesignChanged(int designId)
    {
        selectedDesignId = designId;
    }

    private async Task AddToSelectedAsync(BarcodeData item)
    {
        var existing = selectedBarcodes.FirstOrDefault(x =>
            string.Equals(x.Barcode, item.Barcode, StringComparison.OrdinalIgnoreCase) &&
            x.ItemId == item.ItemId);

        int initialQty = existing != null && existing.Quantity > 0 ? existing.Quantity : (defaultAddQty > 0 ? defaultAddQty : 1);

        var parameters = new DialogParameters
        {
            ["Item"] = item,
            ["CurrentQuantity"] = initialQty,
            ["IsAlreadyInQueue"] = existing != null
        };

        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        var dialog = await DialogService.ShowAsync<BarcodeQuantityDialog>(
            AppData.IsArabic ? "تحديد كمية الباركود" : "Enter Barcode Quantity",
            parameters,
            options);

        var result = await dialog.Result;
        if (result != null && !result.Canceled && result.Data is int confirmedQty && confirmedQty > 0)
        {
            if (existing != null)
            {
                existing.Quantity = confirmedQty;
                snackbar.Add(AppData.IsArabic
                    ? $"تم تحديث كمية {item.ItemName} إلى {existing.Quantity}"
                    : $"Updated quantity of {item.ItemName} to {existing.Quantity}", Severity.Info);
            }
            else
            {
                var newItem = item.CopyTo<BarcodeData>();
                newItem.Quantity = confirmedQty;
                selectedBarcodes.Add(newItem);
                snackbar.Add(AppData.IsArabic
                    ? $"تمت إضافة {item.ItemName} ({item.Barcode}) بالكمية {confirmedQty}"
                    : $"Added {item.ItemName} ({item.Barcode}) with quantity {confirmedQty}", Severity.Success);
            }
            StateHasChanged();
        }
    }

    private void AddToSelected(BarcodeData item)
    {
        _ = AddToSelectedAsync(item);
    }

    private void AddAllFilteredAsync()
    {
        var itemsToAdd = currentCatalogPageItems;
        if (!itemsToAdd.Any()) return;

        int addedCount = 0;
        int qtyToAdd = defaultAddQty > 0 ? defaultAddQty : 1;

        foreach (var item in itemsToAdd)
        {
            var existing = selectedBarcodes.FirstOrDefault(x =>
                string.Equals(x.Barcode, item.Barcode, StringComparison.OrdinalIgnoreCase) &&
                x.ItemId == item.ItemId);

            if (existing != null)
            {
                existing.Quantity += qtyToAdd;
            }
            else
            {
                var newItem = item.CopyTo<BarcodeData>();
                newItem.Quantity = qtyToAdd;
                selectedBarcodes.Add(newItem);
            }
            addedCount++;
        }

        snackbar.Add(AppData.IsArabic
            ? $"تمت إضافة {addedCount} باركود إلى قائمة الطباعة"
            : $"Added {addedCount} barcodes to print queue", Severity.Success);
    }

    private void RemoveFromSelected(BarcodeData item)
    {
        selectedBarcodes.Remove(item);
    }

    private void ClearAllSelected()
    {
        selectedBarcodes.Clear();
        snackbar.Add(AppData.IsArabic ? "تم مسح القائمة المحددة" : "Cleared selected print queue", Severity.Info);
    }

    private void ApplyBatchQuantity()
    {
        if (batchSetQty < 1) batchSetQty = 1;
        foreach (var item in selectedBarcodes)
        {
            item.Quantity = batchSetQty;
        }
        snackbar.Add(AppData.IsArabic
            ? $"تم تحديد كمية الطباعة لجميع العناصر إلى {batchSetQty}"
            : $"Set all barcode print quantities to {batchSetQty}", Severity.Success);
    }

    private async Task PrintBarcodesAsync()
    {
        if (!selectedBarcodes.Any())
        {
            snackbar.Add(AppData.IsArabic ? "يرجى تحديد باركود واحد على الأقل للطباعة." : "Please select at least one barcode to print.", Severity.Warning);
            return;
        }

        try
        {
            isLoading = true;
            loadingText = AppData.IsArabic ? "جاري إرسال الباركود للطابعة..." : "Sending barcodes to printer...";
            StateHasChanged();

            // 1. Fetch target Barcode Design
            BarcodeDesignDTO? designDto = null;
            if (selectedDesignId > 0)
            {
                designDto = await api.GetBarcodeDesignAsync(selectedDesignId);
            }
            if (designDto == null || designDto.hdr == null)
            {
                designDto = await api.GetBarcodeDesignAsync();
            }

            if (designDto == null || designDto.hdr == null)
            {
                snackbar.Add(AppData.IsArabic
                    ? "لم يتم العثور على تصميم باركود. يرجى تهيئة تصميم الباركود أولاً من الإعدادات."
                    : "No Barcode Design found. Please configure a barcode design first.", Severity.Warning);
                return;
            }

            // 2. Prepare barcode print list
            var barcodeDataList = new List<BarcodeData>();
            foreach (var item in selectedBarcodes)
            {
                var copy = item.CopyTo<BarcodeData>();
                copy.Quantity = item.Quantity > 0 ? item.Quantity : 1;
                barcodeDataList.Add(copy);
            }

            // 3. Print via FormFactor or CrudHelper
            if (AppData.IsWindows)
            {
                var hdr = designDto.hdr;
                if (string.IsNullOrWhiteSpace(hdr.PrinterName))
                {
                    hdr.PrinterName = PermanentSettingsData.BarcodePrinterName;
                }
                var dtls = designDto.dtls ?? new List<BarcodeDesignDtls>();
                foreach (var d in dtls)
                {
                    if (string.IsNullOrEmpty(d.Prefix))
                        d.Prefix = "";
                }

                await formFactor.PrintBarcode(new BarcodeDesignDTO(hdr, dtls), barcodeDataList);
                snackbar.Add(AppData.IsArabic
                    ? $"تم إرسال {TotalLabelsCount} ملصق باركود إلى الطابعة بنجاح."
                    : $"Successfully sent {TotalLabelsCount} barcode labels to printer.", Severity.Success);
            }
            else
            {
                await CrudHelper.PrintBarcodeMethod(designDto, barcodeDataList, JSRuntime, Dialog);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error printing barcodes: {ex.Message}", Severity.Error);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task PreviewBarcodesAsync()
    {
        if (!selectedBarcodes.Any())
        {
            snackbar.Add(AppData.IsArabic ? "يرجى تحديد باركود واحد على الأقل للمعاينة." : "Please select at least one barcode to preview.", Severity.Warning);
            return;
        }

        try
        {
            isLoading = true;
            loadingText = AppData.IsArabic ? "جاري إنشاء معاينة الباركود..." : "Generating barcode preview...";
            StateHasChanged();

            BarcodeDesignDTO? designDto = null;
            if (selectedDesignId > 0)
            {
                designDto = await api.GetBarcodeDesignAsync(selectedDesignId);
            }
            if (designDto == null || designDto.hdr == null)
            {
                designDto = await api.GetBarcodeDesignAsync();
            }

            if (designDto == null || designDto.hdr == null)
            {
                snackbar.Add(AppData.IsArabic
                    ? "لم يتم العثور على تصميم باركود. يرجى تهيئة تصميم الباركود أولاً من الإعدادات."
                    : "No Barcode Design found. Please configure a barcode design first.", Severity.Warning);
                return;
            }

            var barcodeDataList = new List<BarcodeData>();
            foreach (var item in selectedBarcodes)
            {
                var copy = item.CopyTo<BarcodeData>();
                copy.Quantity = item.Quantity > 0 ? item.Quantity : 1;
                barcodeDataList.Add(copy);
            }

            await CrudHelper.PrintBarcodeMethodLocal(designDto, barcodeDataList, downloader: null, model: Dialog, isPreview: true);
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error generating preview: {ex.Message}", Severity.Error);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
}
