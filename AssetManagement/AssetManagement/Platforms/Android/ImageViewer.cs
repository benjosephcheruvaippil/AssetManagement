using Android.Content;

namespace AssetManagement.Platforms.Android
{
    public static class ImageViewer
    {
        public static void Open(string filePath)
        {
            var context = global::Android.App.Application.Context;

            var file = new Java.IO.File(filePath);

            var uri = AndroidX.Core.Content.FileProvider.GetUriForFile(
                context,
                context.PackageName + ".fileprovider",
                file);

            var intent = new Intent(Intent.ActionView);
            intent.SetDataAndType(uri, "image/*");
            intent.SetFlags(ActivityFlags.NewTask);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission);

            // Try Google Photos first
            intent.SetPackage("com.google.android.apps.photos");

            try
            {
                context.StartActivity(intent);
            }
            catch
            {
                intent.SetPackage(null);
                context.StartActivity(intent);
            }
        }
    }
}
