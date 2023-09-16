using AssetManagement.Services;
using AssetManagement.ViewModels;
using Mopups.Interfaces;

namespace AssetManagement.Views;

public partial class AppFlyoutPage : FlyoutPage
{
    public AssetListPageViewModel _viewModel;
    public IPopupNavigation _popupNavigation;
    public IAssetService _assetService;
    public AppFlyoutPage(AssetListPageViewModel viewModel, IPopupNavigation popupNavigation, IAssetService assetService)
	{
		InitializeComponent();
        
        _viewModel = viewModel;
        _popupNavigation = popupNavigation;
        _assetService = assetService;

        flyoutPage.btnExpensePage.Clicked += OpenHomePageClicked;
        flyoutPage.btnIncomePage.Clicked += OpenSecondPageClicked;       
    }

    private void OpenHomePageClicked(object sender, EventArgs e)
    {
        if (!((IFlyoutPageController)this).ShouldShowSplitMode)
            IsPresented = false;
        Detail = new NavigationPage(new AssetListPage(_viewModel, _popupNavigation, _assetService));
    }

    private void OpenSecondPageClicked(object sender, EventArgs e)
    {
        if (!((IFlyoutPageController)this).ShouldShowSplitMode)
            IsPresented = false;
        Detail = new NavigationPage(new AssetListPage(_viewModel, _popupNavigation, _assetService));
    }
}