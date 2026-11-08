using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using CinemaManagement.Models;

namespace CinemaManagement.Views;

public partial class BrowseMoviesView : UserControl
{
    public string Username { get; set; } = "";

    public BrowseMoviesView()
    {
        InitializeComponent();
        if (!DesignerProperties.GetIsInDesignMode(this))
            DataContext = new ViewModels.BrowseMoviesViewModel();
    }

    private void ViewDetails_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Movie movie)
        {
            MovieDetailsWindow popup = new MovieDetailsWindow(movie);
            popup.Owner = Window.GetWindow(this);
            popup.ShowDialog();
        }
    }

    private void ViewShowtimes_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Movie movie)
        {
            MovieShowtimesWindow popup = new MovieShowtimesWindow(movie, Username);
            popup.Owner = Window.GetWindow(this);
            popup.ShowDialog();
        }
    }
}