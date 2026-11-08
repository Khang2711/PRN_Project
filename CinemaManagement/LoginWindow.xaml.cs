using System.Windows;
using CinemaManagement.ViewModels;

namespace CinemaManagement;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel();
        DataContext = _viewModel;

        _viewModel.SignInSucceeded += OpenMainWindow;
        _viewModel.RegistrationSucceeded += FinishRegistration;
        _viewModel.ClearSignInPasswordRequested += () =>
        {
            PasswordBox.Clear();
            PasswordBox.Focus();
        };
        _viewModel.ClearConfirmationPasswordRequested += () => ConfirmPasswordBox.Clear();

        UsernameTextBox.Focus();
    }

    // PasswordBox is a WPF view control: pass the password only when the user clicks.
    // All validation and database logic is handled by LoginViewModel.
    private void SignInButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SignInCommand.Execute(PasswordBox.Password);
    }

    private void SignUpButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SignUpCommand.Execute(new LoginViewModel.RegistrationPasswords(
            RegisterPasswordBox.Password,
            ConfirmPasswordBox.Password));
    }

    private void OpenMainWindow(string username, string role)
    {
        var mainWindow = new MainWindow(username, role);
        Application.Current.MainWindow = mainWindow;
        mainWindow.Show();
        Close();
    }

    private void FinishRegistration()
    {
        MessageBox.Show(
            "Your account has been created successfully. You can now sign in.",
            "Registration Successful",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        AccountTabControl.SelectedIndex = 0;
        PasswordBox.Clear();
        RegisterPasswordBox.Clear();
        ConfirmPasswordBox.Clear();
    }
}
