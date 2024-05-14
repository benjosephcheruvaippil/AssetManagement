using AssetManagement.Services;
using AssetManagement.ViewModels;
//using Mopups.Interfaces;
using SQLite;

namespace AssetManagement.Views;

public partial class AppFlyoutPage : FlyoutPage
{
    public AssetListPageViewModel _viewModel;
    //public IPopupNavigation _popupNavigation;
    public IAssetService _assetService;
    public AppFlyoutPage(AssetListPageViewModel viewModel, IAssetService assetService)
	{
		InitializeComponent();
        
        _viewModel = viewModel;
        //_popupNavigation = popupNavigation;
        _assetService = assetService;

        flyoutPage.btnExpensePage.Clicked += OpenExpensePageClicked;
        flyoutPage.btnIncomePage.Clicked += OpenIncomePageClicked;
        flyoutPage.btnAssetPage.Clicked += OpenAssetPageClicked;
        //flyoutPage.btnAssetCloudPage.Clicked += OpenAssetCloudPageClicked;
        flyoutPage.btnIncomeExpenseReport.Clicked += OpenIncomeExpenseReportPageClicked;
        flyoutPage.btnAssetReport.Clicked += OpenAssetReportPageClicked;
        flyoutPage.btnSettings.Clicked += OpenSettingsPageClicked;
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
        Detail = new NavigationPage(new AssetPage(_viewModel, _assetService));
    }

    //private void OpenAssetCloudPageClicked(object sender, EventArgs e)
    //{
    //    if (!((IFlyoutPageController)this).ShouldShowSplitMode)
    //        IsPresented = false;
    //    Detail = new NavigationPage(new AssetCloudPage(_viewModel, _popupNavigation, _assetService));
    //}

    private void OpenIncomeExpenseReportPageClicked(object sender, EventArgs e)
    {
        if (!((IFlyoutPageController)this).ShouldShowSplitMode)
            IsPresented = false;
        Detail = new NavigationPage(new IncomeExpenseReportsPage());
    }

    private void OpenAssetReportPageClicked(object sender, EventArgs e)
    {
        if (!((IFlyoutPageController)this).ShouldShowSplitMode)
            IsPresented = false;
        Detail = new NavigationPage(new AssetReportPage(_viewModel, _assetService));
    }

    private void OpenSettingsPageClicked(object sender, EventArgs e)
    {
        if (!((IFlyoutPageController)this).ShouldShowSplitMode)
            IsPresented = false;
        Detail = new NavigationPage(new SettingsPage());
    }
}