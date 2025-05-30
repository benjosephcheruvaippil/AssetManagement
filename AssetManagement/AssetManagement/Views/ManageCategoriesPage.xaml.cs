using AssetManagement.Models;
using SQLite;

namespace AssetManagement.Views;

public partial class ManageCategoriesPage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    AppTheme currentTheme = Application.Current.RequestedTheme;
    public string OldCategoryName = "";
    private bool isProgrammaticChange = false;
    public ManageCategoriesPage()
	{
		InitializeComponent();
	}
    protected async override void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await LoadCategoriesInPage("");
        }
        catch (Exception)
        {
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
                await _dbConnection.CreateTableAsync<IncomeExpenseCategories>();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void btnSaveCategory_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(entryCategoryName.Text) || categoryTypePicker.SelectedIndex == -1)
            {
                await DisplayAlert("Message", "Please input category name and select category type", "OK");
                return;
            }
            entryCategoryName.Text = entryCategoryName.Text.Trim();
            if (string.IsNullOrEmpty(txtIncomeExpenseCategoryId.Text))//insert
            {
                //check if duplicate names exist
                if (!string.IsNullOrEmpty(entryCategoryName.Text))
                {
                    string inputtedCategoryNameFormatted = entryCategoryName.Text.ToLower();

                    await SetUpDb();
                    var categories = await _dbConnection.Table<IncomeExpenseCategories>().ToListAsync();
                    if (categories.Where(o => o.CategoryName.ToLower() == inputtedCategoryNameFormatted).Count() > 0)
                    {
                        await DisplayAlert("Message", "Duplicate name found in database. Please re-enter.", "OK");
                        return;
                    }
                }
                //check if duplicate names exist

                IncomeExpenseCategories objIncomeExpenseCat = new IncomeExpenseCategories
                {
                    CategoryName = entryCategoryName.Text.Trim(),
                    CategoryType = categoryTypePicker.SelectedItem.ToString(),
                    IsOneTimeExpense = chkIsOneTimeExpense.IsChecked,
                    IsVisible = chkIsVisible.IsChecked
                };
                await SetUpDb();
                int rowsAffected = await _dbConnection.InsertAsync(objIncomeExpenseCat);
                ClearCategoriesForm();
                if (rowsAffected > 0)
                {

                    await LoadCategoriesInPage("");
                }
                else
                {
                    await DisplayAlert("Error", "Something went wrong", "OK");
                }
            }
            else //update
            {
                int incomeExpenseCategoryId = Convert.ToInt32(txtIncomeExpenseCategoryId.Text);
                //check if duplicate names exist
                if (!string.IsNullOrEmpty(entryCategoryName.Text))
                {
                    string inputtedCategoryNameFormatted = entryCategoryName.Text.ToLower().Trim();

                    await SetUpDb();
                    var categories = await _dbConnection.Table<IncomeExpenseCategories>().ToListAsync();
                    
                    if (categories.Where(o => o.CategoryName.ToLower() == inputtedCategoryNameFormatted && o.IncomeExpenseCategoryId != incomeExpenseCategoryId).Count() >= 1)
                    {
                        await DisplayAlert("Message", "Duplicate name found in database. Please re-enter.", "OK");
                        return;
                    }
                }
                //check if duplicate names exist
                
                IncomeExpenseCategories objIncomeExpenseCat = new IncomeExpenseCategories
                {
                    IncomeExpenseCategoryId = incomeExpenseCategoryId,
                    CategoryName = entryCategoryName.Text.Trim(),
                    CategoryType = categoryTypePicker.SelectedItem.ToString(),
                    IsOneTimeExpense = chkIsOneTimeExpense.IsChecked,
                    IsVisible = chkIsVisible.IsChecked
                };
                await SetUpDb();
                int rowsAffected = await _dbConnection.UpdateAsync(objIncomeExpenseCat);
                if (rowsAffected > 0)
                {
                    //update all records in IncomeExpenseModel table
                    var recordsUpdatedIncomeExpense = await _dbConnection.ExecuteAsync($"Update IncomeExpenseModel set CategoryName='{entryCategoryName.Text.Trim()}' where CategoryName='{OldCategoryName}'");
                    //update all records in Assets table
                    //var recordsUpdateAssets = await _dbConnection.ExecuteAsync($"Update Assets set Holder='{entryOwnerName.Text.Trim()}' where Holder='{OldOwnerName}'");
                    ClearCategoriesForm();
                    await LoadCategoriesInPage("");
                }
                else
                {
                    await DisplayAlert("Error", "Something went wrong", "OK");
                }
            }
        }
        catch(Exception ex)
        {
            await DisplayAlert("Error", "Something went wrong", "OK");
        }
    }

    private async Task LoadCategoriesInPage(string categoryName)
    {
        try
        {
            List<IncomeExpenseCategories> categories = new List<IncomeExpenseCategories>();
            isProgrammaticChange = false;
            tblscCategories.Clear();
            await SetUpDb();
            if (string.IsNullOrEmpty(categoryName))
            {
                categories = await _dbConnection.QueryAsync<IncomeExpenseCategories>("select IncomeExpenseCategoryId, CategoryName, CategoryType from IncomeExpenseCategories order by CategoryType asc, CategoryName asc");
            }
            else 
            {
                categories = await _dbConnection.QueryAsync<IncomeExpenseCategories>("select IncomeExpenseCategoryId, CategoryName, CategoryType from IncomeExpenseCategories where CategoryName like '%" + categoryName + "%' order by CategoryType asc, CategoryName asc");
            }
            
            foreach (var item in categories)
            {
                TextCell objCell = new TextCell();
                objCell.Text = item.CategoryName + " | " + item.IncomeExpenseCategoryId;
                if (currentTheme == AppTheme.Dark)
                {
                    //set to white color
                    objCell.TextColor = Color.FromArgb("#FFFFFF");
                }
                objCell.Detail = item.CategoryType;

                tblscCategories.Add(objCell);

                objCell.Tapped += ObjCell_Tapped;
            }
        }
        catch (Exception ex)
        {
            string exceptionMessage = ex.ToString();
        }
    }

    private async void ObjCell_Tapped(object sender, EventArgs e)
    {

        var tappedViewCell = (TextCell)sender;
        var textCell = tappedViewCell.Text.ToString().Split("|");

        entryCategoryName.Text = textCell[0].Trim();
        categoryTypePicker.SelectedItem = tappedViewCell.Detail;
        txtIncomeExpenseCategoryId.Text = textCell[1].Trim();
        int incomeExpenseCategoryId = Convert.ToInt32(textCell[1].Trim());
        var incomeExpenseRecord = await _dbConnection.Table<IncomeExpenseCategories>().Where(i => i.IncomeExpenseCategoryId == incomeExpenseCategoryId).FirstOrDefaultAsync();
        //if (incomeExpenseRecord.IsVisible == null)
        //{
        //    int result = await _dbConnection.ExecuteAsync("update IncomeExpenseCategories set IsVisible=1 where IncomeExpenseCategoryId=?", incomeExpenseCategoryId);
        //    incomeExpenseRecord.IsVisible = true;
        //}
        chkIsOneTimeExpense.IsChecked = incomeExpenseRecord.IsOneTimeExpense == null ? false : (bool)incomeExpenseRecord.IsOneTimeExpense;
        chkIsVisible.IsChecked = incomeExpenseRecord.IsVisible == null ? false : (bool)incomeExpenseRecord.IsVisible;
        OldCategoryName = entryCategoryName.Text;
    }

    private async void btnDeleteCategory_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(txtIncomeExpenseCategoryId.Text))
            {
                entryCategoryName.Text = entryCategoryName.Text.Trim();
                bool userResponse = await DisplayAlert("Warning", "Are you sure to delete?", "Yes", "No");
                if (userResponse)
                {
                    //check if there is any transaction in IncomeExpenseModel and Assets table before deleting the owner
                    var incomeExpenseRecord = await _dbConnection.Table<IncomeExpenseModel>().Where(i => i.CategoryName == entryCategoryName.Text).ToListAsync();
                    if (incomeExpenseRecord.Count > 0)
                    {
                        await DisplayAlert("Info", "Cannot delete category since there are records with this category.", "Ok");
                        return;
                    }
                    //check if there is any transaction in IncomeExpenseModel and Assets table before deleting the owner
                    IncomeExpenseCategories objIncomeExpenseCat = new IncomeExpenseCategories()
                    {
                        IncomeExpenseCategoryId = Convert.ToInt32(txtIncomeExpenseCategoryId.Text)
                    };

                    await SetUpDb();
                    int rowsAffected = await _dbConnection.DeleteAsync(objIncomeExpenseCat);
                    ClearCategoriesForm();
                    await LoadCategoriesInPage("");
                }
            }
            else
            {
                await DisplayAlert("Info", "Please select a category to delete", "Ok");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Something went wrong: " + ex.Message.ToString(), "Ok");
        }
    }

    public void ClearCategoriesForm()
    {
        isProgrammaticChange = true;
        entCategorySearch.Text = "";
        txtIncomeExpenseCategoryId.Text = "";
        entryCategoryName.Text = "";
        categoryTypePicker.SelectedIndex = -1;
    }

    private void categoryTypePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            if (categoryTypePicker.SelectedItem != null)
            {
                string categoryType = categoryTypePicker.SelectedItem.ToString();
                if (categoryType == "Expense")
                {
                    lblOneTimeExpense.IsVisible = true;
                    chkIsOneTimeExpense.IsVisible = true;
                }
                else
                {
                    lblOneTimeExpense.IsVisible = false;
                    chkIsOneTimeExpense.IsVisible = false;
                }
            }
            else
            {
                lblOneTimeExpense.IsVisible = false;
                chkIsOneTimeExpense.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", "Something went wrong: " + ex.Message.ToString(), "Ok");
        }
    }

    private async void entCategorySearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!isProgrammaticChange)
        {
            await LoadCategoriesInPage(entCategorySearch.Text.Trim());
        }
    }
}