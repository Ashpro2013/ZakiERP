namespace ZakiSAASERP.Shared.Services;

public sealed class ModelService(IDialogService DialogService, IViewService api)
{
    public async Task ShowMessage(string message)
    {
        var dialog = await DialogMethod(message);
        var result = await dialog.Result;
    }
    public async Task<IDialogReference> DialogMethod(string title, bool ShowCancel = false)
    {
        var options = new DialogOptions() { CloseOnEscapeKey = true };
        var parameters = new DialogParameters { ["ShowCancel"] = ShowCancel };
        return await DialogService.ShowAsync<DeleteDialog>(title, parameters, options);
    }
    public async Task<bool> DeleteDialogMethod(string dialogstring = null)
    {
        var val = dialogstring ?? Localizer("DeleteDialog");
        var dialog = await DialogMethod(Localizer(val), true);
        var result = await dialog.Result;
        if (result?.Data is null)
            return false;

        return (bool)result.Data;
    }
    public async Task<IDialogReference> InputDialog(string dialogText, string Title, bool ShowCancel = false, bool IsPassword = false, string input = "")
    {
        var options = new DialogOptions() { CloseOnEscapeKey = true };
        var parameters = new DialogParameters { ["ShowCancel"] = ShowCancel, ["IsPassword"] = IsPassword, ["FormTitle"] = dialogText, ["InputText"] = input };
        return await DialogService.ShowAsync<InputDialog>(Title, parameters, options);
    }
    public async Task<string?> InputDialogMethod(string dialogText, string Title, bool ShowCancel = false, bool IsPassword = false, string input = "")
    {
        var dialog = await InputDialog(dialogText, Title, ShowCancel, IsPassword, input);
        var result = await dialog.Result;
        return result!.Canceled ? null : result!.Data == null ? null : result!.Data!.ToString();
    }

    /// <summary>
    /// Opens the item search dialog based on ItemSearchMode setting and returns ItemSelectionResult
    /// with pre-populated AvailableUnits based on the search mode.
    /// - ItemSearchMode == 1 (ItemCode): ItemSearchDialog, returns ALL units
    /// - ItemSearchMode == 2 (MultiUnit): ProductSearchDialog, returns ONLY selected unit
    /// </summary>
    public async Task<ItemSelectionResult?> GetItemSearchMethod(string searchText, string Title = "Product Search", int? StoreId = null, bool showItemSearch = false, bool IsSales = true)
    {
        var options = new DialogOptions() { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraLarge, FullWidth = true };

        // Check ItemSearchMode setting: 1=ItemCode (ItemMaster), 2=MultiUnit (ItemMultiUnit)
        if (PermanentSettingsData.ItemSearchMode == 1 || showItemSearch)
        {
            // Use ItemSearchDialog (ItemMaster based) - Load ALL units
            var parameters = new DialogParameters { ["InitialSearch"] = searchText, ["BranchId"] = AppData.BranchId, ["StoreId"] = StoreId, ["IsSales"] = IsSales };
            var dialog = await DialogService.ShowAsync<ItemSearchDialog>(Title, parameters, options);
            var result = await dialog.Result;

            if (result?.Data is ItemMaster item)
            {
                // Load ALL units for this item
                var allUnits = await api.GetCombofill("munit", item.ItemId);

                // Get MultiUnit details for rate info when switching units
                var multiUnits = await api.GetItemMultiUnits(item.ItemId);
                var purchaseRate = (item.PurchaseRate > 0 ? item.PurchaseRate : item.StandardCost) ?? 0;

                return new ItemSelectionResult
                {
                    FK_ItemId = item.ItemId,
                    ItemCode = item.ItemCode,
                    ItemName = item.ItemName,
                    ItemNameArabic = item.ItemNameArabic,
                    Barcode = item.Barcode,
                    Location = item.Location,
                    FK_UnitId = item.FK_UnitId ?? 0,
                    UnitName = allUnits.FirstOrDefault(u => u.Id == item.FK_UnitId)?.Name,
                    QtyPerUnit = 1, // Base unit
                    Rate = IsSales ? (item.SalesRate ?? 0) : purchaseRate,
                    IncRate = IsSales ? (item.InclusiveRate ?? 0) : purchaseRate,
                    Cost = purchaseRate,
                    StandardCost = item.StandardCost ?? purchaseRate,
                    AvailableUnits = allUnits, // ALL units
                    MultiUnitDetails = multiUnits
                };
            }
            return null;
        }
        else
        {
            // Use ProductSearchDialog (ItemMultiUnit based) - Only selected unit
            var parameters = new DialogParameters { ["SearchText"] = searchText, ["FormTitle"] = Title, ["BranchId"] = AppData.BranchId, ["StoreId"] = StoreId, ["IsSales"] = IsSales };
            var dialog = await DialogService.ShowAsync<ProductSearchDialog>(Title, parameters, options);
            var result = await dialog.Result;

            if (result?.Data is ItemMultiUnit mu)
            {
                // Get unit name for the selected MultiUnit
                var unitName = await api.GetUnitName(mu.FK_UnitId);
                var purchaseRate = mu.Cost ?? 0;

                return new ItemSelectionResult
                {
                    FK_ItemId = mu.FK_ItemId,
                    ItemName = mu.MultiUnitName,
                    ItemNameArabic = mu.MultiUnitArabicName,
                    Barcode = mu.Barcode,
                    FK_UnitId = mu.FK_UnitId,
                    UnitName = unitName,
                    QtyPerUnit = mu.QTYPerUnit ?? 1,
                    Rate = IsSales ? (mu.Rate ?? 0) : purchaseRate,
                    IncRate = IsSales ? (mu.IncRate ?? 0) : purchaseRate,
                    Cost = purchaseRate,
                    StandardCost = purchaseRate,
                    // ONLY the selected unit
                    AvailableUnits = new List<CommonModel>
                    {
                        new() { Id = mu.FK_UnitId, Name = unitName }
                    },
                    MultiUnitDetails = new List<ItemMultiUnit> { mu }
                };
            }
            return null;
        }
    }

    public async Task<ExMasters> TransactionSearchMethod(string sForm, string sSearchText = "", int? filterId = null)
    {
        var parameters = new DialogParameters();
        var options = new DialogOptions() { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraLarge, FullWidth = true };
        parameters = sForm switch
        {
            "Sales Quotation" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("SalesQuotationSearch"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("Customer"),
                ["DescriptionHead"] = Localizer("QuotationDate"),
                ["CodeHead"] = Localizer("QuotationNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText
            },
            "Sales Invoice" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("SalesInvoiceSearch"),
                ["IdHead"] = Localizer("Id"),
                ["CodeHead"] = Localizer("InvoiceNo"),
                ["NameHead"] = Localizer("Customer"),
                ["DescriptionHead"] = Localizer("InvoiceDate"),
                ["ReferenceHead"] = Localizer("BillType"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText
            },
            "Sales Invoice Return" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("SalesInvoiceRtnTitle"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("Customer"),
                ["DescriptionHead"] = Localizer("SalesInvoiceRtnDate"),
                ["CodeHead"] = Localizer("SalesInvoiceRtnNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText
            },
            "Sales Order" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("SalesOrderSearch"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("Customer"),
                ["DescriptionHead"] = Localizer("OrderDate"),
                ["CodeHead"] = Localizer("OrderNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText
            },
            "Delivery Note" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("DeliveryNoteSearch"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("Customer"),
                ["DescriptionHead"] = Localizer("DNDate"),
                ["CodeHead"] = Localizer("DNNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText
            },
            "Delivery Note Return" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("DeliveryNoteRtnSearch"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("Customer"),
                ["DescriptionHead"] = Localizer("DeliveryNoteRtnDate"),
                ["CodeHead"] = Localizer("DeliveryNoteRtnNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText
            },
            "Purchase Order" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("PurchaseOrderSearch"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("Supplier"),
                ["DescriptionHead"] = Localizer("OrderDate"),
                ["CodeHead"] = Localizer("OrderNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = ""
            },
            "Goods Receipt" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("GoodsReceiptSearch"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("Supplier"),
                ["DescriptionHead"] = Localizer("GoodsReceiptDate"),
                ["CodeHead"] = Localizer("GoodsReceiptNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText
            },
            "Purchase" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("PurchaseSearch"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("Supplier"),
                ["DescriptionHead"] = Localizer("PurchaseDate"),
                ["CodeHead"] = Localizer("PurchaseNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText
            },
            "Purchase Return" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("PurchaseReturnSearch"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("Supplier"),
                ["DescriptionHead"] = Localizer("PurchaseReturnDate"),
                ["CodeHead"] = Localizer("PurchaseReturnNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText
            },
            "Proforma Invoice" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("ProformaInvoiceSearch"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("Customer"),
                ["DescriptionHead"] = Localizer("ProformaInvoiceDate"),
                ["CodeHead"] = Localizer("ProformaInvoiceNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText
            },
            "Print Design" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = "Search Print Design",
                ["IdHead"] = "Id",
                ["NameHead"] = "Invoice/Report",
                ["DescriptionHead"] = "Print Type",
                ["CodeHead"] = Localizer("Design Name"),
                ["AmountHead"] = "Paper Type",
                ["SearchText"] = sSearchText
            },
            "Barcode Design" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = "Search Barcode Design",
                ["IdHead"] = "Id",
                ["NameHead"] = "Heigt",
                ["DescriptionHead"] = "Printer Name",
                ["CodeHead"] = "Design Name",
                ["AmountHead"] = "Width",
                ["SearchText"] = sSearchText
            },
            "Supplier Payment" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("SupplierPaymentSearch"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("Supplier"),
                ["DescriptionHead"] = Localizer("Date"),
                ["CodeHead"] = Localizer("VoucherNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText
            },
            "Damage Stock" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("DamageStockSearch"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("Account"),
                ["DescriptionHead"] = Localizer("Date"),
                ["CodeHead"] = Localizer("DocumentNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText
            },
            "Stock Adjustment" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("StockAdjustmentSearch"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("Reference"),
                ["DescriptionHead"] = Localizer("Date"),
                ["CodeHead"] = Localizer("VoucherNo"),
                ["AmountHead"] = Localizer("Adjusted Amount"),
                ["SearchText"] = sSearchText
            },
            "Material Transfer Request" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("MaterialTransferRequestSearch"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("Status"),
                ["DescriptionHead"] = Localizer("Date"),
                ["CodeHead"] = Localizer("VoucherNo"),
                ["ReferenceHead"] = Localizer("ReferenceNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText,
                ["IsNameStatus"] = true
            },
            "Material Transfer IN" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("MaterialTransferINSearch"),
                ["IdHead"] = "",
                ["NameHead"] = Localizer("FromBranch"),
                ["ExtraHead"] = Localizer("ToBranch"),
                ["DescriptionHead"] = Localizer("Date"),
                ["CodeHead"] = Localizer("VoucherNo"),
                ["ReferenceHead"] = Localizer("ReferenceNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText
            },
            "Material Transfer OUT" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("MaterialTransferOUTSearch"),
                ["IdHead"] = "",
                ["NameHead"] = Localizer("FromBranch"),
                ["ExtraHead"] = Localizer("ToBranch"),
                ["DescriptionHead"] = Localizer("Date"),
                ["CodeHead"] = Localizer("VoucherNo"),
                ["ReferenceHead"] = Localizer("ReferenceNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText
            },
            "Internal Material Transfer" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("InternalMaterialTransferSearch"),
                ["IdHead"] = "",
                ["NameHead"] = Localizer("User"),
                ["DescriptionHead"] = Localizer("Date"),
                ["CodeHead"] = Localizer("VoucherNo"),
                ["ReferenceHead"] = Localizer("ReferenceNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText
            },
            "Privileges" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("User Privileges"),
                ["IdHead"] = "",
                ["NameHead"] = Localizer("Previlage Name"),
                ["ExtraHead"] = Localizer(""),
                ["DescriptionHead"] = Localizer(""),
                ["CodeHead"] = Localizer(""),
                ["ReferenceHead"] = Localizer(""),
                ["AmountHead"] = Localizer(""),
                ["SearchText"] = sSearchText
            },
            "Vehicle" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("VehicleSearch"),
                ["IdHead"] = Localizer("Id"),
                ["CodeHead"] = Localizer("PlateNo"),
                ["NameHead"] = Localizer("Customer"),
                ["DescriptionHead"] = Localizer("Model"),
                ["ExtraHead"] = Localizer("ChassisNo"),
                ["SearchText"] = sSearchText
            },
            "Technician" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("TechnicianSearch"),
                ["IdHead"] = Localizer("Id"),
                ["CodeHead"] = Localizer("Code"),
                ["NameHead"] = Localizer("Name"),
                ["DescriptionHead"] = Localizer("Position"),
                ["SearchText"] = sSearchText
            },
            "Repair" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("RepairSearch"),
                ["IdHead"] = Localizer("Id"),
                ["CodeHead"] = Localizer("Code"),
                ["NameHead"] = Localizer("Name"),
                ["DescriptionHead"] = Localizer("Description"),
                ["AmountHead"] = Localizer("Rate"),
                ["SearchText"] = sSearchText
            },
            "Job Order" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("JobOrderSearch"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("Status"),
                ["DescriptionHead"] = Localizer("Date"),
                ["CodeHead"] = Localizer("VoucherNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["SearchText"] = sSearchText,
                ["IsNameStatus"] = true,
                ["FilterId"] = filterId
            },
            "Job Card" => new DialogParameters
            {
                ["FormTitle"] = sForm,
                ["FormTitle"] = sForm,
                ["FormTitle2"] = Localizer("JobCardSearch"),
                ["IdHead"] = Localizer("Id"),
                ["NameHead"] = Localizer("PlateNo"),
                ["DescriptionHead"] = Localizer("Customer"),
                ["CodeHead"] = Localizer("VoucherNo"),
                ["AmountHead"] = Localizer("Amount"),
                ["ReferenceHead"] = Localizer("Status"), // ExMasters ReferenceNo maps to this if available, or just use it.
                ["SearchText"] = sSearchText,
                ["IsNameStatus"] = false // Default
            },
            _ => parameters
        };
        var dialog = await DialogService.ShowAsync<TransactionSearchDialog>("", parameters, options);
        var result = await dialog.Result;
        return (ExMasters)result!.Data!;
    }
    public async Task<DialogResult?> MasterCreationDialogMethod<T>(Dictionary<string, object?>? extraParameters = null, MaxWidth maxWidth = MaxWidth.Large) where T : new()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = maxWidth,
            FullWidth = true
        };
        var parameters = new DialogParameters
        {
            { "ComponentType", typeof(T) },
            { "ExtraParameters", extraParameters }
        };
        var dialog = await DialogService.ShowAsync<MasterCreationDialog>("", parameters, options);
        return await dialog.Result;
    }
    public async Task<ZakiSAASERP.Models.Masters.SupplierMaster?> GetSupplierSearchMethod(string sForm)
    {
        var parameters = new DialogParameters { ["InitialSearch"] = sForm };
        var options = new DialogOptions() { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<SupplierSearchDialog>(Localizer("SupplierSearch"), parameters, options);
        var result = await dialog.Result;
        return result!.Data as ZakiSAASERP.Models.Masters.SupplierMaster;
    }
    public async Task<ZakiSAASERP.Models.Masters.CustomerMaster?> GetCustomerSearchMethod(string sSearch = "")
    {
        var parameters = new DialogParameters { ["InitialSearch"] = sSearch };
        var options = new DialogOptions() { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<CustomerSearchDialog>(Localizer("CustomerSearch"), parameters, options);
        var result = await dialog.Result;
        return result!.Data as ZakiSAASERP.Models.Masters.CustomerMaster;
    }

    public async Task InvoicePreviewDialog(byte[] pdfData)
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var parameters = new DialogParameters
        {
            ["pdfData"] = pdfData
        };
        await DialogService.ShowAsync<InvoicePreviewDialog>("", parameters, options);
    }
    public async Task<ExUserPrivilege> PrivilegeDialog(ExUserPrivilege privilegeDtl)
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.ExtraSmall
        };
        var parameters = new DialogParameters
        {
            ["privilegeDtl"] = privilegeDtl
        };
        var val = await DialogService.ShowAsync<PrivilegeEditDialogView>(privilegeDtl.MenuCaption, parameters, options);
        var result = await val.Result;
        if (!result!.Canceled)
        {
            return privilegeDtl;
        }
        return null;
    }
    public async Task<ExFuncDetails> FuncPrivilegeDialog(ExFuncDetails funcDtl)
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.ExtraSmall
        };
        var parameters = new DialogParameters
        {
            ["funcDtl"] = funcDtl
        };
        var val = await DialogService.ShowAsync<FuncPrivilegeDialogView>("", parameters, options);
        var result = await val.Result;
        if (!result!.Canceled)
        {
            return funcDtl;
        }
        return null;
    }
}
