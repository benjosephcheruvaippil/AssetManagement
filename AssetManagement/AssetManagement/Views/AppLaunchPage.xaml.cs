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
    public readonly string _from;
    public AppLaunchPage(AssetListPageViewModel viewModel, IAssetService assetService,string from)
	{
		InitializeComponent();
        _viewModel = viewModel;
        _assetService = assetService;
        _from = from;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        if (_from == Constants.FromLaunchPage)
        {
            Constants.SetCurrency("en-US");
            Application.Current.MainPage = new AppFlyoutPage(_viewModel, _assetService);
        }
    }
}