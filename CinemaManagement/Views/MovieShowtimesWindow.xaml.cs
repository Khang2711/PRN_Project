using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CinemaManagement.Views
{
    public partial class MovieShowtimesWindow : Window
    {
        private readonly Movie _movie;
        private readonly string _username;

        public MovieShowtimesWindow(Movie movie, string username)
        {
            InitializeComponent();
            _movie = movie;
            _username = username;
            DataContext = new MovieShowtimesViewModel(movie);
        }

        private void ViewSeats_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not Showtime showtime)
                return;

            SeatAvailabilityWindow popup = new SeatAvailabilityWindow(showtime, _movie.Title, _username)
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

        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MovieShowtimesViewModel vm)
                vm.SelectedDate = null;
        }
    }
}