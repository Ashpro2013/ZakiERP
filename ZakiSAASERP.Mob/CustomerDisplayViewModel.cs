using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ZakiSAASERP.Mob;

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
