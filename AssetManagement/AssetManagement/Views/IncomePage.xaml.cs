using AssetManagement.Models;
using SQLite;
using System.Globalization;

namespace AssetManagement.Views;

public partial class IncomePage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    public IncomePage()
    {
        InitializeComponent();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        LoadIncomeInPage();
        await ShowCurrentMonthIncome();
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

    public async Task ShowCurrentMonthIncome()
    {
        DateTime currentDate = DateTime.Now;
        string currentMonth = currentDate.ToString("MMMM");
        DateTime startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
        int lastDayOfMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
        DateTime endOfMonth = new DateTime(currentDate.Year, currentDate.Month, lastDayOfMonth, 23, 59, 59);

        await SetUpDb();
        var query = await _dbConnection.Table<IncomeExpenseModel>().Where(d => d.TransactionType == "Income" && d.Date >= startOfMonth && d.Date <= endOfMonth).ToListAsync();
        var totalIncome = query.Sum(s => s.Amount);
        lblCurrentMonthIncome.Text = currentMonth + ": " + string.Format(new CultureInfo("en-IN"), "{0:C0}", totalIncome);
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
        int transactionId = Convert.ToInt32(textCell[2].Trim());
        int year = Convert.ToInt32(date.Split("-")[2]);
        int month = Convert.ToInt32(date.Split("-")[1]);
        int day = Convert.ToInt32(date.Split("-")[0]);

        //call database to get corresponding Id details
        var incomeDetail = _dbConnection.Table<IncomeExpenseModel>().Where(i => i.TransactionId == transactionId).FirstOrDefaultAsync();
        //call database to get corresponding Id details

        pickerIncomeCategory.SelectedItem = textCell[0].Trim();
        pickerOwnerName.SelectedItem = incomeDetail.Result.OwnerName;
        entryTaxAmount.Text = Convert.ToString(incomeDetail.Result.TaxAmountCut);      

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
                TaxAmountCut=Convert.ToDouble(entryTaxAmount.Text),
                TransactionType = "Income",
                Date = dpDateIncome.Date != DateTime.Now.Date ? dpDateIncome.Date : DateTime.Now,
                CategoryName = Convert.ToString(pickerIncomeCategory.SelectedItem),
                OwnerName=Convert.ToString(pickerOwnerName.SelectedItem),
                Remarks = entryIncomeRemarks.Text
            };
            await SetUpDb();
            int rowsAffected = await _dbConnection.InsertAsync(objIncomeExpense);
            entryIncomeAmount.Text = "";
            entryTaxAmount.Text = "";
            entryIncomeRemarks.Text = "";
            if (rowsAffected > 0)
            {
                LoadIncomeInPage();
                await ShowCurrentMonthIncome();
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
                TaxAmountCut = Convert.ToDouble(entryTaxAmount.Text),
                TransactionType = "Income",
                Date = dpDateIncome.Date != DateTime.Now.Date ? dpDateIncome.Date : DateTime.Now,
                CategoryName = Convert.ToString(pickerIncomeCategory.SelectedItem),
                OwnerName = Convert.ToString(pickerOwnerName.SelectedItem),
                Remarks = entryIncomeRemarks.Text
            };
            await SetUpDb();
            int rowsAffected = await _dbConnection.UpdateAsync(objIncomeExpense);
            entryIncomeAmount.Text = "";
            entryTaxAmount.Text = "";
            entryIncomeRemarks.Text = "";
            txtIncomeTransactionId.Text = "";

            if (rowsAffected > 0)
            {
                LoadIncomeInPage();
                await ShowCurrentMonthIncome();
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
                await ShowCurrentMonthIncome();
            }
        }
    }
}