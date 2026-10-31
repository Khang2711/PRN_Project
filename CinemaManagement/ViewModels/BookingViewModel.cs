using CinemaManagement.Helpers;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CinemaManagement.ViewModels;

public class BookingViewModel : BaseViewModel
{
    // Danh sách lưu trong bộ nhớ khi ứng dụng đang chạy.
    private static readonly ObservableCollection<Booking> AllBookings = new();

    private readonly string _role;

    private Showtime? _selectedShowtime;

    private bool _confirmed;

    private string _message = "Go to Showtimes and select a movie.";

    public string Username { get; }

    public ObservableCollection<Booking> MyBookings { get; } = new();

    public ICommand ConfirmBookingCommand { get; }

    public bool IsCustomer =>
        !string.IsNullOrWhiteSpace(Username)
        && (_role == "User" || _role == "Customer");

    public Showtime? SelectedShowtime
    {
        get => _selectedShowtime;

        set
        {
            _selectedShowtime = value;

            // Chọn suất chiếu mới thì được xác nhận đơn mới.
            _confirmed = false;

            Message = "Check the information, then click Confirm Booking.";

            OnPropertyChanged();

            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string Message
    {
        get => _message;

        private set
        {
            _message = value;
            OnPropertyChanged();
        }
    }

    public BookingViewModel(string username, string role)
    {
        Username = username;

        _role = role;

        ConfirmBookingCommand = new RelayCommand(
            _ => ConfirmBooking(),
            _ => IsCustomer
                 && SelectedShowtime != null
                 && !_confirmed
        );

        LoadMyBookings();
    }

    private void ConfirmBooking()
    {
        if (!IsCustomer || SelectedShowtime == null || _confirmed)
        {
            return;
        }

        Showtime showtime = SelectedShowtime;

        if (showtime.StartTime <= DateTime.Now)
        {
            Message =
                "This showtime has already started. Select another showtime.";

            return;
        }

        if (!showtime.IsActive)
        {
            Message = "This showtime is not available for booking.";
            return;
        }

        if (showtime.TicketPrice <= 0)
        {
            Message = "The ticket price is invalid.";
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            // Tim khach hang trong database
            var customer = (
                from u in db.Users
                where u.Username == Username && u.IsActive
                select u
            ).FirstOrDefault();

            if (customer == null)
            {
                Message = "Customer account does not exist in database.";
                return;
            }

            // Kiem tra suat chieu trong database
            var selectedShowtime = (
                from s in db.Showtimes
                where s.ShowtimeId == showtime.ShowtimeId
                      && s.IsActive
                select s
            ).FirstOrDefault();

            if (selectedShowtime == null ||
                selectedShowtime.StartTime <= DateTime.Now)
            {
                Message = "This showtime is no longer available.";
                return;
            }

            // Tao booking moi
            Booking booking = new Booking
            {
                BookingCode = "BK" +
                    Guid.NewGuid().ToString("N")
                        .Substring(0, 12).ToUpperInvariant(),

                CustomerId = customer.UserId,

                ShowtimeId = selectedShowtime.ShowtimeId,

                BookingTime = DateTime.Now,

                Status = "Pending",

                PaymentStatus = "Unpaid",

                IsCancelled = false
            };

            // Luu booking vao database
            db.Bookings.Add(booking);
            db.SaveChanges();

            // Load lai danh sach booking
            LoadMyBookings();

            // Ngăn bấm xác nhận liên tục tạo nhiều đơn.
            _confirmed = true;

            Message = "Saved " + booking.BookingCode
                + ". Open View Booking to see your booking.";

            CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            Message = "Cannot save booking: "
                + ex.GetBaseException().Message;

            MessageBox.Show(
                Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void LoadMyBookings()
    {
        MyBookings.Clear();

        if (!IsCustomer)
        {
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            // Lay booking cua khach hang tu database
            var data = (
                from b in db.Bookings
                    .Include("Customer")
                    .Include("Showtime.Movie")
                    .Include("Showtime.Room")
                    .AsNoTracking()
                join u in db.Users.AsNoTracking()
                    on b.CustomerId equals u.UserId
                where u.Username == Username
                orderby b.BookingTime descending
                select b
            ).ToList();

            foreach (Booking booking in data)
            {
                MyBookings.Add(booking);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Cannot load bookings from database!\n\n"
                + ex.GetBaseException().Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }
}