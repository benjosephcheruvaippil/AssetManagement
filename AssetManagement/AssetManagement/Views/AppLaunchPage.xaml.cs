using AndroidX.Lifecycle;
using AssetManagement.Common;
using AssetManagement.Models;
using AssetManagement.Models.Constants;
using AssetManagement.Models.DataTransferObject;
using AssetManagement.Services;
using AssetManagement.ViewModels;
using SQLite;

namespace AssetManagement.Views;

public partial class AppLaunchPage : ContentPage
{
    private AssetListPageViewModel _viewModel;
    private SQLiteAsyncConnection _dbConnection;
    private readonly IAssetService _assetService;
    public readonly string _from;
    public AppLaunchPage(AssetListPageViewModel viewModel, IAssetService assetService,string from)
	{
		InitializeComponent();
        _viewModel = viewModel;
        _assetService = assetService;
        _from = from;
    }

    protected async override void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            CommonFunctions objCommon = new CommonFunctions();
            await objCommon.CommonDataForDefaults();
            await LoadCurrenctListInDropdown();
        }
        catch (Exception)
        {
            //await DisplayAlert("Error", ex.Message, "OK");
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
                await _dbConnection.CreateTableAsync<Assets>();
                await _dbConnection.CreateTableAsync<IncomeExpenseModel>();
                await _dbConnection.CreateTableAsync<IncomeExpenseCategories>();
                await _dbConnection.CreateTableAsync<DataSyncAudit>();
                await _dbConnection.CreateTableAsync<AssetAuditLog>();
                await _dbConnection.CreateTableAsync<Owners>();
                await _dbConnection.CreateTableAsync<UserCurrency>();
                await _dbConnection.CreateTableAsync<Currency>();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task LoadCurrenctListInDropdown()
    {
        try
        {
            await SetUpDb();
            var currencyList = await _dbConnection.Table<Currency>().ToListAsync();
            var currenctListOrdered = currencyList.OrderBy(x => x.Country).Select(s => new CurrencyDTO
            {
                CurrencyId = s.CurrencyId,
                DisplayText = s.Country + " - " + s.CurrencyName
            }).ToList();

            pickerCurrencyList.ItemsSource = currenctListOrdered;
            pickerCurrencyList.ItemDisplayBinding = new Binding("DisplayText");
            //check if user currency is already saved
            if (_from == Constants.FromSettingsPage)
            {
                var userCurrency = await _dbConnection.Table<UserCurrency>().FirstOrDefaultAsync();
                if (userCurrency != null)
                {
                    string displayText = userCurrency.Country + " - " + userCurrency.CurrencyName;
                    pickerCurrencyList.SelectedItem = currenctListOrdered.FirstOrDefault(c => c.DisplayText == displayText);
                }
            }
            //check if user currency is already saved
        }
        catch (Exception)
        {
            return;
        }
    }

    private async void Done_Clicked(object sender, EventArgs e)
    {
        if (pickerCurrencyList.SelectedIndex == -1)
        {
            await DisplayAlert("Message", "Please select a currency to continue", "Ok");
            return;
        }
        //add user currency into table
        await SetUpDb();
        await _dbConnection.ExecuteAsync("DELETE FROM UserCurrency");
        if (pickerCurrencyList.SelectedItem is CurrencyDTO selectedItem)
        {
            int id = selectedItem.CurrencyId;
            var currencyDetails = await _dbConnection.Table<Currency>().Where(c => c.CurrencyId == id).FirstOrDefaultAsync();
            UserCurrency userCurrency = new UserCurrency
            {
                Country = currencyDetails.Country,
                CurrencyName = currencyDetails.CurrencyName,
                CurrencyCode = currencyDetails.CurrencyCode
            };
            await _dbConnection.InsertAsync(userCurrency);
            Constants.SetCurrency(userCurrency.CurrencyCode);
        }
        //add user currency into table
        if (_from == Constants.FromLaunchPage)
        {
            Application.Current.MainPage = new AppFlyoutPage(_viewModel, _assetService);
        }
        else if (_from == Constants.FromSettingsPage)
        {
            await DisplayAlert("Message", "Saved", "Ok");
        }
    }

    private void pickerCurrencyList_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
}