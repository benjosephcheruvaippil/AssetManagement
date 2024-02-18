using AssetManagement.Models;
using CommunityToolkit.Maui.Storage;
using SQLite;

namespace AssetManagement.Views;

public partial class SettingsPage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    public SettingsPage()
	{
		InitializeComponent();
	}

    //private async Task SetUpDb()
    //{
    //    try
    //    {
    //        if (_dbConnection == null)
    //        {
    //            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
    //            _dbConnection = new SQLiteAsyncConnection(dbPath);
    //            await _dbConnection.CreateTableAsync<Assets>();
    //            await _dbConnection.CreateTableAsync<IncomeExpenseModel>();
    //            await _dbConnection.CreateTableAsync<DataSyncAudit>();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        await DisplayAlert("Error", ex.Message, "OK");
    //    }
    //}

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
                await DisplayAlert("Message", "Database saved in " + fileSaverResult.FilePath, "Ok");
            }
            //trying new way of achieving it

            await DisplayAlert("Info", "Backup Successful", "OK");
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
            if (File.Exists(destinationFilePath))
            {
                File.Delete(destinationFilePath);
            }

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

            await DisplayAlert("Info", "Database restored successfully", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}