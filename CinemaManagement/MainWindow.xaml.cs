using System;
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

        _bookingViewModel = new BookingViewModel(_username, _role);

        BookingControl.DataContext = _bookingViewModel;
        ViewBookingControl.DataContext = _bookingViewModel;

        ShowtimesControl.AddHandler(
            Selector.SelectionChangedEvent,
            new SelectionChangedEventHandler(Showtime_SelectionChanged),
            true
        );

        if (!_bookingViewModel.IsCustomer)
        {
            BookingTab.Visibility = Visibility.Collapsed;
            ViewBookingTab.Visibility = Visibility.Collapsed;
            CustomerProfileTab.Visibility = Visibility.Collapsed;
        }
        else
        {
            CustomerProfileControl.LoadProfile(_username);
        }

        bool isAdmin = string.Equals(
            _role,
            "Admin",
            StringComparison.OrdinalIgnoreCase
        );

        if (ShowtimesTab.Content is ShowtimesView showtimesView
            && showtimesView.DataContext is ShowtimesViewModel showtimesVM)
        {
            showtimesVM.CanManageShowtimes = isAdmin;
        }

        if (isAdmin)
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

    private void Showtime_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!_bookingViewModel.IsCustomer)
        {
            return;
        }

        if (e.OriginalSource is DataGrid grid
            && grid.SelectedItem is Showtime showtime)
        {
            _bookingViewModel.SelectedShowtime = showtime;
            MainTabControl.SelectedItem = BookingTab;
            grid.SelectedItem = null;
        }
    }

    private void SignOutButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "Are you sure you want to sign out?",
            "Sign Out",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var loginWindow = new LoginWindow();
        Application.Current.MainWindow = loginWindow;
        loginWindow.Show();
        Close();
    }
}