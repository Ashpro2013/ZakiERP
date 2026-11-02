using ZakiSAASERP.Models.User;
using Microsoft.AspNetCore.Components.Web;

namespace ZakiSAASERP.Shared.Pages.Users;

public partial class UserView : ITabAware
{
    [Parameter] public int TabId { get; set; }
    public class BranchDropItem
    {
        public BranchMaster Branch { get; set; } = new();
        public string Zone { get; set; } = "available";
    }

    #region Variables
    private UserDetails? userDetails { get; set; } = new();
    MudTextField<string>? txtUserName;
    MudTextField<string>? txtPassword;
    private MudDataGrid<UserDetails>? grid;

    private List<UserPrivilegeHdr> privileges = new();
    private List<BranchMaster> allBranches = new();
    private List<BranchDropItem> _dropZoneItems = new();
    private List<UserBranch> userBranches = new();
    private WebUserPrivilege _privilege = new();
    #endregion

    #region Events
    protected override async Task OnInitializedAsync()
    {
        await LoadPrivileges();
        await LoadBranches();
        if (AppData.privileges is not null)
        {
            _privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "ManageUser") ?? new();
        }
    }
    private async Task LoadPrivileges()
    {
        try
        {
            privileges = await api.GetUserprivileges();
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    private async Task LoadBranches()
    {
        try
        {
            allBranches = await api.GetAllBranches();
            RefreshDropZone();
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    private void RefreshDropZone()
    {
        _dropZoneItems = allBranches.Select(b => new BranchDropItem { Branch = b, Zone = "available" }).ToList();
    }

    private async Task<GridData<UserDetails>> LoadServerData(GridState<UserDetails> state)
    {
        var pagination = new PaginationModel(
           page: state.Page + 1,
            pageSize: state.PageSize,
            sortColumn: state.SortDefinitions?.FirstOrDefault()?.SortBy,
            sortDirection: state.SortDefinitions?.FirstOrDefault()?.Descending == true ? "desc" : "asc",
            search: null
        );
        var result = await api.GetUserPDetails(pagination);
        return new GridData<UserDetails>
        {
            Items = result.PUsers,
            TotalItems = result.TotalCount
        };
    }

    private void ItemDropped(MudItemDropInfo<BranchDropItem> dropItem)
    {
        dropItem.Item.Zone = dropItem.DropzoneIdentifier;
    }

    private void ToggleBranch(BranchDropItem item)
    {
        item.Zone = item.Zone == "available" ? "selected" : "available";
        StateHasChanged();
    }

    private void SelectAllBranches()
    {
        foreach (var item in _dropZoneItems)
        {
            item.Zone = "selected";
        }
        StateHasChanged();
    }

    private void ClearAllBranches()
    {
        foreach (var item in _dropZoneItems)
        {
            item.Zone = "available";
        }
        StateHasChanged();
    }

    async Task SaveOrUpdate()
    {
        var selectedBranches = _dropZoneItems.Where(x => x.Zone == "selected").Select(x => x.Branch).ToList();
        if (!selectedBranches.Any())
        {
            Dialog.DialogMethod("Please select at least one branch.");
            return;
        }

        userDetails!.FK_BranchId = selectedBranches.First().BranchId;

        // Trim inputs
        userDetails.UserName = userDetails.UserName.Trim();
        if (userDetails.UserName.Contains('@'))
        {
            userDetails.UserName = userDetails.UserName.Split('@')[0];
        }
        userDetails.PassWord = userDetails.PassWord.Trim();

        if (userDetails?.UserId == 0)
        {
            if (!_privilege.SaveData && !string.Equals(AppData.UserName, "admin", StringComparison.OrdinalIgnoreCase))
            {
                Dialog.DialogMethod("You have no privilege to Save User!");
                return;
            }
            var result = await api.CreateUser(userDetails);
            if (result.Message == null)
            {
                // Save user branches
                await SaveUserBranches(result.Id);
                Dialog.DialogMethod(Localizer("SaveMessage"));
                await NewClick();
            }
            else
            {
                Dialog.DialogMethod(result.Message);
            }
        }
        else
        {
            if (!_privilege.EditData && !string.Equals(AppData.UserName, "admin", StringComparison.OrdinalIgnoreCase))
            {
                Dialog.DialogMethod("You have no privilege to Edit User!");
                return;
            }
            var result = await api.UpdateUser(userDetails!);
            if (result.Message == null)
            {
                // Update user branches
                await SaveUserBranches(userDetails.UserId);
                Dialog.DialogMethod(Localizer("UpdateMessage"));
                await NewClick();
            }
            else
            {
                Dialog.DialogMethod(result.Message);
            }
        }
    }

    private async Task SaveUserBranches(int userId)
    {
        // Clear existing user branches
        userBranches.Clear();

        // Add new user branches
        var selected = _dropZoneItems.Where(x => x.Zone == "selected").Select(x => x.Branch).ToList();
        foreach (var branch in selected)
        {
            userBranches.Add(new UserBranch
            {
                FK_UserId = userId,
                FK_BranchId = branch.BranchId
            });
        }

        // Call API to save user branches
        var result = await api.SaveUserBranches(userBranches);
        if (result.Message is not null)
        {
            Dialog.DialogMethod("Error saving branches: " + result.Message);
        }
    }

    async Task Delete()
    {
        if (!_privilege.DeleteData && !string.Equals(AppData.UserName, "admin", StringComparison.OrdinalIgnoreCase))
        {
            Dialog.DialogMethod("You have no privilege to Delete User!");
            return;
        }
        if (await Dialog.DeleteDialogMethod())
        {
            var result = await api.DeleteUser(userDetails!.UserId);
            if (result.Message == null)
            {
                Dialog.DialogMethod("Deleted Successfully");
                await NewClick();
            }
            else
            {
                Dialog.DialogMethod(result.Message);
            }
        }
        else
        {
            await NewClick();
        }
    }

    async Task NewClick()
    {
        await grid!.ReloadServerData();
        userDetails = new UserDetails();
        RefreshDropZone();
        txtUserName?.FocusAsync();
    }


    async Task RowClicked(DataGridRowClickEventArgs<UserDetails> args)
    {
        userDetails = args.Item;

        // Load user branches for this user
        var loadedBranches = await api.GetUserBranches(userDetails.UserName);
        RefreshDropZone();
        foreach (var item in _dropZoneItems)
        {
            if (loadedBranches.Any(ub => ub.BranchId == item.Branch.BranchId)) item.Zone = "selected";
        }

        await txtUserName!.FocusAsync();
        await txtUserName!.SelectAsync();
    }
    async Task KeyDown(KeyboardEventArgs e, int fieldId)
    {
        if (e.Key == "Enter" || e.Key == "ArrowDown")
            await DownKeyEvent(fieldId);
        else if (e.Key == "ArrowUp")
        {
            if (fieldId == 2) await txtUserName!.FocusAsync();
        }
    }

    private async Task DownKeyEvent(int fieldId)
    {
        if (fieldId == 1) await txtPassword!.FocusAsync();
    }
    #endregion
}
