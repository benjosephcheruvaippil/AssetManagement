using AssetManagement.Models;
using AssetManagement.Models.Api.Response;
using AssetManagement.Models.Constants;
using AssetManagement.Models.DataTransferObject;
using AssetManagement.Services;
using AssetManagement.ViewModels;
using CommunityToolkit.Maui.Storage;
using ExcelDataReader;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

//using Mopups.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Org.Apache.Http.Conn;
using SkiaSharp;
using SQLite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetManagement.Views;

public partial class AssetPage : TabbedPage
{
    private AssetListPageViewModel _viewModel;
    private SQLiteAsyncConnection _dbConnection;
    //private IPopupNavigation _popupNavigation;
    private readonly IAssetService _assetService;
    private readonly HttpClient httpClient = new();

    //private Image fullScreenImage;
    //private double currentScale = 1, startScale = 1;
    //private AbsoluteLayout fullScreenLayout;

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
        await LoadAssets("");
    }

    private async Task LoadAssets(string searchText)
    {
        try
        {
            await _viewModel.LoadAssets(searchText);
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

    //private async void btnGetDetail_Clicked(object sender, EventArgs e)
    //{
    //    Assets obj = _viewModel.GetSelectedRecordDetail();

    //    if (obj.AssetId != 0)
    //    {

    //        entEntityName.Text = obj.InvestmentEntity;
    //        entType.SelectedItem = obj.Type;
    //        entAmount.Text = Convert.ToString(obj.Amount);
    //        entInterestRate.Text = Convert.ToString(obj.InterestRate);
    //        entInterestFrequency.SelectedItem = obj.InterestFrequency;
    //        entHolder.SelectedItem = obj.Holder;
    //        entStartDate.Date = obj.StartDate;
    //        entMaturityDate.Date = obj.MaturityDate;
    //        entAsOfDate.Date = obj.AsOfDate;
    //        entRemarks.Text = obj.Remarks;

    //        lblAssetId.Text = Convert.ToString(obj.AssetId);
    //    }
    //    else
    //    {
    //        await DisplayAlert("Message", "Please select an asset", "OK");
    //    }
    //    //btnSave.IsVisible = true;
    //    //btnDelete.IsVisible = true;
    //}

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        ShowOrHideComponents();
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

    public void ShowOrHideComponents()
    {
        string deviceInfo = DeviceInfo.Current.Manufacturer + "-" + DeviceInfo.Current.Idiom.ToString() + "-" + DeviceInfo.Current.Model;
        if (deviceInfo == Constants.DeviceNumber || Constants.DeviceNumber == "")
        {
            btnUploadAssetExcelToGoogleDrive.IsVisible = true;
            imageContainer.IsVisible = true;
        }
    }

    private async void LoadOwnersInDropdown()
    {
        await SetUpDb();
        var owners = await _dbConnection.Table<Owners>().ToListAsync();
        entHolder.ItemsSource = owners.Select(s => s.OwnerName).ToList();
        entNominee.ItemsSource = owners.Select(s => s.OwnerName).ToList();
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
        List<Assets> recordsExceptProperty = records.Where(a => a.Type != Constants.AssetTypeProperty).ToList();
        decimal NetAssetValue = recordsExceptProperty.Sum(s => s.Amount);
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
        if (NCDAssets == 0)
        {
            lblNCD.IsVisible = false;
        }
        else
        {
            lblNCD.Text = "Non Convertible Debentures: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", NCDAssets);
        }
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
        foreach (var item in recordsExceptProperty)
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
            string holder = entHolder.SelectedItem == null ? "" : entHolder.SelectedItem.ToString();
            if (string.IsNullOrEmpty(entEntityName.Text) || entType.SelectedItem == null || string.IsNullOrEmpty(entAmount.Text) || string.IsNullOrEmpty(holder))
            {
                await DisplayAlert("Message", "Please input all required fields", "OK");
                return;
            }

            if (string.IsNullOrEmpty(entInterestRate.Text))
            {
                entInterestRate.Text = "0";
            }

            int rowsAffected = 0;
            Models.Assets objAsset = new Assets()
            {
                InvestmentEntity = entEntityName.Text,
                Type = entType.SelectedItem.ToString(),
                Amount = Convert.ToDecimal(entAmount.Text),
                InterestRate = Convert.ToDecimal(entInterestRate.Text),
                InterestFrequency = entInterestFrequency.SelectedItem == null ? "" : entInterestFrequency.SelectedItem.ToString(),
                Holder = holder,
                //StartDate = entStartDate.Date,
                //MaturityDate = entMaturityDate.Date,
                //AsOfDate = entAsOfDate.Date,
                Remarks = entRemarks.Text,
                Nominee = entNominee.SelectedItem == null ? "" : entNominee.SelectedItem.ToString(),
                RiskNumber = !string.IsNullOrEmpty(entRiskValue.Text) ? Convert.ToDecimal(entRiskValue.Text) : null
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
                    Nominee = objAsset.Nominee,
                    RiskNumber = objAsset.RiskNumber,
                    AsOfDate = objAsset.AsOfDate,
                    LiquidAssetValue = netAssetValue,
                    NetAssetValue = netAssetValue,
                    ActionType = string.IsNullOrEmpty(lblAssetId.Text) ? "Insert" : "Update",
                    CreatedDate = DateTime.Now
                };
                SaveAssetAuditLog(objAssetAuditLog);
                //add asset audit log
                await DisplayAlert("Message", "Asset Saved", "OK");
                if (!string.IsNullOrEmpty(entAssetSearch.Text))
                {
                    await LoadAssets(entAssetSearch.Text.Trim());
                }
                else
                {
                    await LoadAssets("");
                }
            }
        }
        catch(Exception)
        {
            await DisplayAlert("Error", "Something went wrong. Please try again.", "Got It");
        }
    }

    private async void btnDelete_Clicked(object sender, EventArgs e)
    {
        try
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
                            RiskNumber = assetDeleted.RiskNumber,
                            Nominee = assetDeleted.Nominee,
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
                        await LoadAssets(string.IsNullOrEmpty(entAssetSearch.Text) ? "" : entAssetSearch.Text.Trim());
                    }
                }
            }
            else
            {
                await DisplayAlert("Message", "Please select an asset", "OK");
            }
        }
        catch(Exception ex)
        {
            return;
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
            Nominee = assetAuditLog.Nominee,
            RiskNumber = assetAuditLog.RiskNumber,
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

        if (type == "Insurance_MF" || type == "PPF" || type == "EPF" || type == "Equity Mutual Fund" || type == "Debt Mutual Fund" || type == "Stocks" || type == "NPS" || type == "Others" || type == Constants.AssetTypeProperty)
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
        activityIndicator.IsVisible = true;
        activityIndicator.IsRunning = true;
        await DownloadAssetDetailsExcel("download");
        activityIndicator.IsVisible = false;
        activityIndicator.IsRunning = false;
    }

    public async Task<DownloadExcelDTO> DownloadAssetDetailsExcel(string type)
    {
        DownloadExcelDTO objResponse = new DownloadExcelDTO();
        try
        {
            var assetList = await _dbConnection.Table<Assets>()
                .OrderBy(i => i.Type)
                .ToListAsync();

            AssetDTO objAssetDTO = new AssetDTO();
            List<AssetDTO> objAssetDTOList = new List<AssetDTO>();

            foreach (var asset in assetList)
            {
                objAssetDTO = new AssetDTO
                {
                    InvestmentEntity = asset.InvestmentEntity,
                    Type = asset.Type,
                    Amount = asset.Amount,
                    InterestRate = asset.InterestRate,
                    InterestFrequency = asset.InterestFrequency,
                    Holder = asset.Holder,
                    StartDate = asset.StartDate,
                    MaturityDate = asset.MaturityDate,
                    AsOfDate = asset.AsOfDate,
                    Nominee = asset.Nominee,
                    RiskNumber = asset.RiskNumber,
                    Remarks = asset.Remarks,
                    AssetDocumentsList = await _dbConnection.Table<AssetDocuments>().Where(d => d.AssetId == asset.AssetId).ToListAsync()
                };
                objAssetDTOList.Add(objAssetDTO);
            }

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
            workSheet.Cells[1, 10].Value = "Nominee";
            workSheet.Cells[1, 11].Value = "Supporting Documents(Link)";
            workSheet.Cells[1, 12].Value = "Remarks";

            int recordIndex = 2;
            //decimal totalIncome = 0, totalTaxCut = 0;
            foreach (var asset in objAssetDTOList)
            {
                workSheet.Cells[recordIndex, 1].Value = asset.InvestmentEntity;
                workSheet.Cells[recordIndex, 2].Value = asset.Type;
                workSheet.Cells[recordIndex, 3].Value = asset.Amount;
                workSheet.Cells[recordIndex, 4].Value = asset.InterestRate;
                workSheet.Cells[recordIndex, 5].Value = asset.InterestFrequency;
                workSheet.Cells[recordIndex, 6].Value = asset.Holder;
                workSheet.Cells[recordIndex, 7].Value = asset.StartDate?.ToString("dd-MM-yyyy");
                workSheet.Cells[recordIndex, 8].Value = asset.MaturityDate?.ToString("dd-MM-yyyy");
                workSheet.Cells[recordIndex, 9].Value = asset.AsOfDate?.ToString("dd-MM-yyyy");
                workSheet.Cells[recordIndex, 10].Value = asset.Nominee;
                workSheet.Cells[recordIndex, 11].Value = string.Join("\n", asset.AssetDocumentsList.Select(s => s.FilePath));
                workSheet.Cells[recordIndex, 12].Value = asset.Remarks;
                workSheet.Row(recordIndex).CustomHeight = false;
                //workSheet.Row(recordIndex).Height = 16;
                recordIndex++;
            }

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
            workSheet.Column(11).AutoFit();
            workSheet.Column(11).Width = Math.Min(workSheet.Column(11).Width, 100);
            workSheet.Column(12).AutoFit();     
            workSheet.Column(12).Width = Math.Min(workSheet.Column(12).Width, 120);

            workSheet.Cells[workSheet.Dimension.Address].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[workSheet.Dimension.Address].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


            byte[] excelByteArray = excel.GetAsByteArray();
            string fileName = "Asset_Report_" + DateTime.Now.ToString("dd-MM-yyyy hh:mm tt") + ".xlsx";
            if (type == "download")
            {
                var stream = new MemoryStream(excelByteArray);
                CancellationTokenSource Ctoken = new CancellationTokenSource();
                var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream, Ctoken.Token);
                if (fileSaverResult.IsSuccessful)
                {
                    await DisplayAlert("Message", "Excel saved in " + fileSaverResult.FilePath, "Ok");
                }
            }
            excel.Dispose();

            objResponse.ExcelByteArray = excelByteArray;
            objResponse.FileName = fileName;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Ok");
            await DisplayAlert("Error", ex.InnerException.ToString(), "Ok");
        }
        return objResponse;
    }

    public async Task<bool> UploadExcelToGoogleDrive(byte[] fileBytes, string fileName)
    {
        string apiUrl = "https://networthtrackerapi20240213185304.azurewebsites.net/api/general/uploadExcelFileToGoogleDrive";
        using (HttpClient client = new HttpClient())
        using(MultipartFormDataContent formData=new MultipartFormDataContent())
        {
            ByteArrayContent fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            formData.Add(fileContent, "fileRequest", fileName);
            HttpResponseMessage response = await client.PostAsync(apiUrl, formData);
            string result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                //upload to google drive success.
                return true;
            }
            return false;
        }
    }

    public async Task<UploadFileResponse> UploadImageToGoogleDrive(string filePath)
    {
        UploadFileResponse uploadResponse = new UploadFileResponse { IsSuccess = false };
        string apiUrl = "https://networthtrackerapi20240213185304.azurewebsites.net/api/general/uploadImageFileToGoogleDrive";

        byte[] originalBytes = await File.ReadAllBytesAsync(filePath);

        using var inputStream = new SKManagedStream(new MemoryStream(originalBytes));
        using var original = SKBitmap.Decode(inputStream);

        if (original == null)
        {
            Console.WriteLine("Image decoding failed. Possibly unsupported format.");
            return uploadResponse;
        }

        // Resize if needed
        int targetWidth = original.Width;
        int targetHeight = original.Height;
        if (original.Width > 1024)
        {
            targetWidth = 1024;
            targetHeight = (int)(original.Height * (1024.0 / original.Width));
        }

        using var resized = original.Resize(new SKImageInfo(targetWidth, targetHeight), SKFilterQuality.Medium);
        if (resized == null)
        {
            Console.WriteLine("Image resizing failed.");
            return uploadResponse;
        }

        using var image = SKImage.FromBitmap(resized);
        using var ms = new MemoryStream();
        image.Encode(SKEncodedImageFormat.Jpeg, 75).SaveTo(ms); // Always output JPEG
        ms.Position = 0;

        // Build file name with .jpg extension regardless of input format
        string fileName = Path.GetFileNameWithoutExtension(filePath) + ".jpg";

        using var client = new HttpClient();
        using var formData = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(ms.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        formData.Add(fileContent, "fileRequest", fileName);

        HttpResponseMessage response = await client.PostAsync(apiUrl, formData);
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            uploadResponse = JsonSerializer.Deserialize<UploadFileResponse>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            uploadResponse.IsSuccess = true;
        }

        return uploadResponse;
    }

    //public async Task<UploadFileResponse> UploadImageToGoogleDrive(string filePath)
    //{
    //    UploadFileResponse uploadResponse = new UploadFileResponse();
    //    uploadResponse.IsSuccess = false;

    //    string apiUrl = "https://networthtrackerapi20240213185304.azurewebsites.net/api/general/uploadImageFileToGoogleDrive";
    //    using (HttpClient client = new HttpClient())
    //    using (MultipartFormDataContent formData = new MultipartFormDataContent())
    //    {
    //        byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
    //        string fileName = Path.GetFileName(filePath);
    //        string contentType = GetMimeType(filePath);

    //        ByteArrayContent fileContent = new ByteArrayContent(fileBytes);
    //        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
    //        formData.Add(fileContent, "fileRequest", fileName);
    //        HttpResponseMessage response = await client.PostAsync(apiUrl, formData);
    //        string result = await response.Content.ReadAsStringAsync();
    //        if (response.IsSuccessStatusCode)
    //        {
    //            //upload to google drive success.
    //            uploadResponse = JsonSerializer.Deserialize<UploadFileResponse>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    //            uploadResponse.IsSuccess = true;
    //            return uploadResponse;
    //        }
    //        return uploadResponse;
    //    }
    //}

    private string GetMimeType(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream" // Default fallback
        };
    }

    private async void dgAssetsDataTable_ItemSelected(object sender, SelectionChangedEventArgs e)
    {        
        if (e.CurrentSelection.Count > 0)
        {
            AssetDTO obj = await _viewModel.GetSelectedRecordDetail("tab2", null);

            await SetDataInManageAssets(obj);
            //if (obj.AssetId != 0)
            //{
            //    entEntityName.Text = obj.InvestmentEntity;
            //    entType.SelectedItem = obj.Type;
            //    entAmount.Text = Convert.ToString(obj.Amount);
            //    entInterestRate.Text = Convert.ToString(obj.InterestRate);
            //    entInterestFrequency.SelectedItem = obj.InterestFrequency;
            //    entHolder.SelectedItem = obj.Holder;
            //    entStartDate.Date = obj.StartDate;
            //    entMaturityDate.Date = obj.MaturityDate;
            //    entAsOfDate.Date = obj.AsOfDate;
            //    entRemarks.Text = obj.Remarks;
            //    entNominee.SelectedItem = obj.Nominee;
            //    entRiskValue.Text = Convert.ToString(obj.RiskNumber);

            //    lblAssetId.Text = Convert.ToString(obj.AssetId);

            //    //ScrollView scroll = new ScrollView();
            //    await manageAssetsScroll.ScrollToAsync(0, 0, true);
            //}
            //else
            //{
            //    await DisplayAlert("Message", "Please select an asset", "OK");
            //}
        }
    }

    public async Task SetDataInManageAssets(AssetDTO objAsset)
    {
        if (objAsset.AssetId != 0)
        {
            btnUploadImages.IsVisible = true;
            btnCaptureImage.IsVisible = true;

            entEntityName.Text = objAsset.InvestmentEntity;
            entType.SelectedItem = objAsset.Type;
            entAmount.Text = Convert.ToString(objAsset.Amount);
            entInterestRate.Text = Convert.ToString(objAsset.InterestRate);
            entInterestFrequency.SelectedItem = objAsset.InterestFrequency;
            entHolder.SelectedItem = objAsset.Holder;
            entStartDate.Date = (DateTime)objAsset.StartDate;
            entMaturityDate.Date = (DateTime)objAsset.MaturityDate;
            entAsOfDate.Date = (DateTime)objAsset.AsOfDate;
            entRemarks.Text = objAsset.Remarks;
            entNominee.SelectedItem = objAsset.Nominee;
            entRiskValue.Text = Convert.ToString(objAsset.RiskNumber);

            lblAssetId.Text = Convert.ToString(objAsset.AssetId);

            
            List<FileList> imageUrlList = new List<FileList>();
            if (objAsset.AssetDocumentsList.Count > 0)
            {
                foreach(var item in objAsset.AssetDocumentsList)
                {
                    FileList imageUrls = new FileList
                    {
                        FileId = item.FileId,
                        FilePath = item.FilePath
                    };

                    imageUrlList.Add(imageUrls);
                }                
            }

            await LoadImages(imageUrlList);

            await manageAssetsScroll.ScrollToAsync(0, 0, true);
        }
        else
        {
            await DisplayAlert("Message", "Please select an asset", "OK");
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
            entRiskValue.Text = "";

            btnUploadImages.IsVisible = false;
            btnCaptureImage.IsVisible = false;
            imageStack.Children.Clear();
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

    private async void OnEditTapped(object sender, EventArgs e)
    {
        if (sender is Element element && element.BindingContext is MaturingAssets asset)
        {
            int assetId = (int)asset.AssetId; // Get the ID from the BindingContext
            var assetDTO = await _viewModel.GetSelectedRecordDetail("tab1", assetId);
            this.CurrentPage = this.Children[1];
            await SetDataInManageAssets(assetDTO);
        }
    }

    private async void btnUploadAssetExcelToGoogleDrive_Clicked(object sender, EventArgs e)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await DisplayAlert("Message", "Please connect to internet for file upload.", "Ok");
            return;
        }

        activityIndicator.IsVisible = true;
        activityIndicator.IsRunning = true;
        var excelResponse = await DownloadAssetDetailsExcel("upload");
        if (await UploadExcelToGoogleDrive(excelResponse.ExcelByteArray, excelResponse.FileName))
        {
            activityIndicator.IsVisible = false;
            activityIndicator.IsRunning = false;
            await DisplayAlert("Info", "File uploaded to Google Drive.", "Ok");
        }
        else
        {
            activityIndicator.IsVisible = false;
            activityIndicator.IsRunning = false;
            await DisplayAlert("Error", "Failed to upload to Google Drive.", "Ok");
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is Image image)
        {
            //string imageUrl = image.Source.ToString();
            //await Navigation.PushModalAsync(new FullScreenImagePage(imageUrl));
            if (image.Source is UriImageSource uriImageSource)
            {
                string imageUrl = uriImageSource.Uri.ToString();
                await Navigation.PushModalAsync(new FullScreenImagePage(imageUrl));
            }
            else if (image.Source is FileImageSource fileImageSource)
            {
                string imageUrl = fileImageSource.File;
                await Navigation.PushModalAsync(new FullScreenImagePage(imageUrl));
            }
            else
            {
                await DisplayAlert("Error", "Image source not recognized.", "OK");
            }
        }
    }

    private async void SelectImages_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(lblAssetId.Text))
            {
                await DisplayAlert("Message", "Please select an asset to add images.", "Ok");
                return;
            }

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Message", "Please connect to internet for file upload.", "Ok");
                return;
            }

            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select an Image"
            });

            if (result != null)
            {
                //string filePath = result.FullPath; // Get file path
                await SaveImage(result.FullPath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to pick image: {ex.Message}", "OK");
        }
    }

    public async Task SaveImage(string filePath)
    {
        var uploadToGoogleDriveResult = await UploadImageToGoogleDrive(filePath);

        if ((bool)uploadToGoogleDriveResult.IsSuccess)
        {
            int assetId = Convert.ToInt32(lblAssetId.Text);

            AssetDocuments docs = new AssetDocuments();
            docs.AssetId = assetId;
            docs.FileId = uploadToGoogleDriveResult.FileId;
            docs.FilePath = $"https://drive.google.com/uc?export=view&id={uploadToGoogleDriveResult.FileId}";
            await _dbConnection.InsertAsync(docs);

            var getFileList = await _dbConnection.Table<AssetDocuments>().Where(d => d.AssetId == assetId).ToListAsync();
            List<FileList> imageUrlList = new List<FileList>();
            if (getFileList.Count > 0)
            {
                foreach (var item in getFileList)
                {
                    FileList imageUrls = new FileList
                    {
                        FileId = item.FileId,
                        FilePath = item.FilePath
                    };

                    imageUrlList.Add(imageUrls);
                }
            }

            await LoadImages(imageUrlList);

            await DisplayAlert("Success", $"File uploaded to google drive.", "Ok");
        }
        else
        {
            await DisplayAlert("Failed", $"File upload failed.", "Ok");
        }
    }

    private async Task LoadImages(List<FileList> imageUrls)
    {
        imageStack.Children.Clear();

        foreach (var imageUrl in imageUrls)
        {
            var grid = new Grid
            {
                BindingContext = imageUrl.FileId
            };

            // Image
            //var image = new Image
            //{
            //    Source = ImageSource.FromFile(imageUrl), // Load from file path
            //    HeightRequest = 100,
            //    WidthRequest = 100,
            //    Aspect = Aspect.AspectFill
            //};

            var image = new Image
            {
                Source = ImageSource.FromUri(new Uri(imageUrl.FilePath)), // Load from file path
                HeightRequest = 100,
                WidthRequest = 100,
                Aspect = Aspect.AspectFill
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => OpenFullScreenImage(imageUrl.FilePath);
            image.GestureRecognizers.Add(tapGesture);

            // "X" Button (Label)
            var deleteLabel = new Label
            {
                Text = "X",
                TextColor = Colors.Red,
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(5),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };

            var tapDelete = new TapGestureRecognizer();
            tapDelete.Tapped += async (s, e) => await RemoveImage(imageUrl.FileId, imageUrls);
            deleteLabel.GestureRecognizers.Add(tapDelete);

            // Arrange Image & "X" Button inside Grid
            grid.Children.Add(image);
            grid.Children.Add(deleteLabel);
            Grid.SetRow(deleteLabel, 0);
            Grid.SetColumn(deleteLabel, 1);
            deleteLabel.HorizontalOptions = LayoutOptions.End;
            deleteLabel.VerticalOptions = LayoutOptions.Start;

            imageStack.Children.Add(grid);
        }
    }


    private async Task RemoveImage(string fileId, List<FileList> imageUrls)
    {
        bool isConfirmed = await DisplayAlert(
       "Confirm Delete",
       "Are you sure you want to delete this image?",
       "Yes",
       "No"
        );

        if (!isConfirmed)
            return;

        var itemToRemove = imageUrls.FirstOrDefault(img => img.FileId == fileId);

        if (itemToRemove != null)
        {
            var recordToBeDeleted = await _dbConnection.Table<AssetDocuments>().Where(d => d.FileId == fileId).FirstOrDefaultAsync();        
            AssetDocuments objDocs = new AssetDocuments
            {
                AssetDocumentsId = recordToBeDeleted.AssetDocumentsId
            };
            await _dbConnection.DeleteAsync(objDocs);

            imageUrls.Remove(itemToRemove); // Remove the matching image
            await LoadImages(imageUrls); // Refresh UI

            string apiUrl = "https://networthtrackerapi20240213185304.azurewebsites.net/api/general/deleteFileFromGoogleDrive?fileId=" + fileId;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.DeleteAsync(apiUrl);
                //string result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    //delete from google drive success.
                }
            }
        }
    }

    private async void OpenFullScreenImage(string imageUrl)
    {
        await Navigation.PushModalAsync(new FullScreenImagePage(imageUrl));
    }

    private async void btnCaptureImage_Clicked(object sender, EventArgs e)
    {
        FileResult photo;
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                photo = await MediaPicker.CapturePhotoAsync();
                //await LoadPhotoAsync(photo);

                if (photo == null)
                    return;

                // Save into local cache
                var filePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
                using (var stream = await photo.OpenReadAsync())
                using (var newStream = File.OpenWrite(filePath))
                {
                    await stream.CopyToAsync(newStream);
                }

                await SaveImage(filePath);
            }
            else
            {
                await DisplayAlert("Not Supported", "Camera capture is not supported on this device", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Capturing photo failed: {ex.Message}", "OK");
        }
    }

    private async void NetAssetValueIButton_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Info", $"Net Asset Value does not include Property/Real Estate investments.", "OK");
    }
}