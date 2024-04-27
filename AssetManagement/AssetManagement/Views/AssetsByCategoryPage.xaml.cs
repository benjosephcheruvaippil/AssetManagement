using AssetManagement.Models;
using AssetManagement.Services;
using Microsoft.Maui.Controls.Shapes;
using SQLite;
using System.Drawing;
using System.Globalization;
using Color = Microsoft.Maui.Graphics.Color;

namespace AssetManagement.Views;

public partial class AssetsByCategoryPage
{
    private SQLiteAsyncConnection _dbConnection;
    private readonly IAssetService _assetService;
    AppTheme currentTheme = Application.Current.RequestedTheme;
    private string _from;
    public AssetsByCategoryPage(string from, IAssetService assetService)
	{
		InitializeComponent();
        _from = from;
        _assetService = assetService;
        ShowAssetsByEntity();
        //lblFrom.Text = from;
        //_assetService = assetService;
    }

    public async void ShowAssetsByEntity()
	{
        List<Assets> records = await _assetService.GetAssetsList();
        List<EntitywiseModel> investmentEntity;
        if (_from.Contains("Gold"))
        {
            string[] type = _from.Split(',');
            investmentEntity = records
            .Where(w => type.Contains(w.Type))
            .GroupBy(g => g.InvestmentEntity)
            .Select(entity => new EntitywiseModel
            {
                InvestmentEntity = entity.First().InvestmentEntity,
                TotalAmount = string.Format(new CultureInfo("en-IN"), "{0:C0}", entity.Sum(s => s.Amount))
            }).ToList();
        }
        else if (_from == "EPF,PPF,NPS")
        {
            string[] type = _from.Split(',');
            investmentEntity = records
            .Where(w => type.Contains(w.Type))
            .GroupBy(g => g.InvestmentEntity)
            .Select(entity => new EntitywiseModel
            {
                InvestmentEntity = entity.First().InvestmentEntity,
                TotalAmount = string.Format(new CultureInfo("en-IN"), "{0:C0}", entity.Sum(s => s.Amount))
            }).ToList();
        }
        else
        {
            investmentEntity = records
                .Where(w => w.Type == _from)
                .GroupBy(g => g.InvestmentEntity)
                .Select(entity => new EntitywiseModel
                {
                    InvestmentEntity = entity.First().InvestmentEntity,
                    TotalAmount = string.Format(new CultureInfo("en-IN"), "{0:C0}", entity.Sum(s => s.Amount))
                }).ToList();
        }

        foreach(var item in investmentEntity)
        {
            Label name = new Label();
            name.Text = item.InvestmentEntity + " = " + item.TotalAmount;
            name.FontSize = 18;
            name.FontAttributes = FontAttributes.Bold;
            if (currentTheme == AppTheme.Dark)
            {
                //set to black color
                name.TextColor = Color.FromArgb("#000000");
            }
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