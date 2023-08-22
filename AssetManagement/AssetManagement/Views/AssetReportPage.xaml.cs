using AssetManagement.Models;
using Mopups.Interfaces;
using Mopups.Services;
using SQLite;
using System.Data.Common;
using System.Globalization;

namespace AssetManagement.Views;

public partial class AssetReportPage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    private IPopupNavigation _popupNavigation;
    public AssetReportPage(SQLiteAsyncConnection dbConnection, IPopupNavigation popupNavigation)
	{
		InitializeComponent();
        _dbConnection = dbConnection;
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
        decimal debtPortfolioPercentage = (debtPortfolioAmount[0].Amount / totalAmount[0].Amount) * 100;
        //assetsByEquityAndDebt.chil
        Label lblDebt = new Label();
        lblDebt.Text = "Debt/Fixed Income: Rs. " + debtPortfolioAmount[0].Amount + " (" + debtPortfolioPercentage + ")";

        var equityPortfolioAmount = await _dbConnection.QueryAsync<Assets>("select Sum(Amount) as Amount from Assets where Type in ('Insurance_MF'" +
            ",'MF','Stocks')");
        decimal equityPortfolioPercentage = (equityPortfolioAmount[0].Amount / totalAmount[0].Amount) * 100;
        Label lblEquity = new Label();
        lblEquity.Text = "Equity: Rs. " + equityPortfolioAmount[0].Amount + " (" + equityPortfolioPercentage + ")";
        stackLayout.Add(lblDebt);
        stackLayout.Add(lblEquity);
    }
}