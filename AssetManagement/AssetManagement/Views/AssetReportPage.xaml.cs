using AssetManagement.Models;
using AssetManagement.Models.Reports;
using Microcharts;
using Mopups.Interfaces;
using Mopups.Services;
using SkiaSharp;
using SQLite;
using System.Data.Common;
using System.Globalization;
using System.Xml.Linq;

namespace AssetManagement.Views;

public partial class AssetReportPage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    private IPopupNavigation _popupNavigation;
    public AssetReportPage(IPopupNavigation popupNavigation)
	{
		InitializeComponent();
        _popupNavigation=popupNavigation;
        SetUpDb();
        SummaryByHolderName();
        ShowAssetsByEquityAndDebt();
        ShowNetWorthChart();
    }

    private void SetUpDb()
    {
        if (_dbConnection == null)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
            _dbConnection = new SQLiteAsyncConnection(dbPath);
        }
    }

    public async void SummaryByHolderName()
    {
        var holders = await _dbConnection.QueryAsync<Assets>("select Holder from Assets Group By Holder");

        List<Assets> holderDetailList = new List<Assets>();
        foreach (var holder in holders)
        {
            if (!string.IsNullOrEmpty(holder.Holder))
            {
                string query = "select Sum(Amount) as Amount from Assets where Holder='" + holder.Holder + "'";
                List<Assets> holderWiseSum = await _dbConnection.QueryAsync<Assets>(query);

                Assets holderDetails = new Assets
                {
                    Holder=holder.Holder,
                    Amount = holderWiseSum[0].Amount
                };
                holderDetailList.Add(holderDetails);
            }
        }

        holderDetailList= holderDetailList.OrderByDescending(x => x.Amount).ToList();

        foreach (var holder in holderDetailList)
        {
            //TapGestureRecognizer textCell = new TapGestureRecognizer();
            TextCell objHolder = new TextCell();
            objHolder.Text = holder.Holder;
            objHolder.TextColor = Colors.DarkBlue;
            objHolder.Detail = string.Format(new CultureInfo("en-IN"), "{0:C0}", holder.Amount);
            objHolder.Height = 40;
            objHolder.Tapped += (sender, args) =>
            {
                _popupNavigation.PushAsync(new AssetsByHolder(holder.Holder));
            };

            tblscHolderWiseReport.Add(objHolder);
        }
    }

    public async void ShowAssetsByEquityAndDebt()
    {
        var totalAmount = await _dbConnection.QueryAsync<Assets>("select Sum(Amount) as Amount from Assets");
        var debtPortfolioAmount = await _dbConnection.QueryAsync<Assets>("select Sum(Amount) as Amount from Assets where Type in ('Bank'" +
            ",'NCD','MLD','PPF','EPF')");
        decimal debtPortfolioPercentage = Math.Round((debtPortfolioAmount[0].Amount / totalAmount[0].Amount) * 100, 2);
        Label lblDebt = new Label();
        lblDebt.Text = "Debt/Fixed Income: " + string.Format(new CultureInfo("en-IN"), "{0:C0}", debtPortfolioAmount[0].Amount) + " (" + debtPortfolioPercentage + "%)";
        lblDebt.FontSize = 18;
        lblDebt.FontAttributes = FontAttributes.Bold;

        var equityPortfolioAmount = await _dbConnection.QueryAsync<Assets>("select Sum(Amount) as Amount from Assets where Type in ('Insurance_MF'" +
            ",'MF','Stocks')");
        decimal equityPortfolioPercentage = Math.Round((equityPortfolioAmount[0].Amount / totalAmount[0].Amount) * 100, 2);
        Label lblEquity = new Label();
        lblEquity.Text = "Equity: " + string.Format(new CultureInfo("en-IN"), "{0:C0}", equityPortfolioAmount[0].Amount) + " (" + equityPortfolioPercentage + "%)";
        lblEquity.FontSize = 18;
        lblEquity.FontAttributes = FontAttributes.Bold;

        var goldPortfolioAmount= await _dbConnection.QueryAsync<Assets>("select Sum(Amount) as Amount from Assets where Type in ('SGB'" +
            ",'Gold')");
        decimal goldPortfolioPercentage = Math.Round((goldPortfolioAmount[0].Amount / totalAmount[0].Amount) * 100, 2);
        Label lblGold = new Label();
        lblGold.Text = "Gold: " + string.Format(new CultureInfo("en-IN"), "{0:C0}", goldPortfolioAmount[0].Amount) + " (" + goldPortfolioPercentage + "%)";
        lblGold.FontSize = 18;
        lblGold.FontAttributes = FontAttributes.Bold;

        stackLayout.Add(lblDebt);
        stackLayout.Add(lblEquity);
        stackLayout.Add(lblGold);
    }

    public async void ShowNetWorthChart()
    {
        ChartEntry[] entries = new[]
        {
            new ChartEntry(212)
            {
                Label = "Windows",
                ValueLabel = "112",
                Color = SKColor.Parse("#2c3e50")
            },
            new ChartEntry(248)
            {
                Label = "Android",
                ValueLabel = "648",
                Color = SKColor.Parse("#77d065")
            },
            new ChartEntry(128)
            {
                Label = "iOS",
                ValueLabel = "428",
                Color = SKColor.Parse("#b455b6")
            },
            new ChartEntry(514)
            {
                Label = ".NET MAUI",
                ValueLabel = "214",
                Color = SKColor.Parse("#3498db")
            }
        };

        var oldestLog = await _dbConnection.Table<AssetAuditLog>().OrderBy(o => o.CreatedDate).FirstOrDefaultAsync();
        List<NetWorthChangeReport> listNetWorthChangeReport = new List<NetWorthChangeReport>();
        if (oldestLog != null)
        {
            int startYear = oldestLog.CreatedDate.Year;
            //q1
            DateTime fromDate= DateTime.Now;
            DateTime toDate= DateTime.Now;
            var Q1_List = await _dbConnection.Table<AssetAuditLog>().Where(a => a.CreatedDate >= fromDate && a.CreatedDate <= toDate).ToListAsync();
            //find average amount
            double? totalNetAssetValue = 0;
            foreach (var log in Q1_List)
            {
                totalNetAssetValue = totalNetAssetValue + log.NetAssetValue;
            }
            int avgNetAssetValueForQ1 = (int)Math.Round((double)(totalNetAssetValue / Q1_List.Count));
            NetWorthChangeReport objNetWorthChangeReport = new NetWorthChangeReport();
            objNetWorthChangeReport.NetAssetValue = avgNetAssetValueForQ1;
            objNetWorthChangeReport.DisplayLabel = "Jan-Mar " + startYear.ToString();
        }
        //q1
        //q2
        //q3
        //q4

        //List<ChartEntry> listChartEntry = new List<ChartEntry>();
        //foreach(var log in assetLog)
        //{
        //    int roundedAssetValue = (int)Math.Round((decimal)log.NetAssetValue);
        //    double r = roundedAssetValue / 50000;
        //    int chartParameter = (int)Math.Round(r);
        //    ChartEntry chartEntry = new ChartEntry(chartParameter)
        //    {
        //        Label = log.CreatedDate.ToString("dd-MM-yyyy"),
        //        ValueLabel = string.Format(new CultureInfo("en-IN"), "{0:C0}", roundedAssetValue),
        //        Color = SKColor.Parse("#3498db")
        //    };
        //    listChartEntry.Add(chartEntry);
        //}



        netWorthChangeChart.Chart = new LineChart
        {
            //Entries = listChartEntry,
            Entries = entries,
            LabelTextSize = 70
        };

        assetAllocationChart.Chart = new PieChart
        {
            //Entries = listChartEntry,
            Entries = entries,
            LabelTextSize = 70
        };
    }

    public void ShowAssetAllocationChart()
    {

    }
}