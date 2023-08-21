using Microsoft.Maui.Controls.Shapes;
using System.Globalization;

namespace AssetManagement.Views;

public partial class AssetsByHolder
{
    private string _holderName;
	public AssetsByHolder(string holderName)
	{
		InitializeComponent();
        _holderName = holderName;
        ShowAssetsByHolder();

    }

    public async Task ShowAssetsByHolder()
    {
        //List<Assets> records = await _assetService.GetAssetsList();
        ////var result = records.Select(s=>s.InvestmentEntity).Where(i => i.Type == "Bank").GroupBy(g=>g.InvestmentEntity).Sum(s => s.Amount);

        //var investmentEntity = records
        //    .Where(w => w.Type == _from)
        //    .GroupBy(g => g.InvestmentEntity)
        //    .Select(entity => new EntitywiseModel
        //    {
        //        InvestmentEntity = entity.First().InvestmentEntity,
        //        TotalAmount = entity.Sum(s => s.Amount).ToString("#,#.##", new CultureInfo(0x0439))
        //    }).ToList();

        //foreach (var item in investmentEntity)
        //{
        //    Label name = new Label();
        //    name.Text = item.InvestmentEntity + " = Rs " + item.TotalAmount;
        //    name.FontSize = 18;
        //    name.FontAttributes = FontAttributes.Bold;
        //    Line objLine = new Line
        //    {
        //        X1 = 1,
        //        Y1 = 1,
        //        X2 = 180,
        //        Y2 = 1,
        //        StrokeThickness = 3,
        //        Stroke = SolidColorBrush.Red
        //    };
        //    stackLayout.Children.Add(name);
        //    stackLayout.Children.Add(objLine);
        //}
    }
}