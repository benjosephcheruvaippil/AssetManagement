using AssetManagement.Models;
using AssetManagement.Models.Constants;
using AssetManagement.Models.FirestoreModel;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
using SQLite;
using System.Globalization;

namespace AssetManagement.Views;

public partial class ExpensePage : ContentPage
{
    //private AssetListPageViewModel _viewModel;
    private SQLiteAsyncConnection _dbConnection;
    //private IPopupNavigation _popupNavigation;
    //private readonly IAssetService _assetService;
    public ExpensePage()
    {
        InitializeComponent();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        LoadExpensesInPage();// show expenses in the expense tab
        await ShowCurrentMonthExpenses();
        SetLastUploadedDate();
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
        DateTime startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1, 0, 0, 0);
        int lastDayOfMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
        DateTime endOfMonth = new DateTime(currentDate.Year, currentDate.Month, lastDayOfMonth, 23, 59, 59);

        await SetUpDb();
        var query = await _dbConnection.Table<IncomeExpenseModel>().Where(d => d.TransactionType == "Expense" && d.Date >= startOfMonth && d.Date <= endOfMonth).ToListAsync();
        var totalExpense = query.Sum(s => s.Amount);
        lblCurrentMonthExpenses.Text = currentMonth + ": " + string.Format(new CultureInfo("en-IN"), "{0:C0}", totalExpense);
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
        bool userResponse = await DisplayAlert("Message", "Are you sure to upload data to firestore DB?", "Yes", "No");
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
            incomeExpense.TaxAmountCut = trans.TaxAmountCut;
            incomeExpense.OwnerName = trans.OwnerName;
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
                model.TaxAmountCut=item.TaxAmountCut;
                model.TransactionType = item.TransactionType;
                if (DateTime.TryParse(item.Date, out DateTime result))
                {
                    model.Date = Convert.ToDateTime(item.Date);
                }
                else
                {
                    model.Date = DateTime.Now;
                }

                model.OwnerName=item.OwnerName;
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
}