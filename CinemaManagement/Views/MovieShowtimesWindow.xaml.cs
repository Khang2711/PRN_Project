using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using System.Windows;

namespace CinemaManagement.Views;

public partial class MovieShowtimesWindow : Window
{
    private readonly Movie _movie;
    private readonly string _username;

    public MovieShowtimesWindow(Movie movie, string username)
    {
        InitializeComponent();
        _movie = movie;
        _username = username;
        var vm = new MovieShowtimesViewModel(movie);
        vm.SeatPreviewRequested += OpenSeatPreview;
        DataContext = vm;
    }

    private void OpenSeatPreview(Showtime showtime)
    {
        // Dialog lifetime and navigation are UI concerns.
        var popup = new SeatAvailabilityWindow(showtime, _movie.Title, _username)
        {
            Owner = this
        };
        if (popup.ShowDialog() == true && Owner is MainWindow mainWindow)
        {
            var selectedSeatIds = popup.SelectedSeatIds;
            Close();
            mainWindow.OpenBooking(showtime, selectedSeatIds);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
