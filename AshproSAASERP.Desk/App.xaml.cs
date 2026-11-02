namespace AshproSAASERP.Desk;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; }

    public App(IServiceProvider services)
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Dark;
        Services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(Services.GetRequiredService<StartupPage>()) { Title = "Zaki ERP" };

#if WINDOWS
        window.HandlerChanged += (sender, args) =>
        {
            try
            {
                if (window.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
                {
                    var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                    var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                    var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                    var iconPath = Path.Combine(AppContext.BaseDirectory, "appicon.ico");
                    if (!File.Exists(iconPath))
                    {
                        iconPath = Path.Combine(AppContext.BaseDirectory, "Resources", "AppIcon", "appicon.ico");
                    }
                    if (File.Exists(iconPath))
                    {
                        appWindow.SetIcon(iconPath);
                    }

                    appWindow.Closing += (senderAppWindow, closingArgs) =>
                    {
                        try
                        {
                            bool isMssql = string.Equals(AshproSAASERP.Shared.Services.AppData.DBProvider, "MSSQL", StringComparison.OrdinalIgnoreCase)
                                           || string.IsNullOrWhiteSpace(AshproSAASERP.Shared.Services.AppData.DBProvider);

                            if (AshproSAASERP.Shared.Services.AppData.IsWindows && isMssql && AshproSAASERP.Models.Services.PermanentSettingsData.BackupWhenClosing)
                            {
                                int choice = MessageBox(hWnd,
                                    "Do you want to backup the database before closing the application?",
                                    "Back Up Database",
                                    MB_YESNOCANCEL | MB_ICONQUESTION | MB_DEFBUTTON1);

                                if (choice == IDCANCEL)
                                {
                                    closingArgs.Cancel = true;
                                    return;
                                }

                                if (choice == IDYES)
                                {
                                    var connStr = !string.IsNullOrWhiteSpace(AshproSAASERP.Shared.Services.AppData.ConnectionString)
                                        ? AshproSAASERP.Shared.Services.AppData.ConnectionString
                                        : AshproSAASERP.Desk.Services.RegistryHelper.GetData("ConString", "");

                                    var backupResult = AshproSAASERP.MSSQL.MssqlBackupHelper.BackupDatabase(connStr, AshproSAASERP.Models.Services.PermanentSettingsData.BackupFolder);
                                    if (backupResult.Success)
                                    {
                                        MessageBox(hWnd,
                                            $"Database backup completed successfully!\n\nLocation:\n{backupResult.BackupPath}",
                                            "Backup Succeeded",
                                            MB_OK | MB_ICONINFORMATION);
                                    }
                                    else
                                    {
                                        int errChoice = MessageBox(hWnd,
                                            $"Database backup failed:\n{backupResult.Message}\n\nDo you still want to close the application?",
                                            "Backup Failed",
                                            MB_YESNO | MB_ICONWARNING);

                                        if (errChoice == IDNO)
                                        {
                                            closingArgs.Cancel = true;
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    };
                }
            }
            catch
            {
                // Fallback silently if platform icon cannot be set
            }
        };
#endif

        return window;
    }

#if WINDOWS
    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

    private const uint MB_OK = 0x00000000;
    private const uint MB_YESNO = 0x00000004;
    private const uint MB_YESNOCANCEL = 0x00000003;
    private const uint MB_ICONINFORMATION = 0x00000040;
    private const uint MB_ICONQUESTION = 0x00000020;
    private const uint MB_ICONWARNING = 0x00000030;
    private const uint MB_DEFBUTTON1 = 0x00000000;

    private const int IDYES = 6;
    private const int IDNO = 7;
    private const int IDCANCEL = 2;
#endif
}
