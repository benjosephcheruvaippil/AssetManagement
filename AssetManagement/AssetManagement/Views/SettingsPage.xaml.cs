using AssetManagement.Models;
using AssetManagement.Models.Constants;
using AssetManagement.Services;
using AssetManagement.ViewModels;
using CommunityToolkit.Maui.Storage;
using SQLite;
using System.IO.Compression;

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
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    private async void btnBackup_Clicked(object sender, EventArgs e)
    {
        try
        {
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Assets.db3");

            if (!File.Exists(dbPath))
            {
                await DisplayAlertAsync("Info", "Database not found", "OK");
                return;
            }

            string basePath = FileSystem.AppDataDirectory;
            string appDataPath = Path.Combine(FileSystem.AppDataDirectory, "media");

            // Create temporary backup folder
            string tempBackupFolder = Path.Combine(basePath, "TempBackup");

            if (Directory.Exists(tempBackupFolder))
                Directory.Delete(tempBackupFolder, true);

            Directory.CreateDirectory(tempBackupFolder);

            // 1️⃣ Copy database
            File.Copy(dbPath, Path.Combine(tempBackupFolder, "Assets.db3"), true);

            // 2️⃣ Copy ALL files from AppDataDirectory (except db and temp folder)
            string mediaBackupPath = Path.Combine(tempBackupFolder, "media");
            Directory.CreateDirectory(mediaBackupPath);

            foreach (var file in Directory.GetFiles(appDataPath))
            {
                string fileName = Path.GetFileName(file);

                // Skip database and temp files
                if (fileName == "Assets.db3")
                    continue;

                File.Copy(file, Path.Combine(mediaBackupPath, fileName), true);
            }

            // 3️⃣ Create ZIP
            string zipFileName = "AssetsBackup_" +
                                 DateTime.Now.ToString("dd-MM-yyyy_HH-mm") +
                                 ".zip";

            string zipPath = Path.Combine(basePath, zipFileName);

            if (File.Exists(zipPath))
                File.Delete(zipPath);

            ZipFile.CreateFromDirectory(tempBackupFolder, zipPath);

            // 4️⃣ Save using FileSaver
            using var stream = new MemoryStream(File.ReadAllBytes(zipPath));

            CancellationTokenSource cts = new CancellationTokenSource();

            var result = await FileSaver.Default.SaveAsync(zipFileName, stream, cts.Token);

            await DisplayAlertAsync("Success", "Backup saved successfully", "OK");

            // Cleanup
            Directory.Delete(tempBackupFolder, true);
            File.Delete(zipPath);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    private async void btnRestoreDb_Clicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select Backup File"
            });

            if (result == null)
                return;

            if (result.FileName.EndsWith(".db3") || result.FileName.EndsWith(".db"))
            {
                Stream fileStream = await result.OpenReadAsync();

                string destinationFilePath = Path.Combine(FileSystem.AppDataDirectory, "Assets.db3");

                using (var destinationFileStream = File.Create(destinationFilePath))
                {
                    fileStream.Seek(0, SeekOrigin.Begin);
                    fileStream.CopyTo(destinationFileStream);
                }

                await DisplayAlertAsync("Info", "Database restored successfully. App will get restarted to load the database.", "OK");
                //this code is android specific for restarting the app programmatically
                _appRestarter.RestartApp();
                return;
            }

            string basePath = FileSystem.AppDataDirectory;
            string appDataPath = Path.Combine(FileSystem.AppDataDirectory, "media");

            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
            Directory.CreateDirectory(appDataPath);

            string tempRestoreFolder = Path.Combine(basePath, "TempRestore");

            if (Directory.Exists(tempRestoreFolder))
                Directory.Delete(tempRestoreFolder, true);
            Directory.CreateDirectory(tempRestoreFolder);

            // 1️⃣ Extract ZIP
            ZipFile.ExtractToDirectory(result.FullPath, tempRestoreFolder);

            // 2️⃣ Close DB connection before replacing
            await _dbConnection.CloseAsync();

            // 3️⃣ Restore database
            string restoredDbPath = Path.Combine(tempRestoreFolder, "Assets.db3");
            string originalDbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Assets.db3");

            if (File.Exists(restoredDbPath))
            {
                File.Copy(restoredDbPath, originalDbPath, true);
            }

            // 4️⃣ Restore files into AppDataDirectory
            string mediaPath = Path.Combine(tempRestoreFolder, "media");
            foreach (var file in Directory.GetFiles(mediaPath))
            {
                string fileName = Path.GetFileName(file);

                if (fileName == "Assets.db3")
                    continue;

                string destinationPath = Path.Combine(appDataPath, fileName);
                File.Copy(file, destinationPath, true);
            }

            Directory.Delete(tempRestoreFolder, true);

            await DisplayAlertAsync("Success",
                "Restore completed. App will restart to load the database.",
                "OK");

            _appRestarter.RestartApp();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "There is an error with the file. Please unzip the file and find assets.db3 and restore that file again.", "OK");
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
            lblLockText.Text = "App Lock On";
            int rowsAffected = await _dbConnection.ExecuteAsync("Update Owners set Locked=?", true);
            //await DisplayAlertAsync("Message", "App locked using pattern/fingerprint", "OK");
        }
        else
        {
            lblLockText.Text = "App Lock Off";
            int rowsAffected = await _dbConnection.ExecuteAsync("Update Owners set Locked=?", false);

            //await DisplayAlertAsync("Message", "App lock is removed", "OK");
        }
    }
}