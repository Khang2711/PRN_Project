using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;

namespace CinemaManagement.Views;

public partial class SeatAvailabilityWindow : Window
{
    public IReadOnlyList<int> SelectedSeatIds { get; private set; } = Array.Empty<int>();

    public SeatAvailabilityWindow(Showtime showtime, string movieTitle, string username)
    {
        InitializeComponent();
        var vm = new SeatAvailabilityViewModel(showtime, movieTitle, username);
        vm.ContinueBookingRequested += ids =>
        {
            SelectedSeatIds = ids;
            DialogResult = true;
        };
        DataContext = vm;
    }
}
