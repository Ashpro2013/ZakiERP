using AshproSAASERP.Models.DTO;
using AshproSAASERP.Models.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AshproSAASERP.Mob;

public partial class CustomerDisplayViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<SaleItem> items = new();

    [ObservableProperty]
    private decimal total;

    public void Update(CustomerDisplayDto dto)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Items.Clear();

            foreach (var item in dto.Items)
                Items.Add(item);

            Total = dto.Total;
        });
    }
}
