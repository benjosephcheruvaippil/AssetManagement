using AssetManagement.Models;
using AssetManagement.Models.Reports;
using SQLite;
using System.Globalization;

namespace AssetManagement.Views;

public partial class ReportsPage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    public ReportsPage(SQLiteAsyncConnection dbConnection)
	{
		InitializeComponent();
        _dbConnection = dbConnection;
        LoadIncomeExpenseReport();

    }

    private async Task SetUpDb()
    {
        if (_dbConnection == null)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
            _dbConnection = new SQLiteAsyncConnection(dbPath);
        }
    }

    private async void LoadIncomeExpenseReport()
    {
        tblscIncomeExpenseReport.Clear();  
        await SetUpDb();
        List<IncomeExpenseReport> objReportList = new List<IncomeExpenseReport>();
        DateTime currentDate = DateTime.Now;
        tblscIncomeExpenseReport.Title = "Income & Expense - Year " + currentDate.Year;

        for (int i = 1; i <= currentDate.Month; i++)
        {
            string currentMonth = DateTimeFormatInfo.CurrentInfo.GetMonthName(i);
            DateTime startOfMonth = new DateTime(currentDate.Year, i, 1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

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
    }
}