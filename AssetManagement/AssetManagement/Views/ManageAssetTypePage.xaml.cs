using System.Collections.ObjectModel;

namespace AssetManagement.Views;

public partial class ManageAssetTypePage : ContentPage
{
    private ObservableCollection<PlaceInfoNew> places;
    public ManageAssetTypePage()
    {
        InitializeComponent();
        //contactForm.DataObject = new PlaceInfoNew();

        places = new ObservableCollection<PlaceInfoNew>
        {
            new PlaceInfoNew { Name = "Kochi",     Description = "Queen of the Arabian Sea" },
            new PlaceInfoNew { Name = "Bangalore", Description = "Silicon Valley of India" },
            new PlaceInfoNew { Name = "Sydney",    Description = "Harbour City" },
            new PlaceInfoNew { Name = "Mumbai",    Description = "Financial Capital" }
            //new PlaceInfoNew { Name = "Kolkata",   Description = "City of Joy" },
            //new PlaceInfoNew { Name = "NEtherlands",     Description = "Capital City" },
            //new PlaceInfoNew { Name = "New York",     Description = "Capital City" },
            //new PlaceInfoNew { Name = "California",     Description = "Capital City" },
            //new PlaceInfoNew { Name = "Machnuts",     Description = "Capital City" },
            //new PlaceInfoNew { Name = "London",     Description = "Capital City" }
        };

        dataPager.Source = places;
        dataPager.PageSize = 2;
        verticalListView.ItemsSource = places.Skip(0).Take(2).ToList();
    }

    private void verticalListView_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (e.DataItem is PlaceInfoNew tappedItem)
        {
            string? name = tappedItem.Name;
            string? description = tappedItem.Description;

            entryPlaceName.Text = name;
            entryPlaceDescription.Text = description;
            // await DisplayAlert("Row Clicked", $"Name: {name}", "OK");
        }
    }

    private void dataPager_PageChanged(object sender, Syncfusion.Maui.DataGrid.DataPager.PageChangedEventArgs e)
    {
        int newPage = e.NewPageIndex;
        int oldPage = e.OldPageIndex;

        var pageSize = dataPager.PageSize;
        var startIndex = newPage * pageSize;

        // Assuming you have your original list in a variable `items`
        var currentPageItems = places.Skip(startIndex).Take(pageSize).ToList();

        verticalListView.ItemsSource = currentPageItems;
    }

    private void btnSave_Clicked(object sender, EventArgs e)
    {

        places.Add(new PlaceInfoNew
        {
            Name = entryPlaceName.Text,
            Description = entryPlaceDescription.Text
        });

        dataPager.Source = places;
        dataPager.MoveToFirstPage();

        var currentPageItems = places.Skip(0).Take(2).ToList();

        verticalListView.ItemsSource = currentPageItems;
    }

    private void btnClear_Clicked(object sender, EventArgs e)
    {
        entryPlaceName.Text = "";
        entryPlaceDescription.Text = "";
    }
}

public class PlaceInfoNew
{
    public string Name { get; set; }
    public string Description { get; set; }
}