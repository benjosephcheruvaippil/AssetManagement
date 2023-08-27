using AssetManagement.Models;
using AssetManagement.Models.Constants;
using AssetManagement.Models.FirestoreModel;
using AssetManagement.Services;
using AssetManagement.ViewModels;
using ExcelDataReader;
using Google.Cloud.Firestore;
using Microsoft.Maui.Graphics;
using Mopups.Interfaces;
using Newtonsoft.Json;
using SQLite;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace AssetManagement.Views;

public partial class AssetListPage : TabbedPage
{
    private AssetListPageViewModel _viewModel;
    private SQLiteAsyncConnection _dbConnection;
    private IPopupNavigation _popupNavigation;
    private readonly IAssetService _assetService;
    public AssetListPage(AssetListPageViewModel viewModel, IPopupNavigation popupNavigation, IAssetService assetService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        this.BindingContext = _viewModel;
        _popupNavigation = popupNavigation;
        _assetService = assetService;

        CurrentPageChanged += AssetListPage_CurrentPageChanged;

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

    private async void AssetListPage_CurrentPageChanged(object sender, EventArgs e)
    {
        var selectedTab = CurrentPage;

        // You can now perform any actions based on the selected tab
        if (selectedTab.Title == "Expense")
        {
            //LoadExpensesInPage();// show expenses in the expense tab
            //await ShowCurrentMonthExpenses();
            SetLastUploadedDate();
        }
        else if (selectedTab.Title == "Income")
        {
            //await ShowCurrentMonthIncome();
            //LoadIncomeInPage();
        }
        else if (selectedTab.Title == "Asset Details")
        {
            //base.OnAppearing();
            _viewModel.GetAssetsList();
            await ShowPrimaryAssetDetails();
        }
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        LoadExpensesInPage();// show expenses in the expense tab
        await ShowCurrentMonthExpenses();

        LoadIncomeInPage();
        await ShowCurrentMonthIncome();

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
            await _dbConnection.CreateTableAsync<IncomeExpenseModel>();
            await _dbConnection.CreateTableAsync<DataSyncAudit>();
        }
    }

    public async void SetLastUploadedDate()
    {
        await SetUpDb();
        var lastUploadedDate = await _dbConnection.Table<DataSyncAudit>().OrderByDescending(d => d.Date).Take(1).ToListAsync();
        if (lastUploadedDate.Count > 0)
        {
            lblLastUploaded.Text = "Last Uploaded: " + lastUploadedDate[0].Date.ToString("dd-MM-yyyy hh:mm tt");
        }
    }

    public async Task ShowCurrentMonthExpenses()
    {
        DateTime currentDate = DateTime.Now;
        string currentMonth = currentDate.ToString("MMMM");
        DateTime startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
        DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        await SetUpDb();
        var query = await _dbConnection.Table<IncomeExpenseModel>().Where(d => d.TransactionType == "Expense" && d.Date >= startOfMonth && d.Date <= endOfMonth).ToListAsync();
        var totalExpense = query.Sum(s => s.Amount);
        lblCurrentMonthExpenses.Text = currentMonth + ": " + string.Format(new CultureInfo("en-IN"), "{0:C0}", totalExpense);
    }

    public async Task ShowCurrentMonthIncome()
    {
        DateTime currentDate = DateTime.Now;
        string currentMonth = currentDate.ToString("MMMM");
        DateTime startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
        DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        await SetUpDb();
        var query = await _dbConnection.Table<IncomeExpenseModel>().Where(d => d.TransactionType == "Income" && d.Date >= startOfMonth && d.Date <= endOfMonth).ToListAsync();
        var totalIncome = query.Sum(s => s.Amount);
        lblCurrentMonthIncome.Text = currentMonth + ": " + string.Format(new CultureInfo("en-IN"), "{0:C0}", totalIncome);
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

    private void entryDays_TextChanged(object sender, TextChangedEventArgs e)
    {
        string daysLeft = entryDays.Text;
        if (string.IsNullOrEmpty(entryDays.Text))
        {
            daysLeft = "0";
        }
        _viewModel.GetMaturingAssetsListByDaysLeft(Convert.ToInt32(daysLeft));
    }

    private async void btnSaveExpense_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(entryExpenseAmount.Text))
        {
            await DisplayAlert("Message", "Please input required values", "OK");
            return;
        }
        if (string.IsNullOrEmpty(txtTransactionId.Text))//insert
        {

            IncomeExpenseModel objIncomeExpense = new IncomeExpenseModel()
            {
                Amount = Convert.ToDouble(entryExpenseAmount.Text),
                TransactionType = "Expense",
                Date = dpDateExpense.Date != DateTime.Now.Date ? dpDateExpense.Date : DateTime.Now,
                CategoryName = Convert.ToString(pickerExpenseCategory.SelectedItem),
                Remarks = entryExpenseRemarks.Text
            };
            await SetUpDb();
            int rowsAffected = await _dbConnection.InsertAsync(objIncomeExpense);
            entryExpenseAmount.Text = "";
            entryExpenseRemarks.Text = "";
            if (rowsAffected > 0)
            {
                //await DisplayAlert("Message", "Saved", "OK");

                LoadExpensesInPage();
            }
            else
            {
                await DisplayAlert("Error", "Something went wrong", "OK");
            }
        }
        else //update
        {
            IncomeExpenseModel objIncomeExpense = new IncomeExpenseModel()
            {
                TransactionId = Convert.ToInt32(txtTransactionId.Text),
                Amount = Convert.ToDouble(entryExpenseAmount.Text),
                TransactionType = "Expense",
                Date = dpDateExpense.Date != DateTime.Now.Date ? dpDateExpense.Date : DateTime.Now,
                CategoryName = Convert.ToString(pickerExpenseCategory.SelectedItem),
                Remarks = entryExpenseRemarks.Text
            };
            await SetUpDb();
            int rowsAffected = await _dbConnection.UpdateAsync(objIncomeExpense);
            entryExpenseAmount.Text = "";
            entryExpenseRemarks.Text = "";
            txtTransactionId.Text = "";

            if (rowsAffected > 0)
            {
                //await DisplayAlert("Message", "Saved", "OK");

                LoadExpensesInPage();
            }
            else
            {
                await DisplayAlert("Error", "Something went wrong", "OK");
            }
        }
    }

    private void ObjCell_Tapped(object sender, EventArgs e)
    {

        var tappedViewCell = (TextCell)sender;
        var textCell = tappedViewCell.Text.ToString().Split("|");
        string date = textCell[1].Trim().Split(" ")[0];
        int year = Convert.ToInt32(date.Split("-")[2]);
        int month = Convert.ToInt32(date.Split("-")[1]);
        int day = Convert.ToInt32(date.Split("-")[0]);

        pickerExpenseCategory.SelectedItem = textCell[0].Trim();

        if (tappedViewCell.Detail.Contains("-"))
        {
            dpDateExpense.Date = new DateTime(year, month, day);

            entryExpenseAmount.Text = tappedViewCell.Detail.Split("-")[0].Trim();
            entryExpenseRemarks.Text = tappedViewCell.Detail.Split("-")[1].Trim();
        }
        else
        {
            dpDateExpense.Date = new DateTime(year, month, day);

            entryExpenseAmount.Text = tappedViewCell.Detail;
            entryExpenseRemarks.Text = "";
        }

        txtTransactionId.Text = textCell[2].Trim();
    }

    private async void LoadExpensesInPage()
    {
        pickerExpenseCategory.SelectedItem = "Household Items"; //set this value by default
        dpDateExpense.MinimumDate = new DateTime(2020, 1, 1);
        dpDateExpense.MaximumDate = new DateTime(2050, 12, 31);
        dpDateExpense.Date = DateTime.Now;
        dpDateExpense.Format = "dd-MM-yyyy";
        tblscExpenses.Clear();
        await SetUpDb();

        List<IncomeExpenseModel> expenses = await _dbConnection.QueryAsync<IncomeExpenseModel>("select TransactionId,Amount,CategoryName,Date,Remarks from IncomeExpenseModel where TransactionType=='Expense' order by Date desc Limit 5");

        foreach (var item in expenses)
        {
            TextCell objCell = new TextCell();
            objCell.Text = item.CategoryName + " | " + item.Date.ToString("dd-MM-yyyy hh:mm tt") + " | " + item.TransactionId;

            if (!string.IsNullOrEmpty(item.Remarks))
            {
                objCell.Detail = Convert.ToString(item.Amount) + "- " + item.Remarks;
            }
            else
            {
                objCell.Detail = Convert.ToString(item.Amount);
            }

            tblscExpenses.Add(objCell);

            objCell.Tapped += ObjCell_Tapped;
        }
    }

    private async void LoadIncomeInPage()
    {
        //set date
        dpDateIncome.MinimumDate = new DateTime(2020, 1, 1);
        dpDateIncome.MaximumDate = new DateTime(2050, 12, 31);
        dpDateIncome.Date = DateTime.Now;
        dpDateIncome.Format = "dd-MM-yyyy";
        //set date
        tblscIncome.Clear();
        await SetUpDb();

        List<IncomeExpenseModel> income = await _dbConnection.QueryAsync<IncomeExpenseModel>("select TransactionId,Amount,CategoryName,Date,Remarks from IncomeExpenseModel where TransactionType=='Income' order by Date desc Limit 5");

        foreach (var item in income)
        {
            TextCell objCell = new TextCell();
            objCell.Text = item.CategoryName + " | " + item.Date.ToString("dd-MM-yyyy hh:mm tt") + " | " + item.TransactionId;

            if (!string.IsNullOrEmpty(item.Remarks))
            {
                objCell.Detail = Convert.ToString(item.Amount) + "- " + item.Remarks;
            }
            else
            {
                objCell.Detail = Convert.ToString(item.Amount);
            }

            tblscIncome.Add(objCell);

            objCell.Tapped += ObjCell_IncomeTapped; ;
        }
    }

    private void ObjCell_IncomeTapped(object sender, EventArgs e)
    {
        var tappedViewCell = (TextCell)sender;
        var textCell = tappedViewCell.Text.ToString().Split("|");
        string date = textCell[1].Trim().Split(" ")[0];
        int year = Convert.ToInt32(date.Split("-")[2]);
        int month = Convert.ToInt32(date.Split("-")[1]);
        int day = Convert.ToInt32(date.Split("-")[0]);

        pickerIncomeCategory.SelectedItem = textCell[0].Trim();

        if (tappedViewCell.Detail.Contains("-"))
        {
            dpDateIncome.Date = new DateTime(year, month, day);

            entryIncomeAmount.Text = tappedViewCell.Detail.Split("-")[0].Trim();
            entryIncomeRemarks.Text = tappedViewCell.Detail.Split("-")[1].Trim();
        }
        else
        {
            dpDateIncome.Date = new DateTime(year, month, day);

            entryIncomeAmount.Text = tappedViewCell.Detail;
            entryIncomeRemarks.Text = "";
        }

        txtIncomeTransactionId.Text = textCell[2].Trim();
    }

    private void btnClearExpense_Clicked(object sender, EventArgs e)
    {
        ClearExpense();
    }

    public void ClearExpense()
    {
        txtTransactionId.Text = "";
        entryExpenseAmount.Text = "";
        pickerExpenseCategory.SelectedIndex = -1;
        entryExpenseRemarks.Text = "";
        dpDateExpense.Date = DateTime.Now;
    }

    private async void btnUploadData_Clicked(object sender, EventArgs e)
    {
        bool userResponse = await DisplayAlert("Message", "Are you sure to upload data?", "Yes", "No");
        if (!userResponse)
        {
            return;
        }

        activityIndicator.IsRunning = true;
        var localPath = Path.Combine(FileSystem.CacheDirectory, "firestoredemo-d2bdc-firebase-adminsdk-zmue4-6f935f5ddc.json");

        using var json = await FileSystem.OpenAppPackageFileAsync("firestoredemo-d2bdc-firebase-adminsdk-zmue4-6f935f5ddc.json");
        using var dest = File.Create(localPath);
        await json.CopyToAsync(dest);

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", localPath);
        dest.Close();
        string projectId = "firestoredemo-d2bdc";
        var _fireStoreDb = FirestoreDb.Create(projectId);

        await DeleteAllDocumentsInCollection(Constants.IncomeExpenseFirestoreCollection);
        //deleted all records in firestore


        await SetUpDb();
        List<IncomeExpenseModel> transactions = await _dbConnection.Table<IncomeExpenseModel>().ToListAsync();
        int writeFlag = 0;



        CollectionReference collectionReference = _fireStoreDb.Collection(Constants.IncomeExpenseFirestoreCollection);

        foreach (var trans in transactions)
        {
            IncomeExpense incomeExpense = new IncomeExpense();
            incomeExpense.TransactionId = trans.TransactionId;
            incomeExpense.Amount = trans.Amount;
            incomeExpense.TransactionType = trans.TransactionType;
            incomeExpense.Date = Convert.ToString(trans.Date);
            incomeExpense.CategoryName = trans.CategoryName;
            incomeExpense.Remarks = trans.Remarks;

            var result = await collectionReference.AddAsync(incomeExpense);
            writeFlag++;
        }

        if (writeFlag == transactions.Count)
        {
            DataSyncAudit objSync = new DataSyncAudit
            {
                Date = DateTime.Now,
                Action = "Upload"
            };
            await _dbConnection.InsertAsync(objSync);
            lblLastUploaded.Text = "Last Uploaded: " + DateTime.Now.ToString("dd-MM-yyyy hh:mm tt");
            activityIndicator.IsRunning = false;
            await DisplayAlert("Message", "Data Upload Successful", "OK");

        }
        else
        {
            activityIndicator.IsRunning = false;
            await DisplayAlert("Error", "Something went wrong", "OK");
        }
    }

    private async void btnDownloadData_Clicked(object sender, EventArgs e)
    {
        activityIndicator.IsRunning = true;
        await SetUpDb();
        var existingRecords = await _dbConnection.Table<IncomeExpenseModel>().Take(1).ToListAsync();
        if (existingRecords.Count > 0)
        {
            bool userResponse = await DisplayAlert("Message", "There are existing records in the local database.Do you want to overwrite them?", "Yes", "No");
            if (userResponse)
            {
                int recordsDeleted = await _dbConnection.ExecuteAsync("Delete from IncomeExpenseModel"); //delete all present records in sqlite db
                await DownloadData();
            }
            activityIndicator.IsRunning = false;
        }
        else
        {
            await DownloadData();
        }
    }

    public async Task DeleteAllDocumentsInCollection(string collectionPath)
    {
        var localPath = Path.Combine(FileSystem.CacheDirectory, "firestoredemo-d2bdc-firebase-adminsdk-zmue4-6f935f5ddc.json");

        using var json = await FileSystem.OpenAppPackageFileAsync("firestoredemo-d2bdc-firebase-adminsdk-zmue4-6f935f5ddc.json");
        using var dest = File.Create(localPath);
        await json.CopyToAsync(dest);

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", localPath);
        dest.Close();
        string projectId = "firestoredemo-d2bdc";
        var _fireStoreDb = FirestoreDb.Create(projectId);

        CollectionReference collectionRef = _fireStoreDb.Collection(collectionPath);

        // Get all documents in the collection
        QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync();

        // Delete each document
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            await document.Reference.DeleteAsync();
        }
    }

    public async Task DownloadData()
    {
        var localPath = Path.Combine(FileSystem.CacheDirectory, "firestoredemo-d2bdc-firebase-adminsdk-zmue4-6f935f5ddc.json");

        using var json = await FileSystem.OpenAppPackageFileAsync("firestoredemo-d2bdc-firebase-adminsdk-zmue4-6f935f5ddc.json");
        using var dest = File.Create(localPath);
        await json.CopyToAsync(dest);

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", localPath);
        dest.Close();
        string projectId = "firestoredemo-d2bdc";
        var _fireStoreDb = FirestoreDb.Create(projectId);

        int rowsAffected = 0;
        try
        {
            Query orderQuery = _fireStoreDb.Collection(Constants.IncomeExpenseFirestoreCollection);
            QuerySnapshot orderQuerySnapshot = await orderQuery.GetSnapshotAsync();
            List<IncomeExpense> incomeExpObj = new List<IncomeExpense>();

            foreach (DocumentSnapshot documentSnapshot in orderQuerySnapshot.Documents)
            {
                if (documentSnapshot.Exists)
                {
                    Dictionary<string, object> dictionary = documentSnapshot.ToDictionary();
                    string jsonObj = JsonConvert.SerializeObject(dictionary);
                    IncomeExpense newIncExp = JsonConvert.DeserializeObject<IncomeExpense>(jsonObj);
                    incomeExpObj.Add(newIncExp);
                }
            }

            foreach (var item in incomeExpObj)
            {
                IncomeExpenseModel model = new IncomeExpenseModel();
                model.Amount = item.Amount;
                model.TransactionType = item.TransactionType;
                if (DateTime.TryParse(item.Date, out DateTime result))
                {
                    model.Date = Convert.ToDateTime(item.Date);
                }
                else
                {
                    model.Date = DateTime.Now;
                }

                model.CategoryName = item.CategoryName;
                model.Remarks = item.Remarks;

                rowsAffected = rowsAffected + await _dbConnection.InsertAsync(model);
            }


            if (rowsAffected == incomeExpObj.Count)
            {
                activityIndicator.IsRunning = false;
                await DisplayAlert("Message", "Success", "OK");
            }
            else
            {
                activityIndicator.IsRunning = false;
                await DisplayAlert("Error", "Something went wrong", "OK");
            }
        }
        catch (Exception) { throw; }
    }

    private async void btnSaveIncome_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(entryIncomeAmount.Text))
        {
            await DisplayAlert("Message", "Please input required values", "OK");
            return;
        }
        if (string.IsNullOrEmpty(txtIncomeTransactionId.Text))//insert
        {
            IncomeExpenseModel objIncomeExpense = new IncomeExpenseModel()
            {
                Amount = Convert.ToDouble(entryIncomeAmount.Text),
                TransactionType = "Income",
                Date = dpDateIncome.Date != DateTime.Now.Date ? dpDateIncome.Date : DateTime.Now,
                CategoryName = Convert.ToString(pickerIncomeCategory.SelectedItem),
                Remarks = entryIncomeRemarks.Text
            };
            await SetUpDb();
            int rowsAffected = await _dbConnection.InsertAsync(objIncomeExpense);
            entryIncomeAmount.Text = "";
            entryIncomeRemarks.Text = "";
            if (rowsAffected > 0)
            {
                LoadIncomeInPage();
                //ClearIncome();
            }
            else
            {
                await DisplayAlert("Error", "Something went wrong", "OK");
            }
        }
        else //update
        {
            IncomeExpenseModel objIncomeExpense = new IncomeExpenseModel()
            {
                TransactionId = Convert.ToInt32(txtIncomeTransactionId.Text),
                Amount = Convert.ToDouble(entryIncomeAmount.Text),
                TransactionType = "Income",
                Date = dpDateIncome.Date != DateTime.Now.Date ? dpDateIncome.Date : DateTime.Now,
                CategoryName = Convert.ToString(pickerIncomeCategory.SelectedItem),
                Remarks = entryIncomeRemarks.Text
            };
            await SetUpDb();
            int rowsAffected = await _dbConnection.UpdateAsync(objIncomeExpense);
            entryIncomeAmount.Text = "";
            entryIncomeRemarks.Text = "";
            txtIncomeTransactionId.Text = "";

            if (rowsAffected > 0)
            {
                LoadIncomeInPage();
                //ClearIncome();
            }
            else
            {
                await DisplayAlert("Error", "Something went wrong", "OK");
            }
        }
    }

    private void btnClearIncome_Clicked(object sender, EventArgs e)
    {
        ClearIncome();
    }

    public void ClearIncome()
    {
        txtIncomeTransactionId.Text = "";
        entryIncomeAmount.Text = "";
        pickerIncomeCategory.SelectedIndex = -1;
        entryIncomeRemarks.Text = "";
        dpDateIncome.Date = DateTime.Now;
    }

    private async void btnGoToReports_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ReportsPage(_dbConnection));
    }

    private async void btnDeleteExpense_Clicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(txtTransactionId.Text))
        {
            bool userResponse = await DisplayAlert("Warning", "Are you sure to delete?", "Yes", "No");
            if (userResponse)
            {
                IncomeExpenseModel objExpense = new IncomeExpenseModel()
                {
                    TransactionId = Convert.ToInt32(txtTransactionId.Text)
                };

                await SetUpDb();
                int rowsAffected = await _dbConnection.DeleteAsync(objExpense);
                ClearExpense();
                LoadExpensesInPage();
            }
        }
    }

    private async void btnDeleteIncome_Clicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(txtIncomeTransactionId.Text))
        {
            bool userResponse = await DisplayAlert("Warning", "Are you sure to delete?", "Yes", "No");
            if (userResponse)
            {
                IncomeExpenseModel objIncome = new IncomeExpenseModel()
                {
                    TransactionId = Convert.ToInt32(txtIncomeTransactionId.Text)
                };

                await SetUpDb();
                int rowsAffected = await _dbConnection.DeleteAsync(objIncome);
                ClearIncome();
                LoadIncomeInPage();
            }
        }
    }

    private async void btnAssetReport_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AssetReportPage(_dbConnection, _popupNavigation));
    }
}