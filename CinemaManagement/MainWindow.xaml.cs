
using System;
using System.Windows;
using System.Windows.Controls;
using CinemaManagement.Views;
using CinemaManagement.ViewModels;

namespace CinemaManagement;

public partial class MainWindow : Window
{
    private readonly string _username;
    private readonly string _role;

    public MainWindow(string username, string role)
    {
        InitializeComponent();

        // Lay thong tin tu LoginWindow
        _username = username;
        _role = role;

        // Hien thi tai khoan dang dang nhap
        CurrentUserText.Text = $"{_username} ({_role})";

        // Kiem tra quyen Admin
        bool isAdmin = string.Equals(
            _role,
            "Admin",
            StringComparison.OrdinalIgnoreCase
        );

        // Truyen quyen vao ShowtimesViewModel
        if (ShowtimesTab.Content is ShowtimesView showtimesView
            && showtimesView.DataContext is ShowtimesViewModel showtimesVM)
        {
            showtimesVM.CanManageShowtimes = isAdmin;
        }

        // Phan quyen cac tab
        if (isAdmin)
        {
            // Admin khong can Browse Movies trong ban demo
            BrowseMoviesTab.Visibility = Visibility.Collapsed;

            // Mo trang quan ly suat chieu
            MainTabControl.SelectedItem = ShowtimesTab;
        }
        else
        {
            // User khong duoc truy cap chuc nang quan ly
            ManageMoviesTab.Visibility = Visibility.Collapsed;
            RoomsSeatsTab.Visibility = Visibility.Collapsed;
            PaymentsTab.Visibility = Visibility.Collapsed;
            AccountTab.Visibility = Visibility.Collapsed;

            // User mo trang danh sach phim
            MainTabControl.SelectedItem = BrowseMoviesTab;
        }
    }

    // Dang xuat
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
            return;

        // Quay lai LoginWindow
        var loginWindow = new LoginWindow();

        Application.Current.MainWindow = loginWindow;

        loginWindow.Show();

        Close();
    }
}
