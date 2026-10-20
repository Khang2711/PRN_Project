using System.Collections.ObjectModel;
using System.Windows.Input;
using CinemaManagement.Helpers;
using CinemaManagement.Models;

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

        if (showtime.Status != "Upcoming"
            && showtime.Status != "Scheduled")
        {
            Message = "This showtime is not available for booking.";
            return;
        }

        if (showtime.TicketPrice <= 0)
        {
            Message = "The ticket price is invalid.";
            return;
        }

        Booking booking = new Booking
        {
            BookingCode = "BK" + (AllBookings.Count + 1).ToString("D4"),

            CustomerUsername = Username,

            ShowtimeId = showtime.ShowtimeId,

            MovieTitle = showtime.MovieTitle,

            RoomName = showtime.RoomName,

            StartTime = showtime.StartTime,

            TicketPrice = showtime.TicketPrice,

            CreatedAt = DateTime.Now,

            SeatLabel = "Not selected",

            Status = "Unpaid"
        };

        AllBookings.Add(booking);

        LoadMyBookings();

        // Ngăn bấm xác nhận liên tục tạo nhiều đơn.
        _confirmed = true;

        Message = "Saved " + booking.BookingCode
            + ". Open View Booking to see your booking.";

        CommandManager.InvalidateRequerySuggested();
    }

    private void LoadMyBookings()
    {
        MyBookings.Clear();

        if (!IsCustomer)
        {
            return;
        }

        foreach (Booking booking in AllBookings)
        {
            if (string.Equals(
                booking.CustomerUsername,
                Username,
                StringComparison.OrdinalIgnoreCase))
            {
                // Đơn mới nằm phía trên.
                MyBookings.Insert(0, booking);
            }
        }
    }
}