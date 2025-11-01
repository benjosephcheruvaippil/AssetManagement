using AssetManagement.Models;
using AssetManagement.Models.DataTransferObject;
using AssetManagement.ViewModels;
using SQLite;
using Syncfusion.Maui.Toolkit.Chips;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AssetManagement.Views;

public partial class ManageAssetTypePage : ContentPage
{
    private SQLiteAsyncConnection _dbConnection;
    private readonly AssetTypeViewModel _viewModel;
    Frame _selectedFrame = null;
    private int pageSize = 3;
    private List<AssetTypeDTO> _allItems; // store full list
    //private ObservableCollection<AssetTypeDTO> assetTypeList = new ObservableCollection<AssetTypeDTO>();
    public ManageAssetTypePage(AssetTypeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = new AssetTypeViewModel();
        BindingContext = _viewModel;
        //PopulateAssetTypeList("onLoad");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _allItems = await _viewModel.GetAllAssetTypes(); // full dataset
        dataPager.Source = _allItems;
        dataPager.PageSize = pageSize;

        _viewModel.LoadPagedData(_allItems, 1, pageSize); // load page 1
    }

    private async Task SetUpDb()
    {
        try
        {
            if (_dbConnection == null)
            {
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
                _dbConnection = new SQLiteAsyncConnection(dbPath);
                await _dbConnection.CreateTableAsync<AssetTypeModel>();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    //public async void PopulateAssetTypeList(string operation)
    //{
    //    await SetUpDb();
    //    var assetTypes = (await _dbConnection.Table<AssetTypeModel>().ToListAsync())
    //        .Select(s => new AssetTypeDTO
    //        {
    //            AssetTypeId = s.AssetTypeId,
    //            AssetTypeName = s.AssetTypeName,
    //            Description = s.Description,
    //            EnableAsOfDate = s.EnableAsOfDate,
    //            EnableMaturityDate = s.EnableMaturityDate,
    //            IncludeInNetWorth = s.IncludeInNetWorth,
    //            CategoryTag = s.CategoryTag   
    //        }).ToList();

    //    assetTypeList.Clear();
    //    foreach (var assetType in assetTypes)
    //    {
    //        assetTypeList.Add(assetType);
    //    }

    //    dataPager.Source = assetTypeList;
    //    if (operation == "onSave" || operation == "onDelete")
    //    {
    //        dataPager.MoveToFirstPage();
    //    }
    //    dataPager.PageSize = pageSize;
    //    //verticalListView.ItemsSource = assetTypeList.Skip(0).Take(pageSize).ToList();
    //}

    private void verticalListView_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (e.DataItem is AssetTypeDTO tappedItem)
        {
            var chipToSelect = assetTypeTagChipGroup.Items
                                .OfType<SfChip>()
                                .FirstOrDefault(c => c.Text == tappedItem.CategoryTag);

            txtAssetTypeId.Text = tappedItem.AssetTypeId.ToString();
            entryAssetTypeName.Text = tappedItem.AssetTypeName;
            //entryAssetTypeDescription.Text = tappedItem.Description;
            maturityDateAsOfDateRadioGroup.SelectedValue = tappedItem.EnableMaturityDate == true ? "MD" : "AOD";
            chkIncludeInNetworth.IsChecked = tappedItem.IncludeInNetWorth;
            assetTypeTagChipGroup.SelectedItem = chipToSelect;
        }
    }

    private async void dataPager_PageChanged(object sender, Syncfusion.Maui.DataGrid.DataPager.PageChangedEventArgs e)
    {
        //int newPage = e.NewPageIndex;
        //int oldPage = e.OldPageIndex;

        //var pageSize = dataPager.PageSize;
        //var startIndex = newPage * pageSize;

        int newPage = e.NewPageIndex + 1; // DataPager is 0-based
        _viewModel.LoadPagedData(_allItems, newPage, pageSize);

        if (_selectedFrame != null)
        {
            _selectedFrame.BackgroundColor = Colors.White;
            _selectedFrame.BorderColor = Colors.LightGray;
            _selectedFrame = null;
        }

        // Assuming you have your original list in a variable `items`
        //var currentPageItems = assetTypeList.Skip(startIndex).Take(pageSize).ToList();

        //verticalListView.ItemsSource = currentPageItems;
    }

    private async void btnSave_Clicked(object sender, EventArgs e)
    {
        if(string.IsNullOrWhiteSpace(entryAssetTypeName.Text))
        {
            await DisplayAlert("Validation Error", "Asset Type Name is required.", "OK");
            return;
        }

        string CategoryTag = "";
        if (assetTypeTagChipGroup.SelectedItem is SfChip chip)
        {
            CategoryTag = chip.Text;
        }

        AssetTypeModel objAssetTypeModel = new AssetTypeModel()
        {
            AssetTypeId = string.IsNullOrEmpty(txtAssetTypeId.Text) ? 0 : int.Parse(txtAssetTypeId.Text),
            AssetTypeName = entryAssetTypeName.Text.Trim(),
            //Description = entryAssetTypeDescription.Text.Trim(),
            EnableMaturityDate = maturityDateAsOfDateRadioGroup.SelectedValue?.ToString() == "MD" ? true : false,
            EnableAsOfDate = maturityDateAsOfDateRadioGroup.SelectedValue?.ToString() == "AOD" ? true : false,
            IncludeInNetWorth = chkIncludeInNetworth.IsChecked,
            CategoryTag = CategoryTag
        };

        await SetUpDb();

        if (string.IsNullOrEmpty(txtAssetTypeId.Text))
        {
            int rowsAffected = await _dbConnection.InsertAsync(objAssetTypeModel);
        }
        else
        {
            await _dbConnection.UpdateAsync(objAssetTypeModel);
        }

        ClearFields();

        _allItems = await _viewModel.GetAllAssetTypes(); // full dataset
        dataPager.Source = _allItems;
        dataPager.PageSize = pageSize;

        _viewModel.LoadPagedData(_allItems, 1, pageSize); // load page 1

        //PopulateAssetTypeList("onSave");
    }

    private void btnClear_Clicked(object sender, EventArgs e)
    {
        ClearFields();
    }

    public void ClearFields()
    {
        txtAssetTypeId.Text = "";
        entryAssetTypeName.Text = "";
        //entryAssetTypeDescription.Text = "";
        maturityDateAsOfDateRadioGroup.SelectedValue = "MD";
        chkIncludeInNetworth.IsChecked = false;
        assetTypeTagChipGroup.SelectedItem = null;
    }

    private async void btnDelete_Clicked(object sender, EventArgs e)
    {
        await SetUpDb();
        AssetTypeModel objAssetTypeModel = new AssetTypeModel()
        {
            AssetTypeId = string.IsNullOrEmpty(txtAssetTypeId.Text) ? 0 : int.Parse(txtAssetTypeId.Text)
        };

        int result = await _dbConnection.DeleteAsync(objAssetTypeModel);
        if (result > 0)
        {
            await DisplayAlert("Success", "Record deleted successfully.", "OK");
        }
        else
        {
            await DisplayAlert("Error", "Failed to delete the record.", "OK");
        }

        ClearFields();

        _allItems = await _viewModel.GetAllAssetTypes(); // full dataset
        dataPager.Source = _allItems;
        dataPager.PageSize = pageSize;

        _viewModel.LoadPagedData(_allItems, 1, pageSize); // load page 1

        //await _viewModel.LoadData(0);
        //PopulateAssetTypeList("onDelete");
    }

    //private void CollectionView_SelectionChanged(object sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
    //{
    //    if (e.CurrentSelection.FirstOrDefault() is AssetTypeModel tappedItem)
    //    {
    //        var chipToSelect = assetTypeTagChipGroup.Items
    //                            .OfType<SfChip>()
    //                            .FirstOrDefault(c => c.Text == tappedItem.CategoryTag);

    //        txtAssetTypeId.Text = tappedItem.AssetTypeId.ToString();
    //        entryAssetTypeName.Text = tappedItem.AssetTypeName;
    //        //entryAssetTypeDescription.Text = tappedItem.Description;
    //        maturityDateAsOfDateRadioGroup.SelectedValue = tappedItem.EnableMaturityDate == true ? "MD" : "AOD";
    //        chkIncludeInNetworth.IsChecked = tappedItem.IncludeInNetWorth;
    //        assetTypeTagChipGroup.SelectedItem = chipToSelect;
    //    }

    //    (sender as CollectionView).SelectedItem = null;
    //}

    private void OnCardTapped(object sender, EventArgs e)
    {
        if ((sender as Frame)?.BindingContext is AssetTypeDTO tappedItem)
        {
            // Populate your fields
            txtAssetTypeId.Text = tappedItem.AssetTypeId.ToString();
            entryAssetTypeName.Text = tappedItem.AssetTypeName;
            maturityDateAsOfDateRadioGroup.SelectedValue = (bool)tappedItem.EnableMaturityDate ? "MD" : "AOD";
            chkIncludeInNetworth.IsChecked = tappedItem.IncludeInNetWorth;

            var chipToSelect = assetTypeTagChipGroup.Items
                                .OfType<SfChip>()
                                .FirstOrDefault(c => c.Text == tappedItem.CategoryTag);
            assetTypeTagChipGroup.SelectedItem = chipToSelect;

            // Highlight logic
            var tappedFrame = sender as Frame;

            // Remove previous highlight
            if (_selectedFrame != null && _selectedFrame != tappedFrame)
            {
                _selectedFrame.BackgroundColor = Colors.White;
                _selectedFrame.BorderColor = Colors.LightGray;
            }

            // Set new highlight
            tappedFrame.BackgroundColor = Color.FromArgb("#5D6D7E");
            tappedFrame.BorderColor = Color.FromArgb("#5D6D7E");

            // Keep track of currently selected frame
            _selectedFrame = tappedFrame;
        }
    }
}