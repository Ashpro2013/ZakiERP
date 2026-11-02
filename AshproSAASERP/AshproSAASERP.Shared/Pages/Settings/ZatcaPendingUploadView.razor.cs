using AshproSAASERP.Models.Zatca;

namespace AshproSAASERP.Shared.Pages.Settings;

public partial class ZatcaPendingUploadView
{
    #region Properties
    [Parameter] public int TabId { get; set; }
    #endregion

    #region Methods
    private async Task<GridData<ZatcaInvoices>> LoadServerData(GridState<ZatcaInvoices> state)
    {
        var pagination = new PaginationModel(
           page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: ""
        );
        var result = await api.GetZatcaPendingInvoicesAsync(pagination);

        return new GridData<ZatcaInvoices>
        {
            Items = result.ZatcaInvoices,
            TotalItems = result.count
        };
    }
    async Task UploadAll()
    {
        await api.UploadPendingSimplifiedInvoices(AppData.BranchId);
        await Dialog.DialogMethod("Successfully Uploaded");
    }
    #endregion

}
