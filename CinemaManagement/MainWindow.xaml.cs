using System;
using System.Collections.Generic;
using System.Windows;
using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using CinemaManagement.Views;

namespace CinemaManagement;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(string username, string role)
    {
        InitializeComponent();

        // ViewModel owns navigation and visibility. The View only composes child screens.
        var bookingViewModel = new BookingViewModel(username, role);
        _viewModel = new MainWindowViewModel(username, role, bookingViewModel);
        DataContext = _viewModel;
        _viewModel.SignOutRequested += SignOut;

        BrowseMoviesControl.Username = username;
        BookingControl.DataContext = bookingViewModel;
        ViewBookingControl.DataContext = bookingViewModel;

        if (_viewModel.IsStaff || _viewModel.IsAdmin)
            PaymentsControl.DataContext = new PaymentsViewModel(username);
        if (_viewModel.IsAdmin)
            AccountControl.DataContext = new AccountViewModel(username);
        if (_viewModel.IsCustomer)
            CustomerProfileControl.LoadProfile(username);

        if (ShowtimesControl.DataContext is ShowtimesViewModel showtimesVm)
            showtimesVm.CanManageShowtimes = _viewModel.IsAdmin;
    }

    // Kept for callers that already navigate here from the showtime preview.
    public void OpenBooking(Showtime showtime, IEnumerable<int>? preselectedSeatIds = null)
        => _viewModel.OpenBooking(showtime, preselectedSeatIds);

    public void RefreshCustomerBookings()
        => _viewModel.RefreshCustomerBookings();

    private void SignOut()
    {
        // Confirmation and lifetime of windows are View responsibilities.
        MessageBoxResult result = MessageBox.Show(
            "Are you sure you want to sign out?",
            "Sign Out", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        var loginWindow = new LoginWindow();
        Application.Current.MainWindow = loginWindow;
        loginWindow.Show();
        Close();
    }
}
