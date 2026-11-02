using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Transactions.Sales;

public partial class EnquiryRegistrationView
{
    [Parameter] public int TabId { get; set; }
    private RFQHdr hdr = new();
    private DateTime? entryDate = DateTime.Now;
    private DateTime? rfqDate = DateTime.Now;

    private string? customerName;
    private string? customerAddress;
    private string? customerMobile;
    private string? customerVat;
    List<RFQDtls>? rfqDetails = new();
    RFQDtls newRow = new();
    private string? salesManName;
    private List<SalesManMaster> salesMenFull = new();
    private List<CommonModel> units = new();
    private List<StoreMaster> stores = new();
    private WebUserPrivilege? privilege;
    private PrintDesignDTO? pdto;
    MudNumericField<decimal>? txtQuantity;
    protected override async Task OnInitializedAsync()
    {
        await LoadMasters();
        if (AppData.privileges is not null)
        {
            privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "EnquiryRegistration");
        }
        await New();
    }

    private async Task LoadMasters()
    {
        // Use IndexedDB caching for faster dropdown loading
        salesMenFull = await Api.GetAllSalesMan();
        units = await indexdbHelper.LoadComboFromIndexedDB("Unit");
        stores = await Api.GetAllStores();
        pdto = await indexdbHelper.LoadPrintDesignFromIndexedDB("Enquiry_Registration");
    }

    private async Task New()
    {
        hdr = new RFQHdr();
        rfqDetails = new();
        entryDate = DateTime.Now;
        rfqDate = DateTime.Now;
        hdr.FK_BranchId = 1;
        salesManName = "";
        customerName = ""; customerAddress = ""; customerMobile = ""; customerVat = "";

        // Add one empty row by default
        AddNewRow();

        try
        {
            hdr.RFQNo = await Api.GetNextEnquiryNo(hdr.FK_BranchId.GetValueOrDefault());
        }
        catch { /* Handle error */ }
    }
    private void RowClicked(DataGridRowClickEventArgs<RFQDtls> args)
    {
        newRow = args.Item;
    }
    private async Task DeleteRow(RFQDtls item)
    {
        rfqDetails!.Remove(item);
        if (!rfqDetails.Any())
        {
            AddNewRow();
        }
        StateHasChanged();
    }
    private void AddNewRow()
    {
        // Find default store
        var allowedStores = stores.Where(x => x.FK_BranchId == AppData.BranchId || x.FK_BranchId == null).ToList();
        var defaultStoreId = allowedStores.FirstOrDefault(x => x.IsDefaultStore)?.StoreId
                           ?? allowedStores.FirstOrDefault()?.StoreId;
        var lastItem = rfqDetails!.LastOrDefault();
        if (lastItem is not null)
        {
            if (string.IsNullOrEmpty(lastItem.ItemName))
            {
                Snackbar.Add(Localizer("ItemNameRequired"), Severity.Warning);
                return;
            }
            if (lastItem.Quantity <= 0)
            {
                Snackbar.Add(Localizer("QtyCannotBeZero"), Severity.Warning);
                return;
            }
            // Validate item exists
            if (lastItem.FK_ItemId == 0)
            {
                Snackbar.Add($"{Localizer("InvalidItem")}: {lastItem.ItemCode ?? lastItem.ItemName}", Severity.Error);
                return;
            }
        }
        newRow = new RFQDtls { Quantity = 1, FK_StoreId = defaultStoreId };
        rfqDetails.Add(newRow);
    }
    async Task AddLineItem()
    {
        if (string.IsNullOrEmpty(newRow.ItemName)) { return; }
        if (!rfqDetails!.Any(x => x.ItemCode == newRow.ItemCode))
        {
            rfqDetails!.Add(newRow);
        }
        newRow = new();
        StateHasChanged();
    }
    private async Task Save()
    {
        var validDetails = hdr.RFQDetails.Where(d => d.FK_ItemId > 0 && d.Quantity > 0).ToList();
        hdr.RFQDetails = validDetails;
        if (!validDetails.Any()) { Snackbar.Add("Please add items", Severity.Warning); return; }
        if (hdr.RFQId == 0)
        {
            if (privilege is not null && !privilege.SaveData)
            {
                await Dialog.DialogMethod(Localizer("NoSavePrivilege"));
                return;
            }
            hdr.EntryDate = entryDate;
            hdr.CustomerRFQDate = rfqDate;
            hdr.FK_UserId = AppData.UserId;
            hdr.FK_FinancialYearId = AppData.FYearId;
            try
            {
                var result = await Api.CreateEnquiry(hdr);
                Snackbar.Add(result.Message!, Severity.Success);
                await New();
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
        else
        {
            if (privilege is not null && !privilege.EditData)
            {
                await Dialog.DialogMethod(Localizer("NoEditPrivilege"));
                return;
            }
            hdr.EntryDate = entryDate;
            hdr.CustomerRFQDate = rfqDate;
            // Filter empty rows


            try
            {
                var result = await Api.UpdateEnquiry(hdr);
                Snackbar.Add(result.Message!, Severity.Success);
                await New();
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }
    private async Task Search()
    {
        var parameters = new DialogParameters();
        var options = new DialogOptions { MaxWidth = MaxWidth.False, FullWidth = true, CloseButton = true, BackdropClick = false };
        var dialog = await DialogService.ShowAsync<RefNoSearchDialog>(Localizer("SearchEnquiry"), parameters, options);
        var result = await dialog.Result;

        if (!result!.Canceled)
        {
            var selected = (RFQHdr)result.Data!;
            await LoadEnquiry(selected.RFQId);
        }
    }

    private async Task LoadEnquiry(int id)
    {
        hdr = await Api.GetEnquiryById(id);
        rfqDetails = hdr.RFQDetails.ToList(); // Load details into local list for editing
        entryDate = hdr.EntryDate;
        rfqDate = hdr.CustomerRFQDate;

        if (hdr.FK_SalesManId > 0)
        {
            var sm = salesMenFull.FirstOrDefault(x => x.SalesmanId == hdr.FK_SalesManId);
            if (sm is not null) salesManName = sm.Name;
        }

        if (hdr.Customer is not null)
        {
            customerName = hdr.Customer.Name;
            customerAddress = hdr.Customer.Address;
            customerMobile = hdr.Customer.Mobile;
            customerVat = hdr.Customer.VatNo;
        }
        else if (hdr.FK_CustomerId > 0)
        {
            var c = await Api.GetCustomerById(hdr.FK_CustomerId.GetValueOrDefault());
            if (c is not null)
            {
                customerName = c.Name;
                customerAddress = c.Address;
                customerMobile = c.Mobile;
                customerVat = c.VatNo;
            }
        }

        // Always add a blank row for editing/additions
        AddNewRow();
    }

    private async Task OpenSalesManSearch()
    {
        var parameters = new DialogParameters { ["salesManList"] = salesMenFull };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium };
        var dialog = await DialogService.ShowAsync<SalesManSearchDialog>(Localizer("SalesManSearch"), parameters, options);
        var result = await dialog.Result;
        if (!result!.Canceled && result.Data is SalesManMaster selected)
        {
            hdr.FK_SalesManId = selected.SalesmanId;
            salesManName = selected.Name;
        }
    }

    private async Task OpenCustomerSearch()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large };
        var dialog = await DialogService.ShowAsync<CustomerSearchDialog>(Localizer("CustomerSearch"), options);
        var result = await dialog.Result;
        if (!result!.Canceled && result.Data is CustomerMaster selected)
        {
            hdr.FK_CustomerId = selected.CustomerId;
            customerName = selected.Name;
            customerAddress = selected.Address;
            customerMobile = selected.Mobile;
            customerVat = selected.VatNo;
        }
    }

    private async Task Cancel()
    {
        if (hdr.RFQId > 0)
        {
            if (await Dialog.DeleteDialogMethod(Localizer("CancelDialog")))
            {
                if (privilege is not null && !privilege.DeleteData)
                {
                    await Dialog.DialogMethod(Localizer("No...Privilege"));
                    return;
                }
                var result = await Api.DeleteEnquiry(hdr.RFQId);
                // Check Id or specific message just in case Identity is not returned by Service for bool repo methods
                if (result.Id > 0 || (result.Message?.ToLower().Contains("success") == true))
                {
                    if (!string.IsNullOrEmpty(result.Message))
                        Snackbar.Add(result.Message, Severity.Success);
                    else
                        Snackbar.Add("Cancelled Successfully", Severity.Success);

                    await New();
                }
                else
                {
                    await Dialog.DialogMethod(result.Message ?? "Failed to delete");
                }
            }
        }
        else
            await New();
    }

    // Barcode/Item Search Logic for Grid Row

    private async Task OnBarcodeKeyDown(KeyboardEventArgs e, RFQDtls row)
    {
        if (e.Key == "Enter")
        {
            await BarcodeChanged(row);
            await txtQuantity.FocusAsync();
        }
    }
    private void OnQuantityKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            AddNewRow();
            StateHasChanged();
        }
    }
    private async Task BarcodeChanged(RFQDtls row)
    {
        // Use the row.ItemCode as the search query
        var searchVal = row.ItemCode;
        var selectedProduct = await Dialog.GetItemSearchMethod(searchVal!, showItemSearch: true);

        if (selectedProduct is not null)
        {
            row.FK_ItemId = selectedProduct.FK_ItemId;
            row.ItemName = selectedProduct.ItemName; // Or create a dedicated ItemName in RFQDtls? It has ItemName [NotMapped].
            row.ItemCode = selectedProduct.ItemCode;
            row.ItemArabicName = selectedProduct.ItemNameArabic;
            row.FK_UnitId = selectedProduct.FK_UnitId;
            StateHasChanged();
        }
    }

    private async Task PrintHelperMethod(bool IsPreview = false)
    {
        try
        {
            if (hdr.RFQId == 0) { return; }

            var validDetails = hdr.RFQDetails.Where(x => x.FK_ItemId > 0 && x.Quantity > 0).ToList();
            if (!validDetails.Any())
            {
                Snackbar.Add("No valid items to print", Severity.Warning);
                return;
            }

            // Use cached print design, fallback to API if not cached
            if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
            {
                pdto = await Api.GetPrintDesignAsync(new PrintDesignReq(0, "Enquiry_Registration"));
            }

            if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
            {
                await Dialog.DialogMethod("Print Design not found");
                return;
            }

            // Map RFQHdr to TransactionHdr for printing
            var transHdr = new TransactionHdr
            {
                HdrId = hdr.RFQId,
                HdrNo = hdr.RFQNo,
                // TransactionHdr doesn't have EntryDate, so we map InvoiceDate
                InvoiceDate = (hdr.EntryDate ?? DateTime.Now).ToString("dd-MM-yyyy"),
                Remarks = hdr.Remarks,
                RefNo = hdr.CustomerRefNo
            };

            // Map RFQDtls to TransactionDtls
            int sl = 1;
            var transDtls = validDetails.Select(d => new TransactionDtls
            {
                SL = sl++,
                FK_ItemId = d.FK_ItemId,
                Barcode = d.ItemCode, // Map ItemCode to Barcode
                ItemName = d.ItemName,
                ItemArabicName = d.ItemArabicName,
                Quantity = d.Quantity,
                FK_UnitId = d.FK_UnitId ?? 0,
                UnitName = units.FirstOrDefault(u => u.Id == d.FK_UnitId)?.Name ?? ""
            }).ToList();

            var party = new Party
            {
                CustomerId = hdr.FK_CustomerId.GetValueOrDefault(),
                Name = customerName,
                Address = customerAddress,
                Mobile = customerMobile,
                VatNo = customerVat
            };

            PrintParameterDTO dto = new PrintParameterDTO(pdto.hdr, pdto.dtls, transHdr, transDtls, party, AppData.CompanyDetails!, null);
            await CrudHelper.PrintInvoiceMethodLocal(Downloader, dto, Dialog, IsPreview);
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }

    async Task PrintPrviewMethod() => await PrintHelperMethod(true);
    async Task PrintMethod() => await PrintHelperMethod(false);
}
