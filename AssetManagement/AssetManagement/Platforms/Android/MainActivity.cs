﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Java.Lang;
using SQLite;

namespace AssetManagement;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    //protected override void OnCreate(Bundle savedInstanceState)
    //{
    //    base.OnCreate(savedInstanceState);
    //}

}
