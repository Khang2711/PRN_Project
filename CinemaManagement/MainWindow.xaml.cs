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

        // Lấy thông tin từ LoginWindow.
        _username = username;
        _role = role;

        // Hiển thị tài khoản đang đăng nhập.
        CurrentUserText.Text = $"{_username} ({_role})";

        // Booking và View Booking dùng chung ViewModel.
        _bookingViewModel = new BookingViewModel(_username, _role);

        BookingControl.DataContext = _bookingViewModel;
        ViewBookingControl.DataContext = _bookingViewModel;

        // Nhận sự kiện chọn dòng trong bảng Showtimes.
        ShowtimesControl.AddHandler(
            Selector.SelectionChangedEvent,
            new SelectionChangedEventHandler(Showtime_SelectionChanged),
            true
        );

        // Hai tab đặt vé chỉ dành cho khách hàng.
        if (!_bookingViewModel.IsCustomer)
        {
            BookingTab.Visibility = Visibility.Collapsed;
            ViewBookingTab.Visibility = Visibility.Collapsed;
        }

        // Kiểm tra quyền Admin.
        bool isAdmin = string.Equals(
            _role,
            "Admin",
            StringComparison.OrdinalIgnoreCase
        );

        // Truyền quyền quản lý suất chiếu vào ShowtimesViewModel.
        if (ShowtimesTab.Content is ShowtimesView showtimesView
            && showtimesView.DataContext is ShowtimesViewModel showtimesVM)
        {
            showtimesVM.CanManageShowtimes = isAdmin;
        }

        // Phân quyền các tab.
        if (isAdmin)
        {
            // Admin không cần Browse Movies trong bản demo.
            BrowseMoviesTab.Visibility = Visibility.Collapsed;

            // Mở trang quản lý suất chiếu.
            MainTabControl.SelectedItem = ShowtimesTab;
        }
        else
        {
            // User không được truy cập chức năng quản lý.
            ManageMoviesTab.Visibility = Visibility.Collapsed;
            RoomsSeatsTab.Visibility = Visibility.Collapsed;
            PaymentsTab.Visibility = Visibility.Collapsed;
            AccountTab.Visibility = Visibility.Collapsed;
            ShowtimesTab.Visibility = Visibility.Collapsed;

            // User mở trang danh sách phim.
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
            // Đưa thông tin suất chiếu đã chọn sang Booking.
            _bookingViewModel.SelectedShowtime = showtime;

            // Mở màn hình xác nhận đặt vé.
            MainTabControl.SelectedItem = BookingTab;

            // Cho phép lần sau bấm lại cùng dòng.
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

        // Quay lại LoginWindow.
        var loginWindow = new LoginWindow();

        Application.Current.MainWindow = loginWindow;

        loginWindow.Show();

        Close();
    }
}