using AssetManagement.Services;
using AssetManagement.ViewModels;
using AssetManagement.Views;
using Mopups.Interfaces;
using Mopups.Services;
using SQLite;

namespace AssetManagement;

public partial class App : Application
{
    private AssetListPageViewModel _viewModel;
    private IPopupNavigation _popupNavigation;
    private readonly IAssetService _assetService;
    public App(AssetListPageViewModel viewModel, IPopupNavigation popupNavigation, IAssetService assetService)
	{
		InitializeComponent();
        _viewModel= viewModel;
        _popupNavigation= popupNavigation;
        _assetService= assetService;

        MainPage = new NavigationPage(new AssetListPage(_viewModel, _popupNavigation, _assetService));
    }
}
