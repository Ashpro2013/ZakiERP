namespace ZakiSAASERP.Shared.Pages;

public partial class Home
{
    #region Variables
    public ChartOptions chartOptions = new ChartOptions();
    public ChartOptions barChartOptions = new ChartOptions { MaxNumYAxisTicks = 5, YAxisTicks = 5 };
    public ChartOptions purchaseBarChartOptions = new ChartOptions { MaxNumYAxisTicks = 5, YAxisTicks = 5 };
    public ChartOptions pieChartOptions = new ChartOptions();
    public ChartEntity? cEntity { get; set; } = new();
    double[] data = Array.Empty<double>();
    string[] labels = Array.Empty<string>();
    double[] pValue = Array.Empty<double>();
    string[] Products = Array.Empty<string>();
    List<ChartSeries> salesBarSeries = new();
    string[] salesBarLabels = Array.Empty<string>();
    List<ChartSeries> purchaseBarSeries = new();
    string[] purchaseBarLabels = Array.Empty<string>();

    public bool HasPieChartData => data is { Length: > 0 } && labels is { Length: > 0 } && labels.Length == data.Length && data.Sum() > 0;
    public bool HasDonutChartData => pValue is { Length: > 0 } && Products is { Length: > 0 } && Products.Length == pValue.Length && pValue.Sum() > 0;
    public bool HasSalesBarData => salesBarSeries is { Count: > 0 } && salesBarSeries.Any(s => s.Data?.Any() == true);
    public bool HasPurchaseBarData => purchaseBarSeries is { Count: > 0 } && purchaseBarSeries.Any(s => s.Data?.Any() == true);
    #endregion

    #region Events
    protected override async Task OnInitializedAsync()
    {
        try
        {
            DashboardState.OnDashboardRefresh += LoadData;
            chartOptions.MaxNumYAxisTicks = 2;
            if (PermanentSettingsData.ZatcaPhase == 2)
            {
                appBar.SetCount(await api.GetFailedCount());
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error initializing dashboard: {ex.Message}"); }
    }

    private async Task ChartLoadMethod(bool IsReloading = false)
    {
        cEntity = await indexdbHelper.LoadChartEntity(IsReloading)!;
        if (cEntity == null)
        {
            cEntity = new();
            data = Array.Empty<double>();
            labels = Array.Empty<string>();
            pValue = Array.Empty<double>();
            Products = Array.Empty<string>();
            salesBarSeries = new();
            salesBarLabels = Array.Empty<string>();
            purchaseBarSeries = new();
            purchaseBarLabels = Array.Empty<string>();
            return;
        }

        // Top Sold Categories (Pie Chart)
        if (cEntity.GroupwiseSalesList?.Any() == true)
        {
            var validSales = cEntity.GroupwiseSalesList
                .Where(x => x != null && x.Value > 0)
                .ToList();

            if (validSales.Any())
            {
                data = validSales.Select(x => x.Value).ToArray();
                labels = validSales.Select(x => string.IsNullOrWhiteSpace(x.Head) ? "Other" : x.Head).ToArray();
            }
            else
            {
                data = Array.Empty<double>();
                labels = Array.Empty<string>();
            }
        }
        else
        {
            data = Array.Empty<double>();
            labels = Array.Empty<string>();
        }

        // Top Stocked Categories (Donut Chart)
        if (cEntity.GroupwiseStockList?.Any() == true)
        {
            var validStock = cEntity.GroupwiseStockList
                .Where(x => x != null && x.Value > 0)
                .ToList();

            if (validStock.Any())
            {
                pValue = validStock.Select(x => x.Value).ToArray();
                Products = validStock.Select(x => string.IsNullOrWhiteSpace(x.Head) ? "Other" : x.Head).ToArray();
            }
            else
            {
                pValue = Array.Empty<double>();
                Products = Array.Empty<string>();
            }
        }
        else
        {
            pValue = Array.Empty<double>();
            Products = Array.Empty<string>();
        }

        // Build Sales bar chart — last 30 days daily totals
        if (cEntity.DailySalesList?.Any() == true)
        {
            salesBarLabels = cEntity.DailySalesList.Select(x => string.IsNullOrWhiteSpace(x.Head) ? "" : x.Head).ToArray();
            salesBarSeries = new List<ChartSeries>
            {
                new ChartSeries
                {
                    Name = "Sales",
                    Data = cEntity.DailySalesList.Select(x => x.Value).ToArray()
                }
            };
        }
        else
        {
            salesBarSeries = new();
            salesBarLabels = Array.Empty<string>();
        }

        // Build Purchase bar chart — last 30 days daily totals
        if (cEntity.DailyPurchaseList?.Any() == true)
        {
            purchaseBarLabels = cEntity.DailyPurchaseList.Select(x => string.IsNullOrWhiteSpace(x.Head) ? "" : x.Head).ToArray();
            purchaseBarSeries = new List<ChartSeries>
            {
                new ChartSeries
                {
                    Name = "Purchase",
                    Data = cEntity.DailyPurchaseList.Select(x => x.Value).ToArray()
                }
            };
        }
        else
        {
            purchaseBarSeries = new();
            purchaseBarLabels = Array.Empty<string>();
        }
    }

    override protected async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            DashboardState.Refresh();
        }
    }

    private async void LoadData()
    {
        try
        {
            await LoadDashboardData();
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading dashboard data: {ex.Message}");
        }
    }

    private async Task LoadDashboardData()
    {
        await ChartLoadMethod(true);
    }

    public void Dispose()
    {
        DashboardState.OnDashboardRefresh -= LoadData;
    }
    #endregion
}
