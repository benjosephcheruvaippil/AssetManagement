using AssetManagement.Services;
using AssetManagement.ViewModels;
using AssetManagement.Views;
using Microsoft.Extensions.Logging;

namespace AssetManagement;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        builder.Services.AddSingleton<IAssetService, AssetService>();

        builder.Services.AddSingleton<AssetListPage>();

        builder.Services.AddSingleton<AssetListPageViewModel>();

        return builder.Build();
	}
}
