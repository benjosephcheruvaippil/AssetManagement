using AssetManagement.Models;
using AssetManagement.Services;
using Microsoft.Maui.Controls.Shapes;
using SQLite;
using System.Globalization;

namespace AssetManagement.Views;

public partial class AssetsByHolder
{
    private string _holderName;
    private SQLiteAsyncConnection _dbConnection;
    public AssetsByHolder(string holderName)
	{
		InitializeComponent();
        _holderName = holderName;
        ShowAssetsByHolder();

    }

    private async Task SetUpDb()
    {
        if (_dbConnection == null)
        {
            string dbPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
            _dbConnection = new SQLiteAsyncConnection(dbPath);
            await _dbConnection.CreateTableAsync<Assets>();
            await _dbConnection.CreateTableAsync<IncomeExpenseModel>();
            await _dbConnection.CreateTableAsync<DataSyncAudit>();
        }
    }

    public async Task ShowAssetsByHolder()
    {
        await SetUpDb();
        List<Assets> records = await _dbConnection.Table<Assets>().OrderBy(d => d.Type).ToListAsync();

        var investmentEntity = records
            .Where(w => w.Holder == _holderName)
            .Select(entity => new EntitywiseModel
            {
                InvestmentEntity = entity.InvestmentEntity,
                TotalAmount = string.Format(new CultureInfo("en-IN"), "{0:C0}", entity.Amount)
            }).ToList();

        foreach (var item in investmentEntity)
        {
            Label name = new Label();
            name.Text = item.InvestmentEntity + " = " + item.TotalAmount;
            name.FontSize = 18;
            name.FontAttributes = FontAttributes.Bold;
            Line objLine = new Line
            {
                X1 = 1,
                Y1 = 1,
                X2 = 180,
                Y2 = 1,
                StrokeThickness = 3,
                Stroke = SolidColorBrush.Red
            };
            stackLayout.Children.Add(name);
            stackLayout.Children.Add(objLine);
        }
    }
}