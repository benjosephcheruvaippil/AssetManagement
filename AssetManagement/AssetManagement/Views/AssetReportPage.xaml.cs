using AssetManagement.Models;
using AssetManagement.Models.Constants;
using AssetManagement.Models.Reports;
using AssetManagement.Services;
using AssetManagement.ViewModels;
using Microcharts;
//using Mopups.Interfaces;
//using Mopups.Services;
using SkiaSharp;
using SQLite;
using System.Data.Common;
using System.Globalization;
using System.Xml.Linq;

namespace AssetManagement.Views;

public partial class AssetReportPage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    //private IPopupNavigation _popupNavigation;
    public AssetListPageViewModel _viewModel;
    public IAssetService _assetService;
    AppTheme currentTheme = Application.Current.RequestedTheme;
    public AssetReportPage(AssetListPageViewModel viewModel, IAssetService assetService)
    {
        InitializeComponent();
        //_popupNavigation = popupNavigation;
        _viewModel = viewModel;
        _assetService = assetService;
        SetUpDb();
        //SummaryByHolderName();
        //ShowAssetsByEquityAndDebt();
        //ShowNetWorthChart();
    }

    private void SetUpDb()
    {
        if (_dbConnection == null)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
            _dbConnection = new SQLiteAsyncConnection(dbPath);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        //Device.BeginInvokeOnMainThread(async () =>
        //{
        //    await DisplayAlert("Welcome", "The secondary page has opened!", "OK");
        //});
        Task.Run(async () => { await CheckIfAssetsPresent(); });
        
    }
    public async Task CheckIfAssetsPresent()
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (await SummaryByHolderName())
            {
                ShowAssetsByEquityAndDebt();
                ShowNetWorthChart();
            }
        });
    }

    public async Task<bool> SummaryByHolderName()
    {
        bool dataExists = false;
        try
        {
            var holders = await _dbConnection.QueryAsync<Assets>("select Holder from Assets Group By Holder");
            List<Assets> holderDetailList = new List<Assets>();
            if (holders.Count > 0)
            {
                dataExists = true;
                foreach (var holder in holders)
                {
                    if (!string.IsNullOrEmpty(holder.Holder))
                    {
                        string query = "select Sum(Amount) as Amount from Assets where Holder='" + holder.Holder + "'";
                        List<Assets> holderWiseSum = await _dbConnection.QueryAsync<Assets>(query);

                        Assets holderDetails = new Assets
                        {
                            Holder = holder.Holder,
                            Amount = holderWiseSum[0].Amount
                        };
                        holderDetailList.Add(holderDetails);
                    }
                }

                holderDetailList = holderDetailList.OrderByDescending(x => x.Amount).ToList();

                foreach (var holder in holderDetailList)
                {
                    //TapGestureRecognizer textCell = new TapGestureRecognizer();
                    TextCell objHolder = new TextCell();
                    objHolder.Text = holder.Holder;
                    if (currentTheme == AppTheme.Dark)
                    {
                        //set to white color
                        objHolder.TextColor = Color.FromArgb("#FFFFFF");
                    }
                    objHolder.Detail = string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", holder.Amount);
                    objHolder.Height = 40;
                    objHolder.Tapped += (sender, args) =>
                    {
                        //_popupNavigation.PushAsync(new AssetsByHolder(holder.Holder));
                        ShowAssetsByHolder(holder.Holder);
                    };

                    tblscHolderWiseReport.Add(objHolder);
                }
            }
            else
            {
                await DisplayAlert("Info", "Add assets to see reports here!", "Ok");
                if (Application.Current.MainPage is FlyoutPage flyoutPage)
                {
                    flyoutPage.Detail = new NavigationPage(new AssetPage(_viewModel, _assetService));
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Info", ex.Message, "Ok");
        }
        return dataExists;
    }

    public async void ShowAssetsByHolder(string holderName)
    {
        string displayText = "";
        SetUpDb();
        string query = "select InvestmentEntity,Sum(Amount) as Amount from Assets where Holder='" + holderName + "' group by InvestmentEntity " +
            "order by InvestmentEntity ASC";
        var investmentEntity = await _dbConnection.QueryAsync<Assets>(query);

        foreach (var item in investmentEntity)
        {
            displayText = displayText + item.InvestmentEntity + ": " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", item.Amount) + "\n";
        }

        await DisplayAlert("Asset Info", displayText, "Ok");
    }

    public async void ShowAssetsByEquityAndDebt()
    {
        try
        {
            var totalAmount = await _dbConnection.QueryAsync<Assets>("select Sum(Amount) as Amount from Assets");
            if (totalAmount[0].Amount > 0)
            {
                var debtPortfolioAmount = await _dbConnection.QueryAsync<Assets>("select Sum(Amount) as Amount from Assets where Type in ('Bank'" +
                    ",'NCD','MLD','PPF','EPF')");
                decimal debtPortfolioPercentage = Math.Round((debtPortfolioAmount[0].Amount / totalAmount[0].Amount) * 100, 2);
                Label lblDebt = new Label();
                lblDebt.Text = "Debt/Fixed Income: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", debtPortfolioAmount[0].Amount) + " (" + debtPortfolioPercentage + "%)";
                lblDebt.FontSize = 18;
                lblDebt.FontAttributes = FontAttributes.Bold;

                var equityPortfolioAmount = await _dbConnection.QueryAsync<Assets>("select Sum(Amount) as Amount from Assets where Type in ('Insurance_MF'" +
                    ",'MF','Stocks')");
                decimal equityPortfolioPercentage = Math.Round((equityPortfolioAmount[0].Amount / totalAmount[0].Amount) * 100, 2);
                Label lblEquity = new Label();
                lblEquity.Text = "Equity: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", equityPortfolioAmount[0].Amount) + " (" + equityPortfolioPercentage + "%)";
                lblEquity.FontSize = 18;
                lblEquity.FontAttributes = FontAttributes.Bold;

                var goldPortfolioAmount = await _dbConnection.QueryAsync<Assets>("select Sum(Amount) as Amount from Assets where Type in ('SGB'" +
                    ",'Gold')");
                decimal goldPortfolioPercentage = Math.Round((goldPortfolioAmount[0].Amount / totalAmount[0].Amount) * 100, 2);
                Label lblGold = new Label();
                lblGold.Text = "Gold: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", goldPortfolioAmount[0].Amount) + " (" + goldPortfolioPercentage + "%)";
                lblGold.FontSize = 18;
                lblGold.FontAttributes = FontAttributes.Bold;

                stackLayout.Add(lblDebt);
                stackLayout.Add(lblEquity);
                stackLayout.Add(lblGold);

                //new report
                List<AssetAllocationReport> assetAllocationReport = new List<AssetAllocationReport>();
                AssetAllocationReport objAssetAllocationDebt = new AssetAllocationReport();
                objAssetAllocationDebt.AssetType = "Debt";
                objAssetAllocationDebt.Amount = debtPortfolioAmount[0].Amount.ToString();
                objAssetAllocationDebt.PortfolioPercentage = debtPortfolioPercentage.ToString() + "%";
                assetAllocationReport.Add(objAssetAllocationDebt);

                AssetAllocationReport objAssetAllocationEquity = new AssetAllocationReport();
                objAssetAllocationEquity.AssetType = "Equity";
                objAssetAllocationEquity.Amount = equityPortfolioAmount[0].Amount.ToString();
                objAssetAllocationEquity.PortfolioPercentage = equityPortfolioPercentage.ToString() + "%";
                assetAllocationReport.Add(objAssetAllocationEquity);

                AssetAllocationReport objAssetAllocationGold = new AssetAllocationReport();
                objAssetAllocationGold.AssetType = "Gold";
                objAssetAllocationGold.Amount = goldPortfolioAmount[0].Amount.ToString();
                objAssetAllocationGold.PortfolioPercentage = goldPortfolioPercentage.ToString() + "%";
                assetAllocationReport.Add(objAssetAllocationGold);
                ShowAssetAllocationChart(assetAllocationReport);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Info", ex.Message, "Ok");
        }
    }

    public async void ShowNetWorthChart()
    {
        try
        {
            var oldestLog = await _dbConnection.Table<AssetAuditLog>().OrderBy(o => o.CreatedDate).FirstOrDefaultAsync();
            if (oldestLog != null)
            {
                var latestLog = await _dbConnection.Table<AssetAuditLog>().OrderByDescending(o => o.CreatedDate).FirstOrDefaultAsync();
                List<NetWorthChangeReport> listNetWorthChangeReport = new List<NetWorthChangeReport>();
                if (oldestLog != null)
                {
                    int startYear = oldestLog.CreatedDate.Year;
                    int endYear = latestLog.CreatedDate.Year;
                    for (int year = startYear; year <= endYear; year++)
                    {
                        for (int j = 1; j <= 4; j++)
                        {
                            string quarterName = "";
                            int fromMonth = 0, toMonth = 0;

                            if (j == 1)//q1 - january-march
                            {
                                quarterName = "Jan-Mar";
                                fromMonth = 1;
                                toMonth = 3;
                            }
                            else if (j == 2)//q2 - april-june
                            {
                                quarterName = "Apr-Jun";
                                fromMonth = 4;
                                toMonth = 6;
                            }
                            else if (j == 3)//q3 - july-sept
                            {
                                quarterName = "Jul-Sep";
                                fromMonth = 7;
                                toMonth = 9;
                            }
                            else if (j == 4)//q4 - oct-dec
                            {
                                quarterName = "Oct-Dec";
                                fromMonth = 10;
                                toMonth = 12;
                            }

                            DateTime fromDate = new DateTime(year, fromMonth, 1, 0, 0, 0);
                            DateTime toDate = new DateTime(year, toMonth, DateTime.DaysInMonth(year, toMonth), 23, 59, 59);
                            var quarter_list = await _dbConnection.Table<AssetAuditLog>().Where(a => a.CreatedDate >= fromDate && a.CreatedDate <= toDate).ToListAsync();
                            //find average amount
                            double? totalNetAssetValue = 0;
                            foreach (var log in quarter_list)
                            {
                                totalNetAssetValue = totalNetAssetValue + log.NetAssetValue;
                            }
                            int avgNetAssetValue = 0;
                            if (totalNetAssetValue != 0)
                            {
                                avgNetAssetValue = (int)Math.Round((double)(totalNetAssetValue / quarter_list.Count));
                            }
                            NetWorthChangeReport objNetWorthChangeReport = new NetWorthChangeReport();
                            objNetWorthChangeReport.NetAssetValue = avgNetAssetValue;
                            objNetWorthChangeReport.DisplayLabel = quarterName + " " + year.ToString();
                            listNetWorthChangeReport.Add(objNetWorthChangeReport);
                        }
                    }
                }

                List<ChartEntry> listChartEntry = new List<ChartEntry>();
                listNetWorthChangeReport = listNetWorthChangeReport.Where(l => l.NetAssetValue > 0).ToList();
                foreach (var log in listNetWorthChangeReport)
                {
                    int roundedAssetValue = (int)Math.Round((decimal)log.NetAssetValue);
                    double ratio = roundedAssetValue / 10000;
                    int value = (int)Math.Round(ratio);
                    ChartEntry chartEntry = new ChartEntry(value)
                    {
                        Label = log.DisplayLabel,
                        ValueLabel = string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", roundedAssetValue),
                        Color = SKColor.Parse("#3498db")
                    };
                    listChartEntry.Add(chartEntry);
                }

                netWorthChangeChart.Chart = new LineChart
                {
                    Entries = listChartEntry,
                    LabelTextSize = 70
                };
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Info", ex.Message, "Ok");
        }
    }

    public void ShowAssetAllocationChart(List<AssetAllocationReport> assetAllocation)
    {
        try
        {
            List<ChartEntry> listChartEntry = new List<ChartEntry>();
            foreach (var asset in assetAllocation)
            {
                decimal amount = Convert.ToDecimal(asset.Amount);
                int value = (int)Math.Round(amount / 1000);
                string hexCode = "";


                if (asset.AssetType == "Debt")
                {
                    hexCode = "#0000FF";
                }
                else if (asset.AssetType == "Equity")
                {
                    hexCode = "#00FF00";
                }
                else if (asset.AssetType == "Gold")
                {
                    hexCode = "#FFFF00";
                }
                ChartEntry chartEntry = new ChartEntry(value)
                {
                    Label = asset.AssetType,
                    //ValueLabel = asset.Amount + "(" + asset.PortfolioPercentage + ")",
                    ValueLabel = "(" + asset.PortfolioPercentage + ")",
                    Color = SKColor.Parse(hexCode)
                };
                listChartEntry.Add(chartEntry);
            }
            assetAllocationChart.Chart = new PieChart
            {
                HoleRadius = 2,
                Entries = listChartEntry,
                LabelTextSize = 30,
                LabelMode = LabelMode.LeftAndRight,
                GraphPosition = GraphPosition.AutoFill
            };
        }
        catch (Exception ex)
        {
            DisplayAlert("Info", ex.Message, "Ok");
        }
    }
}