using Android.Content;
using Android.SE.Omapi;
using AssetManagement.Models;
using AssetManagement.Models.Reports;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using CommunityToolkit.Maui.Storage;
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
        dpFromDateIncomeReport.Format = "dd-MM-yyyy";
        dpTODateIncomeReport.Format = "dd-MM-yyyy";
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

    private async void btnGenerateIncomeReport_Clicked(object sender, EventArgs e)
    {
        try
        {
            activityIndicator.IsRunning = true;        
            DateTime fromDateIncomeReport = dpFromDateIncomeReport.Date;
            DateTime toDateIncomeReport = dpTODateIncomeReport.Date;
            DateTime fromDate = new DateTime(fromDateIncomeReport.Year, fromDateIncomeReport.Month, fromDateIncomeReport.Day, 0, 0, 0);
            DateTime toDate = new DateTime(toDateIncomeReport.Year, toDateIncomeReport.Month, toDateIncomeReport.Day, 23, 59, 59);
            var incomeList = await _dbConnection.Table<IncomeExpenseModel>().Where(i => i.TransactionType == "Income" && (i.Date >= fromDate && i.Date <= toDate))
                .OrderBy(i=>i.Date)
                .ToListAsync();

            // Creating an instance
            // of ExcelPackage
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage excel = new ExcelPackage();

            // name of the sheet
            var workSheet = excel.Workbook.Worksheets.Add("Income Report");

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

            // Header of the Excel sheet
            workSheet.Cells[1, 1].Value = "Transaction Type";
            workSheet.Cells[1, 2].Value = "Date";
            workSheet.Cells[1, 3].Value = "Category Name";            
            workSheet.Cells[1, 4].Value = "Owner Name";
            workSheet.Cells[1, 5].Value = "Tax Cut";
            workSheet.Cells[1, 6].Value = "Amount";
            workSheet.Cells[1, 7].Value = "Remarks";

            int recordIndex = 2;
            decimal totalIncome = 0,totalTaxCut=0;
            foreach (var income in incomeList)
            {
                workSheet.Cells[recordIndex, 1].Value = income.TransactionType;
                workSheet.Cells[recordIndex, 2].Value = income.Date.ToString("dd-MM-yyyy");
                workSheet.Cells[recordIndex, 3].Value = income.CategoryName;
                workSheet.Cells[recordIndex, 4].Value = income.OwnerName;
                workSheet.Cells[recordIndex, 5].Value = income.TaxAmountCut;
                workSheet.Cells[recordIndex, 6].Value = income.Amount;
                workSheet.Cells[recordIndex, 7].Value = income.Remarks;
                workSheet.Row(recordIndex).Height = 16;
                totalTaxCut = totalTaxCut + Convert.ToDecimal(income.TaxAmountCut);
                totalIncome = totalIncome + Convert.ToDecimal(income.Amount);
                recordIndex++;
            }

            recordIndex++;
            //workSheet.Cells[recordIndex, 3].Value = "Total Income";
            //workSheet.Cells[recordIndex, 3].Style.Font.Bold = true;
            workSheet.Cells[recordIndex, 5].Value = totalTaxCut;
            workSheet.Cells[recordIndex, 5].Style.Font.Bold = true;
            workSheet.Cells[recordIndex, 6].Value = totalIncome;
            workSheet.Cells[recordIndex, 6].Style.Font.Bold = true;
            

            workSheet.Column(1).AutoFit();
            workSheet.Column(2).AutoFit();
            workSheet.Column(3).AutoFit();
            workSheet.Column(4).AutoFit();
            workSheet.Column(5).AutoFit();
            workSheet.Column(6).AutoFit();
            workSheet.Column(7).AutoFit();

            //Context currentContext = Android.App.Application.Context;
            //string directory = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDocuments);
            //if (!Directory.Exists(directory))
            //{
            //    Directory.CreateDirectory(directory);
            //}
            //string file = Path.Combine(directory, "Income_Report.xlsx");
            //System.IO.File.WriteAllBytes(file, excel.GetAsByteArray());


            var stream=new MemoryStream(excel.GetAsByteArray());
            CancellationTokenSource Ctoken = new CancellationTokenSource();
            var fileSaverResult = await FileSaver.Default.SaveAsync("Income_Report.xlsx", stream, Ctoken.Token);
            if(fileSaverResult.IsSuccessful)
            {
                await DisplayAlert("Message", "Excel saved in " + fileSaverResult.FilePath, "Ok");
            }

            excel.Dispose();
            activityIndicator.IsRunning = false;            
        }
        catch(Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Ok");
            await DisplayAlert("Error", ex.InnerException.ToString(), "Ok");            
        }
    }
}