using AssetManagement.Models;
using AssetManagement.Models.Constants;
using AssetManagement.Services;
using AssetManagement.ViewModels;
using CommunityToolkit.Maui.Storage;
using ExcelDataReader;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
//using Mopups.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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
    //private IPopupNavigation _popupNavigation;
    private readonly IAssetService _assetService;
    private readonly HttpClient httpClient = new();
    //public bool IsRefreshing { get; set; } = true;
    //public ObservableCollection<Assets> Assets { get; set; } = new();
    //public Command RefreshCommand { get; set; }
    //public Assets SelectedAsset { get; set; }
    //public bool PaginationEnabled { get; set; } = true;
    public AssetPage(AssetListPageViewModel viewModel, IAssetService assetService)
    {
        //RefreshCommand = new Command(async () =>
        //{
        //    // Simulate delay
        //    await Task.Delay(2000);

        //    await LoadAssets();

        //    IsRefreshing = true;
        //    OnPropertyChanged(nameof(IsRefreshing));
        //});

        //BindingContext = this;

        InitializeComponent();
        _viewModel = viewModel;
        this.BindingContext = _viewModel;
        //_popupNavigation = popupNavigation;
        _assetService = assetService;

        //tap gesture added for different labels
        var labelBank = new TapGestureRecognizer();
        labelBank.Tapped += (s, e) =>
        {
            //_popupNavigation.PushAsync(new AssetsByCategoryPage("Bank", _assetService));
            ShowBankAssetsPopup("Bank");
        };
        lblBank.GestureRecognizers.Add(labelBank);

        var labelNCD = new TapGestureRecognizer();
        labelNCD.Tapped += (s, e) =>
        {
            //_popupNavigation.PushAsync(new AssetsByCategoryPage("NCD", _assetService));
            ShowBankAssetsPopup("NCD");
        };
        lblNCD.GestureRecognizers.Add(labelNCD);

        var labelMLD = new TapGestureRecognizer();
        labelMLD.Tapped += (s, e) =>
        {
            //_popupNavigation.PushAsync(new AssetsByCategoryPage("MLD", _assetService));
            ShowBankAssetsPopup("MLD");
        };
        lblMLD.GestureRecognizers.Add(labelMLD);

        var labelInsurance_MF = new TapGestureRecognizer();
        labelInsurance_MF.Tapped += (s, e) =>
        {
            //_popupNavigation.PushAsync(new AssetsByCategoryPage("Insurance_MF", _assetService));
            ShowBankAssetsPopup("Insurance_MF");
        };
        lblInsuranceMF.GestureRecognizers.Add(labelInsurance_MF);

        var labelGold = new TapGestureRecognizer();
        labelGold.Tapped += (s, e) =>
        {
            //_popupNavigation.PushAsync(new AssetsByCategoryPage("Gold,SGB", _assetService));
            ShowBankAssetsPopup("Gold,SGB");
        };
        lblGold.GestureRecognizers.Add(labelGold);

        var labelOthers = new TapGestureRecognizer();
        labelOthers.Tapped += (s, e) =>
        {
            //_popupNavigation.PushAsync(new AssetsByCategoryPage("Others", _assetService));
            ShowBankAssetsPopup("Others");
        };
        lblOthers.GestureRecognizers.Add(labelOthers);

        var labelTaxEfficient = new TapGestureRecognizer();
        labelTaxEfficient.Tapped += (s, e) =>
        {
            //_popupNavigation.PushAsync(new AssetsByCategoryPage("EPF,PPF,NPS", _assetService));
            ShowBankAssetsPopup("EPF,PPF,NPS");
        };
        lblTaxEfficient.GestureRecognizers.Add(labelTaxEfficient);

        var labelDebtMF = new TapGestureRecognizer();
        labelDebtMF.Tapped += (s, e) =>
        {
            //_popupNavigation.PushAsync(new AssetsByCategoryPage("EPF,PPF,NPS", _assetService));
            ShowBankAssetsPopup("Debt Mutual Fund");
        };
        lblDebtMF.GestureRecognizers.Add(labelDebtMF);

        var labelEquityMF = new TapGestureRecognizer();
        labelEquityMF.Tapped += (s, e) =>
        {
            //_popupNavigation.PushAsync(new AssetsByCategoryPage("EPF,PPF,NPS", _assetService));
            ShowBankAssetsPopup("Equity Mutual Fund");
        };
        lblEquityMF.GestureRecognizers.Add(labelEquityMF);

        var labelStocks = new TapGestureRecognizer();
        labelStocks.Tapped += (s, e) =>
        {
            //_popupNavigation.PushAsync(new AssetsByCategoryPage("EPF,PPF,NPS", _assetService));
            ShowBankAssetsPopup("Stocks");
        };
        lblStocks.GestureRecognizers.Add(labelStocks);
    }

    public async Task ShowBankAssetsPopup(string labelText)
    {
        string displayText = "";
        List<EntitywiseModel> investmentEntity;
        List<Assets> records = await _assetService.GetAssetsList();
        if (labelText.Contains("Gold"))
        {
            string[] type = labelText.Split(',');
            investmentEntity = records
            .Where(w => type.Contains(w.Type))
            .GroupBy(g => g.InvestmentEntity)
            .Select(entity => new EntitywiseModel
            {
                InvestmentEntity = entity.First().InvestmentEntity,
                TotalAmount = string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", entity.Sum(s => s.Amount))
            }).ToList();
        }
        else if (labelText == "EPF,PPF,NPS")
        {
            string[] type = labelText.Split(',');
            investmentEntity = records
            .Where(w => type.Contains(w.Type))
            .GroupBy(g => g.InvestmentEntity)
            .Select(entity => new EntitywiseModel
            {
                InvestmentEntity = entity.First().InvestmentEntity,
                TotalAmount = string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", entity.Sum(s => s.Amount))
            }).ToList();
        }
        else if (labelText == "Debt Mutual Fund")
        {
            string[] type = labelText.Split(',');
            investmentEntity = records
           .Where(w => type.Contains(w.Type))
           .GroupBy(g => g.InvestmentEntity)
           .Select(entity => new EntitywiseModel
           {
               InvestmentEntity = entity.First().InvestmentEntity,
               TotalAmount = string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", entity.Sum(s => s.Amount))
           }).ToList();
        }
        else if (labelText == "Equity Mutual Fund")
        {
            string[] type = labelText.Split(',');
            investmentEntity = records
           .Where(w => type.Contains(w.Type))
           .GroupBy(g => g.InvestmentEntity)
           .Select(entity => new EntitywiseModel
           {
               InvestmentEntity = entity.First().InvestmentEntity,
               TotalAmount = string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", entity.Sum(s => s.Amount))
           }).ToList();
        }
        else if (labelText == "Stocks")
        {
            string[] type = labelText.Split(',');
            investmentEntity = records
           .Where(w => type.Contains(w.Type))
           .GroupBy(g => g.InvestmentEntity)
           .Select(entity => new EntitywiseModel
           {
               InvestmentEntity = entity.First().InvestmentEntity,
               TotalAmount = string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", entity.Sum(s => s.Amount))
           }).ToList();
        }
        else
        {
            investmentEntity = records
                .Where(w => w.Type == labelText)
                .GroupBy(g => g.InvestmentEntity)
                .Select(entity => new EntitywiseModel
                {
                    InvestmentEntity = entity.First().InvestmentEntity,
                    TotalAmount = string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", entity.Sum(s => s.Amount))
                }).ToList();
        }

        foreach (var item in investmentEntity)
        {
            displayText = displayText + item.InvestmentEntity + ": " + item.TotalAmount + "\n";
        }

        await DisplayAlert("Asset Info", displayText, "Ok");
    }

    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        //this method can be removed after some months.(by 2025 January)
        await AssetUpdatesOnLoad();
        //this method can be removed after some months.(by 2025 January)
        await LoadAssets();
    }

    private async Task LoadAssets()
    {
        try
        {
            await _viewModel.LoadAssets("");
        }
        catch
        {
            await DisplayAlert("Info", "Welcome to asset management!", "OK");
        }
    }

    private async Task AssetUpdatesOnLoad()
    {
        try
        {
            await SetUpDb();
            int MFAssets = await _dbConnection.Table<Assets>().Where(a => a.Type == "MF").CountAsync();
            if (MFAssets > 0)
            {
                var rowsAffected = await _dbConnection.ExecuteAsync("Update Assets set Type='Equity Mutual Fund' where Type='MF'");
            }
        }
        catch
        {
            return;
        }
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
            entInterestFrequency.SelectedItem = obj.InterestFrequency;
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

        LoadOwnersInDropdown();
        lblMaturingAssetsTotalValue.Text = "Total Value: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", await _viewModel.GetAssetsList());
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

    private async void LoadOwnersInDropdown()
    {
        await SetUpDb();
        var owners = await _dbConnection.Table<Owners>().ToListAsync();
        entHolder.ItemsSource = owners.Select(s => s.OwnerName).ToList();
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
                        Remarks = Remarks,
                        AsOfDate= AsOfDate
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
        string result = "Net Asset Value: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", NetAssetValue);
        lblNetAssetValue.Text = result;

        decimal BankAssets = records.Where(b => b.Type == "Bank").Sum(s => s.Amount);
        decimal NCDAssets = records.Where(b => b.Type == "NCD").Sum(s => s.Amount);
        decimal MLDAssets = records.Where(b => b.Type == "MLD").Sum(s => s.Amount);

        decimal Insurance_MF = records.Where(b => b.Type == "Insurance_MF").Sum(s => s.Amount);
        decimal Gold = records.Where(b => b.Type == "Gold" || b.Type=="SGB").Sum(s => s.Amount);
        //decimal PPF = records.Where(b => b.Type == "PPF").Sum(s => s.Amount);
        //decimal EPF = records.Where(b => b.Type == "EPF").Sum(s => s.Amount);
        decimal DebtMutualFunds = records.Where(b => b.Type == "Debt Mutual Fund").Sum(s => s.Amount);
        decimal EquityMutualFunds = records.Where(b => b.Type == "Equity Mutual Fund").Sum(s => s.Amount);
        decimal Stocks = records.Where(b => b.Type == "Stocks").Sum(s => s.Amount);
        decimal Others = records.Where(b => b.Type == "Others").Sum(s => s.Amount);
        decimal TaxEfficient = records.Where(b => b.Type == "PPF" || b.Type == "EPF" || b.Type == "NPS").Sum(s => s.Amount);

        lblBank.Text = "Bank Assets Value: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", BankAssets);
        lblNCD.Text = "Non Convertible Debentures: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", NCDAssets);
        lblMLD.Text = "Market Linked Debentures: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", MLDAssets);

        lblInsuranceMF.Text = "Insurance/MF: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", Insurance_MF);
        lblGold.Text = "Gold: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", Gold);
        lblOthers.Text = "Others: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", Others);
        //lblPPF.Text = "Public Provident Fund: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", PPF);
        //lblEPF.Text = "Employee Provident Fund: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", EPF);
        lblTaxEfficient.Text = "Tax Efficient Investments: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", TaxEfficient);
        lblDebtMF.Text = "Debt Mutual Funds: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", DebtMutualFunds);
        lblEquityMF.Text = "Equity Mutual Funds: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", EquityMutualFunds);
        lblStocks.Text = "Stocks: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", Stocks);     

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
        lblProjectedAssetValue.Text = "Projected Asset Value: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", Math.Round(projectedAmount, 2));

    }

    private async void entryDays_TextChanged(object sender, TextChangedEventArgs e)
    {
        string daysLeft = entryDays.Text;
        if (string.IsNullOrEmpty(entryDays.Text))
        {
            daysLeft = "0";
        }
        lblMaturingAssetsTotalValue.Text = "Total Value: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", await _viewModel.GetMaturingAssetsListByDaysLeft(Convert.ToInt32(daysLeft)));
    }

    private async void btnSave_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(entEntityName.Text) || string.IsNullOrEmpty(entInterestRate.Text) || string.IsNullOrEmpty(entAmount.Text))
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
                InterestFrequency = entInterestFrequency.SelectedItem == null ? "" : entInterestFrequency.SelectedItem.ToString(),
                Holder = entHolder.SelectedItem == null ? "" : entHolder.SelectedItem.ToString(),
                //StartDate = entStartDate.Date,
                //MaturityDate = entMaturityDate.Date,
                //AsOfDate = entAsOfDate.Date,
                Remarks = entRemarks.Text
            };

            if (objAsset.Type == "Insurance_MF" || objAsset.Type == "PPF" || objAsset.Type == "EPF" || objAsset.Type == "Equity Mutual Fund" || objAsset.Type == "Debt Mutual Fund" || objAsset.Type == "Stocks" || objAsset.Type == "NPS" || objAsset.Type == "Others")
            {
                objAsset.AsOfDate = entAsOfDate.Date;
                objAsset.StartDate = Convert.ToDateTime("01-01-0001");
                objAsset.MaturityDate = Convert.ToDateTime("01-01-0001");

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
                //add asset audit log
                double netAssetValue = await _dbConnection.ExecuteScalarAsync<double>("select SUM(Amount) from Assets");

                AssetAuditLog objAssetAuditLog = new AssetAuditLog
                {
                    AssetId = objAsset.AssetId,
                    InvestmentEntity = objAsset.InvestmentEntity,
                    Type = objAsset.Type,
                    Amount = objAsset.Amount,
                    InterestRate = objAsset.InterestRate,
                    InterestFrequency = objAsset.InterestFrequency,
                    Holder = objAsset.Holder,
                    StartDate = objAsset.StartDate,
                    MaturityDate = objAsset.MaturityDate,
                    Remarks = objAsset.Remarks,
                    AsOfDate = objAsset.AsOfDate,
                    LiquidAssetValue = netAssetValue,
                    NetAssetValue = netAssetValue,
                    ActionType = string.IsNullOrEmpty(lblAssetId.Text) ? "Insert" : "Update",
                    CreatedDate = DateTime.Now
                };
                SaveAssetAuditLog(objAssetAuditLog);
                //add asset audit log
                await DisplayAlert("Message", "Asset Saved", "OK");
                await LoadAssets();
            }
        }
        catch(Exception)
        {
            await DisplayAlert("Error", "Something went wrong. Please try again.", "Got It");
        }
    }

    private async void btnDelete_Clicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(lblAssetId.Text))
        {
            bool userResponse = await DisplayAlert("Warning", "Are you sure to delete?", "Yes", "No");
            if (userResponse)
            {
                int assetId = Convert.ToInt32(lblAssetId.Text);
                var assetDeleted = await _dbConnection.GetAsync<Assets>(assetId);               
                Models.Assets objAsset = new Assets()
                {
                    AssetId = assetId
                };

                await SetUpDb();
                int rowsAffected = await _dbConnection.DeleteAsync(objAsset);
                if (rowsAffected > 0)
                {
                    //add asset audit log  
                    double netAssetValue = await _dbConnection.ExecuteScalarAsync<double>("select SUM(Amount) from Assets");

                    AssetAuditLog objAssetAuditLog = new AssetAuditLog
                    {
                        AssetId = assetDeleted.AssetId,
                        InvestmentEntity = assetDeleted.InvestmentEntity,
                        Type = assetDeleted.Type,
                        Amount = assetDeleted.Amount,
                        InterestRate = assetDeleted.InterestRate,
                        InterestFrequency = assetDeleted.InterestFrequency,
                        Holder = assetDeleted.Holder,
                        StartDate = assetDeleted.StartDate,
                        MaturityDate = assetDeleted.MaturityDate,
                        Remarks = assetDeleted.Remarks,
                        AsOfDate = assetDeleted.AsOfDate,
                        LiquidAssetValue = netAssetValue,
                        NetAssetValue = netAssetValue,
                        ActionType = "Delete",
                        CreatedDate = DateTime.Now
                    };
                    SaveAssetAuditLog(objAssetAuditLog);
                    //add asset audit log
                    await DisplayAlert("Message", "Asset Deleted", "OK");
                    ClearManageAssetForm();
                    await LoadAssets();
                }
            }
        }
        else
        {
            await DisplayAlert("Message", "Please select an asset", "OK");
        }
    }

    private async void SaveAssetAuditLog(AssetAuditLog assetAuditLog)
    {
        AssetAuditLog objAssetAuditLog = new AssetAuditLog
        {
            AssetId = assetAuditLog.AssetId,
            InvestmentEntity = assetAuditLog.InvestmentEntity,
            Type = assetAuditLog.Type,
            Amount = assetAuditLog.Amount,
            InterestRate = assetAuditLog.InterestRate,
            InterestFrequency = assetAuditLog.InterestFrequency,
            Holder = assetAuditLog.Holder,
            StartDate = assetAuditLog.StartDate,
            MaturityDate = assetAuditLog.MaturityDate,
            Remarks = assetAuditLog.Remarks,
            AsOfDate = assetAuditLog.AsOfDate,
            LiquidAssetValue = assetAuditLog.NetAssetValue,
            NetAssetValue = assetAuditLog.NetAssetValue,
            ActionType = assetAuditLog.ActionType,
            CreatedDate = DateTime.Now
        };
        await _dbConnection.InsertAsync(objAssetAuditLog);
    }

    private void entType_SelectedIndexChanged(object sender, EventArgs e)
    {
        string type = entType.SelectedItem.ToString();

        if (type == "Insurance_MF" || type == "PPF" || type == "EPF" || type == "Equity Mutual Fund" || type == "Debt Mutual Fund" || type == "Stocks" || type == "NPS" || type == "Others")
        {
            entStartDate.Date = DateTime.Now;
            entMaturityDate.Date = DateTime.Now;

            entStartDate.IsEnabled = false;
            entMaturityDate.IsEnabled = false;
            entAsOfDate.IsEnabled = true;
        }
        else
        {
            entAsOfDate.Date = DateTime.Now;

            entStartDate.IsEnabled = true;
            entMaturityDate.IsEnabled = true;
            entAsOfDate.IsEnabled = false;
        }
    }

    private async void entAssetSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = entAssetSearch.Text.Trim();
        await _viewModel.LoadAssets(searchText);
        //List<Assets> objAssets = await _assetService.GetAssetsList();
        //objAssets = objAssets.Where(a => searchText.Contains(a.InvestmentEntity)).ToList();
        //await DisplayAlert("Message", entAssetSearch.Text, "OK");
    }

    //private async void btnUploadAssets_Clicked(object sender, EventArgs e)
    //{
    //    bool userResponse = await DisplayAlert("Message", "Are you sure to upload data to firestore DB?", "Yes", "No");
    //    if (!userResponse)
    //    {
    //        return;
    //    }
    //    activityIndicator.IsVisible = true;
    //    activityIndicator.IsRunning = true;
    //    var localPath = Path.Combine(FileSystem.CacheDirectory, "firestoredemo-d2bdc-firebase-adminsdk-zmue4-6f935f5ddc.json");

    //    using var json = await FileSystem.OpenAppPackageFileAsync("firestoredemo-d2bdc-firebase-adminsdk-zmue4-6f935f5ddc.json");
    //    using var dest = File.Create(localPath);
    //    await json.CopyToAsync(dest);

    //    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", localPath);
    //    dest.Close();
    //    string projectId = "firestoredemo-d2bdc";
    //    var _fireStoreDb = FirestoreDb.Create(projectId);

    //    await DeleteAllDocumentsInCollection(Constants.AssetFirestoreCollection);
    //    //deleted all records in firestore


    //    await SetUpDb();
    //    List<Assets> assets = await _dbConnection.Table<Assets>().ToListAsync();
    //    int writeFlag = 0;



    //    CollectionReference collectionReference = _fireStoreDb.Collection(Constants.AssetFirestoreCollection);

    //    foreach (var asset in assets)
    //    {
    //        AssetDetails assetDetail = new AssetDetails();
    //        assetDetail.AssetId = asset.AssetId;
    //        assetDetail.InvestmentEntity = asset.InvestmentEntity;
    //        assetDetail.Type = asset.Type;
    //        assetDetail.Amount = Convert.ToDouble(asset.Amount);
    //        assetDetail.InterestRate = Convert.ToDouble(asset.InterestRate);
    //        assetDetail.InterestFrequency = asset.InterestFrequency;
    //        assetDetail.Holder = asset.Holder;
    //        assetDetail.Remarks = asset.Remarks;
    //        assetDetail.StartDate = Convert.ToString(asset.StartDate);
    //        assetDetail.MaturityDate = Convert.ToString(asset.MaturityDate);
    //        assetDetail.AsOfDate = Convert.ToString(asset.AsOfDate);

    //        var result = await collectionReference.AddAsync(assetDetail);
    //        writeFlag++;
    //    }

    //    if (writeFlag == assets.Count)
    //    {
    //        //DataSyncAudit objSync = new DataSyncAudit
    //        //{
    //        //    Date = DateTime.Now,
    //        //    Action = "Upload"
    //        //};
    //        //await _dbConnection.InsertAsync(objSync);
    //        //lblLastUploaded.Text = "Last Uploaded: " + DateTime.Now.ToString("dd-MM-yyyy hh:mm tt");
    //        activityIndicator.IsVisible = false;
    //        activityIndicator.IsRunning = false;
    //        await DisplayAlert("Message", "Data Upload Successful", "OK");

    //    }
    //    else
    //    {
    //        activityIndicator.IsVisible = false;
    //        activityIndicator.IsRunning = false;
    //        await DisplayAlert("Error", "Something went wrong", "OK");
    //    }
    //}

    //private async void btnDownloadAssets_Clicked(object sender, EventArgs e)
    //{      
    //    await SetUpDb();
    //    var existingRecords = await _dbConnection.Table<Assets>().Take(1).ToListAsync();
    //    if (existingRecords.Count > 0)
    //    {
    //        bool userResponse = await DisplayAlert("Message", "There are existing records in the local database.Do you want to overwrite them?", "Yes", "No");
    //        if (userResponse)
    //        {
    //            int recordsDeleted = await _dbConnection.ExecuteAsync("Delete from Assets"); //delete all present records in sqlite db
    //            activityIndicator.IsVisible = true;
    //            activityIndicator.IsRunning = true;
    //            await DownloadData();
    //        }
    //        activityIndicator.IsVisible = false;
    //        activityIndicator.IsRunning = false;
    //    }
    //    else
    //    {
    //        activityIndicator.IsVisible = true;
    //        activityIndicator.IsRunning = true;
    //        await DownloadData();
    //    }
    //}

    //public async Task DeleteAllDocumentsInCollection(string collectionPath)
    //{
    //    var localPath = Path.Combine(FileSystem.CacheDirectory, "firestoredemo-d2bdc-firebase-adminsdk-zmue4-6f935f5ddc.json");

    //    using var json = await FileSystem.OpenAppPackageFileAsync("firestoredemo-d2bdc-firebase-adminsdk-zmue4-6f935f5ddc.json");
    //    using var dest = File.Create(localPath);
    //    await json.CopyToAsync(dest);

    //    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", localPath);
    //    dest.Close();
    //    string projectId = "firestoredemo-d2bdc";
    //    var _fireStoreDb = FirestoreDb.Create(projectId);

    //    CollectionReference collectionRef = _fireStoreDb.Collection(collectionPath);

    //    // Get all documents in the collection
    //    QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync();

    //    // Delete each document
    //    foreach (DocumentSnapshot document in snapshot.Documents)
    //    {
    //        await document.Reference.DeleteAsync();
    //    }
    //}

    //public async Task DownloadData()
    //{
    //    var localPath = Path.Combine(FileSystem.CacheDirectory, "firestoredemo-d2bdc-firebase-adminsdk-zmue4-6f935f5ddc.json");

    //    using var json = await FileSystem.OpenAppPackageFileAsync("firestoredemo-d2bdc-firebase-adminsdk-zmue4-6f935f5ddc.json");
    //    using var dest = File.Create(localPath);
    //    await json.CopyToAsync(dest);

    //    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", localPath);
    //    dest.Close();
    //    string projectId = "firestoredemo-d2bdc";
    //    var _fireStoreDb = FirestoreDb.Create(projectId);

    //    int rowsAffected = 0;
    //    try
    //    {
    //        Query orderQuery = _fireStoreDb.Collection(Constants.AssetFirestoreCollection);
    //        QuerySnapshot orderQuerySnapshot = await orderQuery.GetSnapshotAsync();
    //        List<AssetDetails> assetObj = new List<AssetDetails>();

    //        foreach (DocumentSnapshot documentSnapshot in orderQuerySnapshot.Documents)
    //        {
    //            if (documentSnapshot.Exists)
    //            {
    //                Dictionary<string, object> dictionary = documentSnapshot.ToDictionary();
    //                string jsonObj = JsonConvert.SerializeObject(dictionary);
    //                AssetDetails newAssetObj = JsonConvert.DeserializeObject<AssetDetails>(jsonObj);
    //                assetObj.Add(newAssetObj);
    //            }
    //        }

    //        foreach (var item in assetObj)
    //        {
    //            Assets model = new Assets();
    //            model.InvestmentEntity = item.InvestmentEntity;
    //            model.Type = item.Type;
    //            model.Amount = Convert.ToDecimal(item.Amount);
    //            model.InterestRate = Convert.ToDecimal(item.InterestRate);
    //            model.InterestFrequency = item.InterestFrequency;
    //            model.Holder = item.Holder;
    //            model.Remarks = item.Remarks;
    //            if (DateTime.TryParse(item.StartDate, out DateTime startDate))
    //            {
    //                model.StartDate = Convert.ToDateTime(item.StartDate);
    //            }
    //            else
    //            {
    //                model.StartDate = DateTime.Now;
    //            }

    //            if (DateTime.TryParse(item.MaturityDate, out DateTime maturityDate))
    //            {
    //                model.MaturityDate = Convert.ToDateTime(item.MaturityDate);
    //            }
    //            else
    //            {
    //                model.MaturityDate = DateTime.Now;
    //            }

    //            if (DateTime.TryParse(item.AsOfDate, out DateTime asOfDate))
    //            {
    //                model.AsOfDate = Convert.ToDateTime(item.AsOfDate);
    //            }
    //            else
    //            {
    //                model.AsOfDate = DateTime.Now;
    //            }

    //            rowsAffected = rowsAffected + await _dbConnection.InsertAsync(model);
    //        }


    //        if (rowsAffected == assetObj.Count)
    //        {
    //            activityIndicator.IsVisible = false;
    //            activityIndicator.IsRunning = false;
    //            await DisplayAlert("Message", "Success", "OK");
    //        }
    //        else
    //        {
    //            activityIndicator.IsVisible = false;
    //            activityIndicator.IsRunning = false;
    //            await DisplayAlert("Error", "Something went wrong", "OK");
    //        }
    //    }
    //    catch (Exception) { throw; }
    //}

    private async void btnDownloadAssetsExcel_Clicked(object sender, EventArgs e)
    {
        try
        {
            activityIndicator.IsVisible = true;
            activityIndicator.IsRunning = true;
            var assetList = await _dbConnection.Table<Assets>()
                .OrderBy(i => i.Type)
                .ToListAsync();

            // Creating an instance
            // of ExcelPackage
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage excel = new ExcelPackage();

            // name of the sheet
            var workSheet = excel.Workbook.Worksheets.Add("Asset Report");

            // setting the properties
            // of the work sheet 
            workSheet.TabColor = System.Drawing.Color.Black;
            workSheet.DefaultRowHeight = 12;

            // Setting the properties
            // of the first row
            workSheet.Row(1).Height = 20;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;

            workSheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(9).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(10).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


            // Header of the Excel sheet
            workSheet.Cells[1, 1].Value = "Investment Entity";
            workSheet.Cells[1, 2].Value = "Type";
            workSheet.Cells[1, 3].Value = "Amount";
            workSheet.Cells[1, 4].Value = "Interest Rate";
            workSheet.Cells[1, 5].Value = "Interest Frequency";
            workSheet.Cells[1, 6].Value = "Holder";
            workSheet.Cells[1, 7].Value = "Start Date";
            workSheet.Cells[1, 8].Value = "Maturity Date";
            workSheet.Cells[1, 9].Value = "As Of Date";
            workSheet.Cells[1, 10].Value = "Remarks";

            int recordIndex = 2;
            //decimal totalIncome = 0, totalTaxCut = 0;
            foreach (var asset in assetList)
            {
                workSheet.Cells[recordIndex, 1].Value = asset.InvestmentEntity;
                workSheet.Cells[recordIndex, 2].Value = asset.Type;
                workSheet.Cells[recordIndex, 3].Value = asset.Amount;
                workSheet.Cells[recordIndex, 4].Value = asset.InterestRate;
                workSheet.Cells[recordIndex, 5].Value = asset.InterestFrequency;
                workSheet.Cells[recordIndex, 6].Value = asset.Holder;
                workSheet.Cells[recordIndex, 7].Value = asset.StartDate.ToString("dd-MM-yyyy");
                workSheet.Cells[recordIndex, 8].Value = asset.MaturityDate.ToString("dd-MM-yyyy");
                workSheet.Cells[recordIndex, 9].Value = asset.AsOfDate.ToString("dd-MM-yyyy");
                workSheet.Cells[recordIndex, 10].Value = asset.Remarks;
                workSheet.Row(recordIndex).Height = 16;
                recordIndex++;
            }

            //recordIndex++;
            //workSheet.Cells[recordIndex, 5].Value = totalTaxCut;
            //workSheet.Cells[recordIndex, 5].Style.Font.Bold = true;
            //workSheet.Cells[recordIndex, 6].Value = totalIncome;
            //workSheet.Cells[recordIndex, 6].Style.Font.Bold = true;


            workSheet.Column(1).AutoFit();
            workSheet.Column(2).AutoFit();
            workSheet.Column(3).AutoFit();
            workSheet.Column(4).AutoFit();
            workSheet.Column(5).AutoFit();
            workSheet.Column(6).AutoFit();
            workSheet.Column(7).AutoFit();
            workSheet.Column(8).AutoFit();
            workSheet.Column(9).AutoFit();
            workSheet.Column(10).AutoFit();

            var stream = new MemoryStream(excel.GetAsByteArray());
            CancellationTokenSource Ctoken = new CancellationTokenSource();
            string fileName = "Asset_Report_" + DateTime.Now.ToString("dd-MM-yyyy hh:mm tt")+".xlsx";
            var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream, Ctoken.Token);
            if (fileSaverResult.IsSuccessful)
            {
                await DisplayAlert("Message", "Excel saved in " + fileSaverResult.FilePath, "Ok");
            }

            excel.Dispose();
            activityIndicator.IsVisible = false;
            activityIndicator.IsRunning = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Ok");
            await DisplayAlert("Error", ex.InnerException.ToString(), "Ok");
        }
    }

    private async void dgAssetsDataTable_ItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            Assets obj = _viewModel.GetSelectedRecordDetail();

            if (obj.AssetId != 0)
            {
                entEntityName.Text = obj.InvestmentEntity;
                entType.SelectedItem = obj.Type;
                entAmount.Text = Convert.ToString(obj.Amount);
                entInterestRate.Text = Convert.ToString(obj.InterestRate);
                entInterestFrequency.SelectedItem = obj.InterestFrequency;
                entHolder.SelectedItem = obj.Holder;
                entStartDate.Date = obj.StartDate;
                entMaturityDate.Date = obj.MaturityDate;
                entAsOfDate.Date = obj.AsOfDate;
                entRemarks.Text = obj.Remarks;

                lblAssetId.Text = Convert.ToString(obj.AssetId);

                //ScrollView scroll = new ScrollView();
                await manageAssetsScroll.ScrollToAsync(0, 0, true);
            }
            else
            {
                await DisplayAlert("Message", "Please select an asset", "OK");
            }
        }
    }

    private void btnClear_Clicked(object sender, EventArgs e)
    {
        ClearManageAssetForm();
    }

    public void ClearManageAssetForm()
    {
        try
        {
            manageAssetsScroll.ScrollToAsync(0, 0, true);
            lblAssetId.Text = "";
            entEntityName.Text = "";
            //if (entType != null)
            //{
            //    entType.ItemsSource.Clear();
            //    entType.SelectedIndex = -1;
            //}
            //entType.SelectedIndex = -1;
            entAmount.Text = "";
            entInterestRate.Text = "";
            //entInterestFrequency.SelectedIndex = -1;
            //entHolder.SelectedIndex = -1;
            entStartDate.Date = DateTime.Now;
            entMaturityDate.Date = DateTime.Now;
            entAsOfDate.Date = DateTime.Now;
            entRemarks.Text = "";
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
        }
    }

    private void expanderAdditionalDetails_ExpandedChanged(object sender, CommunityToolkit.Maui.Core.ExpandedChangedEventArgs e)
    {
        if (e.IsExpanded)
        {
            manageAssetsScroll.ScrollToAsync(expanderAdditionalDetails, ScrollToPosition.Start, true);
        }
    }
}