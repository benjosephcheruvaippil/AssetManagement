using AssetManagement.Models;
using Mopups.Interfaces;
using Mopups.Services;
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
        SummaryByHolderName();
        ShowAssetsByEquityAndDebt();
    }

    private async Task SetUpDb()
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
}