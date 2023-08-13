using AssetManagement.Models;
using SQLite;
using System.Data.Common;
using System.Globalization;

namespace AssetManagement.Views;

public partial class AssetReportPage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    public AssetReportPage(SQLiteAsyncConnection dbConnection)
	{
		InitializeComponent();
        _dbConnection = dbConnection;
        SummaryByCategory();

    }

    private async Task SetUpDb()
    {
        if (_dbConnection == null)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
            _dbConnection = new SQLiteAsyncConnection(dbPath);
        }
    }

    public async void SummaryByCategory()
    {
        var holders = await _dbConnection.QueryAsync<Assets>("select Holder from Assets Group By Holder");

        double total = 0;
        foreach (var holder in holders)
        {
            if (!string.IsNullOrEmpty(holder.Holder))
            {
                total = 0;
                string query = "select Sum(Amount) as Amount from Assets where Holder='" + holder.Holder + "'";
                List<Assets> holderWiseSum = await _dbConnection.QueryAsync<Assets>(query);
                //foreach (var item in records)
                //{
                //    if (item.CategoryName == category.CategoryName)
                //    {
                //        total = total + item.Amount;
                //    }

                //}
                TextCell objHolder = new TextCell();
                objHolder.Text = holder.Holder;
                objHolder.TextColor = Colors.DarkBlue;
                objHolder.Detail = "Rs. " + holderWiseSum[0].Amount.ToString("#,#.##", new CultureInfo(0x0439));
                objHolder.Height = 40;
                tblscHolderWiseReport.Add(objHolder);
            }
        }

        //tblscCategoryWiseReport.Title = "Category Wise Report " + selectedYear;


    }
}