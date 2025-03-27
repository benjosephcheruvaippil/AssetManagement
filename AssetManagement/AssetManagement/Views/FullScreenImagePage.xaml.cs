namespace AssetManagement.Views;

public partial class FullScreenImagePage : ContentPage
{
    private double currentScale = 1, startScale = 1;

    //private readonly PinchGestureRecognizer pinchGesture;
    //private readonly TapGestureRecognizer tapGesture;

    public FullScreenImagePage(string imageUrl)
	{
		InitializeComponent();
        if (!string.IsNullOrEmpty(imageUrl))
        {
            fullScreenImage.Source = ImageSource.FromUri(new Uri(imageUrl));

            // Ensure image starts at normal size
            fullScreenImage.Scale = 1;
            fullScreenImage.TranslationX = 0;
            fullScreenImage.TranslationY = 0;
            currentScale = 1;
        }
        else
        {
            DisplayAlert("Error", "Image URL is empty!", "OK");
        }

        // Adjust image size dynamically
        this.SizeChanged += OnPageSizeChanged;
    }

    private void OnPageSizeChanged(object sender, EventArgs e)
    {
        fullScreenImage.WidthRequest = this.Width;
        fullScreenImage.HeightRequest = this.Height;
    }

    private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Started)
        {
            startScale = fullScreenImage.Scale;
        }
        else if (e.Status == GestureStatus.Running)
        {
            double scale = Math.Max(1, Math.Min(startScale * e.Scale, 5)); // Limit zoom between 1x and 5x
            fullScreenImage.Scale = scale;

            // Enable scrolling when zoomed-in
            scrollView.InputTransparent = scale == 1;
        }
    }

    private void fullScreenImage_SizeChanged(object sender, EventArgs e)
    {
        fullScreenImage.Scale = 1;
        fullScreenImage.TranslationX = 0;
        fullScreenImage.TranslationY = 0;
        currentScale = 1;
    }

    private async void OnCloseTapped(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(); // Close modal on tap
    }

    //protected override void OnDisappearing()
    //{
    //    base.OnDisappearing();

    //    // Detach events to prevent disposed object exceptions
    //    pinchGesture.PinchUpdated -= OnPinchUpdated;
    //    tapGesture.Tapped -= OnCloseTapped;

    //    fullScreenImage.GestureRecognizers.Clear(); // Remove all gestures
    //}
}