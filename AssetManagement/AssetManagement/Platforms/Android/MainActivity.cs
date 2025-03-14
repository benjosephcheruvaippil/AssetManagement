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

    protected override async void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Set the current activity resolver for fingerprint plugin
        CrossFingerprint.SetCurrentActivityResolver(() => this);

        CommonFunctions objCommon = new CommonFunctions();
        bool isAuthenticated = await objCommon.AuthenticateWithBiometricsAsync();
        if (!isAuthenticated)
        {
            Finish();
        }

        // Other initialization code
    }

    private bool isFingerprintChecked = false;

    protected override async void OnResume()
    {
        base.OnResume();
        if (!isFingerprintChecked)
        {
            CommonFunctions objCommon = new CommonFunctions();
            bool isAuthenticated = await objCommon.AuthenticateWithBiometricsAsync();
            if(isAuthenticated)
            {
                isFingerprintChecked = true;
            }
            else
            {
                Finish();
            } 
        }
    }
}
