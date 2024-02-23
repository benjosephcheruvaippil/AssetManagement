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

    public async void ShowNetWorthChart()
    {
        var oldestLog = await _dbConnection.Table<AssetAuditLog>().OrderBy(o => o.CreatedDate).FirstOrDefaultAsync();
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
                ValueLabel = string.Format(new CultureInfo("en-IN"), "{0:C0}", roundedAssetValue),
                Color = SKColor.Parse("#3498db")
            };
            listChartEntry.Add(chartEntry);
        }

        netWorthChangeChart.Chart = new LineChart
        {
            Entries = listChartEntry,
            //Entries = entries,
            LabelTextSize = 70
        };

    }

    public void ShowAssetAllocationChart(List<AssetAllocationReport> assetAllocation)
    {
        List<ChartEntry> listChartEntry = new List<ChartEntry>();
        foreach (var asset in assetAllocation)
        {
            decimal amount = Convert.ToDecimal(asset.Amount);
            int value = (int)Math.Round(amount/1000);
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
                ValueLabel = asset.Amount + "(" + asset.PortfolioPercentage + ")",
                Color = SKColor.Parse(hexCode)
            };
            listChartEntry.Add(chartEntry);
        }
        assetAllocationChart.Chart = new PieChart
        {
            HoleRadius = 10,
            Entries = listChartEntry,
            LabelTextSize = 70
            
        };
    }
}