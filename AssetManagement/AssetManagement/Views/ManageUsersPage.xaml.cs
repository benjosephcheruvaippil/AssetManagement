using AssetManagement.Models;
using SQLite;
using System.Text.RegularExpressions;

namespace AssetManagement.Views;

public partial class ManageUsersPage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    AppTheme currentTheme = Application.Current.RequestedTheme;
    public string OldOwnerName = "";
    public ManageUsersPage()
	{
		InitializeComponent();
    }

    protected async override void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await LoadOwnersInPage();
            //LoadExpensesInPage("Last5");// show expenses in the expense tab
            //await ShowCurrentMonthExpenses();
            //SetLastUploadedDate();
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
                await _dbConnection.CreateTableAsync<Owners>();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void btnSaveOwner_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(entryOwnerName.Text))
        {
            await DisplayAlert("Message", "Please input name", "OK");
            return;
        }
        entryOwnerName.Text = entryOwnerName.Text.Trim();

        if (string.IsNullOrEmpty(txtOwnerId.Text))//insert
        {
            //check if duplicate names exist
            if (!string.IsNullOrEmpty(entryOwnerName.Text))
            {
                string inputtedOwnerNameFormatted = entryOwnerName.Text;
                //string inputtedOwnerNameFormatted = Regex.Replace(entryOwnerName.Text.ToLower().Trim(), @"\s+", "");

                await SetUpDb();
                var owners = await _dbConnection.Table<Owners>().ToListAsync();
                //foreach (var owner in owners)
                //{
                //    owner.OwnerName = Regex.Replace(owner.OwnerName.ToLower().Trim(), @"\s+", "");
                //}
                if (owners.Where(o => o.OwnerName == inputtedOwnerNameFormatted).Count() > 0)
                {
                    await DisplayAlert("Message", "Duplicate name found in database. Please re-enter.", "OK");
                    return;
                }
            }
            //check if duplicate names exist

            Owners objOwner = new Owners
            {
                OwnerName = entryOwnerName.Text.Trim()
            };
            await SetUpDb();
            int rowsAffected = await _dbConnection.InsertAsync(objOwner);
            ClearOwnersForm();
            if (rowsAffected > 0)
            {
                
                await LoadOwnersInPage();
            }
            else
            {
                await DisplayAlert("Error", "Something went wrong", "OK");
            }
        }
        else //update
        {
            //check if duplicate names exist
            if (!string.IsNullOrEmpty(entryOwnerName.Text))
            {
                string inputtedOwnerNameFormatted = entryOwnerName.Text.Trim();
                //string inputtedOwnerNameFormatted = Regex.Replace(entryOwnerName.Text.ToLower().Trim(), @"\s+", "");

                await SetUpDb();
                var owners = await _dbConnection.Table<Owners>().ToListAsync();
                //foreach (var owner in owners)
                //{
                //    owner.OwnerName = Regex.Replace(owner.OwnerName.ToLower().Trim(), @"\s+", "");
                //}
                if (owners.Where(o => o.OwnerName == inputtedOwnerNameFormatted).Count() > 1)
                {
                    await DisplayAlert("Message", "Duplicate name found in database. Please re-enter.", "OK");
                    return;
                }
            }
            //check if duplicate names exist
            int ownerId = Convert.ToInt32(txtOwnerId.Text);
            Owners objOwner = new Owners
            {
                OwnerId = ownerId,
                OwnerName = entryOwnerName.Text.Trim()
            };
            await SetUpDb();
            int rowsAffected = await _dbConnection.UpdateAsync(objOwner);           
            if (rowsAffected > 0)
            {
                //update all records in IncomeExpenseModel table
                var recordsUpdatedIncomeExpense = await _dbConnection.ExecuteAsync($"Update IncomeExpenseModel set OwnerName='{entryOwnerName.Text.Trim()}' where OwnerName='{OldOwnerName}'");
                //update all records in Assets table
                var recordsUpdateAssets = await _dbConnection.ExecuteAsync($"Update Assets set Holder='{entryOwnerName.Text.Trim()}' where Holder='{OldOwnerName}'");
                ClearOwnersForm();
                await LoadOwnersInPage();
            }
            else
            {
                await DisplayAlert("Error", "Something went wrong", "OK");
            }
        }
    }

    private async Task LoadOwnersInPage()
    {
        try
        {
            List<Owners> owners = new List<Owners>();
            tblscOwners.Clear();
            await SetUpDb();
            owners = await _dbConnection.QueryAsync<Owners>("select OwnerId, OwnerName from Owners");
            foreach (var item in owners)
            {
                TextCell objCell = new TextCell();
                objCell.Text = item.OwnerName + " | " + item.OwnerId;
                if (currentTheme == AppTheme.Dark)
                {
                    //set to white color
                    objCell.TextColor = Color.FromArgb("#FFFFFF");
                }

                tblscOwners.Add(objCell);

                objCell.Tapped += ObjCell_Tapped;
            }
        }
        catch(Exception ex)
        {
            string exceptionMessage = ex.ToString();
            //return ex;
        }
    }

    private void ObjCell_Tapped(object sender, EventArgs e)
    {

        var tappedViewCell = (TextCell)sender;
        var textCell = tappedViewCell.Text.ToString().Split("|");
       
        entryOwnerName.Text = textCell[0].Trim();
        txtOwnerId.Text = textCell[1].Trim();
        OldOwnerName = entryOwnerName.Text;
    }

    private async void btnDeleteOwner_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(txtOwnerId.Text))
            {
                entryOwnerName.Text = entryOwnerName.Text.Trim();
                bool userResponse = await DisplayAlert("Warning", "Are you sure to delete?", "Yes", "No");
                if (userResponse)
                {
                    //check if there is any transaction in IncomeExpenseModel and Assets table before deleting the owner
                    var incomeExpenseRecord = await _dbConnection.Table<IncomeExpenseModel>().Where(i => i.OwnerName == entryOwnerName.Text).ToListAsync();
                    if (incomeExpenseRecord.Count > 0)
                    {
                        await DisplayAlert("Info", "Cannot delete owner since there are records with this owner.", "Ok");
                        return;
                    }
                    var assetsRecord = await _dbConnection.Table<Assets>().Where(a => a.Holder == entryOwnerName.Text).ToListAsync();
                    if (assetsRecord.Count > 0)
                    {
                        await DisplayAlert("Info", "Cannot delete owner since there are records with this owner.", "Ok");
                        return;
                    }
                    //check if there is any transaction in IncomeExpenseModel and Assets table before deleting the owner
                    Owners objOwner = new Owners()
                    {
                        OwnerId = Convert.ToInt32(txtOwnerId.Text)
                    };

                    await SetUpDb();
                    int rowsAffected = await _dbConnection.DeleteAsync(objOwner);
                    ClearOwnersForm();
                    await LoadOwnersInPage();
                }
            }
            else
            {
                await DisplayAlert("Info", "Please select an owner to delete", "Ok");
            }
        }
        catch(Exception ex)
        {
            await DisplayAlert("Error", "Something went wrong: " + ex.Message.ToString(), "Ok");
        }
    }

    public void ClearOwnersForm()
    {
        txtOwnerId.Text = "";
        entryOwnerName.Text = "";
    }
}