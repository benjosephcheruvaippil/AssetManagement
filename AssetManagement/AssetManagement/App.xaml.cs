using AssetManagement.Models.Constants;
using AssetManagement.Services;
using AssetManagement.ViewModels;
using AssetManagement.Views;
//using Mopups.Interfaces;
//using Mopups.Services;
using SQLite;

namespace AssetManagement;

public partial class App : Application
{
    private AssetListPageViewModel _viewModel;
    //private IPopupNavigation _popupNavigation;
    private readonly IAssetService _assetService;
    public App(AssetListPageViewModel viewModel, IAssetService assetService)
	{
		InitializeComponent();
        _viewModel= viewModel;
        //_popupNavigation= popupNavigation;
        _assetService= assetService;
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCeUx0R3xbf1x1ZFxMZVVbRHNPMyBoS35Rc0VkWHpeeXFcRmVUVUZ3VEFd");

        bool isFirstLaunch = Preferences.Get("IsFirstLaunch", true);

        if (isFirstLaunch)
        {
            MainPage = new NavigationPage(new AppLaunchPage(_viewModel, _assetService,Constants.FromLaunchPage));
            Preferences.Set("IsFirstLaunch", false);
        }
        else
        {
            //MainPage = new NavigationPage(new AssetListPage(_viewModel, _popupNavigation, _assetService));
            MainPage = new AppFlyoutPage(_viewModel, _assetService);
        }
    }
}
