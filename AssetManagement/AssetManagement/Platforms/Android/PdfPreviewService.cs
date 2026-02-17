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
    }
}
