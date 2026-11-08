using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;
using CinemaManagement.Helpers;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.ViewModels;

// F06-F07: account validation and persistence stay in the ViewModel.
// PasswordBox input and window navigation remain View responsibilities.
public sealed class LoginViewModel : BaseViewModel
{
    private string _username = string.Empty;
    private string _fullName = string.Empty;
    private string _registerUsername = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private string _signInError = string.Empty;
    private string _registerError = string.Empty;

    public string Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    public string FullName
    {
        get => _fullName;
        set { _fullName = value; OnPropertyChanged(); }
    }

    public string RegisterUsername
    {
        get => _registerUsername;
        set { _registerUsername = value; OnPropertyChanged(); }
    }

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    public string Phone
    {
        get => _phone;
        set { _phone = value; OnPropertyChanged(); }
    }

    public string SignInError
    {
        get => _signInError;
        private set { _signInError = value; OnPropertyChanged(); }
    }

    public string RegisterError
    {
        get => _registerError;
        private set { _registerError = value; OnPropertyChanged(); }
    }

    public ICommand SignInCommand { get; }
    public ICommand SignUpCommand { get; }

    public event Action<string, string>? SignInSucceeded;
    public event Action? RegistrationSucceeded;
    public event Action? ClearSignInPasswordRequested;
    public event Action? ClearConfirmationPasswordRequested;

    public sealed record RegistrationPasswords(string Password, string Confirmation);

    public LoginViewModel()
    {
        SignInCommand = new RelayCommand(input => SignIn(input as string ?? string.Empty));
        SignUpCommand = new RelayCommand(input =>
        {
            if (input is RegistrationPasswords passwords)
                SignUp(passwords.Password, passwords.Confirmation);
        });
    }

    private void SignIn(string password)
    {
        string username = Username.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            SignInError = "Please enter username and password.";
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
                SignInError = "Invalid username or password.";
                ClearSignInPasswordRequested?.Invoke();
                return;
            }

            SignInSucceeded?.Invoke(account.Username, account.Role);
        }
        catch (Exception)
        {
            SignInError = "Cannot connect to the database. Please try again.";
        }
    }

    private void SignUp(string password, string confirmPassword)
    {
        string fullName = FullName.Trim();
        string username = RegisterUsername.Trim();
        string email = Email.Trim();
        string phone = Phone.Trim();

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            RegisterError = "Please fill in all required fields.";
            return;
        }

        if (username.Length > 50)
        {
            RegisterError = "Username cannot exceed 50 characters.";
            return;
        }

        if (fullName.Length > 100)
        {
            RegisterError = "Full name cannot exceed 100 characters.";
            return;
        }

        if (password.Length < 6)
        {
            RegisterError = "Password must contain at least 6 characters.";
            return;
        }

        if (password != confirmPassword)
        {
            RegisterError = "Passwords do not match.";
            ClearConfirmationPasswordRequested?.Invoke();
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            if (db.Users.Any(u => u.Username == username))
            {
                RegisterError = "This username is already taken.";
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

            Username = username;
            FullName = string.Empty;
            RegisterUsername = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            RegisterError = string.Empty;
            SignInError = string.Empty;

            RegistrationSucceeded?.Invoke();
        }
        catch (DbUpdateException)
        {
            RegisterError = "Could not create the account. The username may already exist.";
        }
        catch (Exception)
        {
            RegisterError = "Cannot connect to the database. Please try again.";
        }
    }

    // Keep the existing hashing format so all current accounts still sign in.
    // A future security upgrade should migrate all stored passwords together.
    private static string HashPassword(string password)
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] hashBytes = MD5.HashData(passwordBytes);
        return Convert.ToHexString(hashBytes);
    }
}
