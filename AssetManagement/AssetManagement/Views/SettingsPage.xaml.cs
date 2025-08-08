using AssetManagement.Models;
using AssetManagement.Models.Constants;
using AssetManagement.Services;
using AssetManagement.ViewModels;
using CommunityToolkit.Maui.Storage;
using SQLite;

namespace AssetManagement.Views;

public partial class SettingsPage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    private AssetListPageViewModel _viewModel;
    private readonly IAssetService _assetService;
    private readonly IAppRestarter _appRestarter;
    public SettingsPage(AssetListPageViewModel viewModel, IAssetService assetService, IAppRestarter appRestarter)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _assetService = assetService;
        _appRestarter = appRestarter;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await SetUpDb();
        var lockDetails = await _dbConnection.Table<Owners>().FirstOrDefaultAsync();
        if (lockDetails != null)
        {
            lockSwitch.IsToggled = lockDetails.Locked == null ? false : (bool)lockDetails.Locked;

            if (lockSwitch.IsToggled)
            {
                lblLockText.Text = "App Lock On";
            }
            else if(!lockSwitch.IsToggled)
            {
                lblLockText.Text = "App Lock Off";
            }
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

    private async void btnBackup_Clicked(object sender, EventArgs e)
    {
        try
        {
            string sourceDatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
            if (!File.Exists(sourceDatabasePath))
            {
                await DisplayAlert("Info", "File not found", "OK");
                return;
            }
            //string destinationBackupPath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, "Assets.db3");

            //SetUpDb();
            //string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
            //_dbConnection = new SQLiteAsyncConnection(dbPath);
            //await _dbConnection.BackupAsync(destinationBackupPath);

            //trying new way of achieving it
            byte[] fileBytes = File.ReadAllBytes(sourceDatabasePath);
            var stream = new MemoryStream(fileBytes);
            CancellationTokenSource Ctoken = new CancellationTokenSource();
            string fileName = "Assets_" + DateTime.Now.ToString("dd-MM-yyyy hh:mm tt") + ".db3";

            var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream, Ctoken.Token);
            if (fileSaverResult.IsSuccessful)
            {
                await DisplayAlert("Message", "Backup file saved in " + fileSaverResult.FilePath, "Ok");
            }
            //trying new way of achieving it

            //await DisplayAlert("Info", "Backup Successful", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void btnRestoreDb_Clicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Pick File Please"
            });

            if (result == null)
                return;

            Stream fileStream = await result.OpenReadAsync();

            //SetUpDb();
            string destinationFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
            //if (File.Exists(destinationFilePath))
            //{
            //    File.Delete(destinationFilePath);
            //}

            using (var destinationFileStream = File.Create(destinationFilePath))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.CopyTo(destinationFileStream);
            }

            //string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
            //_dbConnection = new SQLiteAsyncConnection(destinationFilePath);
            //await _dbConnection.CreateTableAsync<Assets>();
            //await _dbConnection.CreateTableAsync<IncomeExpenseModel>();
            //await _dbConnection.CreateTableAsync<DataSyncAudit>();

            await DisplayAlert("Info", "Database restored successfully. App will get restarted to load the database.", "OK");
            //this code is android specific for restarting the app programmatically
            _appRestarter.RestartApp();
            //this code is android specific for restarting the app programmatically
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void btnManageOwners_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ManageUsersPage());
    }

    private async void btnManageIncomeExpenseCategories_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ManageCategoriesPage());
    }

    private async void btnSelectCurrency_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AppLaunchPage(_viewModel, _assetService, Constants.FromSettingsPage));
    }

    private async void lockSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        await SetUpDb();
        if (e.Value)
        {
            var biometricService = DependencyService.Get<IBiometricService>();

            if (biometricService != null && biometricService.IsBiometricEnrolled())
            {
                lblLockText.Text = "App Lock On";
                int rowsAffected = await _dbConnection.ExecuteAsync("Update Owners set Locked=?", true);
            }
            else
            {
                await DisplayAlert("Biometric Not Set", "Please add at least one Fingerprint or Face ID before enabling app lock.", "OK");
                lockSwitch.IsToggled = false;
            }

            //lblLockText.Text = "App Lock On";
            //int rowsAffected = await _dbConnection.ExecuteAsync("Update Owners set Locked=?", true);
            //await DisplayAlert("Message", "App locked using pattern/fingerprint", "OK");
        }
        else
        {
            lblLockText.Text = "App Lock Off";
            int rowsAffected = await _dbConnection.ExecuteAsync("Update Owners set Locked=?", false);

            //await DisplayAlert("Message", "App lock is removed", "OK");
        }
    }
}