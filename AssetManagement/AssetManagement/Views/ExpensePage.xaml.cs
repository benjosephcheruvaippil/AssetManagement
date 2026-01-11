//using AndroidX.Lifecycle;
using AssetManagement.Common;
using AssetManagement.Models;
using AssetManagement.Models.Constants;
using AssetManagement.Models.DataTransferObject;
using CommunityToolkit.Maui.Storage;
using ExcelDataReader;
using SQLite;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AssetManagement.Views;

public partial class ExpensePage : ContentPage
{
    //private AssetListPageViewModel _viewModel;
    private SQLiteAsyncConnection _dbConnection;
    AppTheme currentTheme = Application.Current.RequestedTheme;
    //private IPopupNavigation _popupNavigation;
    ///private readonly IAssetService _assetService;
    public int PageNumber = 0, PageSize = 30, TotalExpenseRecordCount = 0;
    public bool ApplyFilterClicked = false;
    //Border _selectedFrame = null;
    public ExpensePage()
    {
        InitializeComponent();

        if (currentTheme == AppTheme.Dark)
        {
            //set to white color
            pickerExpenseCategory.TextColor = Color.FromArgb("#FFFFFF");
            pickerExpenseCategory.BackgroundColor = Color.FromArgb("#000000");
        }

        var labelShowRemaining = new TapGestureRecognizer();
        labelShowRemaining.Tapped += (s, e) =>
        {
            LoadExpensesInPage("Pagewise");
        };
        lblShowRemainingRecords.GestureRecognizers.Add(labelShowRemaining);

        double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
        expenseCollectionView.HeightRequest= screenHeight * 0.5;
    }

    protected async override void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            CommonFunctions objCommon = new CommonFunctions();
            await objCommon.SetUserCurrencyGlobally();
            await objCommon.SetCategoriesAsIsVisibleIfNullOrEmpty();
            LoadExpensesInPage("Last5");// show expenses in the expense tab          
            await ShowCurrentMonthExpenses();
            LoadExpenseCategoriesInDropdown();
            await CheckForAppUpdate();
            //SetLastUploadedDate();
        }
        catch(Exception)
        {
            //await DisplayAlert("Error", ex.Message, "OK");
            return;
        }
    }

    private async Task SetUpDb()
    {
        try
        {
            if (_dbConnection == null)
            {
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
                _dbConnection = new SQLiteAsyncConnection(dbPath);
                await _dbConnection.CreateTableAsync<Assets>();
                await _dbConnection.CreateTableAsync<AssetDocuments>();
                await _dbConnection.CreateTableAsync<IncomeExpenseModel>();
                await _dbConnection.CreateTableAsync<IncomeExpenseCategories>();
                await _dbConnection.CreateTableAsync<DataSyncAudit>();
                await _dbConnection.CreateTableAsync<AssetAuditLog>();
                await _dbConnection.CreateTableAsync<Owners>();
                await _dbConnection.CreateTableAsync<UserCurrency>();
                await _dbConnection.CreateTableAsync<Currency>();
            }
        }
        catch(Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    public async Task CheckForAppUpdate()
    {
        try
        {
            CommonFunctions objCommon = new CommonFunctions();
            if (await objCommon.IsAppUpdateAvailable())
            {
                bool update = await DisplayAlert("Update Available","A new version of the app is available. Please update. We recommend you take a backup before updation.", "Update", "Later");

                await _dbConnection.ExecuteAsync("UPDATE Owners SET UpdateAvailableLastChecked = ?", DateTime.Today);

                if (update)
                {
                    await Launcher.OpenAsync($"https://play.google.com/store/apps/details?id=com.companyname.assetmanagement");
                }
                //else
                //{
                //    await _dbConnection.ExecuteAsync("UPDATE Owners SET UpdateAvailableLastChecked = ?", DateTime.Today);
                //}
            }
        }
        catch(Exception)
        {
            return;
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
        try
        {
            DateTime currentDate = DateTime.Now;
            string currentMonth = currentDate.ToString("MMMM");
            DateTime startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1, 0, 0, 0);
            int lastDayOfMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            DateTime endOfMonth = new DateTime(currentDate.Year, currentDate.Month, lastDayOfMonth, 23, 59, 59);

            await SetUpDb();            
            var query = await _dbConnection.Table<IncomeExpenseModel>().Where(d => d.TransactionType == "Expense" && d.Date >= startOfMonth && d.Date <= endOfMonth).ToListAsync();
            var totalExpense = query.Sum(s => s.Amount);
            lblCurrentMonthExpenses.Text = currentMonth + ": " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", totalExpense);
        }
        catch(Exception)
        {
            return;
        }
    }

    private async void LoadExpenseCategoriesInDropdown()
    {
        try
        {
            await SetUpDb();
            var expenseCategories = await _dbConnection.Table<IncomeExpenseCategories>().Where(i => i.CategoryType == "Expense" && i.IsVisible == true).ToListAsync();
            IncomeExpenseCategories objCategories = new IncomeExpenseCategories
            {
                IncomeExpenseCategoryId = 0,
                CategoryName = Constants.AddNewCategoryOption,
                CategoryType = "Expense"
            };
            expenseCategories.Add(objCategories);
            pickerExpenseCategory.ItemsSource = expenseCategories.Select(i => i.CategoryName).ToList();
            if (expenseCategories.Count > 1)
            {
                pickerExpenseCategory.Text = "";
            }
        }
        catch (Exception)
        {
            return;
        }
    }


    private async void btnSaveExpense_Clicked(object sender, EventArgs e)
    {
        if (((pickerExpenseCategory.ItemsSource as IEnumerable<object>)?.Cast<object>().Count() ?? 0) == 0)
        {
            await DisplayAlert("Message", "Please create categories under Settings -> Manage Categories before adding expenses", "OK");
            return;
        }
        else if (string.IsNullOrEmpty(entryExpenseAmount.Text))
        {
            await DisplayAlert("Message", "Please input required values", "OK");
            return;
        }
        else if (string.IsNullOrEmpty(pickerExpenseCategory.Text))
        {
            await DisplayAlert("Message", "Please select a category", "OK");
            return;
        }

        //check if category present in master table
        var expenseCategories = await _dbConnection.Table<IncomeExpenseCategories>().Where(i => i.CategoryType == "Expense").ToListAsync();
        if (!expenseCategories.Any(c => c.CategoryName == pickerExpenseCategory.Text.Trim()))
        {
            await DisplayAlert("Message", "Please create this category under Settings -> Manage Categories before adding expenses", "OK");
            return;
        }
        //check if category present in master table

        if (string.IsNullOrEmpty(txtTransactionId.Text))//insert
        {

            IncomeExpenseModel objIncomeExpense = new IncomeExpenseModel()
            {
                Amount = Convert.ToDouble(entryExpenseAmount.Text),
                TransactionType = "Expense",
                Date = dpDateExpense.Date != DateTime.Now.Date ? dpDateExpense.Date : DateTime.Now,
                CategoryName = Convert.ToString(pickerExpenseCategory.Text.Trim()),
                Remarks = entryExpenseRemarks.Text,
                Mode = "manual"
            };
            await SetUpDb();
            int rowsAffected = await _dbConnection.InsertAsync(objIncomeExpense);
            entryExpenseAmount.Text = "";
            entryExpenseRemarks.Text = "";
            if (rowsAffected > 0)
            {

                LoadExpensesInPage("Last5");
                await ShowCurrentMonthExpenses();
            }
            else
            {
                await DisplayAlert("Error", "Something went wrong", "OK");
            }
        }
        else //update
        {
            int transId = Convert.ToInt32(txtTransactionId.Text);
            var incExpResult = await _dbConnection.Table<IncomeExpenseModel>().Where(i => i.TransactionId == transId).FirstOrDefaultAsync();
            IncomeExpenseModel objIncomeExpense = new IncomeExpenseModel()
            {
                TransactionId = transId,
                Amount = Convert.ToDouble(entryExpenseAmount.Text),
                TransactionType = "Expense",
                Date = dpDateExpense.Date != DateTime.Now.Date ? dpDateExpense.Date : DateTime.Now,
                CategoryName = Convert.ToString(pickerExpenseCategory.Text.Trim()),
                Mode = incExpResult.Mode,
                Remarks = entryExpenseRemarks.Text
            };
            await SetUpDb();
            int rowsAffected = await _dbConnection.UpdateAsync(objIncomeExpense);
            entryExpenseAmount.Text = "";
            entryExpenseRemarks.Text = "";
            txtTransactionId.Text = "";

            if (rowsAffected > 0)
            {

                LoadExpensesInPage("Last5");
                await ShowCurrentMonthExpenses();
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

        pickerExpenseCategory.Text = textCell[0].Trim();

        if (string.IsNullOrEmpty(pickerExpenseCategory.Text))
        {
            DisplayAlert("Info", textCell[0].Trim()+" - Please make this category visible from Manage Category page in order to edit.", "OK");
        }

        if (tappedViewCell.Detail.Contains("-"))
        {
            dpDateExpense.Date = new DateTime(year, month, day);

            string[] amountAndRemarks = tappedViewCell.Detail.Split(new[] { "-" }, StringSplitOptions.TrimEntries);
            string remarks = "";
            entryExpenseAmount.Text = amountAndRemarks[0];
            int loop = 1;
            //logic for '-' symbol handling in remarks. for example if detail has some value like '250 - this is for 24-35 date range', then the below logic will
            //handle it to populate it in remarks.
            foreach(var item in amountAndRemarks)
            {
                if (loop > 1)
                {
                    string next = item;
                    if (loop > 2)
                    {
                        next = " - " + item;
                    }
                    remarks = remarks + next;
                    
                }
                loop++;
            }
            entryExpenseRemarks.Text = remarks;
        }
        else
        {
            dpDateExpense.Date = new DateTime(year, month, day);

            entryExpenseAmount.Text = tappedViewCell.Detail;
            entryExpenseRemarks.Text = "";
        }

        txtTransactionId.Text = textCell[2].Trim();
    }

    private async void LoadExpensesInPage(string hint)
    {  
        pickerExpenseCategory.Text = "Household Items"; //set this value by default
        dpDateExpense.MinimumDate = new DateTime(2020, 1, 1);
        dpDateExpense.MaximumDate = new DateTime(2050, 12, 31);
        dpDateExpense.Date = DateTime.Now;
        dpDateExpense.Format = "dd-MM-yyyy";
        //tblscExpenses.Clear();
        await SetUpDb();

        List<IncomeExpenseModel> expenses = new List<IncomeExpenseModel>();
        List<IncomeExpenseDTO> expensesDTO = new List<IncomeExpenseDTO>();

        if (hint == "Last5")
        {
            PageNumber = 0;
            ApplyFilterClicked = false;
            //tblscExpenses.Title = "Last 5 Transactions";
            lblCardBanner.Text= "Last 5 Transactions";
            lblShowRemainingRecords.IsVisible = true;
            expenses = await _dbConnection.QueryAsync<IncomeExpenseModel>("select TransactionId,Amount,CategoryName,Date,Remarks,Mode from IncomeExpenseModel where TransactionType=='Expense' order by Date desc Limit 5");
            expensesDTO = expenses.Select(s => new IncomeExpenseDTO
            {
                TransactionId = s.TransactionId,
                Amount = s.Amount,
                CurrencySymbol = Constants.GetCurrency(),
                CategoryName = s.CategoryName,
                Date = s.Date,
                Remarks = s.Remarks,
                Mode = s.Mode
            }).ToList();
            expenseCollectionView.ItemsSource = expensesDTO;
        }
        else if (hint == "Pagewise")
        {
            if (ApplyFilterClicked)
            {
                await ApplyFilterPagination();
                await expenseScrollView.ScrollToAsync(expanderFilterDetails, ScrollToPosition.Start, true);
                return;
            }
            PageNumber = PageNumber + 1;
            int offset = (PageNumber - 1) * PageSize;
            if (TotalExpenseRecordCount == 0)
            {
                TotalExpenseRecordCount = await _dbConnection.Table<IncomeExpenseModel>().Where(e => e.TransactionType == "Expense").CountAsync();
            }
            int showRecordCount = 0;
            if (offset == 0)
            {
                showRecordCount = PageSize;
            }
            else
            {
                showRecordCount = PageSize + offset;
            }
            if (TotalExpenseRecordCount - offset < PageSize)
            {
                showRecordCount = TotalExpenseRecordCount;
                lblShowRemainingRecords.IsVisible = false;
            }

            //tblscExpenses.Title = "Showing "+ showRecordCount + " of "+ TotalExpenseRecordCount + " records";
            lblCardBanner.Text = "Showing " + showRecordCount + " of " + TotalExpenseRecordCount + " records";
            expenses = await _dbConnection.QueryAsync<IncomeExpenseModel>("select TransactionId,Amount,CategoryName,Date,Remarks,Mode from IncomeExpenseModel where TransactionType=='Expense' order by Date desc Limit 30 Offset " + offset);
            expensesDTO = expenses.Select(s => new IncomeExpenseDTO
            {
                TransactionId = s.TransactionId,
                Amount = s.Amount,
                CurrencySymbol = Constants.GetCurrency(),
                CategoryName = s.CategoryName,
                Date = s.Date,
                Remarks = s.Remarks,
                Mode = s.Mode
            }).ToList();

            expenseCollectionView.ItemsSource = expensesDTO;
            await expenseScrollView.ScrollToAsync(expanderFilterDetails, ScrollToPosition.Start, true);
        }

        //foreach (var item in expenses)
        //{
        //    TextCell objCell = new TextCell();
        //    objCell.Text = item.CategoryName + " | " + item.Date.ToString("dd-MM-yyyy hh:mm tt") + " | " + item.TransactionId;

        //    if (!string.IsNullOrEmpty(item.Remarks))
        //    {
        //        objCell.Detail = Convert.ToString(item.Amount) + "- " + item.Remarks;
        //    }
        //    else
        //    {
        //        objCell.Detail = Convert.ToString(item.Amount);
        //    }

        //    if (currentTheme == AppTheme.Dark)
        //    {
        //        //set to white color
        //        //tblscExpenses.TextColor= Color.FromArgb("#FFFFFF");
        //        objCell.TextColor = Color.FromArgb("#FFFFFF");
        //    }

        //    //tblscExpenses.Add(objCell);

        //    objCell.Tapped += ObjCell_Tapped;
        //}

        //expenseCollectionView.ItemsSource = expensesDTO;
        //await expenseScrollView.ScrollToAsync(expanderFilterDetails, ScrollToPosition.Start, true);
    }

    private void btnClearExpense_Clicked(object sender, EventArgs e)
    {
        ClearExpense();
    }

    public void ClearExpense()
    {
        txtTransactionId.Text = "";
        entryExpenseAmount.Text = "";
        pickerExpenseCategory.Text = "";
        entryExpenseRemarks.Text = "";
        dpDateExpense.Date = DateTime.Now;
        //if (_selectedFrame != null)
        //{
        //    _selectedFrame.BackgroundColor = Colors.White;
        //    _selectedFrame = null;
        //}
        if (expenseCollectionView.ItemsSource is IEnumerable<IncomeExpenseDTO> items)
        {
            foreach (var item in items)
                item.IsSelected = false; // reset all
        }
    }

    //private async void btnUploadData_Clicked(object sender, EventArgs e)
    //{
    //    bool userResponse = await DisplayAlert("Message", "Are you sure to upload data to firestore DB?", "Yes", "No");
    //    if (!userResponse)
    //    {
    //        return;
    //    }

    //    activityIndicator.IsRunning = true;
    //    var localPath = Path.Combine(FileSystem.CacheDirectory, "firestoredemo-d2bdc-firebase-adminsdk-zmue4-6f935f5ddc.json");

    //    using var json = await FileSystem.OpenAppPackageFileAsync("firestoredemo-d2bdc-firebase-adminsdk-zmue4-6f935f5ddc.json");
    //    using var dest = File.Create(localPath);
    //    await json.CopyToAsync(dest);

    //    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", localPath);
    //    dest.Close();
    //    string projectId = "firestoredemo-d2bdc";
    //    var _fireStoreDb = FirestoreDb.Create(projectId);

    //    await DeleteAllDocumentsInCollection(Constants.IncomeExpenseFirestoreCollection);
    //    //deleted all records in firestore


    //    await SetUpDb();
    //    List<IncomeExpenseModel> transactions = await _dbConnection.Table<IncomeExpenseModel>().ToListAsync();
    //    int writeFlag = 0;



    //    CollectionReference collectionReference = _fireStoreDb.Collection(Constants.IncomeExpenseFirestoreCollection);

    //    foreach (var trans in transactions)
    //    {
    //        IncomeExpense incomeExpense = new IncomeExpense();
    //        incomeExpense.TransactionId = trans.TransactionId;
    //        incomeExpense.Amount = trans.Amount;
    //        incomeExpense.TaxAmountCut = trans.TaxAmountCut;
    //        incomeExpense.OwnerName = trans.OwnerName;
    //        incomeExpense.TransactionType = trans.TransactionType;
    //        incomeExpense.Date = Convert.ToString(trans.Date);
    //        incomeExpense.CategoryName = trans.CategoryName;
    //        incomeExpense.Remarks = trans.Remarks;
    //        incomeExpense.Mode = trans.Mode;

    //        var result = await collectionReference.AddAsync(incomeExpense);
    //        writeFlag++;
    //    }

    //    if (writeFlag == transactions.Count)
    //    {
    //        DataSyncAudit objSync = new DataSyncAudit
    //        {
    //            Date = DateTime.Now,
    //            Action = "Upload"
    //        };
    //        await _dbConnection.InsertAsync(objSync);
    //        lblLastUploaded.Text = "Last Uploaded: " + DateTime.Now.ToString("dd-MM-yyyy hh:mm tt");
    //        activityIndicator.IsRunning = false;
    //        await DisplayAlert("Message", "Data Upload Successful", "OK");

    //    }
    //    else
    //    {
    //        activityIndicator.IsRunning = false;
    //        await DisplayAlert("Error", "Something went wrong", "OK");
    //    }
    //}

    //private async void btnDownloadData_Clicked(object sender, EventArgs e)
    //{
    //    activityIndicator.IsRunning = true;
    //    await SetUpDb();
    //    var existingRecords = await _dbConnection.Table<IncomeExpenseModel>().Take(1).ToListAsync();
    //    if (existingRecords.Count > 0)
    //    {
    //        bool userResponse = await DisplayAlert("Message", "There are existing records in the local database.Do you want to overwrite them?", "Yes", "No");
    //        if (userResponse)
    //        {
    //            int recordsDeleted = await _dbConnection.ExecuteAsync("Delete from IncomeExpenseModel"); //delete all present records in sqlite db
    //            await DownloadData();
    //        }
    //        activityIndicator.IsRunning = false;
    //    }
    //    else
    //    {
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
    //        Query orderQuery = _fireStoreDb.Collection(Constants.IncomeExpenseFirestoreCollection);
    //        QuerySnapshot orderQuerySnapshot = await orderQuery.GetSnapshotAsync();
    //        List<IncomeExpense> incomeExpObj = new List<IncomeExpense>();

    //        foreach (DocumentSnapshot documentSnapshot in orderQuerySnapshot.Documents)
    //        {
    //            if (documentSnapshot.Exists)
    //            {
    //                Dictionary<string, object> dictionary = documentSnapshot.ToDictionary();
    //                string jsonObj = JsonConvert.SerializeObject(dictionary);
    //                IncomeExpense newIncExp = JsonConvert.DeserializeObject<IncomeExpense>(jsonObj);
    //                incomeExpObj.Add(newIncExp);
    //            }
    //        }

    //        foreach (var item in incomeExpObj)
    //        {
    //            IncomeExpenseModel model = new IncomeExpenseModel();
    //            model.Amount = item.Amount;
    //            model.TaxAmountCut = item.TaxAmountCut;
    //            model.TransactionType = item.TransactionType;
    //            if (DateTime.TryParse(item.Date, out DateTime result))
    //            {
    //                model.Date = Convert.ToDateTime(item.Date);
    //            }
    //            else
    //            {
    //                model.Date = DateTime.Now;
    //            }

    //            model.OwnerName = item.OwnerName;
    //            model.CategoryName = item.CategoryName;
    //            model.Remarks = item.Remarks;
    //            model.Mode = item.Mode;

    //            rowsAffected = rowsAffected + await _dbConnection.InsertAsync(model);
    //        }


    //        if (rowsAffected == incomeExpObj.Count)
    //        {
    //            activityIndicator.IsRunning = false;
    //            await DisplayAlert("Message", "Success", "OK");
    //        }
    //        else
    //        {
    //            activityIndicator.IsRunning = false;
    //            await DisplayAlert("Error", "Something went wrong", "OK");
    //        }
    //    }
    //    catch (Exception) { throw; }
    //}

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
                LoadExpensesInPage("Last5");
                await ShowCurrentMonthExpenses();
            }
        }
    }

    private async void btnApplyFilters_Clicked(object sender, EventArgs e)
    {
        ApplyFilterClicked = true;
        PageNumber = 0;
        TotalExpenseRecordCount = 0;
        lblShowRemainingRecords.IsVisible = true;
        await ApplyFilterPagination();
    }

    public async Task ApplyFilterPagination()
    {
        //tblscExpenses.Clear();
        PageNumber = PageNumber + 1;
        int offset = (PageNumber - 1) * PageSize;

        DateTime? fromDate = dpFromDateFilter.Date, toDate = dpToDateFilter.Date.AddDays(1).AddSeconds(-1);
        if (dpFromDateFilter.Date == dpToDateFilter.Date)
        {
            fromDate = dpFromDateFilter.Date;
            toDate = dpToDateFilter.Date.AddDays(1);
        }
       
        string category = entCategoryFilter.Text;
        string remarks = entRemarksFilter.Text;

        List<IncomeExpenseModel> expenses = new List<IncomeExpenseModel>();
        List<IncomeExpenseDTO> expensesDTO = new List<IncomeExpenseDTO>();

        var query = @"select TransactionId,Amount,CategoryName,Date,Remarks,Mode from IncomeExpenseModel where TransactionType=='Expense'";

        var parameters = new List<object>();
        if (fromDate != null && toDate != null)
        {
            query += "AND Date BETWEEN ? AND ? ";
            parameters.Add(fromDate);
            parameters.Add(toDate);
        }
        if (!string.IsNullOrEmpty(category))
        {
            query += "AND CategoryName like ? ";
            parameters.Add($"%{category}%");
        }
        if (!string.IsNullOrEmpty(remarks))
        {
            query += "AND Remarks like ?";
            parameters.Add($"%{remarks}%");
        }

        string pageCountQuery = query;
        query += "ORDER BY Date DESC LIMIT 30 Offset " + offset;

        expenses = await _dbConnection.QueryAsync<IncomeExpenseModel>(query, parameters.ToArray());
        expensesDTO = expenses.Select(s => new IncomeExpenseDTO
        {
            TransactionId = s.TransactionId,
            Amount = s.Amount,
            CurrencySymbol = Constants.GetCurrency(),
            CategoryName = s.CategoryName,
            Date = s.Date,
            Remarks = s.Remarks,
            Mode = s.Mode
        }).ToList();

        //pagination
        if (TotalExpenseRecordCount == 0)
        {
            var totalRecords = await _dbConnection.QueryAsync<IncomeExpenseModel>(pageCountQuery, parameters.ToArray());
            lblCurrentMonthExpenses.Text = "Total: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", totalRecords.Sum(s => s.Amount));
            TotalExpenseRecordCount = totalRecords.Count();
        }
        int showRecordCount = 0;
        if (offset == 0)
        {
            showRecordCount = PageSize;
        }
        else
        {
            showRecordCount = PageSize + offset;
        }
        if (TotalExpenseRecordCount - offset < PageSize)
        {
            showRecordCount = TotalExpenseRecordCount;
            lblShowRemainingRecords.IsVisible = false;
        }

        //tblscExpenses.Title = "Showing " + showRecordCount + " of " + TotalExpenseRecordCount + " records";
        lblCardBanner.Text = "Showing " + showRecordCount + " of " + TotalExpenseRecordCount + " records";
        //pagination

        //foreach (var item in expenses)
        //{
        //    TextCell objCell = new TextCell();
        //    objCell.Text = item.CategoryName + " | " + item.Date.ToString("dd-MM-yyyy hh:mm tt") + " | " + item.TransactionId;

        //    if (!string.IsNullOrEmpty(item.Remarks))
        //    {
        //        objCell.Detail = Convert.ToString(item.Amount) + "- " + item.Remarks;
        //    }
        //    else
        //    {
        //        objCell.Detail = Convert.ToString(item.Amount);
        //    }

        //    if (currentTheme == AppTheme.Dark)
        //    {
        //        //set to white color
        //        //tblscExpenses.TextColor = Color.FromArgb("#FFFFFF");
        //        objCell.TextColor = Color.FromArgb("#FFFFFF");
        //    }

        //    //tblscExpenses.Add(objCell);

        //    objCell.Tapped += ObjCell_Tapped;
        //}

        expenseCollectionView.ItemsSource = expensesDTO;
    }

    private async void pickexpensefile_Clicked(object sender, EventArgs e)
    {
        try
        {
            //get the last file_upload date
            await SetUpDb();
            List<IncomeExpenseModel> expenses = await _dbConnection.QueryAsync<IncomeExpenseModel>("select TransactionId,Amount,CategoryName,Date,Remarks,Mode from IncomeExpenseModel where TransactionType=='Expense' and Mode='file_upload' order by Date desc Limit 1");
            var categoriesWithShortCode = await _dbConnection.Table<IncomeExpenseCategories>().Where(c => c.CategoryType == "Expense" && !string.IsNullOrEmpty(c.ShortCode)).ToListAsync();
            string messageText = "";
            foreach (var code in categoriesWithShortCode)
            {
                messageText = messageText + $"- {code.ShortCode}: {code.CategoryName}\n";
            }
            if (expenses.Count > 0)
            {
                bool userResponse = await DisplayAlert("Message", $"The last upload happened at {expenses[0].Date.ToString("dd-MM-yyyy")}.\n\nInstructions\n\n {messageText} \n- Upload only excel file with only a single sheet.\n- First column is date in dd-mm-yyyy format(text field).\n- Third column is description.\n- Fourth column is amount.\n\nDo you wish to continue uploading the file?", "Yes", "No");
                if (!userResponse)
                {
                    return;
                }
            }
            else
            {
                bool userResponse = await DisplayAlert("Message", $"Instructions\n {messageText} \n- Upload only excel file with only a single sheet.\n- First column is date in dd-mm-yyyy format(text field).\n- Third column is description.\n- Fourth column is amount.\n\nDo you wish to continue uploading the file?", "Yes", "No");
                if (!userResponse)
                {
                    return;
                }
            }
            //get the last file_upload date

            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Pick File Please"
            });

            if (result == null)
                return;

            activityIndicator.IsRunning = true;
            int rowsAdded = 0;
            List<IncomeExpenseModel> listIncomeExpenseModel = new List<IncomeExpenseModel>();
            DataSet dsexcelRecords = new DataSet();
            IExcelDataReader reader = null;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var filestream = await result.OpenReadAsync();
            reader = ExcelReaderFactory.CreateOpenXmlReader(filestream);
            dsexcelRecords = reader.AsDataSet();
            reader.Close();

            if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
            {
                DataTable dtStudentRecords = dsexcelRecords.Tables[0];
                for (int i = 1; i < dtStudentRecords.Rows.Count; i++)
                {
                    string transactionDateString = Convert.ToString(dtStudentRecords.Rows[i][0]).Trim();
                    //string transactionDateString = (dtStudentRecords.Rows[i][0]).ToString("MM/dd/yyyy");
                    string[] dateArr = transactionDateString.Split("-");
                    if(dateArr.Count() == 3)
                    //if (DateTime.TryParse(transactionDateString, out DateTime transactionDate))
                    {
                        //Delete if exists mode=file_upload on the specified date
                        await SetUpDb();

                        DateTime transactionDate = new DateTime(Convert.ToInt32(dateArr[2]), Convert.ToInt32(dateArr[1]), Convert.ToInt32(dateArr[0]));
                        DateTime date = transactionDate.Date;
                        var recordsToBeDeleted = await _dbConnection.Table<IncomeExpenseModel>().Where(i => i.Mode == "file_upload" && i.TransactionType == "Expense" && i.Date >= date).ToListAsync();
                        if (recordsToBeDeleted.Count > 0)
                        {
                            foreach (var record in recordsToBeDeleted)
                            {
                                await _dbConnection.DeleteAsync(record);
                            }
                        }
                        //Delete if exists mode=file_upload on the specified date
                        break;
                    }
                }
                for (int i = 1; i < dtStudentRecords.Rows.Count; i++)
                {
                    string transactionDateString = Convert.ToString(dtStudentRecords.Rows[i][0]).Trim();
                    string[] dateArr = transactionDateString.Split("-");

                    //if (DateTime.TryParse(transactionDateString, out DateTime transactionDate))
                    if (dateArr.Count() == 3)
                    {
                        DateTime transactionDate = new DateTime(Convert.ToInt32(dateArr[2]), Convert.ToInt32(dateArr[1]), Convert.ToInt32(dateArr[0]));
                        DateTime date = transactionDate.Date;
                        bool addExpense = false;
                        string category = "",remarks="";
                        double amount = 0;
                        if (!string.IsNullOrEmpty(Convert.ToString(dtStudentRecords.Rows[i][3])))
                        {
                            amount = Convert.ToDouble(dtStudentRecords.Rows[i][3]);
                        }
                        string description = Convert.ToString(dtStudentRecords.Rows[i][2]);
                        string alteredDescription = description.Replace(" ", "").Replace("\n", "").ToLower();
                        //description = description.Replace(" ", "");
                        //description = description.Replace("\n", "");
                        //description = description.ToLower();

                        if (categoriesWithShortCode.Count == 0)
                        {
                            //no categories with shortcode found, so cannot proceed
                            await DisplayAlert("Error", $"There are no categories with short code. Hence cannot add any records.", "OK");
                            break;
                        }
                        foreach(var kvp in categoriesWithShortCode)
                        {
                            if (alteredDescription.Contains("/" + kvp.ShortCode + "/"))
                            {
                                addExpense = true;
                                category = kvp.CategoryName;
                                break;
                            }
                            else if(alteredDescription.Contains("/" + kvp.ShortCode + "-") || alteredDescription.Contains("/" + kvp.ShortCode + " -"))
                            {
                                addExpense = true;

                                string pattern = $@"/{kvp.ShortCode}\s*-[^/]+";
                                var match = Regex.Match(description, pattern, RegexOptions.IgnoreCase);

                                string input = match.Success ? match.Value.TrimStart('/') : string.Empty;
                                int index = input.IndexOf('-');
                                //category = index >= 0 ? input.Substring(0, index).Trim() : input;
                                category = kvp.CategoryName;
                                remarks = index >= 0 ? input.Substring(index + 1).Trim() : string.Empty;

                                break;
                            }
                        }

                        if (addExpense)
                        {
                            IncomeExpenseModel objIncomeExpense = new IncomeExpenseModel()
                            {
                                Amount = amount,
                                TransactionType = "Expense",
                                Date = date,
                                CategoryName = category,
                                Remarks = remarks,
                                Mode = "file_upload"
                            };
                            listIncomeExpenseModel.Add(objIncomeExpense);
                        }
                    }
                }
            }
            //add expense into database
            if (listIncomeExpenseModel.Count > 0)
            {
                rowsAdded = rowsAdded + await _dbConnection.InsertAllAsync(listIncomeExpenseModel, true);
                activityIndicator.IsRunning = false;
            }
            //add expense into database
            if (rowsAdded > 0)
            {
                await DisplayAlert("Info", $"File Processed Successfully\n\n{rowsAdded.ToString()} records added.", "OK");
                activityIndicator.IsRunning = false;
                await ShowCurrentMonthExpenses();
                LoadExpensesInPage("Last5");
            }
            else
            {
                await DisplayAlert("Info", "No records were added.", "OK");
            }
            activityIndicator.IsRunning = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Alert - StackTrace", $"{ex.Message.ToString()}\n\nNo records added.", "OK");
        }

    }

    private void pickerExpenseCategory_SelectedIndexChanged(object sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
    {
        if (!string.IsNullOrEmpty(pickerExpenseCategory.Text))
        {
            if (pickerExpenseCategory.Text == Constants.AddNewCategoryOption)
            {
                Navigation.PushAsync(new ManageCategoriesPage());
            }
        }
    }

    private async void OnCardTapped(object sender, EventArgs e)
    {
        if ((sender as Border)?.BindingContext is IncomeExpenseDTO tappedItem)
        {
            // Populate your fields
            txtTransactionId.Text = tappedItem.TransactionId.ToString();
            entryExpenseAmount.Text = tappedItem.Amount.ToString();
            pickerExpenseCategory.Text = tappedItem.CategoryName;
            dpDateExpense.Date = tappedItem.Date;
            entryExpenseRemarks.Text = string.IsNullOrEmpty(tappedItem.Remarks) ? string.Empty : tappedItem.Remarks.Trim();

            // Highlight logic using DTO property
            if (expenseCollectionView.ItemsSource is IEnumerable<IncomeExpenseDTO> items)
            {
                foreach (var item in items)
                    item.IsSelected = false; // reset all
            }

            tappedItem.IsSelected = true; // select tapped item

            // Scroll logic stays the same
            var visibleRect = new Rect(
                expenseScrollView.ScrollX,
                expenseScrollView.ScrollY,
                expenseScrollView.Width,
                expenseScrollView.Height);

            // Get position of target element
            var targetRect = expanderFilterDetails.Bounds;

            // Check if the target is within the visible area
            bool isVisible =
                targetRect.Y >= visibleRect.Y &&
                targetRect.Y <= visibleRect.Y + visibleRect.Height;

            // Scroll only if not visible
            if (!isVisible)
            {
                await expenseScrollView.ScrollToAsync(expanderFilterDetails, ScrollToPosition.Start, true);
            }


            //await expenseScrollView.ScrollToAsync(expanderFilterDetails, ScrollToPosition.Start, true);
        }
    }
}