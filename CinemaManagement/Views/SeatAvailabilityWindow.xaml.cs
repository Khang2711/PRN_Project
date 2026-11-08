using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CinemaManagement.Views
{
    public partial class SeatAvailabilityWindow : Window
    {
        private readonly SeatAvailabilityViewModel _viewModel;

        public SeatAvailabilityWindow(Showtime showtime, string movieTitle, string username)
        {
            InitializeComponent();
            _viewModel = new SeatAvailabilityViewModel(showtime, movieTitle, username);
            DataContext = _viewModel;
        }

        private void Seat_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is SeatAvailabilityItem seat)
                _viewModel.ToggleSeat(seat);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadSeats();
            _viewModel.Message = "Đã cập nhật trạng thái ghế.";
        }

        private void Book_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.CreateBooking())
            {
                MessageBox.Show(_viewModel.Message, "Đặt vé thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(_viewModel.Message, "Không thể đặt vé", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}