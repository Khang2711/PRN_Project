using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CinemaManagement.Views
{
    public partial class SeatAvailabilityWindow : Window
    {
        private readonly SeatAvailabilityViewModel _viewModel;
        public IReadOnlyList<int> SelectedSeatIds { get; private set; } = Array.Empty<int>();

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
            // No database writes here. The Booking tab handles confirmation.
            SelectedSeatIds = _viewModel.Seats
                .Where(s => s.IsSelected && !s.IsBooked)
                .Select(s => s.SeatId).ToArray();
            DialogResult = true;
        }

    }
}