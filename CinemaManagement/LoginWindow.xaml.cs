using System.Windows;

namespace CinemaManagement;

public partial class LoginWindow : Window
{
    // Demo accounts for F07. Replace with database authentication later.
    private readonly (string Username, string Password, string Role)[] _accounts =
    {
        ("admin", "admin123", "Admin"),
        ("user", "user123", "User")
    };

    public LoginWindow()
    {
        InitializeComponent();
        UsernameTextBox.Focus();
    }

    private void SignInButton_Click(object sender, RoutedEventArgs e)
    {
        string username = UsernameTextBox.Text.Trim();
        string password = PasswordBox.Password;

        var account = _accounts.FirstOrDefault(a =>
            a.Username.Equals(username, StringComparison.OrdinalIgnoreCase)
            && a.Password == password);

        if (account == default)
        {
            ErrorText.Text = "Invalid username or password.";
            PasswordBox.Clear();
            PasswordBox.Focus();
            return;
        }

        // Pass the authenticated identity and role to the main window.
        var mainWindow = new MainWindow(account.Username, account.Role);
        Application.Current.MainWindow = mainWindow;
        mainWindow.Show();
        Close();
    }
}