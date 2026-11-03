using CinemaManagement.Helpers;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CinemaManagement.ViewModels;

public class AccountViewModel : BaseViewModel
{
    private readonly string _adminUsername;
    public ObservableCollection<User> Users { get; } = new();
    public string[] Roles { get; } = { "Customer", "Staff", "Admin" };
    private User? _selectedUser;
    public User? SelectedUser
    {
        get => _selectedUser;
        set { _selectedUser = value; OnPropertyChanged(); SelectedRole = value?.Role ?? "Customer"; CommandManager.InvalidateRequerySuggested(); }
    }
    private string _selectedRole = "Customer";
    public string SelectedRole { get => _selectedRole; set { _selectedRole = value; OnPropertyChanged(); } }
    private string _fullName = "";
    public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged(); } }
    private string _username = "";
    public string Username { get => _username; set { _username = value; OnPropertyChanged(); } }
    private string _email = "";
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    private string _phone = "";
    public string Phone { get => _phone; set { _phone = value; OnPropertyChanged(); } }
    private string _newRole = "Customer";
    public string NewRole { get => _newRole; set { _newRole = value; OnPropertyChanged(); } }
    public ICommand RefreshCommand { get; }
    public ICommand ChangeRoleCommand { get; }
    public ICommand DeactivateCommand { get; }

    public AccountViewModel(string adminUsername)
    {
        _adminUsername = adminUsername;
        RefreshCommand = new RelayCommand(_ => LoadUsers());
        ChangeRoleCommand = new RelayCommand(_ => ChangeRole(), _ => SelectedUser != null);
        DeactivateCommand = new RelayCommand(_ => Deactivate(), _ => SelectedUser?.IsActive == true);
        LoadUsers();
    }
    private void RequireAdmin(CinemaManagementDbContext db)
    {
        if (!db.Users.Any(x => x.Username == _adminUsername && x.Role == "Admin" && x.IsActive))
            throw new InvalidOperationException("Only an active Admin can manage user accounts.");
    }
    public void LoadUsers()
    {
        try
        {
            using var db = new CinemaManagementDbContext();
            RequireAdmin(db);
            var rows = db.Users.AsNoTracking().OrderBy(x => x.UserId).ToList();
            Users.Clear();
            foreach (var row in rows) Users.Add(row);
            SelectedUser = null;
        }
        catch (Exception ex) { Error("Cannot load users", ex); }
    }
    public bool CreateUser(string password)
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(FullName) || password.Length < 6)
        { MessageBox.Show("Enter a username, full name and password of at least 6 characters."); return false; }
        if (Username.Trim().Length > 50 || FullName.Trim().Length > 100 || Email.Trim().Length > 255 || Phone.Trim().Length > 20)
        { MessageBox.Show("One or more fields exceed the database length limit."); return false; }
        try
        {
            using var db = new CinemaManagementDbContext();
            RequireAdmin(db);
            if (db.Users.Any(x => x.Username == Username.Trim())) { MessageBox.Show("Username already exists."); return false; }
            var user = new User
            {
                FullName = FullName.Trim(), Username = Username.Trim(),
                Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                Role = NewRole, IsActive = true, CreatedAt = DateTime.Now,
                // Compatible with the existing LoginWindow hashing scheme.
                PasswordHash = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(password)))
            };
            db.Users.Add(user);
            db.SaveChanges();
            FullName = Username = Email = Phone = ""; NewRole = "Customer";
            LoadUsers();
            MessageBox.Show("User account created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }
        catch (Exception ex) { Error("Cannot create user", ex); return false; }
    }
    private void ChangeRole()
    {
        if (SelectedUser == null) return;
        var id = SelectedUser.UserId;
        if (!Roles.Contains(SelectedRole)) { MessageBox.Show("Invalid role."); return; }
        if (MessageBox.Show($"Change role of {SelectedUser.Username} to {SelectedRole}?", "Edit User Role", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try
        {
            using var db = new CinemaManagementDbContext();
            using var tx = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            RequireAdmin(db);
            var user = db.Users.Single(x => x.UserId == id);
            if (user.Username == _adminUsername && user.Role == "Admin" && SelectedRole != "Admin")
                throw new InvalidOperationException("You cannot remove your own Admin role.");
            user.Role = SelectedRole;
            db.SaveChanges(); tx.Commit();
            MessageBox.Show("User role updated successfully."); LoadUsers();
        }
        catch (Exception ex) { Error("Cannot update role", ex); }
    }
    private void Deactivate()
    {
        if (SelectedUser == null) return;
        var id = SelectedUser.UserId;
        if (MessageBox.Show($"Deactivate {SelectedUser.Username}? This account will no longer be able to sign in.", "Deactivate User", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        try
        {
            using var db = new CinemaManagementDbContext();
            using var tx = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            RequireAdmin(db);
            var user = db.Users.Single(x => x.UserId == id);
            if (user.Username == _adminUsername) throw new InvalidOperationException("You cannot deactivate your own account.");
            if (!user.IsActive) throw new InvalidOperationException("This account is already inactive.");
            user.IsActive = false;
            db.SaveChanges(); tx.Commit();
            MessageBox.Show("User account deactivated successfully."); LoadUsers();
        }
        catch (Exception ex) { Error("Cannot deactivate user", ex); }
    }
    private static void Error(string title, Exception ex) => MessageBox.Show(ex.GetBaseException().Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
}
