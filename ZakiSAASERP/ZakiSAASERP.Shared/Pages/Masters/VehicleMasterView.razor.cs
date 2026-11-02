using Microsoft.AspNetCore.Components.Forms;

namespace ZakiSAASERP.Shared.Pages.Masters
{
    public partial class VehicleMasterView : ITabAware
    {
        [Parameter] public int TabId { get; set; }
        [Inject] ISnackbar Snackbar { get; set; } = null!;

        private VehicleMaster vehicle = new();
        private MudDataGrid<VehicleMaster>? grid;
        private string searchText = "";
        private EditForm form;
        private string itemImagePreview = "";
        private MudButton saveButton;
        private InfoTextField<string> _plateNoField;
        private WebUserPrivilege? privilege;
        [Parameter] public bool IsDialog { get; set; } = false;

        private async Task FocusSave()
        {
            await saveButton.FocusAsync();
        }

        private IBrowserFile? pendingImageFile;
        private byte[]? pendingImageBytes;
        private string? oldImagePathToDelete;

        private CustomerMaster? selectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                vehicle.FK_CustomerId = value?.CustomerId ?? 0;
                if (value is not null)
                {
                    vehicle.CustomerName = value.Name;
                }
                else
                {
                    vehicle.CustomerName = null;
                }
            }
        }
        private CustomerMaster? _selectedCustomer;

        protected override async Task OnInitializedAsync()
        {
            if (AppData.privileges is not null)
                privilege = AppData.privileges.FirstOrDefault(x => x.Caption == "VehicleMaster");
            await base.OnInitializedAsync();
            await NewClick();
        }

        private async Task<GridData<VehicleMaster>> LoadServerData(GridState<VehicleMaster> state)
        {
            var pagination = new PaginationModel
            {
                page = state.Page + 1,
                pageSize = state.PageSize,
                sortColumn = state.SortDefinitions.FirstOrDefault()?.SortBy,
                sortDirection = state.SortDefinitions.FirstOrDefault()?.Descending == true ? "desc" : "asc",
                search = searchText
            };

            var result = await api.GetVehiclePDetails(pagination);
            return new GridData<VehicleMaster>
            {
                TotalItems = result.TotalCount,
                Items = result.Items ?? new List<VehicleMaster>()
            };
        }

        private bool _isSaving = false;

        private async Task SaveOrUpdate()
        {
            if (_isSaving) return;
            if (vehicle.FK_CustomerId <= 0)
            {
                Snackbar.Add(Localizer("SelectCustomerRequired"), Severity.Error);
                return;
            }

            _isSaving = true;
            StateHasChanged();

            try
            {
                // Handle Image Upload
                if (pendingImageBytes is not null)
                {
                    // If updating and replacing an image, track old one for deletion
                    if (!string.IsNullOrWhiteSpace(vehicle.ImagePath) && oldImagePathToDelete == null)
                    {
                        oldImagePathToDelete = vehicle.ImagePath;
                    }
                }

                if (vehicle.VehicleId == 0)
                {
                    if (privilege is not null && !privilege.SaveData) { Dialog.DialogMethod(Localizer("NoSavePrivilege")); return; }
                    var res = await api.CreateVehicle(vehicle);
                    if (res.Id > 0)
                    {
                        vehicle.VehicleId = res.Id;

                        if (pendingImageBytes is not null)
                        {
                            var newPath = await ProcessPendingImageUpload(vehicle.VehicleId);
                            if (newPath is not null)
                            {
                                vehicle.ImagePath = newPath;
                                // Update with image path
                                await api.UpdateVehicle(vehicle);
                            }
                        }

                        if (IsDialog)
                        {
                            AppData.PassingValue = new CommonModel
                            {
                                Id = vehicle.VehicleId,
                                Name = vehicle.PlateNo,
                                // Add other relevant fields if PassingValue supports them or if user needs them
                            };
                            // In dialog mode, we might want to close? 
                            // But MasterCreationDialog has its own "Ok" button which reads AppData.PassingValue.
                            // Actually MasterCreationDialog's "Submit" button calls Dialog.Close(..., PassingValue).
                            // BUT VehicleMasterView has its OWN "Save" button usually.
                            // If user clicks "Save" inside VehicleMasterView, it saves.
                            // If it's a dialog, it should probably close automatically or user clicks OK in the wrapper.
                            // MasterCreationDialog has "Ok" which calls Submit->Close.
                            // If user clicks "Save" in VehicleMasterView, it saves but stays open?
                            // In CustomerMasterView:
                            // if (IsDialog) { AppData.PassingValue = ...; } 
                            // Dialog.DialogMethod(Localizer("SaveMessage")); // Shows success
                            // await NewClick(); // Resets
                        }

                        Snackbar.Add(Localizer("VehicleCreatedSuccess"), Severity.Success);
                        await NewClick();
                        await grid.ReloadServerData();
                    }
                    else
                    {
                        Snackbar.Add(Localizer(res.Message), Severity.Error);
                    }
                }
                else
                {
                    if (privilege is not null && !privilege.EditData) { Dialog.DialogMethod(Localizer("NoEditPrivilege")); return; }
                    if (pendingImageBytes is not null)
                    {
                        // Upload new image
                        var newPath = await ProcessPendingImageUpload(vehicle.VehicleId);
                        if (newPath is not null)
                        {
                            vehicle.ImagePath = newPath;
                        }
                    }

                    // Delete old image if removed/replaced
                    if (!string.IsNullOrWhiteSpace(oldImagePathToDelete))
                    {
                        // Call delete image API
                        string basePath = PermanentSettingsData.IsWebBased ? "WEBROOT" : PermanentSettingsData.ImageLocalBasePath;
                        var clientFolder = PermanentSettingsData.ImageClientFolder;
                        await api.DeleteImage(basePath, oldImagePathToDelete, clientFolder);
                        oldImagePathToDelete = null;
                        if (string.IsNullOrEmpty(vehicle.ItemImageName)) // If removed completely
                        {
                            vehicle.ImagePath = null;
                        }
                    }

                    var res = await api.UpdateVehicle(vehicle);
                    if (res.Id > 0)
                    {
                        Snackbar.Add(Localizer("VehicleUpdatedSuccess"), Severity.Success);
                        await NewClick();
                        await grid.ReloadServerData();
                    }
                    else
                    {
                        Snackbar.Add(Localizer(res.Message), Severity.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
            finally
            {
                _isSaving = false;
                StateHasChanged();
            }
        }

        private async Task<string?> ProcessPendingImageUpload(int vehicleId)
        {
            if (pendingImageBytes == null || pendingImageFile == null) return null;
            try
            {
                string basePath = PermanentSettingsData.IsWebBased ? "WEBROOT" : PermanentSettingsData.ImageLocalBasePath;
                string clientFolder = PermanentSettingsData.ImageClientFolder;

                // EntityType = "Vehicle"
                return await api.UploadImage(pendingImageBytes, pendingImageFile.Name, "Vehicle", vehicleId, basePath, clientFolder);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task UploadVehicleImage(IBrowserFile file)
        {
            try
            {
                // Simple validation
                if (file.Size > 5120000) // 5MB limit
                {
                    Snackbar.Add(Localizer("FileSizeExceedsLimit"), Severity.Error);
                    return;
                }

                var format = "image/png";
                var buffer = new byte[file.Size];
                using (var stream = file.OpenReadStream(5120000))
                {
                    using (var ms = new MemoryStream())
                    {
                        await stream.CopyToAsync(ms);
                        buffer = ms.ToArray();
                    }
                }

                pendingImageFile = file;
                pendingImageBytes = buffer;
                vehicle.ItemImageName = file.Name;

                var base64 = Convert.ToBase64String(buffer);
                itemImagePreview = $"data:{format};base64,{base64}";
            }
            catch (Exception ex)
            {
                Snackbar.Add(Localizer("ErrorReadingFile") + ": " + ex.Message, Severity.Error);
            }
        }

        private void RemoveVehicleImage()
        {
            if (!string.IsNullOrWhiteSpace(vehicle.ImagePath))
            {
                oldImagePathToDelete = vehicle.ImagePath;
            }
            vehicle.ItemImageName = null;
            vehicle.ImagePath = null;
            itemImagePreview = "";
            pendingImageBytes = null;
            pendingImageFile = null;
        }

        private async Task NewClick()
        {
            vehicle = new VehicleMaster();
            itemImagePreview = "";
            pendingImageBytes = null;
            pendingImageFile = null;
            oldImagePathToDelete = null;

            // Default to Cash Customer
            try
            {
                var pmodel = new PaginationModel(1, 10, null, null, "Cash Customer");
                var res = await api.GetCustomerPDetails(pmodel);
                if (res.PCustomers is not null && res.PCustomers.Any())
                {
                    // Try to find exact match first, otherwise take the first result
                    var cashCustomer = res.PCustomers.FirstOrDefault(x => x.Name.Equals("Cash Customer", StringComparison.OrdinalIgnoreCase))
                                       ?? res.PCustomers.FirstOrDefault();

                    selectedCustomer = cashCustomer; // This also sets vehicle.CustomerName via the setter
                }
                else
                {
                    selectedCustomer = null;
                    _selectedCustomer = null;
                }
            }
            catch
            {
                // Fallback if search fails
                selectedCustomer = null;
                _selectedCustomer = null;
            }
            StateHasChanged();
            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (_plateNoField is not null)
                {
                    await _plateNoField.FocusAsync();
                }
            }
        }


        private async Task Delete()
        {
            if (vehicle.VehicleId == 0) return;

            if (privilege is not null && !privilege.DeleteData) { Dialog.DialogMethod(Localizer("NoDeletePrivilege")); return; }

            var confirm = await DialogService.ShowMessageBox(Localizer("Confirm"), Localizer("DeleteConfirmation"), yesText: Localizer("Delete"), cancelText: Localizer("Cancel"));
            if (confirm == true)
            {
                var res = await api.DeleteVehicle(vehicle.VehicleId);
                if (res.Id > 0)
                {
                    Snackbar.Add(Localizer("VehicleDeletedSuccess"), Severity.Success);
                    await NewClick();
                    await grid.ReloadServerData();
                }
                else
                {
                    Snackbar.Add(Localizer(res.Message), Severity.Error);
                }
            }
        }

        private async Task RowClicked(DataGridRowClickEventArgs<VehicleMaster> args)
        {
            vehicle = await api.GetVehicleById(args.Item.VehicleId);

            // Load Customer
            if (vehicle.FK_CustomerId > 0)
            {
                var loadedName = vehicle.CustomerName; // Preserve the name from DB
                var cust = await api.GetCustomerById(vehicle.FK_CustomerId);

                // Set backing field directly to avoid triggering the setter logic that overwrites CustomerName
                _selectedCustomer = cust;
                selectedCustomer = _selectedCustomer; // Trigger UI update if needed, but we already have name

                // Restore the name from DB
                vehicle.CustomerName = loadedName;
            }
            else
            {
                selectedCustomer = null;
                _selectedCustomer = null;
            }

            // Load Image Preview
            if (!string.IsNullOrWhiteSpace(vehicle.ImagePath))
            {
                string basePath = PermanentSettingsData.IsWebBased ? "WEBROOT" : PermanentSettingsData.ImageLocalBasePath;
                itemImagePreview = await api.GetImageUrl(basePath, vehicle.ImagePath);
            }
            else
            {
                itemImagePreview = "";
            }

            pendingImageBytes = null;
            pendingImageFile = null;
            oldImagePathToDelete = null;
        }

        private async Task<IEnumerable<CustomerMaster>> SearchCustomers(string value, CancellationToken token)
        {
            // Server side search is ideal. Using Paging endpoint or Create a specific Search Endpoint.
            // For now, let's use GetCustomerPDetails filtering.
            var pmodel = new PaginationModel(1, 10, null, null, value);
            var res = await api.GetCustomerPDetails(pmodel);
            return res.PCustomers ?? new List<CustomerMaster>();
        }

        private void ShowImagePopup(string src)
        {
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium };
            var parameters = new DialogParameters { ["ImageUrl"] = src };
            DialogService.Show<ImagePreviewDialog>("Image", parameters, options);
        }
    }
}
