﻿using Android.Content;
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
using AssetManagement.Models.Constants;
using AssetManagement.Models.DataTransferObject;
using AssetManagement.ViewModels;
using System.Collections.ObjectModel;
using System;
//using static Android.Content.ClipData;

namespace AssetManagement.Views;

public partial class IncomeExpenseReportsPage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    AppTheme currentTheme = Application.Current.RequestedTheme;
    decimal yearlyIncome = 0, yearlyExpense = 0, yearlyBalance = 0;
    private bool onLoad = false;
    public ObservableCollection<SelectableItem> Items { get; set; }
    public IncomeExpenseReportsPage()
    {
        try
        {
            InitializeComponent();
            //_dbConnection = dbConnection;
            onLoad = true;
            LoadIncomeExpenseReport(DateTime.Now.Year.ToString(), false);
            dpFromDateIncomeReport.Format = "dd-MM-yyyy";
            dpTODateIncomeReport.Format = "dd-MM-yyyy";
            LoadOwnersInDropdown();
        }
        catch (Exception ex)
        {
            DisplayAlert("Something went wrong: ", ex.Message, "Ok");
        }
    }

    private async Task SetUpDb()
    {
        if (_dbConnection == null)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
            _dbConnection = new SQLiteAsyncConnection(dbPath);
        }
    }

    private async void LoadOwnersInDropdown()
    {
        try
        {
            await SetUpDb();
            var owners = await _dbConnection.Table<Owners>().ToListAsync();
            //Owners objOwner = new Owners
            //{
            //    OwnerId = 0,
            //    OwnerName = Constants.AddNewOwnerOption
            //};
            //owners.Add(objOwner);
            //pickerOwnerName.ItemsSource = owners.Select(s => s.OwnerName).ToList();

            Items = new ObservableCollection<SelectableItem>(owners.Select(name => new SelectableItem { Name = name.OwnerName }));

            BindingContext = this;
        }
        catch (Exception)
        {
            return;
        }
    }

    private async void LoadIncomeExpenseReport(string selectedYear, bool oneTimeExpense)
    {
        try
        {
            yearPicker.SelectedItem = selectedYear;
            tblscIncomeExpenseReport.Clear();
            tblscCategoryWiseReport.Clear();
            await SetUpDb();

            PopulateTableSection(selectedYear, oneTimeExpense);
            SummaryByCategory(selectedYear);
            onLoad = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Something went wrong: ", ex.Message, "Ok");
        }
    }

    public async void PopulateTableSection(string selectedYear, bool oneTimeExpense)
    {
        try
        {
            List<IncomeExpenseReport> objReportList = new List<IncomeExpenseReport>();

            tblscIncomeExpenseReport.Title = "Income & Expense - Year " + selectedYear;

            yearlyIncome = 0;
            yearlyExpense = 0;
            yearlyBalance = 0;

            for (int i = 1; i <= 12; i++)
            {
                string currentMonth = DateTimeFormatInfo.CurrentInfo.GetMonthName(i);
                DateTime startOfMonth = new DateTime(Convert.ToInt32(selectedYear), i, 1, 0, 0, 0); //24 hour format
                DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                endOfMonth = new DateTime(endOfMonth.Year, endOfMonth.Month, endOfMonth.Day, 23, 59, 59); //24 hour format

                IncomeExpenseReport objReport = new IncomeExpenseReport();

                //var expensesOld = await _dbConnection.Table<IncomeExpenseModel>()
                //    .Where(e => e.TransactionType == "Expense" && e.Date >= startOfMonth && e.Date <= endOfMonth)
                //    .ToListAsync();
                var parameters = new object[] { startOfMonth, endOfMonth };

                List<IncomeExpenseModel> expenses = new List<IncomeExpenseModel>();
                if (oneTimeExpense)
                {
                    expenses = await _dbConnection.QueryAsync<IncomeExpenseModel>("select Amount from IncomeExpenseModel iem LEFT JOIN " +
                     "IncomeExpenseCategories iec ON iem.CategoryName = iec.CategoryName Where iem.TransactionType='Expense'" +
                     "and iem.Date >= ? and iem.Date<= ? ", parameters);
                }
                else
                {
                    expenses = await _dbConnection.QueryAsync<IncomeExpenseModel>("select Amount from IncomeExpenseModel iem LEFT JOIN " +
                    "IncomeExpenseCategories iec ON iem.CategoryName = iec.CategoryName Where iem.TransactionType='Expense' and (iec.IsOneTimeExpense=0 or iec.IsOneTimeExpense is null) " +
                    "and iem.Date >= ? and iem.Date<= ? ", parameters);
                }

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
                if (currentTheme == AppTheme.Dark)
                {
                    //set to white color
                    objCell.TextColor = Color.FromArgb("#FFFFFF");
                }
                objCell.Detail = "Expense: " + item.ExpenseAmount + " | " + "Income: " + item.IncomeAmount + " | " + "Balance: " + item.BalanceAmount;
                objCell.Height = 40;
                tblscIncomeExpenseReport.Add(objCell);

                objCell.Tapped += ObjCell_Tapped;
            }

            TextCell objCellYearly = new TextCell();
            objCellYearly.Text = "Summary - " + selectedYear;
            objCellYearly.TextColor = Colors.Brown;
            objCellYearly.Detail = "Expense: " + yearlyExpense.ToString("#,#.##", new CultureInfo(0x0439)) + " | " + "Income: " + yearlyIncome.ToString("#,#.##", new CultureInfo(0x0439)) + " | " + "Balance: " + yearlyBalance.ToString("#,#.##", new CultureInfo(0x0439));
            objCellYearly.Height = 40;
            tblscIncomeExpenseReport.Add(objCellYearly);
            objCellYearly.Tapped += ObjCell_Tapped;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Something went wrong: ", ex.Message, "Ok");
        }
    }

    private async void ObjCell_Tapped(object sender, EventArgs e)
    {
        bool oneTimeExpense = chkOnTimeExpense.IsChecked;
        var tappedViewCell = (TextCell)sender;
        string displayText = "";
        string month = tappedViewCell.Text.ToString();
        int monthInteger = 0;
        switch (month)
        {
            case "January":
                monthInteger = 1;
                break;
            case "February":
                monthInteger = 2;
                break;
            case "March":
                monthInteger = 3;
                break;
            case "April":
                monthInteger = 4;
                break;
            case "May":
                monthInteger = 5;
                break;
            case "June":
                monthInteger = 6;
                break;
            case "July":
                monthInteger = 7;
                break;
            case "August":
                monthInteger = 8;
                break;
            case "September":
                monthInteger = 9;
                break;
            case "October":
                monthInteger = 10;
                break;
            case "November":
                monthInteger = 11;
                break;
            case "December":
                monthInteger = 12;
                break;
            default:
                monthInteger = 0;
                break;
        }

        if (monthInteger == 0)
        {
            displayText = $"Total Expense: {string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", yearlyExpense)}" +
          $"\nTotal Income: {string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", yearlyIncome)}" +
          $"\nTotal Balance: {string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", yearlyBalance)}";
            await DisplayAlert(month, displayText, "Ok");
            return;
        }

        string year = yearPicker.SelectedItem.ToString();
        DateTime startOfMonth = new DateTime(Convert.ToInt32(year), monthInteger, 1, 0, 0, 0); //24 hour format
        DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        endOfMonth = new DateTime(endOfMonth.Year, endOfMonth.Month, endOfMonth.Day, 23, 59, 59); //24 hour format

        List<IncomeExpenseModel> manualAddedExpenseList = new List<IncomeExpenseModel>();
        List<IncomeExpenseModel> fileUploadExpenseList = new List<IncomeExpenseModel>();
        var parameters = new object[] { startOfMonth, endOfMonth };
        if (oneTimeExpense)
        {
            manualAddedExpenseList = await _dbConnection.QueryAsync<IncomeExpenseModel>("select Amount from IncomeExpenseModel iem LEFT JOIN " +
             "IncomeExpenseCategories iec ON iem.CategoryName = iec.CategoryName Where iem.TransactionType='Expense' and (Mode is null or Mode = '' or Mode='manual') " +
             "and iem.Date >= ? and iem.Date<= ? ", parameters);
        }
        else
        {
            manualAddedExpenseList = await _dbConnection.QueryAsync<IncomeExpenseModel>("select Amount from IncomeExpenseModel iem LEFT JOIN " +
            "IncomeExpenseCategories iec ON iem.CategoryName = iec.CategoryName Where iem.TransactionType='Expense' and (Mode is null or Mode = '' or Mode='manual') and (iec.IsOneTimeExpense=0 or iec.IsOneTimeExpense is null) " +
            "and iem.Date >= ? and iem.Date<= ? ", parameters);
        }
        //var manualAddedExpenseList = await _dbConnection.Table<IncomeExpenseModel>().Where(e => e.TransactionType == "Expense" && (e.Mode == "" || e.Mode == null || e.Mode=="manual") && e.Date >= startOfMonth && e.Date <= endOfMonth).ToListAsync();      
        var totalManualAddedExpenses = manualAddedExpenseList.Select(s => s.Amount).Sum();

        if (oneTimeExpense)
        {
            fileUploadExpenseList = await _dbConnection.QueryAsync<IncomeExpenseModel>("select Amount from IncomeExpenseModel iem LEFT JOIN " +
            "IncomeExpenseCategories iec ON iem.CategoryName = iec.CategoryName Where iem.TransactionType='Expense' and  Mode='file_upload' " +
            "and iem.Date >= ? and iem.Date<= ? ", parameters);
        }
        else
        {
            fileUploadExpenseList = await _dbConnection.QueryAsync<IncomeExpenseModel>("select Amount from IncomeExpenseModel iem LEFT JOIN " +
           "IncomeExpenseCategories iec ON iem.CategoryName = iec.CategoryName Where iem.TransactionType='Expense' and  Mode='file_upload' and (iec.IsOneTimeExpense=0 or iec.IsOneTimeExpense is null) " +
           "and iem.Date >= ? and iem.Date<= ? ", parameters);
        }
        //var fileUploadExpenseList = await _dbConnection.Table<IncomeExpenseModel>().Where(e => e.TransactionType == "Expense" && e.Mode == "file_upload" && e.Date >= startOfMonth && e.Date <= endOfMonth).ToListAsync();
        var totalFileUploadExpenses = fileUploadExpenseList.Select(s => s.Amount).Sum();

        var monthlyIncomeList = await _dbConnection.Table<IncomeExpenseModel>().Where(e => e.TransactionType == "Income" && e.Date >= startOfMonth && e.Date <= endOfMonth).ToListAsync();
        var totalMonthlyIncome = monthlyIncomeList.Select(s => s.Amount).Sum();

        double totalMonthlySavings = totalMonthlyIncome - (totalManualAddedExpenses + totalFileUploadExpenses);

        displayText = $"Expense Manually Added: {string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", totalManualAddedExpenses)}\nExpense File Upload: {string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", totalFileUploadExpenses)}" +
           $"\n────────────────────" +
           $"\nTotal Expense: {string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", totalManualAddedExpenses + totalFileUploadExpenses)}" +
           $"\nTotal Income: {string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", totalMonthlyIncome)}" +
           $"\nTotal Balance: {string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", totalMonthlySavings)}";
        await DisplayAlert(month, displayText, "Ok");
    }

    private async void yearPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (onLoad == false)
        {
            string selectedYear = yearPicker.SelectedItem.ToString();
            bool oneTimeExpense = chkOnTimeExpense.IsChecked;
            LoadIncomeExpenseReport(selectedYear, oneTimeExpense);
        }
    }

    private void chkOnTimeExpense_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (onLoad == false)
        {
            string selectedYear = yearPicker.SelectedItem.ToString();
            bool oneTimeExpense = chkOnTimeExpense.IsChecked;
            LoadIncomeExpenseReport(selectedYear, oneTimeExpense);
        }
    }

    public async void SummaryByCategory(string selectedYear)
    {
        List<CategoryWiseAmountDTO> categoryWiseAmountList = new List<CategoryWiseAmountDTO>();
        DateTime yearBegin = new DateTime(Convert.ToInt32(selectedYear), 1, 1, 0, 0, 0);
        DateTime yearEnd = new DateTime(Convert.ToInt32(selectedYear), 12, 31, 23, 59, 59);
        var categories = await _dbConnection.QueryAsync<IncomeExpenseModel>("select CategoryName,TransactionType from IncomeExpenseModel Group By CategoryName,TransactionType");
        var records = await _dbConnection.Table<IncomeExpenseModel>().Where(d => d.Date >= yearBegin && d.Date <= yearEnd).ToListAsync();

        double total = 0;
        foreach (var category in categories)
        {
            CategoryWiseAmountDTO objCategoryWiseAmount = new CategoryWiseAmountDTO();
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

            if (total > 0)
            {
                objCategoryWiseAmount.CategoryName = category.CategoryName;
                var incomeExpenseCategory = await _dbConnection.Table<IncomeExpenseCategories>().Where(c => c.CategoryName == category.CategoryName).FirstOrDefaultAsync();
                objCategoryWiseAmount.OneTimeExpense = incomeExpenseCategory != null ? incomeExpenseCategory.IsOneTimeExpense : false;
                objCategoryWiseAmount.TransactionType = category.TransactionType;
                objCategoryWiseAmount.Amount = total;
                categoryWiseAmountList.Add(objCategoryWiseAmount);
            }
        }

        if (categoryWiseAmountList.Count > 0)
        {
            categoryWiseAmountList = categoryWiseAmountList.OrderBy(o => o.TransactionType).ThenByDescending(o => o.Amount).ToList();
            if (!chkOnTimeExpense.IsChecked)
            {
                List<CategoryWiseAmountDTO> itemsToRemove = new List<CategoryWiseAmountDTO>();
                itemsToRemove = categoryWiseAmountList.Where(c => c.OneTimeExpense == true).ToList();
                categoryWiseAmountList.RemoveAll(item => itemsToRemove.Contains(item));
            }
            foreach (var item in categoryWiseAmountList)
            {
                string oneTimeExpenseText = item.OneTimeExpense == null ? "" : ((bool)item.OneTimeExpense ? "(One Time)" : "");
                TextCell objCategory = new TextCell();
                objCategory.Text = item.CategoryName + " - " + oneTimeExpenseText + item.TransactionType;
                if (currentTheme == AppTheme.Dark)
                {
                    //set to white color
                    objCategory.TextColor = Color.FromArgb("#FFFFFF");
                }
                objCategory.Detail = string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", item.Amount);
                objCategory.Height = 40;
                tblscCategoryWiseReport.Add(objCategory);
            }
        }

        tblscCategoryWiseReport.Title = "Category Wise Report " + selectedYear;
    }


    private async void btnGenerateIncomeReport_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (typePicker.SelectedIndex == -1)
            {
                await DisplayAlert("Message", "Please select report type.", "Ok");
                return;
            }
            activityIndicator.IsRunning = true;
            DateTime fromDateIncomeReport = dpFromDateIncomeReport.Date;
            DateTime toDateIncomeReport = dpTODateIncomeReport.Date;
            DateTime fromDate = new DateTime(fromDateIncomeReport.Year, fromDateIncomeReport.Month, fromDateIncomeReport.Day, 0, 0, 0);
            DateTime toDate = new DateTime(toDateIncomeReport.Year, toDateIncomeReport.Month, toDateIncomeReport.Day, 23, 59, 59);
            List<IncomeExpenseModel> inexpList = new List<IncomeExpenseModel>();
            string type = typePicker.SelectedItem.ToString();
            string categoryName = pickerCategory.SelectedItem.ToString();

            var selectedOwners = Items.Where(i => i.IsSelected).Select(i => i.Name).ToList();

            if (type == "Income")
            {
                if (selectedOwners.Count > 0)
                {
                    inexpList = await _dbConnection.Table<IncomeExpenseModel>().Where(i => i.TransactionType == "Income"
                && (categoryName == "All" || i.CategoryName == categoryName)
                && (i.Date >= fromDate && i.Date <= toDate)
                && selectedOwners.Contains(i.OwnerName))
                    .OrderBy(i => i.Date)
                    .ToListAsync();
                }
                else
                {
                    inexpList = await _dbConnection.Table<IncomeExpenseModel>().Where(i => i.TransactionType == "Income"
                && (categoryName == "All" || i.CategoryName == categoryName)
                && (i.Date >= fromDate && i.Date <= toDate))
                    .OrderBy(i => i.Date)
                    .ToListAsync();
                }
            }
            else if (type == "Expense")
            {
                if (selectedOwners.Count > 0)
                {
                    inexpList = await _dbConnection.Table<IncomeExpenseModel>().Where(i => i.TransactionType == "Expense"
                    && (categoryName == "All" || i.CategoryName == categoryName)
                    && (i.Date >= fromDate && i.Date <= toDate)
                    && selectedOwners.Contains(i.OwnerName))
                        .OrderBy(i => i.Date)
                        .ToListAsync();
                }
                else
                {
                    inexpList = await _dbConnection.Table<IncomeExpenseModel>().Where(i => i.TransactionType == "Expense"
                    && (categoryName == "All" || i.CategoryName == categoryName)
                    && (i.Date >= fromDate && i.Date <= toDate))
                        .OrderBy(i => i.Date)
                        .ToListAsync();
                }   
            }

            // Creating an instance
            // of ExcelPackage
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage excel = new ExcelPackage();

            // name of the sheet
            var workSheet = excel.Workbook.Worksheets.Add("Report");

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

            // Header of the Excel sheet
            workSheet.Cells[1, 1].Value = "Transaction Type";
            workSheet.Cells[1, 2].Value = "Date";
            workSheet.Cells[1, 3].Value = "Category Name";
            workSheet.Cells[1, 4].Value = "Owner Name";
            workSheet.Cells[1, 5].Value = "Tax Cut";
            workSheet.Cells[1, 6].Value = "Amount";
            workSheet.Cells[1, 7].Value = "Mode";
            workSheet.Cells[1, 8].Value = "Remarks";

            int recordIndex = 2;
            decimal totalIncome = 0, totalTaxCut = 0;
            foreach (var income in inexpList)
            {
                workSheet.Cells[recordIndex, 1].Value = income.TransactionType;
                workSheet.Cells[recordIndex, 2].Value = income.Date.ToString("dd-MM-yyyy");
                workSheet.Cells[recordIndex, 3].Value = income.CategoryName;
                workSheet.Cells[recordIndex, 4].Value = income.OwnerName;
                workSheet.Cells[recordIndex, 5].Value = income.TaxAmountCut;
                workSheet.Cells[recordIndex, 6].Value = income.Amount;
                workSheet.Cells[recordIndex, 7].Value = income.Mode;
                workSheet.Cells[recordIndex, 8].Value = income.Remarks;
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
            workSheet.Column(8).AutoFit();

            //Context currentContext = Android.App.Application.Context;
            //string directory = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDocuments);
            //if (!Directory.Exists(directory))
            //{
            //    Directory.CreateDirectory(directory);
            //}
            //string file = Path.Combine(directory, "Income_Report.xlsx");
            //System.IO.File.WriteAllBytes(file, excel.GetAsByteArray());


            var stream = new MemoryStream(excel.GetAsByteArray());
            CancellationTokenSource Ctoken = new CancellationTokenSource();
            string fileName = "";
            if (type == "Income")
            {
                fileName = "Income_Report_" + DateTime.Now.ToString("dd-MM-yyyy hh:mm tt") + ".xlsx";
            }
            else if (type == "Expense")
            {
                fileName = "Expense_Report_" + DateTime.Now.ToString("dd-MM-yyyy hh:mm tt") + ".xlsx";
            }
            var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream, Ctoken.Token);
            if (fileSaverResult.IsSuccessful)
            {
                await DisplayAlert("Message", "Excel saved in " + fileSaverResult.FilePath, "Ok");
            }

            excel.Dispose();
            activityIndicator.IsRunning = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Ok");
            await DisplayAlert("Error", ex.InnerException.ToString(), "Ok");
        }
    }

    private void typePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (typePicker.SelectedItem != null)
        {
            if (typePicker.SelectedItem.ToString() == "Expense")
            {
                LoadExpenseCategoriesInDropdown();
            }
            else if (typePicker.SelectedItem.ToString() == "Income")
            {
                LoadIncomeCategoriesInDropdown();
            }
        }
    }

    private async void LoadExpenseCategoriesInDropdown()
    {
        try
        {
            var expenseCategories = await _dbConnection.Table<IncomeExpenseCategories>().Where(i => i.CategoryType == "Expense").ToListAsync();
            IncomeExpenseCategories objCategories = new IncomeExpenseCategories
            {
                IncomeExpenseCategoryId = 0,
                CategoryName = "All",
                CategoryType = "Expense"
            };
            expenseCategories.Insert(0, objCategories);
            pickerCategory.ItemsSource = expenseCategories.Select(i => i.CategoryName).ToList();
            if (expenseCategories.Count > 1)
            {
                pickerCategory.SelectedIndex = 0;
            }
        }
        catch (Exception)
        {
            return;
        }
    }

    private async void LoadIncomeCategoriesInDropdown()
    {
        try
        {
            var incomeCategories = await _dbConnection.Table<IncomeExpenseCategories>().Where(i => i.CategoryType == "Income").ToListAsync();
            IncomeExpenseCategories objCategories = new IncomeExpenseCategories
            {
                IncomeExpenseCategoryId = 0,
                CategoryName = "All",
                CategoryType = "Income"
            };
            incomeCategories.Insert(0, objCategories);
            pickerCategory.ItemsSource = incomeCategories.Select(i => i.CategoryName).ToList();
            if (incomeCategories.Count > 1)
            {
                pickerCategory.SelectedIndex = 0;
            }
        }
        catch (Exception)
        {
            return;
        }
    }

    private void OnShowSelectedClicked(object sender, EventArgs e)
    {
        var selected = Items.Where(i => i.IsSelected).Select(i => i.Name).ToList();
        SelectedLabel.Text = selected.Count > 0
            ? $"Selected: {string.Join(", ", selected)}"
            : "Selected: None";
    }
}