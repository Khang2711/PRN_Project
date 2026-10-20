using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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

        // Booking và View Booking dùng chung dữ liệu.
        _bookingViewModel = new BookingViewModel(_username, _role);

        BookingControl.DataContext = _bookingViewModel;

        ViewBookingControl.DataContext = _bookingViewModel;

        // Nhận sự kiện chọn dòng trong bảng Showtimes.
        ShowtimesControl.AddHandler(
            Selector.SelectionChangedEvent,
            new SelectionChangedEventHandler(Showtime_SelectionChanged),
            true
        );

        // Hai menu này chỉ dành cho khách hàng.
        if (!_bookingViewModel.IsCustomer)
        {
            BookingTab.Visibility = Visibility.Collapsed;
            ViewBookingTab.Visibility = Visibility.Collapsed;
        }

        // User chỉ dùng các chức năng dành cho người dùng thường.
        if (_role == "User")
        {
            ManageMoviesTab.Visibility = Visibility.Collapsed;
            RoomsSeatsTab.Visibility = Visibility.Collapsed;
            PaymentsTab.Visibility = Visibility.Collapsed;
            AccountTab.Visibility = Visibility.Collapsed;
            MainTabControl.SelectedItem = BrowseMoviesTab;
        }
        else
        {
            BrowseMoviesTab.Visibility = Visibility.Collapsed;
            MainTabControl.SelectedItem = ShowtimesTab;
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
            // Đưa thông tin suất chiếu đã chọn sang Booking.
            _bookingViewModel.SelectedShowtime = showtime;

            // Mở màn hình xác nhận đặt vé.
            MainTabControl.SelectedItem = BookingTab;

            // Cho phép lần sau bấm lại cùng dòng.
            grid.SelectedItem = null;
        }
    }
    private void SignOutButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "Are you sure you want to sign out?",
            "Sign Out",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        // Return to login screen and close this window.
        var loginWindow = new LoginWindow();
        Application.Current.MainWindow = loginWindow;
        loginWindow.Show();
        Close();
    }
}