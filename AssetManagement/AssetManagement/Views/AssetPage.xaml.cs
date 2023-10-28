using AssetManagement.Models;
using AssetManagement.Services;
using AssetManagement.ViewModels;
using ExcelDataReader;
using Mopups.Interfaces;
using SQLite;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Net.Http.Json;

namespace AssetManagement.Views;

public partial class AssetPage : TabbedPage
{
    private AssetListPageViewModel _viewModel;
    private SQLiteAsyncConnection _dbConnection;
    private IPopupNavigation _popupNavigation;
    private readonly IAssetService _assetService;

    private readonly HttpClient httpClient = new();

    public bool IsRefreshing { get; set; }
    public ObservableCollection<Assets> Assets { get; set; } = new();
    public Command RefreshCommand { get; set; }
    public Assets SelectedAsset { get; set; }
    public bool PaginationEnabled { get; set; } = true;
    public AssetPage(AssetListPageViewModel viewModel, IPopupNavigation popupNavigation, IAssetService assetService)
    {
        RefreshCommand = new Command(async () =>
        {
            // Simulate delay
            await Task.Delay(2000);

            await LoadAssets();

            IsRefreshing = false;
            OnPropertyChanged(nameof(IsRefreshing));
        });

        //BindingContext = this;

        InitializeComponent();
        _viewModel = viewModel;
        this.BindingContext = _viewModel;
        _popupNavigation = popupNavigation;
        _assetService = assetService;

        //tap gesture added for different labels
        var labelBank = new TapGestureRecognizer();
        labelBank.Tapped += (s, e) =>
        {
            _popupNavigation.PushAsync(new AssetsByCategoryPage("Bank", _assetService));
        };
        lblBank.GestureRecognizers.Add(labelBank);

        var labelNCD = new TapGestureRecognizer();
        labelNCD.Tapped += (s, e) =>
        {
            _popupNavigation.PushAsync(new AssetsByCategoryPage("NCD", _assetService));
        };
        lblNCD.GestureRecognizers.Add(labelNCD);

        var labelMLD = new TapGestureRecognizer();
        labelMLD.Tapped += (s, e) =>
        {
            _popupNavigation.PushAsync(new AssetsByCategoryPage("MLD", _assetService));
        };
        lblMLD.GestureRecognizers.Add(labelMLD);

        var labelInsurance_MF = new TapGestureRecognizer();
        labelInsurance_MF.Tapped += (s, e) =>
        {
            _popupNavigation.PushAsync(new AssetsByCategoryPage("Insurance_MF", _assetService));
        };
        lblInsuranceMF.GestureRecognizers.Add(labelInsurance_MF);
    }

    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        await LoadAssets();
    }

    private async Task LoadAssets()
    {
        await _viewModel.LoadAssets();
    }

    private async void btnGetDetail_Clicked(object sender, EventArgs e)
    {
        Assets obj = _viewModel.GetSelectedRecordDetail();

        if (obj.AssetId != 0)
        {

            entEntityName.Text = obj.InvestmentEntity;
            entType.SelectedItem = obj.Type;
            entAmount.Text = Convert.ToString(obj.Amount);
            entInterestRate.Text = Convert.ToString(obj.InterestRate);
            entInterestFrequency.Text = obj.InterestFrequency;
            entHolder.SelectedItem = obj.Holder;
            entStartDate.Date = obj.StartDate;
            entMaturityDate.Date = obj.MaturityDate;
            entAsOfDate.Date = obj.AsOfDate;
            entRemarks.Text = obj.Remarks;

            lblAssetId.Text = Convert.ToString(obj.AssetId);
        }
        else
        {
            await DisplayAlert("Message", "Please select an asset", "OK");
        }
        //btnSave.IsVisible = true;
        //btnDelete.IsVisible = true;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        lblMaturingAssetsTotalValue.Text = "Total Value: " + string.Format(new CultureInfo("en-IN"), "{0:C0}", await _viewModel.GetAssetsList());
        await ShowPrimaryAssetDetails();
    }

    private async Task SetUpDb()
    {
        if (_dbConnection == null)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
            _dbConnection = new SQLiteAsyncConnection(dbPath);
            await _dbConnection.CreateTableAsync<Assets>();
            await _dbConnection.CreateTableAsync<IncomeExpenseModel>();
            await _dbConnection.CreateTableAsync<DataSyncAudit>();
        }
    }

    private async void PickFileClicked(object sender, EventArgs e)
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
            await DisplayAlert("Alert - StackTrace", ex.StackTrace.ToString(), "OK");
        }
    }

    public async Task ShowPrimaryAssetDetails()
    {
        await SetUpDb();
        List<Assets> records = await _dbConnection.Table<Assets>().ToListAsync();
        decimal NetAssetValue = records.Sum(s => s.Amount);
        string result = "Net Asset Value: " + string.Format(new CultureInfo("en-IN"), "{0:C0}", NetAssetValue);
        lblNetAssetValue.Text = result;

        decimal BankAssets = records.Where(b => b.Type == "Bank").Sum(s => s.Amount);
        decimal NCDAssets = records.Where(b => b.Type == "NCD").Sum(s => s.Amount);
        decimal MLDAssets = records.Where(b => b.Type == "MLD").Sum(s => s.Amount);

        decimal Insurance_MF = records.Where(b => b.Type == "Insurance_MF").Sum(s => s.Amount);
        decimal PPF = records.Where(b => b.Type == "PPF").Sum(s => s.Amount);
        decimal EPF = records.Where(b => b.Type == "EPF").Sum(s => s.Amount);
        decimal MutualFunds = records.Where(b => b.Type == "MF").Sum(s => s.Amount);
        decimal Stocks = records.Where(b => b.Type == "Stocks").Sum(s => s.Amount);

        lblBank.Text = "Bank Assets Value: " + string.Format(new CultureInfo("en-IN"), "{0:C0}", BankAssets);
        lblNCD.Text = "Non Convertible Debentures: " + string.Format(new CultureInfo("en-IN"), "{0:C0}", NCDAssets);
        lblMLD.Text = "Market Linked Debentures: " + string.Format(new CultureInfo("en-IN"), "{0:C0}", MLDAssets);

        lblInsuranceMF.Text = "Insurance/MF: " + string.Format(new CultureInfo("en-IN"), "{0:C0}", Insurance_MF);
        lblPPF.Text = "Public Provident Fund: " + string.Format(new CultureInfo("en-IN"), "{0:C0}", PPF);
        lblEPF.Text = "Employee Provident Fund: " + string.Format(new CultureInfo("en-IN"), "{0:C0}", EPF);
        lblMF.Text = "Mutual Funds: " + string.Format(new CultureInfo("en-IN"), "{0:C0}", MutualFunds);
        lblStocks.Text = "Stocks: " + string.Format(new CultureInfo("en-IN"), "{0:C0}", Stocks);

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
        lblProjectedAssetValue.Text = "Projected Asset Value: " + string.Format(new CultureInfo("en-IN"), "{0:C0}", Math.Round(projectedAmount, 2));

    }

    private async void entryDays_TextChanged(object sender, TextChangedEventArgs e)
    {
        string daysLeft = entryDays.Text;
        if (string.IsNullOrEmpty(entryDays.Text))
        {
            daysLeft = "0";
        }
        lblMaturingAssetsTotalValue.Text = "Total Value: " + string.Format(new CultureInfo("en-IN"), "{0:C0}", await _viewModel.GetMaturingAssetsListByDaysLeft(Convert.ToInt32(daysLeft)));
    }

    private async void btnSave_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(entEntityName.Text))
        {
            await DisplayAlert("Message", "Please input all required fields", "OK");
            return;
        }

        int rowsAffected = 0;
        Models.Assets objAsset = new Assets()
        {
            InvestmentEntity = entEntityName.Text,
            Type = entType.SelectedItem.ToString(),
            Amount = Convert.ToDecimal(entAmount.Text),
            InterestRate = Convert.ToDecimal(entInterestRate.Text),
            InterestFrequency = entInterestFrequency.Text,
            Holder = entHolder.SelectedItem.ToString(),
            //StartDate = entStartDate.Date,
            //MaturityDate = entMaturityDate.Date,
            //AsOfDate = entAsOfDate.Date,
            Remarks = entRemarks.Text
        };

        if(objAsset.Type=="Insurance_MF" || objAsset.Type=="PPF" || objAsset.Type=="EPF" || objAsset.Type=="MF" || objAsset.Type == "Stocks")
        {
            objAsset.AsOfDate = entAsOfDate.Date;
            objAsset.StartDate= Convert.ToDateTime("01-01-0001");
            objAsset.MaturityDate= Convert.ToDateTime("01-01-0001");

            entStartDate.IsEnabled = false;
            entMaturityDate.IsEnabled = false;
        }
        else
        {
            objAsset.AsOfDate = Convert.ToDateTime("01-01-0001");
            objAsset.StartDate = entStartDate.Date;
            objAsset.MaturityDate = entMaturityDate.Date;

            entAsOfDate.IsEnabled = false;
        }

        await SetUpDb();
        if (string.IsNullOrEmpty(lblAssetId.Text)) //insert asset
        {
            rowsAffected = await _dbConnection.InsertAsync(objAsset);
        }
        else //update asset
        {
            objAsset.AssetId = Convert.ToInt32(lblAssetId.Text);
            rowsAffected = await _dbConnection.UpdateAsync(objAsset);
        }

        if (rowsAffected > 0)
        {
            await DisplayAlert("Message", "Asset Saved", "OK");
            await LoadAssets();
        }
    }

    private async void btnDelete_Clicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(lblAssetId.Text))
        {
            bool userResponse = await DisplayAlert("Warning", "Are you sure to delete?", "Yes", "No");
            if (userResponse)
            {
                Models.Assets objAsset = new Assets()
                {
                    AssetId = Convert.ToInt32(lblAssetId.Text)
                };

                await SetUpDb();
                int rowsAffected = await _dbConnection.DeleteAsync(objAsset);
                if (rowsAffected > 0)
                {
                    await DisplayAlert("Message", "Asset Deleted", "OK");
                    await LoadAssets();
                }
            }
        }
        else
        {
            await DisplayAlert("Message", "Please select an asset", "OK");
        }
    }
}