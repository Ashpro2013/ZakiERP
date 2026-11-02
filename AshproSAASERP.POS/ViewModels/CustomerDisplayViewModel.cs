using AshproSAASERP.Models.DTO;
using AshproSAASERP.Models.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;

namespace AshproSAASERP.POS.ViewModels;

public partial class CustomerDisplayViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<SaleItem> items = [];

    [ObservableProperty]
    private decimal total;

    public void Update(CustomerDisplayDto dto)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            Items.Clear();
            if (dto?.Items != null)
            {
                foreach (var item in dto.Items)
                {
                    Items.Add(item);
                }
            }
            Total = dto?.Total ?? 0m;
        });
    }
}

