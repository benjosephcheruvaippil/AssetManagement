using Android.Content;
using Android.Graphics;
using Android.Graphics.Pdf;
using Android.OS;
using Java.IO;

namespace AssetManagement.Platforms.Android
{
    public static class PdfPreviewService
    {
        public static Bitmap RenderFirstPage(string filePath)
        {
            ParcelFileDescriptor fileDescriptor =
                ParcelFileDescriptor.Open(new Java.IO.File(filePath), ParcelFileMode.ReadOnly);

            PdfRenderer renderer = new PdfRenderer(fileDescriptor);
            PdfRenderer.Page page = renderer.OpenPage(0);

            Bitmap bitmap = Bitmap.CreateBitmap(
                page.Width,
                page.Height,
                Bitmap.Config.Argb8888);

            page.Render(bitmap, null, null, PdfRenderMode.ForDisplay);

            page.Close();
            renderer.Close();
            fileDescriptor.Close();

            return bitmap;
        }

        public static void OpenPdf(string filePath)
        {
            var context = global::Android.App.Application.Context;

            var file = new Java.IO.File(filePath);

            var uri = FileProvider.GetUriForFile(
                context,
                context.PackageName + ".fileprovider",
                file);

            var intent = new Intent(Intent.ActionView);
            intent.SetDataAndType(uri, "application/pdf");
            intent.SetFlags(ActivityFlags.NewTask);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission);

            // Try Google Drive PDF viewer first
            intent.SetPackage("com.google.android.apps.docs");

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
