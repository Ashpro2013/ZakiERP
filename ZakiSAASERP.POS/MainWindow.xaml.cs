using System.ComponentModel;
using ZakiSAASERP.POS.ViewModels;
using ZakiSAASERP.POS.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;

namespace ZakiSAASERP.POS;

public partial class MainWindow : Window
{
    private readonly POSView _posView;
    private POSViewModel ViewModel => (POSViewModel)_posView.DataContext;

    public MainWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _posView = serviceProvider.GetRequiredService<POSView>();
        MainContentGrid.Children.Add(_posView);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        try
        {
            bool isMssql = string.Equals(Services.AppData.DBProvider, "MSSQL", StringComparison.OrdinalIgnoreCase)
                           || string.IsNullOrWhiteSpace(Services.AppData.DBProvider);

            if (Services.AppData.IsWindows && isMssql && Models.Services.PermanentSettingsData.BackupWhenClosing)
            {
                var result = MessageBox.Show(
                    "Do you want to backup the database before closing the application?",
                    "Back Up Database",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes);

                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (result == MessageBoxResult.Yes)
                {
                    var connStr = !string.IsNullOrWhiteSpace(Services.AppData.ConnectionString)
                        ? Services.AppData.ConnectionString
                        : Services.RegistryHelper.GetData("ConString", "");

                    var backupResult = ZakiSAASERP.MSSQL.MssqlBackupHelper.BackupDatabase(connStr, Models.Services.PermanentSettingsData.BackupFolder);
                    if (backupResult.Success)
                    {
                        MessageBox.Show(
                            $"Database backup completed successfully!\n\nLocation:\n{backupResult.BackupPath}",
                            "Backup Succeeded",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        var errResult = MessageBox.Show(
                            $"Database backup failed:\n{backupResult.Message}\n\nDo you still want to close the application?",
                            "Backup Failed",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);

                        if (errResult == MessageBoxResult.No)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }
            }
        }
        catch
        {
        }
    }


    private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (ViewModel == null || ViewModel.IsBusy) return;

        switch (e.Key)
        {
            case Key.F1:
                await ViewModel.NewBillCommand.ExecuteAsync(null);
                e.Handled = true;
                break;
            case Key.F2:
                await ViewModel.HoldBillCommand.ExecuteAsync(null);
                e.Handled = true;
                break;
            case Key.F3:
                await ViewModel.RetrieveBillCommand.ExecuteAsync(null);
                e.Handled = true;
                break;
            case Key.F4:
                ViewModel.ToggleReturnModeCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.F5:
                await ViewModel.OpenDiscountDialogCommand.ExecuteAsync(null);
                e.Handled = true;
                break;
            case Key.F6:
                await ViewModel.SearchCustomerCommand.ExecuteAsync(null);
                e.Handled = true;
                break;
            case Key.F7:
                await ViewModel.SearchBillCommand.ExecuteAsync(null);
                e.Handled = true;
                break;
            case Key.F8:
                await ViewModel.PrintBillCommand.ExecuteAsync(null);
                e.Handled = true;
                break;
            case Key.F9:
                await ViewModel.PdfBillCommand.ExecuteAsync(null);
                e.Handled = true;
                break;
            case Key.F10:
                await ViewModel.OpenPaymentDialogCommand.ExecuteAsync(null);
                e.Handled = true;
                break;
            case Key.F11:
                await ViewModel.OpenPaymentDialogCommand.ExecuteAsync(null);
                e.Handled = true;
                break;
            case Key.F12:
                await ViewModel.PayCardQuickCommand.ExecuteAsync(null);
                e.Handled = true;
                break;
            case Key.Escape:
                ViewModel.FocusBarcodeHandler?.Invoke();
                break;
        }
    }
}

