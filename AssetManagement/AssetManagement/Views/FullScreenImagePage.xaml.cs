namespace AssetManagement.Views;

public partial class FullScreenImagePage : ContentPage
{
    public FullScreenImagePage(string imageUrl, string fromPage)
    {
        //InitializeComponent();

        if (!string.IsNullOrEmpty(imageUrl))
        {
            if (fromPage == "AssetPage")
            {
                //fullScreenImage.Source = ImageSource.FromUri(new Uri(imageUrl));
            }
            else if (fromPage == "ExpensePage")
            {
                //fullScreenImage.Source = ImageSource.FromFile(imageUrl);
            }
        }
        else
        {
            DisplayAlertAsync("Error", "Image URL is empty!", "OK");
        }
    }

    private async void OnCloseTapped(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(); // Close modal on tap
    }
}