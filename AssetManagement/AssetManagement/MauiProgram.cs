using AssetManagement.Services;
using AssetManagement.ViewModels;
using AssetManagement.Views;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using Mopups.Interfaces;
using Mopups.Services;

namespace AssetManagement;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureMopups()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);
        builder.Services.AddSingleton<IAssetService, AssetService>();
		builder.Services.AddSingleton<IPopupNavigation>(MopupService.Instance);

        builder.Services.AddSingleton<AssetListPage>();
        builder.Services.AddSingleton<AssetsByCategoryPage>();

        builder.Services.AddSingleton<AssetListPageViewModel>();

        return builder.Build();
	}
}
