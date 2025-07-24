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
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzk2MjU0M0AzMzMwMmUzMDJlMzAzYjMzMzAzYkZnOUNPVDVIUEFhNlgyL3hwaktWSUQrOEV2OFgya1ZZQ1QwNVF0VytXU3c9;Mzk2MjU0NEAzMzMwMmUzMDJlMzAzYjMzMzAzYkxuY3pKNldRVTNkeTE0K2V1S3NLbENvTjRMN1M1d2NteXl5UTVHeHc0S009;Mgo+DSMBPh8sVXN0S0d+X1ZPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9mSXlRdUVjWX9bcHBWQmhXUkQ=;Mzk2MjU0NkAzMzMwMmUzMDJlMzAzYjMzMzAzYmRKM1ZFVmJVNzZsM1RidEp3K2hyRUQvN25YeFkvWmNDcUdOWTNGUnlMQ2M9;Mgo+DSMBMAY9C3t3VVhhQlJDfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTH5UdEZjWX9edXRRT2deWkd2;Mzk2MjU0OEAzMzMwMmUzMDJlMzAzYjMzMzAzYkU5aC9oT3JCM1o4VzRiajJGMzk5MW1sbExHa3VPamFydnFtdk83Tmcwd1U9;Mzk2MjU0OUAzMzMwMmUzMDJlMzAzYjMzMzAzYmRKM1ZFVmJVNzZsM1RidEp3K2hyRUQvN25YeFkvWmNDcUdOWTNGUnlMQ2M9");

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
