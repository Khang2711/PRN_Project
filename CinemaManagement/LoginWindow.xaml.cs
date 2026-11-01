using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        UsernameTextBox.Focus();
    }

    private void SignInButton_Click(object sender, RoutedEventArgs e)
    {
        string username = UsernameTextBox.Text.Trim();
        string password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            ErrorText.Text = "Please enter username and password.";
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            string passwordHash = HashPassword(password);

            var account = db.Users.FirstOrDefault(u =>
                u.Username == username &&
                u.PasswordHash == passwordHash &&
                u.IsActive);

            if (account == null)
            {
                ErrorText.Text = "Invalid username or password.";
                PasswordBox.Clear();
                PasswordBox.Focus();
                return;
            }

            // Pass the logged-in user's information to the main window.
            var mainWindow = new MainWindow(account.Username, account.Role);
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            Close();
        }
        catch (Exception)
        {
            ErrorText.Text = "Cannot connect to the database. Please try again.";
        }
    }

    private void SignUpButton_Click(object sender, RoutedEventArgs e)
    {
        string fullName = FullNameTextBox.Text.Trim();
        string username = RegisterUsernameTextBox.Text.Trim();
        string email = EmailTextBox.Text.Trim();
        string phone = PhoneTextBox.Text.Trim();
        string password = RegisterPasswordBox.Password;
        string confirmPassword = ConfirmPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            RegisterErrorText.Text = "Please fill in all required fields.";
            return;
        }

        if (username.Length > 50)
        {
            RegisterErrorText.Text = "Username cannot exceed 50 characters.";
            return;
        }

        if (fullName.Length > 100)
        {
            RegisterErrorText.Text = "Full name cannot exceed 100 characters.";
            return;
        }

        if (password.Length < 6)
        {
            RegisterErrorText.Text = "Password must contain at least 6 characters.";
            return;
        }

        if (password != confirmPassword)
        {
            RegisterErrorText.Text = "Passwords do not match.";
            ConfirmPasswordBox.Clear();
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            bool usernameExists = db.Users.Any(u => u.Username == username);

            if (usernameExists)
            {
                RegisterErrorText.Text = "This username is already taken.";
                return;
            }

            var newUser = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                FullName = fullName,
                Email = string.IsNullOrWhiteSpace(email) ? null : email,
                Phone = string.IsNullOrWhiteSpace(phone) ? null : phone,
                Role = "Customer",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            MessageBox.Show(
                "Your account has been created successfully. You can now sign in.",
                "Registration Successful",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Move to Sign In and fill in the registered username.
            AccountTabControl.SelectedIndex = 0;
            UsernameTextBox.Text = username;
            PasswordBox.Clear();

            FullNameTextBox.Clear();
            RegisterUsernameTextBox.Clear();
            EmailTextBox.Clear();
            PhoneTextBox.Clear();
            RegisterPasswordBox.Clear();
            ConfirmPasswordBox.Clear();
            RegisterErrorText.Text = "";
            ErrorText.Text = "";
        }
        catch (DbUpdateException)
        {
            RegisterErrorText.Text = "Could not create the account. The username may already exist.";
        }
        catch (Exception)
        {
            RegisterErrorText.Text = "Cannot connect to the database. Please try again.";
        }
    }

    // Convert the password to an MD5 hash before saving or checking it.
    private string HashPassword(string password)
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] hashBytes = MD5.HashData(passwordBytes);

        return Convert.ToHexString(hashBytes);
    }
}