using System.Windows;
using System.Windows.Controls;

namespace CinemaManagement;

public partial class MainWindow : Window
{
    private readonly string _username;
    private readonly string _role;

    public MainWindow(string username, string role)
    {
        InitializeComponent();

        _username = username;
        _role = role;
        CurrentUserText.Text = $"{_username} ({_role})";

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