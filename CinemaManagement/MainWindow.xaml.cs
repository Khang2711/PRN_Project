using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using CinemaManagement.Views;

namespace CinemaManagement;

public partial class MainWindow : Window
{
    private readonly string _username;
    private readonly string _role;
    private readonly BookingViewModel _bookingViewModel;

    public MainWindow(string username, string role)
    {
        InitializeComponent();
        _username = username;
        _role = role;
        CurrentUserText.Text = $"{_username} ({_role})";
        BrowseMoviesControl.Username = _username;

        if (string.Equals(_role, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(_role, "Staff", StringComparison.OrdinalIgnoreCase))
            PaymentsControl.DataContext = new PaymentsViewModel(_username);
        if (string.Equals(_role, "Admin", StringComparison.OrdinalIgnoreCase))
            AccountControl.DataContext = new AccountViewModel(_username);

        _bookingViewModel = new BookingViewModel(_username, _role);
        BookingControl.DataContext = _bookingViewModel;
        ViewBookingControl.DataContext = _bookingViewModel;

        if (!_bookingViewModel.IsCustomer)
        {
            BookingTab.Visibility = Visibility.Collapsed;
            ViewBookingTab.Visibility = Visibility.Collapsed;
            CustomerProfileTab.Visibility = Visibility.Collapsed;
        }
        else CustomerProfileControl.LoadProfile(_username);

        bool isAdmin = string.Equals(_role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (ShowtimesTab.Content is ShowtimesView showtimesView &&
            showtimesView.DataContext is ShowtimesViewModel showtimesVM)
            showtimesVM.CanManageShowtimes = isAdmin;

        bool isStaff = string.Equals(_role, "Staff", StringComparison.OrdinalIgnoreCase);
        if (isStaff)
        {
            ManageMoviesTab.Visibility = Visibility.Collapsed;
            RoomsSeatsTab.Visibility = Visibility.Collapsed;
            AccountTab.Visibility = Visibility.Collapsed;
            BrowseMoviesTab.Visibility = Visibility.Collapsed;
            MainTabControl.SelectedItem = PaymentsTab;
        }
        else if (isAdmin)
        {
            BrowseMoviesTab.Visibility = Visibility.Collapsed;
            MainTabControl.SelectedItem = ShowtimesTab;
        }
        else
        {
            ManageMoviesTab.Visibility = Visibility.Collapsed;
            RoomsSeatsTab.Visibility = Visibility.Collapsed;
            PaymentsTab.Visibility = Visibility.Collapsed;
            AccountTab.Visibility = Visibility.Collapsed;
            ShowtimesTab.Visibility = Visibility.Collapsed;
            MainTabControl.SelectedItem = BrowseMoviesTab;
        }
    }

    public void RefreshCustomerBookings()
    {
        _bookingViewModel.LoadMyBookings();
    }

    // The preview's "Đặt ghế" action explicitly opens Booking.
    public void OpenBooking(Showtime showtime, IEnumerable<int>? preselectedSeatIds = null)
    {
        if (!_bookingViewModel.IsCustomer) return;
        _bookingViewModel.BeginBooking(showtime, preselectedSeatIds);
        MainTabControl.SelectedItem = BookingTab;
    }

    private void SignOutButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show("Are you sure you want to sign out?",
            "Sign Out", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;
        var loginWindow = new LoginWindow();
        Application.Current.MainWindow = loginWindow;
        loginWindow.Show();
        Close();
    }
}