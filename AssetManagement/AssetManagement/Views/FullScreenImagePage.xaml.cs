namespace AssetManagement.Views;

public partial class FullScreenImagePage : ContentPage
{
    private double currentScale = 1;  // Current zoom level
    private double startScale = 1;    // Zoom level when pinch starts
    private const double MinZoom = 1;  // Minimum zoom (normal size)
    private const double MaxZoom = 10; // Maximum zoom

    public FullScreenImagePage(string imageUrl)
    {
        InitializeComponent();

        if (!string.IsNullOrEmpty(imageUrl))
        {
            fullScreenImage.Source = ImageSource.FromUri(new Uri(imageUrl));

            // Ensure image starts at normal size
            fullScreenImage.Scale = MinZoom;
        }
        else
        {
            DisplayAlert("Error", "Image URL is empty!", "OK");
        }
    }

    private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Started)
        {
            startScale = fullScreenImage.Scale;
        }
        else if (e.Status == GestureStatus.Running)
        {
            // Smooth zooming with proper range (1x to 10x)
            double newScale = Math.Max(MinZoom, Math.Min(startScale * e.Scale, MaxZoom));
            fullScreenImage.Scale = newScale;
        }
        else if (e.Status == GestureStatus.Completed)
        {
            currentScale = fullScreenImage.Scale;
        }
    }

    private async void OnCloseTapped(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(); // Close modal on tap
    }
}



