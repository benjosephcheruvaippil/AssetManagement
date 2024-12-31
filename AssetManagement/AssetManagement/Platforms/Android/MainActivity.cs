using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Plugin.Fingerprint;
using Java.Lang;
using SQLite;
using AssetManagement.Common;

namespace AssetManagement;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    //protected override void OnCreate(Bundle savedInstanceState)
    //{
    //    base.OnCreate(savedInstanceState);
    //}

    protected async override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Set the current activity resolver for fingerprint plugin
        CrossFingerprint.SetCurrentActivityResolver(() => this);

        CommonFunctions objCommon = new CommonFunctions();
        await objCommon.AuthenticateWithBiometricsAsync();

        // Other initialization code
    }
}
