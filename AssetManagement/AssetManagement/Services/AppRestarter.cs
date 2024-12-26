using AssetManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: Dependency(typeof(AppRestarter))]
namespace AssetManagement.Services
{
    public class AppRestarter:IAppRestarter
    {
        public void RestartApp()
        {

            var context = Android.App.Application.Context;
            var packageManager = context.PackageManager;
            var intent = packageManager.GetLaunchIntentForPackage(context.PackageName);

            if (intent != null)
            {
                // Add flags to restart the app
                intent.AddFlags(Android.Content.ActivityFlags.ClearTop |
                                Android.Content.ActivityFlags.NewTask |
                                Android.Content.ActivityFlags.ClearTask);

                context.StartActivity(intent);
            }

            // Terminate the current process
            Java.Lang.JavaSystem.Exit(0);
        }
    }
}
