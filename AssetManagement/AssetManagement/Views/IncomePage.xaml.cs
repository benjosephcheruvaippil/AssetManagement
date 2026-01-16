using AssetManagement.Models;
using AssetManagement.Models.Constants;
using AssetManagement.Models.DataTransferObject;
using SQLite;
using System.Globalization;

namespace AssetManagement.Views;

public partial class IncomePage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    AppTheme currentTheme = Application.Current.RequestedTheme;
    public int PageNumber = 0, PageSize = 30, TotalIncomeRecordCount = 0;
    public bool ApplyFilterClicked = false;
    //Border _selectedFrame = null;
    public IncomePage()
    {
        InitializeComponent();

        var labelShowRemaining = new TapGestureRecognizer();
        labelShowRemaining.Tapped += (s, e) =>
        {
            LoadIncomeInPage("Pagewise");
        };
        lblShowRemainingRecords.GestureRecognizers.Add(labelShowRemaining);

        double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
        incomeCollectionView.HeightRequest = screenHeight * 0.5;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        LoadOwnersInDropdown();
        LoadIncomeCategoriesInDropdown();
        LoadIncomeInPage("Last5");
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
        lblCurrentMonthIncome.Text = currentMonth + ": " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", totalIncome);
    }

    private async void LoadOwnersInDropdown()
    {
        try
        {
            await SetUpDb();
            var owners = await _dbConnection.Table<Owners>().ToListAsync();
            Owners objOwner = new Owners
            {
                OwnerId = 0,
                OwnerName = Constants.AddNewOwnerOption
            };
            owners.Add(objOwner);
            pickerOwnerName.ItemsSource = owners.Select(s => s.OwnerName).ToList();
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
            await SetUpDb();
            var incomeCategories = await _dbConnection.Table<IncomeExpenseCategories>().Where(i => i.CategoryType == "Income" && i.IsVisible == true).ToListAsync();
            IncomeExpenseCategories objCategories = new IncomeExpenseCategories
            {
                IncomeExpenseCategoryId = 0,
                CategoryName = Constants.AddNewCategoryOption,
                CategoryType = "Income"
            };
            incomeCategories.Add(objCategories);
            pickerIncomeCategory.ItemsSource = incomeCategories.Select(i => i.CategoryName).ToList();

            if (currentTheme == AppTheme.Dark)
            {
                //set to white color
                pickerIncomeCategory.TextColor = Color.FromArgb("#FFFFFF");
                pickerIncomeCategory.BackgroundColor = Color.FromArgb("#000000");
            }
        }
        catch(Exception)
        {
            return;
        }
    }

    private async void LoadIncomeInPage(string hint)
    {
        //set date
        dpDateIncome.MinimumDate = new DateTime(2020, 1, 1);
        dpDateIncome.MaximumDate = new DateTime(2050, 12, 31);
        dpDateIncome.Date = DateTime.Now;
        dpDateIncome.Format = "dd-MM-yyyy";
        //set date
        //tblscIncome.Clear();
        await SetUpDb();

        List<IncomeExpenseModel> income = new List<IncomeExpenseModel>();
        List<IncomeExpenseDTO> incomeDTO = new List<IncomeExpenseDTO>();

        if (hint == "Last5")
        {
            PageNumber = 0;
            ApplyFilterClicked = false;
            //tblscIncome.Title = "Last 5 Transactions";
            lblCardBanner.Text = "Last 5 Transactions";
            lblShowRemainingRecords.IsVisible = true;
            income = await _dbConnection.QueryAsync<IncomeExpenseModel>("select TransactionId,Amount,TaxAmountCut,CategoryName,OwnerName,Date,Remarks from IncomeExpenseModel where TransactionType=='Income' order by Date desc Limit 5");

            int totalIncomesCount = await _dbConnection.ExecuteScalarAsync<int>("select count(*) from IncomeExpenseModel where TransactionType=='Income'");

            if (totalIncomesCount == 0)
            {
                incomeCollectionView.IsVisible = false;
                lblCardBanner.Text = "";
                lblShowRemainingRecords.IsVisible = false;
            }
            else if (totalIncomesCount <= 5)
            {
                incomeCollectionView.IsVisible = true;
                lblCardBanner.Text = "Last 5 Transactions";
                lblShowRemainingRecords.IsVisible = false;
            }
            else
            {
                incomeCollectionView.IsVisible = true;
                lblCardBanner.Text = "Last 5 Transactions";
                lblShowRemainingRecords.IsVisible = true;
            }

            incomeDTO = income.Select(s => new IncomeExpenseDTO
            {
                TransactionId = s.TransactionId,
                Amount = s.Amount,
                CurrencySymbol = Constants.GetCurrency(),
                TaxAmountCut = s.TaxAmountCut,
                CategoryName = s.CategoryName,
                Date = s.Date,
                OwnerName = s.OwnerName,
                Remarks = s.Remarks,
                Mode = s.Mode
            }).ToList();
            incomeCollectionView.ItemsSource = incomeDTO;
        }
        else if(hint == "Pagewise")
        {
            if (ApplyFilterClicked)
            {
                await ApplyFilterPagination();
                await incomeScrollView.ScrollToAsync(expanderFilterDetails, ScrollToPosition.Start, true);
                return;
            }
            PageNumber = PageNumber + 1;
            int offset = (PageNumber - 1) * PageSize;
            //if (TotalIncomeRecordCount == 0)
            //{
            //    TotalIncomeRecordCount = await _dbConnection.Table<IncomeExpenseModel>().Where(e => e.TransactionType == "Income").CountAsync();
            //}
            TotalIncomeRecordCount = await _dbConnection.Table<IncomeExpenseModel>().Where(e => e.TransactionType == "Income").CountAsync();
            int showRecordCount = 0;
            if (offset == 0)
            {
                showRecordCount = PageSize;
            }
            else
            {
                showRecordCount = PageSize + offset;
            }
            if (TotalIncomeRecordCount - offset < PageSize)
            {
                showRecordCount = TotalIncomeRecordCount;
                lblShowRemainingRecords.IsVisible = false;
            }

            //tblscIncome.Title = "Showing " + showRecordCount + " of " + TotalIncomeRecordCount + " records";
            lblCardBanner.Text = "Showing " + showRecordCount + " of " + TotalIncomeRecordCount + " records";
            income = await _dbConnection.QueryAsync<IncomeExpenseModel>("select TransactionId,Amount,TaxAmountCut,CategoryName,OwnerName,Date,Remarks from IncomeExpenseModel where TransactionType=='Income' order by Date desc Limit 30 Offset " + offset);
            incomeDTO = income.Select(s => new IncomeExpenseDTO
            {
                TransactionId = s.TransactionId,
                Amount = s.Amount,
                CurrencySymbol = Constants.GetCurrency(),
                TaxAmountCut = s.TaxAmountCut,
                CategoryName = s.CategoryName,
                Date = s.Date,
                OwnerName = s.OwnerName,
                Remarks = s.Remarks,
                Mode = s.Mode
            }).ToList();

            incomeCollectionView.ItemsSource = incomeDTO;
            await incomeScrollView.ScrollToAsync(expanderFilterDetails, ScrollToPosition.Start, true);
        }
        

        //foreach (var item in income)
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
        //        //tblscIncome.TextColor = Color.FromArgb("#FFFFFF");
        //        objCell.TextColor = Color.FromArgb("#FFFFFF");
        //    }

        //    //tblscIncome.Add(objCell);

        //    objCell.Tapped += ObjCell_IncomeTapped; ;
        //}
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

        pickerIncomeCategory.Text = textCell[0].Trim();

        if (string.IsNullOrEmpty(pickerIncomeCategory.Text))
        {
            DisplayAlertAsync("Info", textCell[0].Trim() + " - Please make this category visible from Manage Category page in order to edit.", "OK");
        }

        pickerOwnerName.SelectedItem = incomeDetail.Result.OwnerName;
        entryTaxAmount.Text = Convert.ToString(incomeDetail.Result.TaxAmountCut);      

        if (tappedViewCell.Detail.Contains("-"))
        {
            dpDateIncome.Date = new DateTime(year, month, day);

            string[] amountAndRemarks = tappedViewCell.Detail.Split(new[] { "-" }, StringSplitOptions.TrimEntries);
            string remarks = "";
            entryIncomeAmount.Text = amountAndRemarks[0];
            int loop = 1;
            //logic for '-' symbol handling in remarks. for example if detail has some value like '250 - this is for 24-35 date range', then the below logic will
            //handle it to populate it in remarks.
            foreach (var item in amountAndRemarks)
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
            entryIncomeRemarks.Text = remarks;
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
        if (((pickerIncomeCategory.ItemsSource as IEnumerable<object>)?.Cast<object>().Count() ?? 0) == 0)
        {
            await DisplayAlertAsync("Message", "Please create income categories under Settings -> Manage Categories before adding expenses", "OK");
            return;
        }
        else if (string.IsNullOrEmpty(entryIncomeAmount.Text))
        {
            await DisplayAlertAsync("Message", "Please input required values", "OK");
            return;
        }
        else if (string.IsNullOrEmpty(pickerIncomeCategory.Text))
        {
            await DisplayAlertAsync("Message", "Please select a category", "OK");
            return;
        }
        else if (pickerOwnerName.Items.Count == 0)
        {
            await DisplayAlertAsync("Message", "Please create owner/users under Settings -> Manage Owners/Users before adding income", "OK");
            return;
        }
        else if (pickerOwnerName.SelectedIndex == -1)
        {
            await DisplayAlertAsync("Message", "Please select an owner", "OK");
            return;
        }

        if (string.IsNullOrEmpty(entryTaxAmount.Text))
        {
            entryTaxAmount.Text = "0";
        }

        //check if category present in master table
        var incomeCategories = await _dbConnection.Table<IncomeExpenseCategories>().Where(i => i.CategoryType == "Income").ToListAsync();
        if (!incomeCategories.Any(c => c.CategoryName == pickerIncomeCategory.Text.Trim()))
        {
            await DisplayAlertAsync("Message", "Please create this category under Settings -> Manage Categories before adding income", "OK");
            return;
        }
        //check if category present in master table

        if (string.IsNullOrEmpty(txtIncomeTransactionId.Text))//insert
        {
            IncomeExpenseModel objIncomeExpense = new IncomeExpenseModel()
            {
                Amount = Convert.ToDouble(entryIncomeAmount.Text),
                TaxAmountCut=Convert.ToDouble(entryTaxAmount.Text),
                TransactionType = "Income",
                Date = (DateTime)dpDateIncome.Date != DateTime.Now.Date ? (DateTime)dpDateIncome.Date : DateTime.Now,
                CategoryName = Convert.ToString(pickerIncomeCategory.Text.Trim()),
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
                LoadIncomeInPage("Last5");
                await ShowCurrentMonthIncome();
                //ClearIncome();
            }
            else
            {
                await DisplayAlertAsync("Error", "Something went wrong", "OK");
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
                Date = (DateTime)dpDateIncome.Date != DateTime.Now.Date ? (DateTime)dpDateIncome.Date : DateTime.Now,
                CategoryName = Convert.ToString(pickerIncomeCategory.Text.Trim()),
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
                LoadIncomeInPage("Last5");
                await ShowCurrentMonthIncome();
                //ClearIncome();
            }
            else
            {
                await DisplayAlertAsync("Error", "Something went wrong", "OK");
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
        entryTaxAmount.Text = "";
        pickerIncomeCategory.Text = "";
        entryIncomeRemarks.Text = "";
        dpDateIncome.Date = DateTime.Now;
        //if (_selectedFrame != null)
        //{
        //    _selectedFrame.BackgroundColor = Colors.White;
        //    _selectedFrame = null;
        //}
        if (incomeCollectionView.ItemsSource is IEnumerable<IncomeExpenseDTO> items)
        {
            foreach (var item in items)
                item.IsSelected = false; // reset all
        }
    }

    private async void btnDeleteIncome_Clicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(txtIncomeTransactionId.Text))
        {
            bool userResponse = await DisplayAlertAsync("Warning", "Are you sure to delete?", "Yes", "No");
            if (userResponse)
            {
                IncomeExpenseModel objIncome = new IncomeExpenseModel()
                {
                    TransactionId = Convert.ToInt32(txtIncomeTransactionId.Text)
                };

                await SetUpDb();
                int rowsAffected = await _dbConnection.DeleteAsync(objIncome);
                ClearIncome();
                LoadIncomeInPage("Last5");
                await ShowCurrentMonthIncome();
            }
        }
    }

    private async void btnApplyFilters_Clicked(object sender, EventArgs e)
    {
        ApplyFilterClicked = true;
        PageNumber = 0;
        TotalIncomeRecordCount = 0;
        lblShowRemainingRecords.IsVisible = true;
        await ApplyFilterPagination();
    }

    public async Task ApplyFilterPagination()
    {
        //tblscIncome.Clear();
        PageNumber = PageNumber + 1;
        int offset = (PageNumber - 1) * PageSize;

        DateTime? fromDate = dpFromDateFilter.Date, toDate = dpToDateFilter.Date.Value.AddDays(1).AddSeconds(-1);
        if (dpFromDateFilter.Date == dpToDateFilter.Date)
        {
            fromDate = dpFromDateFilter.Date;
            toDate = dpToDateFilter.Date.Value.AddDays(1);
        }

        string category = entCategoryFilter.Text;
        string remarks = entRemarksFilter.Text;

        List<IncomeExpenseModel> income = new List<IncomeExpenseModel>();
        List<IncomeExpenseDTO> incomeDTO = new List<IncomeExpenseDTO>();

        var query = @"select TransactionId,Amount,TaxAmountCut,CategoryName,OwnerName,Date,Remarks from IncomeExpenseModel where TransactionType=='Income'";

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

        income = await _dbConnection.QueryAsync<IncomeExpenseModel>(query, parameters.ToArray());
        incomeDTO = income.Select(s => new IncomeExpenseDTO
        {
            TransactionId = s.TransactionId,
            Amount = s.Amount,
            CurrencySymbol = Constants.GetCurrency(),
            TaxAmountCut = s.TaxAmountCut,
            CategoryName = s.CategoryName,
            Date = s.Date,
            OwnerName = s.OwnerName,
            Remarks = s.Remarks,
            Mode = s.Mode
        }).ToList();

        //pagination
        if (TotalIncomeRecordCount == 0)
        {
            var totalRecords = await _dbConnection.QueryAsync<IncomeExpenseModel>(pageCountQuery, parameters.ToArray());
            lblCurrentMonthIncome.Text = "Total: " + string.Format(new CultureInfo(Constants.GetCurrency()), "{0:C0}", totalRecords.Sum(s => s.Amount));
            TotalIncomeRecordCount = totalRecords.Count();
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
        if (TotalIncomeRecordCount - offset < PageSize)
        {
            showRecordCount = TotalIncomeRecordCount;
            lblShowRemainingRecords.IsVisible = false;
        }

        //tblscIncome.Title = "Showing " + showRecordCount + " of " + TotalIncomeRecordCount + " records";
        lblCardBanner.Text = "Showing " + showRecordCount + " of " + TotalIncomeRecordCount + " records";
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
        //        //tblscIncome.TextColor = Color.FromArgb("#FFFFFF");
        //        objCell.TextColor = Color.FromArgb("#FFFFFF");
        //    }

        //    //tblscIncome.Add(objCell);

        //    objCell.Tapped += ObjCell_IncomeTapped;
        //}

        incomeCollectionView.ItemsSource = incomeDTO;
    }

    private void pickerIncomeCategory_SelectedIndexChanged(object sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
    {
        if (!string.IsNullOrEmpty(pickerIncomeCategory.Text))
        {
            if (pickerIncomeCategory.Text == Constants.AddNewCategoryOption)
            {
                Navigation.PushAsync(new ManageCategoriesPage());
            }
        }
    }

    private void pickerOwnerName_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (pickerOwnerName.SelectedItem != null)
        {
            if (pickerOwnerName.SelectedItem.ToString() == Constants.AddNewOwnerOption)
            {

                Navigation.PushAsync(new ManageUsersPage());
            }
        }
    }

    private async void OnCardTapped(object sender, EventArgs e)
    {
        if ((sender as Border)?.BindingContext is IncomeExpenseDTO tappedItem)
        {
            // Populate your fields
            txtIncomeTransactionId.Text = tappedItem.TransactionId.ToString();
            entryIncomeAmount.Text = tappedItem.Amount.ToString();
            entryTaxAmount.Text = tappedItem.TaxAmountCut.ToString();
            pickerIncomeCategory.Text = tappedItem.CategoryName;
            pickerOwnerName.SelectedItem = tappedItem.OwnerName;
            dpDateIncome.Date = tappedItem.Date;
            entryIncomeRemarks.Text = string.IsNullOrEmpty(tappedItem.Remarks) ? string.Empty : tappedItem.Remarks.Trim();

            // Highlight logic using DTO property
            if (incomeCollectionView.ItemsSource is IEnumerable<IncomeExpenseDTO> items)
            {
                foreach (var item in items)
                    item.IsSelected = false; // reset all
            }

            tappedItem.IsSelected = true; // select tapped item

            // Scroll logic stays the same
            var visibleRect = new Rect(
            incomeScrollView.ScrollX,
            incomeScrollView.ScrollY,
            incomeScrollView.Width,
            incomeScrollView.Height);

            // Get position of target element
            var targetRect = expanderFilterDetails.Bounds;

            // Check if the target is within the visible area
            bool isVisible =
                targetRect.Y >= visibleRect.Y &&
                targetRect.Y <= visibleRect.Y + visibleRect.Height;

            // Scroll only if not visible
            if (!isVisible)
            {
                await incomeScrollView.ScrollToAsync(expanderFilterDetails, ScrollToPosition.Start, true);
            }


            //await expenseScrollView.ScrollToAsync(expanderFilterDetails, ScrollToPosition.Start, true);
        }
    }

    //private async void pickerOwnerName_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //    if(pickerOwnerName.SelectedIndex == -1)
    //    {
    //        return;
    //    }

    //    if(pickerOwnerName.SelectedItem.ToString() == "Add New Owner")
    //    {

    //        await Navigation.PushAsync(new ManageUsersPage());
    //        pickerOwnerName.SelectedIndex = -1;
    //    }
    //    return;
    //}
}