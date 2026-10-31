using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using CinemaManagement.Models;

namespace CinemaManagement.Views;

public partial class BrowseMoviesView : UserControl
{
    public BrowseMoviesView()
    {
        InitializeComponent();
        if (!DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = new ViewModels.BrowseMoviesViewModel();
        }
    }

    private void ViewDetails_Click(object sender, RoutedEventArgs e)
    {
        Button button = (Button)sender;

        Movie movie = (Movie)button.CommandParameter;

        MovieDetailsWindow popup = new MovieDetailsWindow(movie);

        popup.Owner = Window.GetWindow(this);

        popup.ShowDialog();
    }
}
