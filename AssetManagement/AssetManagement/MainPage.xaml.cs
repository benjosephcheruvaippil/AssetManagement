using AssetManagement.Models;
using CommunityToolkit.Mvvm.Input;
using ExcelDataReader;
using Plugin.LocalNotification;
using SQLite;
using System.Collections.ObjectModel;
using System.Data;

namespace AssetManagement;

public partial class MainPage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    public ObservableCollection<Assets> Assets { get; set; } = new ObservableCollection<Assets>();

    public MainPage()
	{
		InitializeComponent();
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

    private async void OnCounterClicked(object sender, EventArgs e)
    {
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
                    if (dtStudentRecords.Rows[i][7] is DBNull)
                    {
                        //set as current date
                        StartDate = DateTime.MinValue;
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
        await SetUpDb();
        List<Assets> records = await _dbConnection.Table<Assets>().ToListAsync();
        decimal NetAssetValue = records.Sum(s => s.Amount);
        string result = "Net Asset Value: Rs." + NetAssetValue;
        lblNetAssetValue.Text = result;

        decimal BankAssets = records.Where(b => b.Type == "Bank").Sum(s => s.Amount);
        decimal NCDAssets = records.Where(b => b.Type == "NCD").Sum(s => s.Amount);
        decimal MLDAssets = records.Where(b => b.Type == "MLD").Sum(s => s.Amount);

        lblBank.Text = "Bank Assets Value: Rs." + BankAssets;
        lblNCD.Text = "NCD Assets Value: Rs." + NCDAssets;
        lblMLD.Text = "MLD Assets Value: Rs." + MLDAssets;

        decimal projectedAmount = 0;
        foreach (var item in records)
        {
            int daysTillMaturity = (item.MaturityDate - item.StartDate).Days;
            decimal totalInterest = (item.Amount * daysTillMaturity * item.InterestRate) / (365 * 100); //(P × d × R)/ (365 ×100)
            decimal finalAmount = item.Amount + totalInterest;

            projectedAmount = projectedAmount + finalAmount;
        }
        lblProjectedAssetValue.Text = "Projected Asset Value: Rs." + Math.Round(projectedAmount, 2);


    }

    private async void AssetsMaturingSoon_Clicked(object sender, EventArgs e)
    {
        await SetUpDb();
        List<Assets> records = await _dbConnection.Table<Assets>().ToListAsync();
        List<Assets> assetsMaturingIn10Days = (from rec in records
                                               where (rec.MaturityDate < DateTime.Now.AddDays(60))
                                               select new Assets
                                               {
                                                   InvestmentEntity = rec.InvestmentEntity,
                                                   Amount = rec.Amount,
                                                   MaturityDate = rec.MaturityDate
                                               }).ToList();
        foreach (var asset in assetsMaturingIn10Days)
        {
            var request = new NotificationRequest
            {
                NotificationId = 1000,
                Title = asset.InvestmentEntity,
                Subtitle = Convert.ToString(asset.MaturityDate),
                Description = Convert.ToString(asset.Amount),
                BadgeNumber = 42,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Now.AddSeconds(5),
                    NotifyRepeatInterval = TimeSpan.FromDays(1)
                }
            };
            await LocalNotificationCenter.Current.Show(request);
        }

        //GetAssetsList();
    }
}

