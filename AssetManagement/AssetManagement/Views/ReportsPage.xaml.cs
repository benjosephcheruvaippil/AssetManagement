using AssetManagement.Models;
using AssetManagement.Models.Reports;
using SQLite;
using System.Globalization;
//using static Android.Content.ClipData;

namespace AssetManagement.Views;

public partial class ReportsPage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    public ReportsPage(SQLiteAsyncConnection dbConnection)
	{
		InitializeComponent();
        _dbConnection = dbConnection;
        LoadIncomeExpenseReport(DateTime.Now.Year.ToString());

    }

    private async Task SetUpDb()
    {
        if (_dbConnection == null)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
            _dbConnection = new SQLiteAsyncConnection(dbPath);
        }
    }

    private async void LoadIncomeExpenseReport(string selectedYear)
    {
        tblscIncomeExpenseReport.Clear();  
        await SetUpDb();

        PopulateTableSection(selectedYear);
        SummaryByCategory(selectedYear);
    }

    public async void PopulateTableSection(string selectedYear)
    {
        List<IncomeExpenseReport> objReportList = new List<IncomeExpenseReport>();

        tblscIncomeExpenseReport.Title = "Income & Expense - Year " + selectedYear;

        decimal yearlyIncome = 0, yearlyExpense = 0, yearlyBalance = 0;

        for (int i = 1; i <= 12; i++)
        {
            string currentMonth = DateTimeFormatInfo.CurrentInfo.GetMonthName(i);
            DateTime startOfMonth = new DateTime(Convert.ToInt32(selectedYear), i, 1, 0, 0, 0); //24 hour format
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            endOfMonth = new DateTime(endOfMonth.Year, endOfMonth.Month, endOfMonth.Day, 23, 59, 59); //24 hour format

            IncomeExpenseReport objReport = new IncomeExpenseReport();

            var expenses = await _dbConnection.Table<IncomeExpenseModel>()
                .Where(e => e.TransactionType == "Expense" && e.Date >= startOfMonth && e.Date <= endOfMonth)
                .ToListAsync();
            decimal expenseAmount = (decimal)expenses.Sum(s => s.Amount);

            var income = await _dbConnection.Table<IncomeExpenseModel>()
                .Where(e => e.TransactionType == "Income" && e.Date >= startOfMonth && e.Date <= endOfMonth)
                .ToListAsync();
            decimal incomeAmount = (decimal)income.Sum(s => s.Amount);

            decimal balance = incomeAmount - expenseAmount;
            yearlyIncome = yearlyIncome + incomeAmount;
            yearlyExpense = yearlyExpense + expenseAmount;
            yearlyBalance = yearlyBalance + balance;

            objReport.Month = currentMonth;
            objReport.ExpenseAmount = expenseAmount;
            objReport.IncomeAmount = incomeAmount;
            objReport.BalanceAmount = balance;
            objReportList.Add(objReport);
        }

        foreach (var item in objReportList)
        {
            TextCell objCell = new TextCell();
            objCell.Text = item.Month;
            objCell.Detail = "Expense: " + item.ExpenseAmount + " | " + "Income: " + item.IncomeAmount + " | " + "Balance: " + item.BalanceAmount;
            objCell.Height = 40;
            tblscIncomeExpenseReport.Add(objCell);
        }

        TextCell objCellYearly = new TextCell();
        objCellYearly.Text = selectedYear;
        objCellYearly.TextColor = Colors.Brown;
        objCellYearly.Detail = "Expense: " + yearlyExpense + " | " + "Income: " + yearlyIncome + " | " + "Balance: " + yearlyBalance;
        objCellYearly.Height = 40;
        tblscIncomeExpenseReport.Add(objCellYearly);
    }

    private async void yearPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        string selectedYear = yearPicker.SelectedItem.ToString();
        LoadIncomeExpenseReport(selectedYear);
        SummaryByCategory(selectedYear);
    }

    public async void SummaryByCategory(string selectedYear)
    {
        await SetUpDb();
        string query = "select CategoryName,Sum(Amount) as Amount from IncomeExpenseModel Group by CategoryName where strftime('%Y',CAST(Date As TEXT))=" + "'" + selectedYear + "'";
        var categories = await _dbConnection.QueryAsync<IncomeExpenseModel>(query);
    }
}