using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;

namespace CinemaManagement.Views;

public partial class CustomerProfileView : UserControl
{
    private string _username = string.Empty;
    private User? _currentUser;

    public CustomerProfileView()
    {
        InitializeComponent();
        SetEditMode(false);
    }

    public void LoadProfile(string username)
    {
        _username = username;
        LoadUserData();
    }

    private void LoadUserData()
    {
        try
        {
            using var db = new CinemaManagementDbContext();

            _currentUser = db.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Username == _username && u.IsActive);

            if (_currentUser == null)
            {
                StatusText.Text = "Cannot find this active account.";
                ClearFields();
                return;
            }

            UsernameTextBox.Text = _currentUser.Username;
            FullNameTextBox.Text = _currentUser.FullName;
            EmailTextBox.Text = _currentUser.Email ?? string.Empty;
            PhoneTextBox.Text = _currentUser.Phone ?? string.Empty;
            RoleTextBox.Text = _currentUser.Role;
            StatusText.Text = string.Empty;
        }
        catch (Exception ex)
        {
            StatusText.Text = "Cannot load profile: " + ex.GetBaseException().Message;
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentUser == null)
        {
            return;
        }

        SetEditMode(true);
        StatusText.Text = string.Empty;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        string fullName = FullNameTextBox.Text.Trim();
        string email = EmailTextBox.Text.Trim();
        string phone = PhoneTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            StatusText.Text = "Full name is required.";
            return;
        }

        if (fullName.Length > 100 || email.Length > 255 || phone.Length > 20)
        {
            StatusText.Text = "Full name, email or phone is too long.";
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
                StatusText.Text = "Please enter a valid email address.";
                return;
            }
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            User? user = db.Users.FirstOrDefault(
                u => u.Username == _username && u.IsActive);

            if (user == null)
            {
                StatusText.Text = "Account not found or inactive.";
                return;
            }

            user.FullName = fullName;
            user.Email = string.IsNullOrWhiteSpace(email) ? null : email;
            user.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone;

            db.SaveChanges();

            _currentUser = user;
            StatusText.Foreground = System.Windows.Media.Brushes.ForestGreen;
            StatusText.Text = "Your profile has been updated successfully.";
            SetEditMode(false);
        }
        catch (Exception ex)
        {
            StatusText.Foreground = System.Windows.Media.Brushes.Firebrick;
            StatusText.Text = "Cannot save profile: " + ex.GetBaseException().Message;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        LoadUserData();
        SetEditMode(false);
    }

    private void SetEditMode(bool isEditing)
    {
        FullNameTextBox.IsReadOnly = !isEditing;
        EmailTextBox.IsReadOnly = !isEditing;
        PhoneTextBox.IsReadOnly = !isEditing;

        EditButton.Visibility = isEditing ? Visibility.Collapsed : Visibility.Visible;
        SaveButton.Visibility = isEditing ? Visibility.Visible : Visibility.Collapsed;
        CancelButton.Visibility = isEditing ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ClearFields()
    {
        UsernameTextBox.Text = string.Empty;
        FullNameTextBox.Text = string.Empty;
        EmailTextBox.Text = string.Empty;
        PhoneTextBox.Text = string.Empty;
        RoleTextBox.Text = string.Empty;
    }
}