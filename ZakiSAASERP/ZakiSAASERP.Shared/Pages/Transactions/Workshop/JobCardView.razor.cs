using ZakiSAASERP.Models.Transactions.Workshop;

namespace ZakiSAASERP.Shared.Pages.Transactions.Workshop;

public partial class JobCardView : ComponentBase, ITabAware
{
    #region Variables
    [Parameter] public int TabId { get; set; }
    [Inject] IBrowserViewportService ResizeService { get; set; }

    [Inject] IJSRuntime JSRuntime { get; set; }

    // Header Model
    JobCardHdr jobCard = new();
    WebUserPrivilege privilege = new();

    // Details List (Grid) linked to UI
    List<JobCardDtls> entTransactions = new();

    // Helper objects
    protected MudDatePicker _txtVoucherDate;
    MudTextField<string>? txtKilometer;
    MudTextField<string>? txtReference;
    MudTextField<string>? txtContactNo;
    MudTextField<string>? txtDeliveredBy;

    // State flags
    bool _isSaving = false;
    bool DisablePrintButton = false;
    int _activeTabIndex = 0;

    // UI Properties
    string SalesManName { get; set; } = "";
    List<CommonModel> unitList = new();
    List<CommonModel> salesmanList = new();
    List<StoreMaster> storeList = new();
    List<ExMasters> vehicles = new();
    private bool IsAutoCode => PermanentSettingsData.AutoJobCardNo;

    public int Width { get; set; }
    private string strCurrencyFormat => PermanentSettingsData.CurrencyFormat;

    // Wrapper for TimeIn (DateTime?) to bind to MudTextField (TimeSpan?)
    public TimeSpan? TimeInSpan
    {
        get => jobCard.TimeIn?.TimeOfDay;
        set
        {
            if (value.HasValue)
            {
                var baseDate = jobCard.TimeIn?.Date ?? jobCard.VoucherDate ?? DateTime.Today;
                jobCard.TimeIn = baseDate + value.Value;
            }
            else
            {
                jobCard.TimeIn = null;
            }
        }
    }
    #endregion

    #region Initialize
    protected override async Task OnInitializedAsync()
    {
        try
        {
            var size = await ResizeService.GetCurrentBrowserWindowSizeAsync();
            Width = size.Width;

            // Load initial data
            unitList = await api.GetCombofill("Unit", 1);
            storeList = await api.GetAllStores();
            vehicles = await api.GetMasters("vehicle", AppData.BranchId);
            salesmanList = await api.GetCombofill("SalesMan", 1);
            if (AppData.privileges is not null)
            {
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "JobCard") ?? new();
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

    // Mobile Entry
    JobCardDtls mobileItem = new();
    JobCardDtls? editingItem = null; // Track original item for updates

    // --- Calculation Logic ---
    void CalculateRow(JobCardDtls row)
    {
        // Line Logic
        row.Value = (row.Qty * row.Rate);
        row.VatAmount = (row.Value * row.VatPercent) / 100;
        row.LineTotal = row.Value + row.VatAmount;

        TotalFindMethod();
    }

    void CalculateMobileItem()
    {
        mobileItem.Value = (mobileItem.Qty * mobileItem.Rate);
        mobileItem.VatAmount = (mobileItem.Value * mobileItem.VatPercent) / 100;
        mobileItem.LineTotal = mobileItem.Value + mobileItem.VatAmount;
    }

    void TotalFindMethod()
    {
        // Header Logic: Sum of lines
        jobCard.TotalAmount = entTransactions.Sum(x => x.Value);
        jobCard.TotalVat = entTransactions.Sum(x => x.VatAmount);
        jobCard.GrandTotal = entTransactions.Sum(x => x.LineTotal);
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
            if (last.FK_ItemId == 0 && string.IsNullOrEmpty(last.ItemName))
            {
                snackbar.Add(Localizer("PleaseAddAtLeastOneItem"), Severity.Warning);
                return;
            }
        }

        // Find default store
        var allowedStores = storeList.Where(x => x.FK_BranchId == AppData.BranchId || x.FK_BranchId == null).ToList();
        var defaultStoreId = (allowedStores.FirstOrDefault(x => x.IsDefaultStore)?.StoreId
                           ?? allowedStores.FirstOrDefault()?.StoreId
                           ?? 0) as int?;

        var newItem = new JobCardDtls
        {
            FK_JobCardId = jobCard.JobCardId,
            FK_StoreId = defaultStoreId,
            Qty = 1,
            VatPercent = 15 // Default VAT
        };
        entTransactions.Add(newItem);
    }

    private void MobileAddClick()
    {
        mobileItem = new JobCardDtls { Qty = 1, VatPercent = 15 };
        editingItem = null; // Clear edit mode
        // Set default store for mobile item
        var allowedStores = storeList.Where(x => x.FK_BranchId == AppData.BranchId || x.FK_BranchId == null).ToList();
        mobileItem.FK_StoreId = (allowedStores.FirstOrDefault(x => x.IsDefaultStore)?.StoreId
                           ?? allowedStores.FirstOrDefault()?.StoreId
                           ?? 0) as int?;

        _activeTabIndex = 1; // Switch to Items tab
    }

    private void AddMobileRow()
    {
        if (mobileItem.FK_ItemId == 0 && string.IsNullOrEmpty(mobileItem.ItemName))
        {
            snackbar.Add(Localizer("PleaseSelectAnItem"), Severity.Warning);
            return;
        }

        if (mobileItem.Qty <= 0)
        {
            snackbar.Add(Localizer("QuantityMustBeGreaterThanZero"), Severity.Warning);
            return;
        }

        if (editingItem is not null)
        {
            // Update Mode: Copy values back to original reference
            editingItem.FK_ItemId = mobileItem.FK_ItemId;
            editingItem.ItemName = mobileItem.ItemName;
            editingItem.Barcode = mobileItem.Barcode;
            editingItem.FK_UnitId = mobileItem.FK_UnitId;
            editingItem.UnitName = mobileItem.UnitName;
            editingItem.FK_StoreId = mobileItem.FK_StoreId;
            editingItem.Qty = mobileItem.Qty;
            editingItem.Rate = mobileItem.Rate;
            editingItem.VatPercent = mobileItem.VatPercent;
            editingItem.Value = mobileItem.Value;
            editingItem.VatAmount = mobileItem.VatAmount;
            editingItem.LineTotal = mobileItem.LineTotal;
            editingItem.Remarks = mobileItem.Remarks;

            // Recalculate if needed (though values are copied)
            // CalculateRow(editingItem); 

            TotalFindMethod();
            snackbar.Add(Localizer("ItemUpdated"), Severity.Success);

            // Reset
            MobileAddClick();
        }
        else
        {
            // Add Mode
            mobileItem.SL = entTransactions.Count + 1;

            CalculateMobileItem();
            entTransactions.Add(mobileItem);
            TotalFindMethod();

            snackbar.Add(Localizer("ItemAdded"), Severity.Success);

            // Reset
            MobileAddClick();
        }
    }

    void RemoveRow(JobCardDtls item)
    {
        entTransactions.Remove(item);
        if (!entTransactions.Any())
        {
            // Do not auto-add row on mobile if empty, keeps it clean.
            // Only for desktop?
            if (Width > 600) AddRow();
        }
        TotalFindMethod();
    }

    private void RowClicked(DataGridRowClickEventArgs<JobCardDtls> args)
    {
        editingItem = args.Item; // Keep reference to original

        // Clone to mobileItem for editing
        mobileItem = new JobCardDtls
        {
            JobCardDtlsId = args.Item.JobCardDtlsId,
            FK_JobCardId = args.Item.FK_JobCardId,
            SL = args.Item.SL,
            FK_ItemId = args.Item.FK_ItemId,
            ItemName = args.Item.ItemName,
            Barcode = args.Item.Barcode,
            FK_UnitId = args.Item.FK_UnitId,
            UnitName = args.Item.UnitName,
            FK_StoreId = args.Item.FK_StoreId,
            Qty = args.Item.Qty,
            Rate = args.Item.Rate,
            VatPercent = args.Item.VatPercent,
            Value = args.Item.Value,
            VatAmount = args.Item.VatAmount,
            LineTotal = args.Item.LineTotal,
            Remarks = args.Item.Remarks
        };

        _activeTabIndex = 1; // Switch to Items tab
    }

    // --- Search & Lookups ---

    async Task SearchJobOrder()
    {
        // Use TransactionSearchMethod with "Job Order" and pass VehicleId for filtering if selected
        var result = await Dialog.TransactionSearchMethod("Job Order", "", jobCard.FK_VehicleId);
        if (result is not null)
        {
            await LoadJobOrderDetails(result.Id);
        }
    }

    async Task SearchVehicle()
    {
        var result = await Dialog.TransactionSearchMethod("Vehicle");
        if (result is not null)
        {
            await LoadVehicleDetails(result.Id);
        }
    }

    async Task SearchSalesMan()
    {
        var result = await Dialog.TransactionSearchMethod("SalesMan"); // Ensure "SalesMan" logic exists in ModelService/Dialog
        if (result is not null)
        {
            jobCard.FK_SalesManId = result.Id;
            SalesManName = result.Name; // Using Reference for Name from search result if available, or fetch
                                        // Ideally fetch logic:
            var sm = await api.GetSalesManById(result.Id);
            if (sm is not null) SalesManName = sm.Name;
        }
    }

    async Task OnVehicleIconClick()
    {
        if (!string.IsNullOrEmpty(jobCard.PlateNo))
        {
            ClearVehicle();
        }
        else
        {
            await SearchVehicle();
        }
    }

    void ClearVehicle()
    {
        jobCard.FK_VehicleId = 0;
        jobCard.PlateNo = "";
        jobCard.ChassisNo = "";
        jobCard.Model = "";
        jobCard.Year = null;
        jobCard.VehicleInfo = "";

        // Clear Customer References
        jobCard.FK_CustomerId = 0;
        jobCard.CustomerName = "";
        jobCard.ContactNo = "";

        // Clear Job Order References (since Job Order depends on Vehicle)
        jobCard.FK_JobOrderId = null;
        jobCard.JobOrderNo = "";
        jobCard.Kilometer = "";
        jobCard.DeliveredBy = "";
        jobCard.TimeIn = null;
        jobCard.Reference = "";

        // Clear Salesman if it came from JobOrder/Vehicle? 
        // Safer to clear to ensure "Remove All References" is met, user can re-select.
        jobCard.FK_SalesManId = null;

        StateHasChanged();
    }

    async Task LoadVehicleDetails(int vehicleId)
    {
        var vehicle = await api.GetVehicleById(vehicleId);
        if (vehicle is not null)
        {
            // Case 4: Check if Job Order is related to this plate
            if (jobCard.FK_JobOrderId > 0)
            {
                var linkedOrder = await api.GetJobOrderById(jobCard.FK_JobOrderId.Value);
                if (linkedOrder is not null && linkedOrder.FK_VehicleId != vehicleId)
                {
                    jobCard.FK_JobOrderId = null;
                    jobCard.JobOrderNo = "";
                }
            }

            jobCard.FK_VehicleId = vehicleId;
            jobCard.PlateNo = vehicle.PlateNo;
            jobCard.ChassisNo = vehicle.ChassisNo;
            jobCard.Model = vehicle.Model;
            jobCard.Year = vehicle.Year;
            jobCard.VehicleInfo = $"{vehicle.PlateNo}, {vehicle.Model} {vehicle.Year}";

            // Populate Customer if linked
            if (vehicle.FK_CustomerId > 0)
            {
                jobCard.FK_CustomerId = vehicle.FK_CustomerId;
                jobCard.CustomerName = vehicle.CustomerName;
                // If name missing, fetch customer?
                if (string.IsNullOrEmpty(jobCard.CustomerName))
                {
                    var cust = await api.GetCustomerById(vehicle.FK_CustomerId);
                    if (cust is not null) jobCard.CustomerName = cust.Name;
                }
            }
        }
    }

    async Task LoadJobOrderDetails(int jobOrderId)
    {
        var orderDto = await api.GetJobOrderById(jobOrderId);
        if (orderDto is not null)
        {
            // Link Job Order
            jobCard.FK_JobOrderId = orderDto.JobOrderId;
            jobCard.JobOrderNo = orderDto.VoucherNo;

            // Case 3: Always replace details with Job Card (Job Order) details
            jobCard.FK_VehicleId = orderDto.FK_VehicleId;
            jobCard.PlateNo = orderDto.PlateNo;
            jobCard.ChassisNo = orderDto.ChassisNo; // Assuming DTO has this or we need to fetch vehicle? 
            // DTO has NotMapped props populated in properties like Vehicle info?
            // JobOrderHdr usually has PlateNo, ChassisNo etc populated in Repo GetById.
            if (string.IsNullOrEmpty(jobCard.ChassisNo) && orderDto.Vehicle is not null) jobCard.ChassisNo = orderDto.Vehicle.ChassisNo;

            jobCard.Model = orderDto.Model;
            jobCard.Year = orderDto.Year;

            // Map Additional Details
            jobCard.Kilometer = orderDto.Kilometer;
            jobCard.ContactNo = orderDto.ContactNo;
            jobCard.DeliveredBy = orderDto.DeliveredBy;
            jobCard.TimeIn = orderDto.TimeIn;
            // Force TimeInSpan update if needed, but binding should handle it if blazor detects change.
            // Explicitly setting property might help if setter logic is complex, but setter updates TimeIn.
            // jobCard.TimeIn updated -> TimeInSpan getter will reflect it.

            jobCard.Reference = orderDto.ReferenceNo;

            jobCard.FK_CustomerId = orderDto.FK_CustomerId;
            jobCard.CustomerName = orderDto.CustomerName;
            jobCard.VehicleInfo = orderDto.VehicleInfo;

            // Map SalesMan: use FK if available directly, else fallback to object
            if (orderDto.FK_SalesManId > 0)
            {
                jobCard.FK_SalesManId = orderDto.FK_SalesManId;
                var sm = await api.GetSalesManById(orderDto.FK_SalesManId ?? 0);
                if (sm is not null) SalesManName = sm.Name;
            }
            else if (orderDto.SalesMan is not null)
            {
                jobCard.FK_SalesManId = orderDto.SalesMan.SalesmanId;
                SalesManName = orderDto.SalesMan.Name;
            }

            StateHasChanged();
        }
    }

    async Task OpenItemSearch(JobCardDtls row)
    {
        var selectedProduct = await Dialog.GetItemSearchMethod("");
        if (selectedProduct is not null)
        {
            await MapItemToRow(row, selectedProduct);
        }
    }

    async Task OnBarcodeEnter(JobCardDtls row)
    {
        if (string.IsNullOrWhiteSpace(row.Barcode))
        {
            await OpenItemSearch(row);
            return;
        }

        // Try to find item by barcode (using multiunit endpoint logic matching SalesQuotation)
        // SalesQuotation uses (1, 1, barcode) which seems to represent branch/store or dummy values for lookup context if needed.
        // However, looking at IViewService: GetItemMultiUnit(int itemId, int unitId, string barcode)
        // If itemId=0, it likely searches by barcode.
        var itemData = await api.GetItemMultiUnit(0, 0, row.Barcode);
        if (itemData is not null)
        {
            row.FK_ItemId = itemData.FK_ItemId;
            row.FK_UnitId = itemData.FK_UnitId;
            row.ItemName = itemData.MultiUnitName ?? itemData.ItemMaster?.ItemName;
            row.QtyPerUnit = itemData.QTYPerUnit ?? 1;
            row.Rate = itemData.Rate ?? 0;
            row.Barcode = itemData.Barcode;
            row.VatPercent = itemData.ItemMaster?.Vat?.Percentage ?? 15; // Fallback or fetch
                                                                         // SalesQuotation fetches VAT via "vatFromItemId" usually
            row.VatPercent = await api.GetValueMethod("vatFromItemId", itemData.FK_ItemId);

            // Fetch MultiUnits
            row.MultiUnits = await api.GetItemMultiUnits(itemData.FK_ItemId);

            CalculateRow(row);
            StateHasChanged();
            await Task.Delay(50);
            await FocusById("qty_" + entTransactions.IndexOf(row));
        }
        else
        {
            // Not found, try search with the text
            var selectedProduct = await Dialog.GetItemSearchMethod(row.Barcode);
            if (selectedProduct is not null)
            {
                await MapItemToRow(row, selectedProduct);
            }
        }
    }

    async Task MapItemToRow(JobCardDtls row, ItemSelectionResult selectedProduct)
    {
        row.FK_ItemId = selectedProduct.FK_ItemId;
        row.FK_UnitId = selectedProduct.FK_UnitId;
        row.ItemName = selectedProduct.MultiUnitName; // Use alias
        row.Barcode = selectedProduct.Barcode;
        row.Rate = selectedProduct.Rate ?? 0;
        row.QtyPerUnit = selectedProduct.QTYPerUnit ?? 1; // Use alias

        row.VatPercent = await api.GetValueMethod("vatFromItemId", selectedProduct.FK_ItemId);

        // Populate Units from ItemSelectionResult (Respects search mode)
        row.MultiUnits = selectedProduct.AvailableUnits?.Select(u => new ItemMultiUnit
        {
            FK_UnitId = u.Id,
            FK_ItemId = row.FK_ItemId,
            Unit = new UnitMaster { UnitId = u.Id, Name = u.Name }
        }).ToList() ?? new List<ItemMultiUnit>();

        // Fallback fetch if empty
        if (row.MultiUnits.Count == 0)
        {
            row.MultiUnits = await api.GetItemMultiUnits(row.FK_ItemId);
        }

        CalculateRow(row);
        StateHasChanged();
        await Task.Delay(50);
        await FocusById("qty_" + entTransactions.IndexOf(row));
    }

    async Task UnitChanged(JobCardDtls row, int newUnitId)
    {
        row.FK_UnitId = newUnitId;
        // Fetch new details for this unit
        var itemData = await api.GetItemMultiUnit(row.FK_ItemId, newUnitId, "");
        if (itemData is not null)
        {
            row.ItemName = itemData.MultiUnitName ?? itemData.ItemMaster?.ItemName;
            row.Rate = itemData.Rate ?? 0;
            row.Barcode = itemData.Barcode;
            row.QtyPerUnit = itemData.QTYPerUnit ?? 1;
        }

        CalculateRow(row);
    }


    async Task MobileItemSearch()
    {
        var result = await Dialog.GetItemSearchMethod(mobileItem.Barcode);
        if (result is not null)
        {
            await MapItemToRow(mobileItem, result);
            CalculateMobileItem();
        }
    }

    // --- CRUD ---

    async Task NewClick()
    {
        jobCard = new JobCardHdr
        {
            VoucherDate = DateTime.Now,
            Status = "Opened",
            FK_UserId = AppData.UserId ?? 0,
            FK_FinancialYearId = AppData.FYearId ?? 0,
            FK_BranchId = AppData.BranchId
        };

        if (!IsAutoCode)
        {
            try
            {
                jobCard.VoucherNo = (await api.GetNewJobCardDocNo(AppData.BranchId)).ToString();
            }
            catch { }
        }
        else
        {
            await LoadAutoCodeIfEnabled();
        }

        entTransactions = new List<JobCardDtls>();
        AddRow();

        DisablePrintButton = true;
        _activeTabIndex = 0;

        StateHasChanged();
    }

    async Task SaveOrUpdate()
    {
        if (_isSaving) return;
        _isSaving = true;
        StateHasChanged();

        try
        {
            // Validations
            // Job Order is now OPTIONAL
            if (jobCard.FK_VehicleId == 0)
            {
                snackbar.Add("Please Select a Vehicle", Severity.Warning);
                return;
            }
            if (jobCard.FK_CustomerId == 0)
            {
                snackbar.Add("Please Select a Customer", Severity.Warning); // Or allow walk-in? User said "fill customer details", implies required.
                return;
            }

            if (entTransactions.Where(x => x.FK_ItemId > 0).Count() == 0)
            {
                snackbar.Add(Localizer("PleaseAddAtLeastOneItem"), Severity.Warning);
                return;
            }

            // Sync Grid
            jobCard.JobCardDetails = entTransactions.Where(x => x.FK_ItemId > 0).ToList();

            OutputResponse result;
            if (jobCard.JobCardId == 0)
            {
                if (privilege.SaveData == false)
                {
                    snackbar.Add(Localizer("You don't have the privilege to save"), Severity.Error);
                    return;
                }
                result = await api.CreateJobCard(jobCard);
            }
            else
            {
                if (privilege.EditData == false)
                {
                    snackbar.Add(Localizer("You don't have the privilege to edit"), Severity.Error);
                    return;
                }
                result = await api.UpdateJobCard(jobCard);
            }

            if (result.Id > 0)
            {
                if (jobCard.JobCardId == 0) jobCard.JobCardId = result.Id;

                snackbar.Add(Localizer("JobCardSaved"), Severity.Success);

                if (await Dialog.DeleteDialogMethod(Localizer("PrintDialog")))
                {
                    await PrintMethod();
                }

                await NewClick();
            }
            else
            {
                Dialog.DialogMethod(result.Message);
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
        if (jobCard.JobCardId > 0)
        {
            if (await Dialog.DeleteDialogMethod(Localizer("CancelDialog")))
            {
                if (privilege.DeleteData == false)
                {
                    snackbar.Add(Localizer("You don't have the privilege to delete"), Severity.Error);
                    return;
                }
                jobCard.IsDeleted = true;
                jobCard.Status = "Cancelled";
                var result = await api.UpdateJobCard(jobCard);
                if (result.Id > 0)
                {
                    if (!string.IsNullOrEmpty(result.Message))
                        snackbar.Add(result.Message, Severity.Success);
                    else
                        snackbar.Add(Localizer("CancelledSuccessfully"), Severity.Success);

                    await NewClick();
                }
                else
                {
                    Dialog.DialogMethod(result.Message);
                }
            }
        }
    }



    async Task PrintHelperMethod(bool IsPreview = false)
    {
        try
        {
            var validDetails = entTransactions.Where(x => !string.IsNullOrEmpty(x.ItemName)).ToList();
            if (jobCard.JobCardId == 0)
            {
                snackbar.Add("Please save before printing", Severity.Warning);
                return;
            }
            if (!validDetails.Any())
            {
                snackbar.Add("No items to print", Severity.Warning);
                return;
            }

            var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Job_Card"));
            if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
            {
                // Fallback check: try with space if user manually named it "Job Card"
                pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Job Card"));
                if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
                {
                    Dialog.DialogMethod("Print Design 'Job_Card' not found. Please import the design.");
                    return;
                }
            }

            var thdr = jobCard.CopyTo<TransactionHdr>();
            thdr.HdrNo = jobCard.VoucherNo;
            thdr.HdrDate = jobCard.VoucherDate ?? DateTime.Now;
            thdr.InvoiceDate = jobCard.VoucherDate?.ToString("dd-MM-yyyy");
            thdr.VoucherType = pdto.hdr.VoucherType; // Explicitly set Voucher Type
            thdr.GrossAmount = jobCard.TotalAmount; // Map TotalAmount -> GrossAmount
            thdr.TaxTotal = jobCard.TotalVat;       // Map TotalVat -> TaxTotal
            thdr.GrandTotal = jobCard.GrandTotal;   // Ensure GrandTotal is set
            thdr.NetAmount = jobCard.GrandTotal;    // Set NetAmount as fallback/same

            thdr.RefNo = jobCard.JobOrderNo; // Mapping Job Order No to RefNo
            thdr.CustomerName = jobCard.CustomerName;
            thdr.TaxTotal = jobCard.TotalVat;
            thdr.Remarks = jobCard.VehicleInfo; // Mapping Vehicle Info to Remarks

            // Mapping Requested Extra Fields
            thdr.Extra1 = jobCard.PlateNo;
            thdr.Extra2 = jobCard.ChassisNo;
            thdr.Extra3 = jobCard.Model;
            thdr.Extra4 = jobCard.Year?.ToString();
            thdr.Extra5 = jobCard.Kilometer;
            thdr.Extra6 = jobCard.TimeIn?.ToString("hh:mm tt"); // Time In formatted
            thdr.Extra7 = jobCard.ContactNo;
            thdr.Extra8 = jobCard.DeliveredBy;
            thdr.Extra9 = jobCard.Status;
            thdr.Extra10 = SalesManName;

            var tdtls = new List<TransactionDtls>();
            foreach (var item in validDetails)
            {
                var td = new TransactionDtls();
                td.SL = entTransactions.IndexOf(item) + 1;
                td.FK_ItemId = item.FK_ItemId;
                td.ItemName = item.ItemName;
                td.Barcode = item.Barcode;
                td.FK_UnitId = item.FK_UnitId;
                td.UnitName = item.UnitName;
                td.Quantity = item.Qty;
                td.ExRate = item.Rate; // Rate mapped to ExRate
                td.LineTotal = item.LineTotal;
                td.TaxAmount = item.VatAmount;
                td.TaxPerc = item.VatPercent;
                td.Description = item.Remarks;
                tdtls.Add(td);
            }

            var party = new Party();
            if (jobCard.FK_CustomerId > 0)
            {
                var cust = await api.GetCustomerById(jobCard.FK_CustomerId);
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
                party.Name = jobCard.CustomerName;
                party.Mobile = jobCard.ContactNo;
            }

            PrintParameterDTO dto = new PrintParameterDTO(pdto.hdr, pdto.dtls, thdr, tdtls, party, AppData.CompanyDetails!, null);
            await CrudHelper.PrintInvoiceMethodLocal(Downloader, dto, Dialog, IsPreview);
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    async Task PrintPrviewMethod() => await PrintHelperMethod(true);
    async Task PrintMethod() => await PrintHelperMethod(false);

    private async Task LoadAutoCodeIfEnabled()
    {
        if (IsAutoCode && jobCard.JobCardId == 0)
        {
            jobCard.VoucherNo = await api.GetNextJobCardCode(AppData.BranchId);
        }
    }

    async Task SearchClickAsync()
    {
        // "Job Card" key needs to be handled in TransactionSearchMethod
        var result = await Dialog.TransactionSearchMethod("Job Card"); // Expecting "Job Card" key mapping in ModelService
        if (result is not null)
        {
            var loadedCard = await api.GetJobCardById(result.Id);
            if (loadedCard is not null)
            {
                jobCard = loadedCard;
                entTransactions = jobCard.JobCardDetails.ToList();
                if (!entTransactions.Any()) AddRow();

                // Fetch multi-units for each item for display?
                // Or just load basic. For editing unit, user interacts.
                foreach (var item in entTransactions)
                {
                    item.MultiUnits = await api.GetItemMultiUnits(item.FK_ItemId);

                    if (PermanentSettingsData.ItemSearchMode == 2 && item.MultiUnits is not null)
                    {
                        var conversion = item.MultiUnits.FirstOrDefault(x => x.FK_UnitId == item.FK_UnitId);
                        if (conversion is not null)
                        {
                            if (!string.IsNullOrEmpty(conversion.MultiUnitName))
                                item.ItemName = conversion.MultiUnitName;
                            if (!string.IsNullOrEmpty(conversion.Barcode))
                                item.Barcode = conversion.Barcode;
                        }
                    }
                }

                // Load Display Props (JobOrderNo, CustomerName etc) since they are NotMapped
                // and API might populate them if Repo allows, or we fetch them here
                if (jobCard.FK_JobOrderId > 0)
                {
                    var order = await api.GetJobOrderById(jobCard.FK_JobOrderId ?? 0);
                    if (order is not null)
                    {
                        jobCard.JobOrderNo = order.VoucherNo;
                        jobCard.CustomerName = order.CustomerName;
                    }
                }

                if (jobCard.FK_SalesManId > 0)
                {
                    var sm = await api.GetSalesManById(jobCard.FK_SalesManId ?? 0);
                    if (sm is not null) SalesManName = sm.Name;
                }
            }

            TotalFindMethod();
            DisablePrintButton = false;
            StateHasChanged();
        }
    }
}

    #endregion
