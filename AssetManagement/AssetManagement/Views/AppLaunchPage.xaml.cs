using AndroidX.Lifecycle;
using AssetManagement.Models.Constants;
using AssetManagement.Services;
using AssetManagement.ViewModels;

namespace AssetManagement.Views;

public partial class AppLaunchPage : ContentPage
{
    private AssetListPageViewModel _viewModel;
    //private IPopupNavigation _popupNavigation;
    private readonly IAssetService _assetService;
    public AppLaunchPage(AssetListPageViewModel viewModel, IAssetService assetService)
	{
		InitializeComponent();
        _viewModel = viewModel;
        _assetService = assetService;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Constants.SetCurrency("en-US");
        Application.Current.MainPage = new AppFlyoutPage(_viewModel, _assetService);
    }
}