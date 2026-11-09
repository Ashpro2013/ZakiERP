using static MudBlazor.CategoryTypes;

namespace ZakiSAASERP.Shared.Pages.Reports.Transaction;

public partial class ZatcaPendingView
{
    public DateTime? dtmFrom { get; set; } = DateTime.Now;
    public DateTime? dtmTo { get; set; } = DateTime.Now;
    public int SalesCount { get; set; } = 0;
    public int CreditNoteCount { get; set; } = 0;
    public int DebitNoteCount { get; set; } = 0;
    public int FailedCount { get; set; }
    public int WarningCount { get; set; }
    public int SuccessCount { get; set; }
    public int StandardCount { get; set; }
    public int SimplifiedCount { get; set; }
    public bool ShowFirst { get; set; } = false;
    public bool ShowLast { get; set; } = false;
    public bool ShowSecond { get; set; } = false;
    public bool ShowGrid { get; set; } = false;
    public bool ShowSelected { get; set; } = false;
    public bool ShowingFailed { get; set; } = false;
    public int iTypeId { get; set; } = 0;
    public List<ExInvoiceICV>? IcvList { get; set; }
    private ExInvoiceICV? selectedIcv = null;
    private MudDataGrid<ExInvoiceICV>? grid = new();
    public bool IsInvoice { get; set; } = true;
    public bool IsSimplified { get; set; } = true;
    [Parameter] public int TabId { get; set; }
    protected override void OnInitialized()
    {
        dtmFrom = dtmFrom?.Date.AddDays(-7);
        dtmTo = dtmTo?.Date.AddHours(24).AddMinutes(-1);
        StateHasChanged();
    }
    async Task Generate()
    {
        ShowFirst = true;
        var dto = new ICVReqDTO(dtmFrom, dtmTo, AppData.BranchId, 1);
        var result = await api.GetPendingICVData(dto);
        SalesCount = result.CountOne;
        CreditNoteCount = result.CountTwo;
        DebitNoteCount = 0;
    }
    async Task FirstClick(int iType)
    {
        iTypeId = iType;
        ShowSecond = true;
        var dto = new ICVReqDTO(dtmFrom, dtmTo, AppData.BranchId, iType);
        var result = await api.GetPendingICVData(dto);
        StandardCount = result.CountOne;
        SimplifiedCount = result.CountTwo;
        IsInvoice = iType == 2;
    }
    async Task SecondClick(int iType)
    {
        ShowLast = true;
        IsSimplified = iType != 1;
        iTypeId = IsInvoice ? (IsSimplified ? 5 : 4) : (IsSimplified ? 6 : 7);
        var dto = new ICVReqDTO(dtmFrom, dtmTo, AppData.BranchId, iTypeId);
        var result = await api.GetPendingICVData(dto);
        SuccessCount  = result.CountOne;
        WarningCount = result.CountTwo;
        FailedCount = result.CountThree;
    }
    async Task LastClick(int iType)
    {
        selectedIcv = null;
        ShowSelected = false;
        ShowingFailed = iType == 3 ? true : false;
        ShowGrid = true;
        iTypeId = iType switch
        {
            1 => IsSimplified ? (IsInvoice ? 4 : 10) : (IsInvoice ? 1 : 7),
            2 => IsSimplified ? (IsInvoice ? 5 : 11) : (IsInvoice ? 2 : 8),
            _ => IsSimplified ? (IsInvoice ? 6 : 12) : (IsInvoice ? 3 : 9),
        };
        await grid!.ReloadServerData();
    }
    private async Task<GridData<ExInvoiceICV>> LoadServerData(GridState<ExInvoiceICV> state)
    {
        var pagination = new PaginationModel(state.Page + 1, state.PageSize, state.SortDefinitions?.FirstOrDefault()?.SortBy,
            state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc", null, AppData.BranchId);
        var pdto = new ICVReqDTO(dtmFrom, dtmTo, AppData.BranchId, iTypeId);
        var dto = new IcvPendReqDTO(pagination, pdto);
        var result = await api.GetPendingICVList(dto);
        return new GridData<ExInvoiceICV>
        {
            Items = result.PendingInvoices,
            TotalItems = result.TotalCount
        };
    }
    async Task RowClicked(DataGridRowClickEventArgs<ExInvoiceICV> args)
    {
        selectedIcv = args.Item;
        ShowSelected = args.Item != null;
        StateHasChanged();
    }

    private string RowStyleFunc(ExInvoiceICV item, int index)
    {
        if (IsSelected(item))
        {
            return "background-color: rgba(var(--mud-palette-primary-rgb), 0.25) !important; cursor: pointer; font-weight: bold;";
        }
        return "cursor: pointer;";
    }

    private string RowClassFunc(ExInvoiceICV item, int index)
    {
        if (IsSelected(item))
        {
            return "selected-zatca-row";
        }
        return "";
    }

    private bool IsSelected(ExInvoiceICV? item)
    {
        if (item == null || selectedIcv == null) return false;
        if (ReferenceEquals(item, selectedIcv)) return true;
        if (item.InvoiceICVId != 0 && selectedIcv.InvoiceICVId != 0)
            return item.InvoiceICVId == selectedIcv.InvoiceICVId;
        if (item.InvICVNo != 0 && selectedIcv.InvICVNo != 0)
            return item.InvICVNo == selectedIcv.InvICVNo && string.Equals(item.VoucherType, selectedIcv.VoucherType, StringComparison.OrdinalIgnoreCase);
        return false;
    }
    async Task UploadAll()
    {
        try
        {
            IcvReuploadAll dto = new IcvReuploadAll(IcvList!, AppData.CompanyDetails!);
            foreach (var item in IcvList!)
            {
                if (item == null)
                {
                    await Dialog.DialogMethod("Please select an invoice to upload.");
                    return;
                }
                if (item.DetailedStatus == "Success" || item.DetailedStatus == "REPORTED")
                {
                    await Dialog.DialogMethod("This invoice has already been successfully uploaded.");
                    return;
                }
                if (item.Message!.Contains("401"))
                {
                    await Dialog.DialogMethod("The selected invoice has an invalid CSR. Please generate a new CSR before re-uploading.");
                    return;
                }
            }
            await api.ZatcaReUploadAll(dto);
            await Dialog.DialogMethod("Successfully Uploaded");
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            await SecondClick(IsSimplified ? 2 : 1);
            appBar.SetCount(await api.GetFailedCount());
            await grid!.ReloadServerData();
        }
    }
    async Task UploadSelected()
    {
        try
        {
            if (selectedIcv == null)
            {
                await Dialog.DialogMethod("Please select an invoice to upload.");
                return;
            }
            if (selectedIcv.DetailedStatus == "Success" || selectedIcv.DetailedStatus == "REPORTED")
            {
                await Dialog.DialogMethod("This invoice has already been successfully uploaded.");
                return;
            }
            if (selectedIcv.Message!.Contains("401"))
            {
                await Dialog.DialogMethod("The selected invoice has an invalid CSR. Please generate a new CSR before re-uploading.");
                return;
            }
            IcvReupload dto = new IcvReupload(selectedIcv, AppData.CompanyDetails!);
            var data = await api.ZatcaReUploadSelected(dto);
            if (!data.IsSuccess)
            {
                await Dialog.DialogMethod(data.Message!);
            }
            else
            {
                selectedIcv = null;
                ShowSelected = false;
               
                await Dialog.DialogMethod("Successfully Uploaded");
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            await SecondClick(IsSimplified ? 2 : 1);
            appBar.SetCount(await api.GetFailedCount());
            await grid!.ReloadServerData();
        }
    }
}

