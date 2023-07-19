using AssetManagement.Models;
using AssetManagement.Services;
using AssetManagement.ViewModels;
using ExcelDataReader;
using Mopups.Interfaces;
using Plugin.LocalNotification;
using SQLite;
using System.Data;
using System.Globalization;

namespace AssetManagement.Views;

public partial class AssetListPage : ContentPage
{
    private AssetListPageViewModel _viewModel;
    private SQLiteAsyncConnection _dbConnection;
    private IPopupNavigation _popupNavigation;
    private readonly IAssetService _assetService;
    public AssetListPage(AssetListPageViewModel viewModel, IPopupNavigation popupNavigation, IAssetService assetService)
	{
		InitializeComponent();
		_viewModel = viewModel;
		this.BindingContext= _viewModel;
        _popupNavigation = popupNavigation;
        _assetService = assetService;

        //tap gesture added for different labels
        var labelBank = new TapGestureRecognizer();
        labelBank.Tapped += (s, e) =>
        {
            _popupNavigation.PushAsync(new AssetsByCategoryPage("from bank assets", _assetService));
        };
        lblBank.GestureRecognizers.Add(labelBank);
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.GetAssetsList();
        await ShowPrimaryAssetDetails();
    }

    private async Task SetUpDb()
    {
        if (_dbConnection == null)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
            _dbConnection = new SQLiteAsyncConnection(dbPath);
            await _dbConnection.CreateTableAsync<Assets>();
        }
    }

    private async void PickFileClicked(object sender, EventArgs e)
    {
        //PickFileClicked
        //OnCounterClicked
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Pick File Please"
            });

            if (result == null)
                return;

            DataSet dsexcelRecords = new DataSet();
            IExcelDataReader reader = null;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var filestream = await result.OpenReadAsync();
            reader = ExcelReaderFactory.CreateOpenXmlReader(filestream);
            dsexcelRecords = reader.AsDataSet();
            reader.Close();
            await SetUpDb();
            await _dbConnection.DeleteAllAsync<Assets>(); // delete all records currenly present in the table

            if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
            {
                DataTable dtStudentRecords = dsexcelRecords.Tables[0];
                for (int i = 1; i < dtStudentRecords.Rows.Count; i++)
                {
                    string InvestmentEntity = Convert.ToString(dtStudentRecords.Rows[i][1]);
                    string Type = Convert.ToString(dtStudentRecords.Rows[i][2]);
                    decimal Amount = Convert.ToDecimal(dtStudentRecords.Rows[i][3]);
                    decimal InterestRate = Convert.ToDecimal(dtStudentRecords.Rows[i][4]);
                    string InterestFrequency = Convert.ToString(dtStudentRecords.Rows[i][5]);
                    string Holder = Convert.ToString(dtStudentRecords.Rows[i][6]);
                    DateTime StartDate;
                    DateTime MaturityDate;
                    DateTime AsOfDate;
                    if (dtStudentRecords.Rows[i][7] is DBNull)
                    {
                        //set as current date
                        StartDate = Convert.ToDateTime("01-01-0001");
                    }
                    else
                    {
                        StartDate = Convert.ToDateTime(dtStudentRecords.Rows[i][7]);
                    }
                    if (dtStudentRecords.Rows[i][8] is DBNull)
                    {
                        MaturityDate = Convert.ToDateTime("01-01-0001");
                    }
                    else
                    {
                        MaturityDate = Convert.ToDateTime(dtStudentRecords.Rows[i][8]);
                    }
                    string Remarks = Convert.ToString(dtStudentRecords.Rows[i][9]);

                    if (dtStudentRecords.Rows[i][10] is DBNull)
                    {
                        AsOfDate = Convert.ToDateTime("01-01-0001");
                    }
                    else
                    {
                        AsOfDate = Convert.ToDateTime(dtStudentRecords.Rows[i][10]);
                    }

                    var assets = new Assets
                    {
                        InvestmentEntity = InvestmentEntity,
                        Type = Type,
                        Amount = Amount,
                        InterestRate = InterestRate,
                        InterestFrequency = InterestFrequency,
                        Holder = Holder,
                        StartDate = StartDate,
                        MaturityDate = MaturityDate,
                        Remarks = Remarks
                    };
                    await SetUpDb();
                    int rowsAffected = await _dbConnection.InsertAsync(assets);
                }
            }
            await DisplayAlert("Info", "File Processed Successfully", "OK");
            _viewModel.GetAssetsList();
            await ShowPrimaryAssetDetails();
            //throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            //await DisplayAlert("Alert - Message", ex.Message.ToString(), "OK");
            await DisplayAlert("Alert - StackTrace", ex.StackTrace.ToString(), "OK");
        }
    }

    private async void ShowRecords_Clicked(object sender, EventArgs e)
    {

        await ShowPrimaryAssetDetails();
    }

    private async void AssetsMaturingSoon_Clicked(object sender, EventArgs e)
    {
        //await SetUpDb();
        //List<Assets> records = await _dbConnection.Table<Assets>().ToListAsync();
        //List<Assets> assetsMaturingIn10Days = (from rec in records
        //                                       where (rec.MaturityDate < DateTime.Now.AddDays(60))
        //                                       select new Assets
        //                                       {
        //                                           InvestmentEntity = rec.InvestmentEntity,
        //                                           Amount = rec.Amount,
        //                                           MaturityDate = rec.MaturityDate
        //                                       }).ToList();
        //foreach (var asset in assetsMaturingIn10Days)
        //{
        //    var request = new NotificationRequest
        //    {
        //        NotificationId = 1000,
        //        Title = asset.InvestmentEntity,
        //        Subtitle = Convert.ToString(asset.MaturityDate),
        //        Description = Convert.ToString(asset.Amount),
        //        BadgeNumber = 42,
        //        Schedule = new NotificationRequestSchedule
        //        {
        //            NotifyTime = DateTime.Now.AddSeconds(5),
        //            NotifyRepeatInterval = TimeSpan.FromDays(1)
        //        }
        //    };
        //    await LocalNotificationCenter.Current.Show(request);
        //}

        //GetAssetsList();
    }

    public async Task ShowPrimaryAssetDetails()
    {
        await SetUpDb();
        List<Assets> records = await _dbConnection.Table<Assets>().ToListAsync();
        decimal NetAssetValue = records.Sum(s => s.Amount);
        string result = "Net Asset Value: Rs." + NetAssetValue.ToString("#,#.##", new CultureInfo(0x0439));
        lblNetAssetValue.Text = result;

        decimal BankAssets = records.Where(b => b.Type == "Bank").Sum(s => s.Amount);
        decimal NCDAssets = records.Where(b => b.Type == "NCD").Sum(s => s.Amount);
        decimal MLDAssets = records.Where(b => b.Type == "MLD").Sum(s => s.Amount);

        decimal Insurance_MF = records.Where(b => b.Type == "Insurance_MF").Sum(s => s.Amount);
        decimal PPF = records.Where(b => b.Type == "PPF").Sum(s => s.Amount);
        decimal EPF = records.Where(b => b.Type == "EPF").Sum(s => s.Amount);
        decimal MutualFunds = records.Where(b => b.Type == "MF").Sum(s => s.Amount);
        decimal Stocks = records.Where(b => b.Type == "Stocks").Sum(s => s.Amount);

        lblBank.Text = "Bank Assets Value: Rs." + BankAssets.ToString("#,#.##", new CultureInfo(0x0439));
        lblNCD.Text = "Non Convertible Debentures: Rs." + NCDAssets.ToString("#,#.##", new CultureInfo(0x0439));
        lblMLD.Text = "Market Linked Debentures: Rs." + MLDAssets.ToString("#,#.##", new CultureInfo(0x0439));

        lblInsuranceMF.Text = "Insurance/MF: Rs." + Insurance_MF.ToString("#,#.##", new CultureInfo(0x0439));
        lblPPF.Text = "Public Provident Fund: Rs." + PPF.ToString("#,#.##", new CultureInfo(0x0439));
        lblEPF.Text = "Employee Provident Fund: Rs." + EPF.ToString("#,#.##", new CultureInfo(0x0439));
        lblMF.Text = "Mutual Funds: Rs." + MutualFunds.ToString("#,#.##", new CultureInfo(0x0439));
        lblStocks.Text = "Stocks: Rs." + Stocks.ToString("#,#.##", new CultureInfo(0x0439));

        decimal projectedAmount = 0;
        foreach (var item in records)
        {
            if (item.MaturityDate.ToShortDateString() != "01-01-0001") //debt instruments which have a maturity
            {
                int daysTillMaturity = (item.MaturityDate - item.StartDate).Days;
                decimal totalInterest = (item.Amount * daysTillMaturity * item.InterestRate) / (365 * 100); //(P × d × R)/ (365 ×100)
                decimal finalAmount = item.Amount + totalInterest;

                projectedAmount = projectedAmount + finalAmount;
            }
            else // assets that dont have maturity like stocks,mutual funds,gold etc
            {
                projectedAmount = projectedAmount + item.Amount;
            }
        }
        lblProjectedAssetValue.Text = "Projected Asset Value: Rs." + Math.Round(projectedAmount, 2).ToString("#,#.##", new CultureInfo(0x0439)); ;

    }

    private void Popup_Clicked(object sender, EventArgs e)
    {
        //_popupNavigation.PushAsync(new AssetsByCategoryPage(""));
    }
}