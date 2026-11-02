using ZakiSAASERP.Models.Transactions.Workshop;
using ZakiSAASERP.Shared.Pages.Masters;
using Microsoft.AspNetCore.Components.Forms;

namespace ZakiSAASERP.Shared.Pages.Transactions.Workshop;

public partial class JobOrderView : ComponentBase, ITabAware
{
    [Parameter] public int TabId { get; set; }
    #region Variables
    [Inject] IBrowserViewportService ResizeService { get; set; }

    [Inject] IJSRuntime JSRuntime { get; set; }

    // Header Model
    JobOrderHdr jobOrder = new();
    WebUserPrivilege privilege = new();

    // Details List (Grid) linked to UI
    List<JobOrderDtls> entTransactions = new();

    // Helper Lists
    List<TechnicianMaster> technicianList = new();
    List<CommonModel> salesmanList = new();

    // Helper objects
    IBrowserFile? pendingIstimaraFile;
    byte[]? pendingIstimaraBytes;
    string istimaraImagePreview = "";
    string? oldIstimaraPathToDelete;
    protected MudDatePicker _txtVoucherDate;
    protected MudDatePicker _txtPromisedDate;
    MudTextField<string>? txtPlateNo;
    MudTextField<string>? txtKilometer;
    MudTextField<string>? txtReference;
    MudSelect<int?>? cmbSalesMan;
    MudTextField<string>? txtContactNo;
    MudTextField<string>? txtDeliveredBy;
    EditForm? form;

    // State flags
    bool _isSaving = false;
    bool DisablePrintButton = false;
    int _activeTabIndex = 0;
    TimeSpan? timeInSpan = DateTime.Now.TimeOfDay;

    private bool IsAutoCode => PermanentSettingsData.AutoJobOrderNo;

    // Mobile Entry
    JobOrderDtls mobileItem = new();

    public int Width { get; set; }
    private string strCurrencyFormat => PermanentSettingsData.CurrencyFormat;
    private SalesManMaster selectedSalesman;
    #endregion

    #region Initialize
    protected override async Task OnInitializedAsync()
    {
        try
        {
            var size = await ResizeService.GetCurrentBrowserWindowSizeAsync();
            Width = size.Width;

            // Load initial data
            salesmanList = await api.GetCombofill("SalesMan", 1);
            salesmanList = await api.GetCombofill("SalesMan", 1);
            technicianList = await api.GetAllTechnicians();
            if (AppData.privileges is not null)
            {
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "JobOrder") ?? new();
            }

            if (Width > 600)
            {
                AddRow();
            }
            await NewClick();

            // Initial Focus
            await FocusById("VoucherNo");
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion

    #region Focus Helper
    async Task FocusById(string id)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("selectTextById", id);
        }
        catch { /* Ignore if element not found */ }
    }
    #endregion

    #region Methods

    // --- Calculation Logic ---
    void CalculateRow(JobOrderDtls row)
    {
        // Line Logic: Qty * Rate = Total; Total + Vat = LineTotal
        row.Total = (row.Qty * row.Rate);
        row.VatAmount = (row.Total * row.VatPercent) / 100;
        row.LineTotal = row.Total + row.VatAmount;

        TotalFindMethod();
    }

    void TotalFindMethod()
    {
        // Header Logic: Sum of lines
        jobOrder.TotalAmount = entTransactions.Sum(x => x.Total);
        jobOrder.TotalVat = entTransactions.Sum(x => x.VatAmount);
        jobOrder.GrandTotal = entTransactions.Sum(x => x.LineTotal);
    }

    // --- Grid Operations ---
    void AddRow()
    {
        if (Width < 600)
        {
            if (_activeTabIndex == 1)
            {
                AddMobileRow();
            }
            else
            {
                MobileAddClick();
            }
            return;
        }

        if (entTransactions.Any())
        {
            var last = entTransactions.Last();
            if (last.FK_RepairId == 0 && string.IsNullOrEmpty(last.RepairName))
            {
                snackbar.Add(Localizer("SelectRepair"), Severity.Warning);
                return;
            }
        }
        var newItem = new JobOrderDtls
        {
            FK_JobOrderId = jobOrder.JobOrderId,
            Qty = 1,
            VatPercent = 15 // Default VAT
        };
        entTransactions.Add(newItem);
    }

    private void MobileAddClick()
    {
        mobileItem = new JobOrderDtls { Qty = 1, VatPercent = 15 };
        _activeTabIndex = 1; // Switch to Items tab
    }

    void RemoveRow(JobOrderDtls item)
    {
        entTransactions.Remove(item);
        if (!entTransactions.Any())
        {
            AddRow();
        }
        TotalFindMethod();
    }

    // Event handler for row click (if needed for selection/edit)
    private void RowClickEvent(TableRowClickEventArgs<JobOrderDtls> args)
    {
        // Handle row click
    }

    // --- Search & Lookups ---

    async Task SearchVehicle()
    {
        // Search by Plate No
        string searchValue = string.IsNullOrEmpty(jobOrder.PlateNo) ? "" : jobOrder.PlateNo;
        var result = await Dialog.TransactionSearchMethod("Vehicle", searchValue);
        if (result is not null)
        {
            await LoadVehicleDetails(result.Id);
            await txtKilometer!.FocusAsync();
        }
    }

    async Task LoadVehicleDetails(int vehicleId)
    {
        var vehicleDto = await api.GetVehicleById(vehicleId);
        if (vehicleDto is not null)
        {
            // Map Vehicle to Header
            jobOrder.FK_VehicleId = vehicleDto.VehicleId;
            jobOrder.PlateNo = vehicleDto.PlateNo;
            jobOrder.ChassisNo = vehicleDto.ChassisNo;
            jobOrder.Model = vehicleDto.Model;
            jobOrder.Year = vehicleDto.Year;
            jobOrder.VehicleInfo = $"{vehicleDto.Model} {vehicleDto.Year}";
            jobOrder.Kilometer = ""; // Reset or fetch if available in future

            if (vehicleDto.FK_CustomerId > 0)
            {
                await LoadCustomerDetails(vehicleDto.FK_CustomerId);
            }
        }
    }

    async Task LoadCustomerDetails(int customerId)
    {
        var customerDto = await api.GetCustomerById(customerId);
        if (customerDto is not null)
        {
            jobOrder.FK_CustomerId = customerDto.CustomerId;
            jobOrder.CustomerCode = customerDto.Code;
            jobOrder.CustomerName = customerDto.Name;
            jobOrder.ContactNo = customerDto.Mobile;
        }
    }

    async Task OpenRepairSearch(JobOrderDtls row)
    {
        var result = await Dialog.TransactionSearchMethod("Repair");
        if (result is not null)
        {
            row.FK_RepairId = result.Id;
            row.RepairName = result.Name;
            row.RepairCode = result.Code;

            var repair = await api.GetRepairById(result.Id);


            CalculateRow(row);

            StateHasChanged();
            await Task.Delay(100);
            await FocusById("technician_" + entTransactions.IndexOf(row));
        }
    }

    async Task OpenTechnicianSearch(JobOrderDtls row)
    {
        // Using TransactionSearchMethod with "Technician" key
        var result = await Dialog.TransactionSearchMethod("Technician");
        if (result is not null)
        {
            row.FK_TechnicianId = result.Id;
            row.TechnicianName = result.Name; // Or Name property from result
            StateHasChanged();
            await Task.Delay(100);
            await FocusById("qty_" + entTransactions.IndexOf(row));
        }
    }

    async Task OpenCreateVehicle()
    {
        try
        {

            var result = await Dialog.MasterCreationDialogMethod<VehicleMasterView>();
            if (!result.Canceled && result.Data is not null)
            {
                await LoadVehicleDetails(AppData.PassingValue.Id);
            }
        }
        catch (Exception ex)
        {

            snackbar.Add(ex.Message);
        }
    }

    // --- File Upload ---
    private async Task UploadIstimara(IBrowserFile file)
    {
        if (file == null) return;

        if (file.Size > 5 * 1024 * 1024)
        {
            snackbar.Add(Localizer("FileSizeExceedsLimit"), Severity.Error);
            return;
        }

        try
        {
            var format = "image/png";
            var buffer = new byte[file.Size];
            using (var stream = file.OpenReadStream(5 * 1024 * 1024))
            {
                using (var ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms);
                    buffer = ms.ToArray();
                }
            }

            pendingIstimaraFile = file;
            pendingIstimaraBytes = buffer;

            var base64 = Convert.ToBase64String(buffer);
            istimaraImagePreview = $"data:{format};base64,{base64}";
        }
        catch (Exception ex)
        {
            snackbar.Add(Localizer("ErrorReadingFile"), Severity.Error);
        }
    }

    private void RemoveIstimaraImage()
    {
        if (!string.IsNullOrWhiteSpace(jobOrder.IstimaraPath))
        {
            oldIstimaraPathToDelete = jobOrder.IstimaraPath;
        }
        jobOrder.IstimaraPath = null;
        istimaraImagePreview = "";
        pendingIstimaraBytes = null;
        pendingIstimaraFile = null;
    }

    private void ShowImagePopup(string src)
    {
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium };
        var parameters = new DialogParameters { ["ImageUrl"] = src };
        DialogService.Show<ImagePreviewDialog>("Istimara Image", parameters, options);
    }

    private async Task<string?> ProcessPendingImageUpload(int jobOrderId)
    {
        if (pendingIstimaraBytes == null || pendingIstimaraFile == null) return null;
        try
        {
            string basePath = PermanentSettingsData.IsWebBased ? "WEBROOT" : PermanentSettingsData.ImageLocalBasePath;
            string clientFolder = PermanentSettingsData.ImageClientFolder;

            // EntityType = "JobOrder"
            return await api.UploadImage(pendingIstimaraBytes, pendingIstimaraFile.Name, "JobOrder", jobOrderId, basePath, clientFolder);
        }
        catch (Exception)
        {
            return null;
        }
    }

    // --- CRUD ---

    async Task NewClick()
    {
        jobOrder = new JobOrderHdr
        {
            VoucherDate = DateTime.Now,
            PromisedDate = DateTime.Now.AddDays(1),
            Status = "Opened",
            FK_UserId = AppData.UserId ?? 0,
            FK_FinancialYearId = AppData.FYearId ?? 0,
            FK_BranchId = AppData.BranchId
        };

        if (!IsAutoCode)
        {
            try
            {
                // Using GetNewJobOrderDocNo as defined in IViewService (returns int)
                jobOrder.VoucherNo = (await api.GetNewJobOrderDocNo(AppData.BranchId)).ToString();
            }
            catch { }
        }
        else
        {
            await LoadAutoCodeIfEnabled();
        }

        entTransactions = new List<JobOrderDtls>();
        AddRow();

        pendingIstimaraFile = null;
        timeInSpan = DateTime.Now.TimeOfDay;
        DisablePrintButton = true;
        _activeTabIndex = 0;
        selectedSalesman = null;
        istimaraImagePreview = "";
        pendingIstimaraBytes = null;
        pendingIstimaraFile = null;
        oldIstimaraPathToDelete = null;

        StateHasChanged();
    }

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        _isSaving = true;
        StateHasChanged(); // Trigger UI update to show loading state

        try
        {
            // Validations
            if (jobOrder.FK_VehicleId == 0)
            {
                snackbar.Add(Localizer("VehicleRequired"), Severity.Warning);
                return;
            }

            // Sync Time
            if (timeInSpan.HasValue)
            {
                jobOrder.TimeIn = DateTime.Today.Add(timeInSpan.Value);
            }

            if (entTransactions.Where(x => x.FK_RepairId > 0 || !string.IsNullOrEmpty(x.RepairName)).Count() == 0)
            {
                snackbar.Add(Localizer("PleaseAddAtLeastOneItem"), Severity.Warning);
                return;
            }

            // Sync Grid
            jobOrder.JobOrderDetails = entTransactions.Where(x => x.FK_RepairId > 0 || !string.IsNullOrEmpty(x.RepairName)).ToList();

            // Handle Old Image Deletion
            if (!string.IsNullOrWhiteSpace(oldIstimaraPathToDelete))
            {
                string basePath = PermanentSettingsData.IsWebBased ? "WEBROOT" : PermanentSettingsData.ImageLocalBasePath;
                var clientFolder = PermanentSettingsData.ImageClientFolder;
                await api.DeleteImage(basePath, oldIstimaraPathToDelete, clientFolder);
                oldIstimaraPathToDelete = null;
            }

            OutputResponse result;
            if (jobOrder.JobOrderId == 0)
            {
                if (privilege.SaveData == false)
                {
                    snackbar.Add(Localizer("You don't have the privilege to save"), Severity.Error);
                    return;
                }
                result = await api.CreateJobOrder(jobOrder);
                if (result.Id > 0 && pendingIstimaraBytes is not null)
                {
                    jobOrder.JobOrderId = result.Id;
                    var newPath = await ProcessPendingImageUpload(jobOrder.JobOrderId);
                    if (newPath is not null)
                    {
                        jobOrder.IstimaraPath = newPath;
                        await api.UpdateJobOrder(jobOrder); // Update with path
                    }
                }
            }
            else
            {
                if (privilege.EditData == false)
                {
                    snackbar.Add(Localizer("You don't have the privilege to edit"), Severity.Error);
                    return;
                }
                if (pendingIstimaraBytes is not null)
                {
                    var newPath = await ProcessPendingImageUpload(jobOrder.JobOrderId);
                    if (newPath is not null) jobOrder.IstimaraPath = newPath;
                }
                result = await api.UpdateJobOrder(jobOrder);
            }

            if (result.Id > 0)
            {
                if (jobOrder.JobOrderId == 0) jobOrder.JobOrderId = result.Id;

                snackbar.Add(Localizer("JobOrderSaved"), Severity.Success);

                if (await Dialog.DeleteDialogMethod(Localizer("PrintDialog")))
                {
                    await PrintMethod();
                }

                await NewClick();
            }
            else
            {
                if (result.Message == "NoAlreadyExists")
                {
                    snackbar.Add(Localizer("NoAlreadyExists"), Severity.Error);
                }
                else
                {
                    Dialog.DialogMethod(result.Message);
                }
            }

        }
        catch (Exception ex)
        {
            snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }

    async Task Cancel()
    {
        if (jobOrder.JobOrderId > 0)
        {
            if (await Dialog.DeleteDialogMethod(Localizer("CancelDialog")))
            {
                if (privilege.DeleteData == false)
                {
                    snackbar.Add(Localizer("You don't have the privilege to delete"), Severity.Error);
                    return;
                }
                jobOrder.IsDeleted = true;
                jobOrder.Status = "Cancelled";
                await api.UpdateJobOrder(jobOrder);
                await NewClick();
            }
        }
    }

    // --- Printing ---
    async Task PrintMethod()
    {
        await PrintHelperMethod();
    }

    async Task PrintHelperMethod(bool IsPreview = false)
    {
        try
        {
            var validDetails = entTransactions.Where(x => !string.IsNullOrEmpty(x.RepairName)).ToList();
            if (jobOrder.JobOrderId == 0)
            {
                snackbar.Add("Please save before printing", Severity.Warning);
                return;
            }
            if (!validDetails.Any())
            {
                snackbar.Add("No items to print", Severity.Warning);
                return;
            }

            var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Job_Order"));
            if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
            {
                // Fallback check: try with space if user manually named it "Job Order"
                pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Job Order"));
                if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
                {
                    Dialog.DialogMethod("Print Design 'Job_Order' not found. Please import the design.");
                    return;
                }
            }

            var thdr = new TransactionHdr();
            // Manual Mapping since JobOrderHdr structure differs and has no CopyTo target for TransactionHdr easily
            thdr.HdrNo = jobOrder.VoucherNo;
            thdr.HdrDate = jobOrder.VoucherDate ?? DateTime.Now;
            thdr.InvoiceDate = jobOrder.VoucherDate?.ToString("dd-MM-yyyy");
            thdr.VoucherType = pdto.hdr.VoucherType;
            thdr.GrossAmount = jobOrder.TotalAmount;
            thdr.TaxTotal = jobOrder.TotalVat;
            thdr.GrandTotal = jobOrder.GrandTotal;
            thdr.NetAmount = jobOrder.GrandTotal;

            thdr.RefNo = jobOrder.ReferenceNo;
            thdr.CustomerName = jobOrder.CustomerName;
            thdr.Remarks = jobOrder.VehicleInfo;

            // Mapping Requested Extra Fields - similar to Job Card
            thdr.Extra1 = jobOrder.PlateNo;
            thdr.Extra2 = jobOrder.ChassisNo;
            thdr.Extra3 = jobOrder.Model;
            thdr.Extra4 = jobOrder.Year?.ToString();
            thdr.Extra5 = jobOrder.Kilometer;
            thdr.Extra6 = jobOrder.TimeIn?.ToString("hh:mm tt");
            thdr.Extra7 = jobOrder.ContactNo;
            thdr.Extra8 = jobOrder.DeliveredBy;
            thdr.Extra9 = jobOrder.Status;

            if (jobOrder.FK_SalesManId > 0 && salesmanList is not null)
            {
                var sm = salesmanList.FirstOrDefault(s => s.Id == jobOrder.FK_SalesManId);
                thdr.Extra10 = sm?.Name;
            }

            var tdtls = new List<TransactionDtls>();
            foreach (var item in validDetails)
            {
                var td = new TransactionDtls();
                td.SL = entTransactions.IndexOf(item) + 1;
                // Mapping Repair Code to Description to avoid barcode rendering
                td.Description = item.RepairCode;
                // Mapping Repair Name to ItemName
                td.ItemName = item.RepairName;
                // Mapping Technician to UnitName (as per plan for print column binding)
                td.UnitName = item.TechnicianName;

                td.Quantity = item.Qty;
                td.ExRate = item.Rate; // Rate mapped to ExRate
                td.TaxPerc = item.VatPercent;
                td.TaxAmount = item.VatAmount;
                td.LineTotal = item.LineTotal;

                tdtls.Add(td);
            }

            var party = new Party();
            if (jobOrder.FK_CustomerId > 0)
            {
                var cust = await api.GetCustomerById(jobOrder.FK_CustomerId);
                if (cust != null)
                {
                    party.Name = cust.Name;
                    party.Mobile = cust.Mobile;
                    party.Address = cust.Address;
                    party.VatNo = cust.VatNo;
                    party.Code = cust.Code;
                }
            }
            else
            {
                party.Name = jobOrder.CustomerName;
                party.Mobile = jobOrder.ContactNo;
            }

            PrintParameterDTO dto = new PrintParameterDTO(pdto.hdr, pdto.dtls, thdr, tdtls, party, AppData.CompanyDetails!, null);
            await CrudHelper.PrintInvoiceMethodLocal(Downloader, dto, Dialog, IsPreview);

        }
        catch (Exception ex)
        {
            snackbar.Add(ex.Message, Severity.Error);
        }
    }

    async Task SearchClickAsync()
    {
        var result = await Dialog.TransactionSearchMethod("Job Order");
        if (result is not null)
        {
            // Load Job Order
            var loadedOrder = await api.GetJobOrderById(result.Id);
            if (loadedOrder is not null)
            {
                jobOrder = loadedOrder;
                entTransactions = jobOrder.JobOrderDetails.ToList();
                if (!entTransactions.Any()) AddRow();

                // Set UI helpers
                if (jobOrder.TimeIn.HasValue) timeInSpan = jobOrder.TimeIn.Value.TimeOfDay;
                if (jobOrder.FK_SalesManId > 0)
                {
                    selectedSalesman = new SalesManMaster { SalesmanId = jobOrder.FK_SalesManId ?? 0, Name = salesmanList.FirstOrDefault(x => x.Id == jobOrder.FK_SalesManId)?.Name };
                }

                // Load Image Preview
                if (!string.IsNullOrWhiteSpace(jobOrder.IstimaraPath))
                {
                    string basePath = PermanentSettingsData.IsWebBased ? "WEBROOT" : PermanentSettingsData.ImageLocalBasePath;
                    istimaraImagePreview = await api.GetImageUrl(basePath, jobOrder.IstimaraPath);
                }
                else
                {
                    istimaraImagePreview = "";
                }

                pendingIstimaraBytes = null;
                pendingIstimaraFile = null;
                oldIstimaraPathToDelete = null;

                TotalFindMethod();
                DisablePrintButton = false;
                StateHasChanged();
            }
        }
    }

    private async Task LoadAutoCodeIfEnabled()
    {
        if (IsAutoCode && jobOrder.JobOrderId == 0)
        {
            jobOrder.VoucherNo = await api.GetNextJobOrderCode(AppData.BranchId);
        }
    }

    private void OnSalesmanChanged(SalesManMaster salesman)
    {
        selectedSalesman = salesman;
        if (salesman is not null)
        {
            jobOrder.FK_SalesManId = salesman.SalesmanId;
        }
        else
        {
            jobOrder.FK_SalesManId = null;
        }
    }

    private async Task<IEnumerable<SalesManMaster>> SearchSalesman(string value, CancellationToken token)
    {
        if (string.IsNullOrEmpty(value))
            return salesmanList.Select(x => new SalesManMaster { SalesmanId = x.Id, Name = x.Name });

        return salesmanList.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase))
            .Select(x => new SalesManMaster { SalesmanId = x.Id, Name = x.Name });
    }

    private void AddMobileRow()
    {
        if (mobileItem.FK_RepairId == 0 && string.IsNullOrEmpty(mobileItem.RepairName))
        {
            snackbar.Add(Localizer("SelectRepair"), Severity.Warning);
            return;
        }

        // Add to grid
        var newItem = new JobOrderDtls
        {
            FK_JobOrderId = jobOrder.JobOrderId,
            FK_RepairId = mobileItem.FK_RepairId,
            RepairName = mobileItem.RepairName,
            RepairCode = mobileItem.RepairCode,
            FK_TechnicianId = mobileItem.FK_TechnicianId,
            TechnicianName = mobileItem.TechnicianName,
            Qty = mobileItem.Qty,
            Rate = mobileItem.Rate,
            Total = mobileItem.Total,
            VatPercent = mobileItem.VatPercent,
            VatAmount = mobileItem.VatAmount,
            LineTotal = mobileItem.LineTotal
        };

        entTransactions.Add(newItem);
        TotalFindMethod();

        // Reset mobile item
        mobileItem = new JobOrderDtls { Qty = 1, VatPercent = 15 };
    }

    private void CalculateMobileItem()
    {
        mobileItem.Total = (mobileItem.Qty * mobileItem.Rate);
        mobileItem.VatAmount = (mobileItem.Total * mobileItem.VatPercent) / 100;
        mobileItem.LineTotal = mobileItem.Total + mobileItem.VatAmount;
    }

    private async Task MobileRepairSearch()
    {
        var result = await Dialog.TransactionSearchMethod("Repair");
        if (result is not null)
        {
            mobileItem.FK_RepairId = result.Id;
            mobileItem.RepairName = result.Name;
            mobileItem.RepairCode = result.Code;

            // Fetch rate/details if needed, similar to desktop
            var repair = await api.GetRepairById(result.Id);
            // If repair has default rate, set it here. For now keeping it simple.

            CalculateMobileItem();
        }
    }

    private async Task MobileTechnicianSearch()
    {
        var result = await Dialog.TransactionSearchMethod("Technician");
        if (result is not null)
        {
            mobileItem.FK_TechnicianId = result.Id;
            mobileItem.TechnicianName = result.Name;
            StateHasChanged();
        }
    }

    private void RowClicked(DataGridRowClickEventArgs<JobOrderDtls> args)
    {
        mobileItem = new JobOrderDtls
        {
            FK_JobOrderId = args.Item.FK_JobOrderId,
            FK_RepairId = args.Item.FK_RepairId,
            RepairName = args.Item.RepairName,
            RepairCode = args.Item.RepairCode,
            FK_TechnicianId = args.Item.FK_TechnicianId,
            TechnicianName = args.Item.TechnicianName,
            Qty = args.Item.Qty,
            Rate = args.Item.Rate,
            Total = args.Item.Total,
            VatPercent = args.Item.VatPercent,
            VatAmount = args.Item.VatAmount,
            LineTotal = args.Item.LineTotal
        };
        _activeTabIndex = 1;
    }

    #endregion
}
