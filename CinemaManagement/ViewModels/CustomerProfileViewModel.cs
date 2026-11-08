using System;
using System.Net.Mail;
using System.Windows.Input;
using CinemaManagement.Helpers;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.ViewModels;

// F09-F10: UI state and profile persistence belong to the ViewModel.
public sealed class CustomerProfileViewModel : BaseViewModel
{
    private string _signedInUsername = string.Empty;
    private User? _currentUser;
    private string _username = string.Empty;
    private string _fullName = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private string _role = string.Empty;
    private string _displayName = "Customer";
    private string _displayUsername = string.Empty;
    private string _displayRole = "Customer";
    private string _statusMessage = string.Empty;
    private bool _isStatusSuccess;
    private bool _isEditing;

    public string Username
    {
        get => _username;
        private set { _username = value; OnPropertyChanged(); }
    }

    public string FullName
    {
        get => _fullName;
        set { _fullName = value; OnPropertyChanged(); }
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

    public string Role
    {
        get => _role;
        private set { _role = value; OnPropertyChanged(); }
    }

    public string DisplayName
    {
        get => _displayName;
        private set { _displayName = value; OnPropertyChanged(); }
    }

    public string DisplayUsername
    {
        get => _displayUsername;
        private set { _displayUsername = value; OnPropertyChanged(); }
    }

    public string DisplayRole
    {
        get => _displayRole;
        private set { _displayRole = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsStatusSuccess
    {
        get => _isStatusSuccess;
        private set { _isStatusSuccess = value; OnPropertyChanged(); }
    }

    public bool IsEditing
    {
        get => _isEditing;
        private set
        {
            _isEditing = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotEditing));
        }
    }

    public bool IsNotEditing => !IsEditing;

    public ICommand EditCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public CustomerProfileViewModel()
    {
        EditCommand = new RelayCommand(_ => Edit());
        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    public void LoadProfile(string username)
    {
        _signedInUsername = username;
        LoadUserData();
    }

    private void LoadUserData()
    {
        try
        {
            using var db = new CinemaManagementDbContext();
            _currentUser = db.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Username == _signedInUsername && u.IsActive);

            if (_currentUser == null)
            {
                SetStatus("Cannot find this active account.");
                ClearFields();
                return;
            }

            Username = _currentUser.Username;
            FullName = _currentUser.FullName;
            Email = _currentUser.Email ?? string.Empty;
            Phone = _currentUser.Phone ?? string.Empty;
            Role = _currentUser.Role;
            DisplayName = string.IsNullOrWhiteSpace(_currentUser.FullName)
                ? _currentUser.Username : _currentUser.FullName;
            DisplayUsername = "@" + _currentUser.Username;
            DisplayRole = _currentUser.Role;
            SetStatus(string.Empty);
        }
        catch (Exception ex)
        {
            SetStatus("Cannot load profile: " + ex.GetBaseException().Message);
        }
    }

    private void Edit()
    {
        if (_currentUser == null)
            return;

        IsEditing = true;
        SetStatus(string.Empty);
    }

    private void Save()
    {
        string fullName = FullName.Trim();
        string email = Email.Trim();
        string phone = Phone.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            SetStatus("Full name is required.");
            return;
        }

        if (fullName.Length > 100 || email.Length > 255 || phone.Length > 20)
        {
            SetStatus("Full name, email or phone is too long.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            try
            {
                _ = new MailAddress(email);
            }
            catch
            {
                SetStatus("Please enter a valid email address.");
                return;
            }
        }

        try
        {
            using var db = new CinemaManagementDbContext();
            User? user = db.Users.FirstOrDefault(
                u => u.Username == _signedInUsername && u.IsActive);

            if (user == null)
            {
                SetStatus("Account not found or inactive.");
                return;
            }

            user.FullName = fullName;
            user.Email = string.IsNullOrWhiteSpace(email) ? null : email;
            user.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone;
            db.SaveChanges();

            _currentUser = user;
            FullName = fullName;
            Email = email;
            Phone = phone;
            // Refresh the summary card to match the persisted profile.
            DisplayName = string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName;
            SetStatus("Your profile has been updated successfully.", true);
            IsEditing = false;
        }
        catch (Exception ex)
        {
            SetStatus("Cannot save profile: " + ex.GetBaseException().Message);
        }
    }

    private void Cancel()
    {
        LoadUserData();
        IsEditing = false;
    }

    private void ClearFields()
    {
        Username = string.Empty;
        FullName = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        Role = string.Empty;
        DisplayName = "Customer";
        DisplayUsername = string.Empty;
        DisplayRole = "Customer";
    }

    private void SetStatus(string message, bool success = false)
    {
        StatusMessage = message;
        IsStatusSuccess = success;
    }
}
