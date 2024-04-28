using AssetManagement.Models;
using SQLite;

namespace AssetManagement.Views;

public partial class ManageCategoriesPage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    public string OldCategoryName = "";
    public ManageCategoriesPage()
	{
		InitializeComponent();
	}
    protected async override void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await LoadCategoriesInPage();
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
                string inputtedCategoryNameFormatted = entryCategoryName.Text;

                await SetUpDb();
                var categories = await _dbConnection.Table<IncomeExpenseCategories>().ToListAsync();
                if (categories.Where(o => o.CategoryName == inputtedCategoryNameFormatted).Count() > 0)
                {
                    await DisplayAlert("Message", "Duplicate name found in database. Please re-enter.", "OK");
                    return;
                }
            }
            //check if duplicate names exist

            IncomeExpenseCategories objIncomeExpenseCat = new IncomeExpenseCategories
            {
                CategoryName = entryCategoryName.Text.Trim(),
                CategoryType = categoryTypePicker.SelectedItem.ToString()
            };
            await SetUpDb();
            int rowsAffected = await _dbConnection.InsertAsync(objIncomeExpenseCat);
            ClearCategoriesForm();
            if (rowsAffected > 0)
            {

                await LoadCategoriesInPage();
            }
            else
            {
                await DisplayAlert("Error", "Something went wrong", "OK");
            }
        }
        else //update
        {
            //check if duplicate names exist
            if (!string.IsNullOrEmpty(entryCategoryName.Text))
            {
                string inputtedCategoryNameFormatted = entryCategoryName.Text.Trim();

                await SetUpDb();
                var categories = await _dbConnection.Table<IncomeExpenseCategories>().ToListAsync();
                if (categories.Where(o => o.CategoryName == inputtedCategoryNameFormatted).Count() > 1)
                {
                    await DisplayAlert("Message", "Duplicate name found in database. Please re-enter.", "OK");
                    return;
                }
            }
            //check if duplicate names exist
            int incomeExpenseCategoryId = Convert.ToInt32(txtIncomeExpenseCategoryId.Text);
            IncomeExpenseCategories objIncomeExpenseCat = new IncomeExpenseCategories
            {
                IncomeExpenseCategoryId = incomeExpenseCategoryId,
                CategoryName = entryCategoryName.Text.Trim(),
                CategoryType = categoryTypePicker.SelectedItem.ToString()
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
                await LoadCategoriesInPage();
            }
            else
            {
                await DisplayAlert("Error", "Something went wrong", "OK");
            }
        }
    }

    private async Task LoadCategoriesInPage()
    {
        try
        {
            List<IncomeExpenseCategories> categories = new List<IncomeExpenseCategories>();
            tblscCategories.Clear();
            await SetUpDb();
            categories = await _dbConnection.QueryAsync<IncomeExpenseCategories>("select IncomeExpenseCategoryId, CategoryName, CategoryType from IncomeExpenseCategories");
            foreach (var item in categories)
            {
                TextCell objCell = new TextCell();
                objCell.Text = item.CategoryName + " | " + item.IncomeExpenseCategoryId;
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

    private void ObjCell_Tapped(object sender, EventArgs e)
    {

        var tappedViewCell = (TextCell)sender;
        var textCell = tappedViewCell.Text.ToString().Split("|");

        entryCategoryName.Text = textCell[0].Trim();
        categoryTypePicker.SelectedItem = tappedViewCell.Detail;
        txtIncomeExpenseCategoryId.Text = textCell[1].Trim();
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
                    await LoadCategoriesInPage();
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
        txtIncomeExpenseCategoryId.Text = "";
        entryCategoryName.Text = "";
        categoryTypePicker.SelectedIndex = -1;
    }
}