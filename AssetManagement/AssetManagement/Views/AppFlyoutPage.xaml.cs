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

        flyoutPage.btnExpensePage.Clicked += OpenExpensePageClicked;
        flyoutPage.btnIncomePage.Clicked += OpenIncomePageClicked;
        flyoutPage.btnAssetPage.Clicked += OpenAssetPageClicked;
        flyoutPage.btnIncomeExpenseReport.Clicked += OpenIncomeExpenseReportPageClicked;
        flyoutPage.btnAssetReport.Clicked += OpenAssetReportPageClicked;
    }

    private void OpenExpensePageClicked(object sender, EventArgs e)
    {
        if (!((IFlyoutPageController)this).ShouldShowSplitMode)
            IsPresented = false;
        Detail = new NavigationPage(new ExpensePage());
    }

    private void OpenIncomePageClicked(object sender, EventArgs e)
    {
        if (!((IFlyoutPageController)this).ShouldShowSplitMode)
            IsPresented = false;
        Detail = new NavigationPage(new IncomePage());
    }

    private void OpenAssetPageClicked(object sender, EventArgs e)
    {
        if (!((IFlyoutPageController)this).ShouldShowSplitMode)
            IsPresented = false;
        Detail = new NavigationPage(new AssetPage());
    }

    private void OpenIncomeExpenseReportPageClicked(object sender, EventArgs e)
    {
        if (!((IFlyoutPageController)this).ShouldShowSplitMode)
            IsPresented = false;
        Detail = new NavigationPage(new AssetListPage(_viewModel, _popupNavigation, _assetService));
    }

    private void OpenAssetReportPageClicked(object sender, EventArgs e)
    {
        if (!((IFlyoutPageController)this).ShouldShowSplitMode)
            IsPresented = false;
        Detail = new NavigationPage(new AssetListPage(_viewModel, _popupNavigation, _assetService));
    }
}