using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using CinemaManagement.Helpers;
using CinemaManagement.Models;

namespace CinemaManagement.ViewModels;

// Holds main-window state, role-based navigation and booking-tab coordination.
// Windows/dialogs themselves remain in the View's code-behind.
public sealed class MainWindowViewModel : BaseViewModel
{
    private const int BrowseMoviesTabIndex = 0;
    private const int ShowtimesTabIndex = 1;
    private const int BookingTabIndex = 4;
    private const int PaymentsTabIndex = 7;
    private readonly BookingViewModel _bookingViewModel;
    private int _selectedTabIndex;

    public string Username { get; }
    public string Role { get; }
    public string CurrentUserDisplay => Username + " (" + Role + ")";

    public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);
    public bool IsStaff => string.Equals(Role, "Staff", StringComparison.OrdinalIgnoreCase);
    public bool IsCustomer => _bookingViewModel.IsCustomer;

    public Visibility BrowseMoviesVisibility => !IsStaff && !IsAdmin ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowtimesVisibility => IsStaff || IsAdmin ? Visibility.Visible : Visibility.Collapsed;
    public Visibility AdminManagementVisibility => IsAdmin ? Visibility.Visible : Visibility.Collapsed;
    public Visibility CustomerVisibility => IsCustomer ? Visibility.Visible : Visibility.Collapsed;
    public Visibility PaymentsVisibility => IsStaff || IsAdmin ? Visibility.Visible : Visibility.Collapsed;

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (_selectedTabIndex == value) return;
            _selectedTabIndex = value;
            OnPropertyChanged();
        }
    }

    public ICommand SignOutCommand { get; }
    public event Action? SignOutRequested;

    public MainWindowViewModel(string username, string role, BookingViewModel bookingViewModel)
    {
        Username = username;
        Role = role;
        _bookingViewModel = bookingViewModel;
        _selectedTabIndex = IsStaff ? PaymentsTabIndex : IsAdmin ? ShowtimesTabIndex : BrowseMoviesTabIndex;
        SignOutCommand = new RelayCommand(_ => SignOutRequested?.Invoke());
    }

    public void OpenBooking(Showtime showtime, IEnumerable<int>? selectedSeatIds = null)
    {
        if (!IsCustomer) return;
        _bookingViewModel.BeginBooking(showtime, selectedSeatIds);
        SelectedTabIndex = BookingTabIndex;
    }

    public void RefreshCustomerBookings()
    {
        _bookingViewModel.LoadMyBookings();
    }
}
