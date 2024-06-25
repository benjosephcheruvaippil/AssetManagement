using AndroidX.Lifecycle;
using AssetManagement.Common;
using AssetManagement.Models;
using AssetManagement.Models.Constants;
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

    protected override void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            CommonFunctions objCommon = new CommonFunctions();
            objCommon.CommonDataForDefaults();
            LoadCurrenctListInDropdown();
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

    private async void LoadCurrenctListInDropdown()
    {
        try
        {
            await SetUpDb();
            var currencyList = await _dbConnection.Table<Currency>().ToListAsync();
            //IncomeExpenseCategories objCategories = new IncomeExpenseCategories
            //{
            //    IncomeExpenseCategoryId = 0,
            //    CategoryName = Constants.AddNewCategoryOption,
            //    CategoryType = "Expense"
            //};
            //expenseCategories.Add(objCategories);
            pickerCurrencyList.ItemsSource = currencyList.Select(c => c.CurrencyName).ToList();
            //if (expenseCategories.Count > 1)
            //{
            //    pickerExpenseCategory.SelectedIndex = 0;
            //}
        }
        catch (Exception)
        {
            return;
        }
    }

    private async void Done_Clicked(object sender, EventArgs e)
    {
        if (_from == Constants.FromLaunchPage)
        {
            //add user currency into table
            await SetUpDb();
            UserCurrency userCurrency = new UserCurrency
            {
                Country = "",
                CurrencyName = "",
                CurrencyCode = ""
            };
            await _dbConnection.InsertAsync(userCurrency);
            //add user currency into table

           

            //Constants.SetCurrency("en-US");
            Application.Current.MainPage = new AppFlyoutPage(_viewModel, _assetService);
        }
    }

    private void pickerCurrencyList_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
}