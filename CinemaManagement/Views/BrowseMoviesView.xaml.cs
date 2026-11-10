using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CinemaManagement.Models;
using CinemaManagement.ViewModels;

namespace CinemaManagement.Views;

public partial class BrowseMoviesView : UserControl
{
    public string Username { get; set; } = "";

    public BrowseMoviesView()
    {
        InitializeComponent();
        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        var vm = new BrowseMoviesViewModel();
        vm.MovieDetailsRequested += OpenMovieDetails;
        vm.MovieShowtimesRequested += OpenMovieShowtimes;
        DataContext = vm;
    }

    // Dialog creation belongs to the View; command intent lives in the ViewModel.
    private void OpenMovieDetails(Movie movie)
    {
        var popup = new MovieDetailsWindow(movie) { Owner = Window.GetWindow(this) };
        popup.ShowDialog();
    }

    private void OpenMovieShowtimes(Movie movie)
    {
        var popup = new MovieShowtimesWindow(movie, Username) { Owner = Window.GetWindow(this) };
        popup.ShowDialog();
    }
}
