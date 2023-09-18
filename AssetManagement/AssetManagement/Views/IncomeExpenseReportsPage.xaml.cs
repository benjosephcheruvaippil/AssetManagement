using AssetManagement.Models;
using AssetManagement.Models.Reports;
using SQLite;
using System.Globalization;
using System.Linq;
//using static Android.Content.ClipData;

namespace AssetManagement.Views;

public partial class IncomeExpenseReportsPage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    private bool onLoad = false;
    public IncomeExpenseReportsPage()
    {
        InitializeComponent();
        //_dbConnection = dbConnection;
        onLoad = true;
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
        yearPicker.SelectedItem = selectedYear;
        tblscIncomeExpenseReport.Clear();
        tblscCategoryWiseReport.Clear();
        await SetUpDb();

        PopulateTableSection(selectedYear);
        SummaryByCategory(selectedYear);
        onLoad = false;
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
            objReport.ExpenseAmount = expenseAmount.ToString("#,#.##", new CultureInfo(0x0439));
            objReport.IncomeAmount = incomeAmount.ToString("#,#.##", new CultureInfo(0x0439));
            objReport.BalanceAmount = balance.ToString("#,#.##", new CultureInfo(0x0439));
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
        objCellYearly.Text = "Summary - " + selectedYear;
        objCellYearly.TextColor = Colors.Brown;
        objCellYearly.Detail = "Expense: " + yearlyExpense.ToString("#,#.##", new CultureInfo(0x0439)) + " | " + "Income: " + yearlyIncome.ToString("#,#.##", new CultureInfo(0x0439)) + " | " + "Balance: " + yearlyBalance.ToString("#,#.##", new CultureInfo(0x0439));
        objCellYearly.Height = 40;
        tblscIncomeExpenseReport.Add(objCellYearly);
    }

    private async void yearPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (onLoad == false)
        {
            string selectedYear = yearPicker.SelectedItem.ToString();
            LoadIncomeExpenseReport(selectedYear);
        }
    }

    public async void SummaryByCategory(string selectedYear)
    {
        DateTime yearBegin = new DateTime(Convert.ToInt32(selectedYear), 1, 1, 0, 0, 0);
        DateTime yearEnd = new DateTime(Convert.ToInt32(selectedYear), 12, 31, 23, 59, 59);
        var categories = await _dbConnection.QueryAsync<IncomeExpenseModel>("select CategoryName from IncomeExpenseModel Group By CategoryName");
        var records = await _dbConnection.Table<IncomeExpenseModel>().Where(d => d.Date >= yearBegin && d.Date <= yearEnd).ToListAsync();

        double total = 0;
        foreach (var category in categories)
        {
            total = 0;
            if (!string.IsNullOrEmpty(category.CategoryName))
            {
                foreach (var item in records)
                {
                    if (item.CategoryName == category.CategoryName)
                    {
                        total = total + item.Amount;
                    }
                }
            }
            //new code
            if (string.IsNullOrEmpty(category.CategoryName))
            {
                foreach (var item in records)
                {
                    if (string.IsNullOrEmpty(item.CategoryName))
                    {
                        total = total + item.Amount;
                    }
                }
                category.CategoryName = "Uncategorized Expense";
            }
            //new code
            TextCell objCategory = new TextCell();
            objCategory.Text = category.CategoryName;
            objCategory.TextColor = Colors.DarkBlue;
            objCategory.Detail = string.Format(new CultureInfo("en-IN"), "{0:C0}", total);
            objCategory.Height = 40;
            tblscCategoryWiseReport.Add(objCategory);
        }

        tblscCategoryWiseReport.Title = "Category Wise Report " + selectedYear;


    }
}