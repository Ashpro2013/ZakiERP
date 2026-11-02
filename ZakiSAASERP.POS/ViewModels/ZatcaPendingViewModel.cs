using ZakiSAASERP.Interface.Transaction.Sales;
using ZakiSAASERP.Models.Zatca;
using ZakiSAASERP.POS.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ZakiSAASERP.POS.ViewModels;

public partial class ZatcaPendingViewModel : ObservableObject
{
    private readonly IZatcaInvoiceRepo _zatcaRepo;
    private readonly IZatcaUploadService _uploadService;

    [ObservableProperty]
    private ObservableCollection<ZatcaInvoices> pendingInvoices = [];

    [ObservableProperty]
    private int totalPending;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = "";

    public Action? CloseAction { get; set; }

    public ZatcaPendingViewModel(IZatcaInvoiceRepo zatcaRepo, IZatcaUploadService uploadService)
    {
        _zatcaRepo = zatcaRepo;
        _uploadService = uploadService;
    }

    [RelayCommand]
    public async Task LoadPendingInvoicesAsync()
    {
        IsBusy = true;
        StatusMessage = "Fetching pending ZATCA invoices...";

        try
        {
            int branchId = AppData.BranchId > 0 ? AppData.BranchId : 1;
            var list = await _zatcaRepo.GetAllZatcaInvoiceByBranchId(branchId);

            PendingInvoices.Clear();
            if (list != null)
            {
                foreach (var inv in list)
                {
                    PendingInvoices.Add(inv);
                }
            }
            TotalPending = PendingInvoices.Count;
            StatusMessage = TotalPending == 0 ? "All invoices are synchronized with ZATCA." : $"{TotalPending} invoices pending submission.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task UploadAllAsync()
    {
        if (TotalPending == 0) return;

        IsBusy = true;
        StatusMessage = "Uploading pending invoices to ZATCA...";

        try
        {
            int branchId = AppData.BranchId > 0 ? AppData.BranchId : 1;
            await _uploadService.UploadPendingSimplifiedInvoices(branchId);
            StatusMessage = "Upload completed. Refreshing status...";
            await LoadPendingInvoicesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Upload error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void Close()
    {
        CloseAction?.Invoke();
    }
}

