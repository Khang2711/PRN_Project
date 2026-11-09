using CinemaManagement.Helpers;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace CinemaManagement.ViewModels;

// Một ghế trên giao diện Booking
public class BookingSeatOption : BaseViewModel
{
    public int SeatId { get; set; }
    public string SeatCode { get; set; } = "";
    public string SeatType { get; set; } = "";
    public bool IsBooked { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged(nameof(IsSelected));
        }
    }
}

// Một ghế trong Booking Details (F30)
public class BookingSeatDetailItem
{
    public string SeatCode { get; set; } = "";
    public string SeatType { get; set; } = "";
    public decimal PriceAtBooking { get; set; }
    public bool IsCancelled { get; set; }
    public string Status => IsCancelled ? "Released" : "Reserved";
}

// Một dòng trong danh sách Booking (F29)
public class BookingListItem
{
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = "";
    public string MovieTitle { get; set; } = "";
    public string RoomName { get; set; } = "";
    public DateTime ShowtimeStart { get; set; }
    public DateTime BookingTime { get; set; }
    public string SeatCodes { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "";
    public string PaymentStatus { get; set; } = "";
    public bool CanCancel { get; set; }
    public List<BookingSeatDetailItem> SeatDetails { get; set; } = new();

    public string ShowtimeText => ShowtimeStart.ToString("dd/MM/yyyy HH:mm");
    public string BookingTimeText => BookingTime.ToString("dd/MM/yyyy HH:mm");
}

public class BookingViewModel : BaseViewModel
{
    private readonly string _role;
    private Showtime? _selectedShowtime;
    private BookingListItem? _selectedBooking;
    private string _message = "Select a showtime to start booking.";
    private string _bookingsMessage = "";
    private string _bookingCodeSearchText = "";

    public string Username { get; }

    public ObservableCollection<BookingSeatOption> Seats { get; } = new();
    public ObservableCollection<BookingListItem> MyBookings { get; } = new();

    // F32: ICollectionView để lọc danh sách booking theo mã
    public ICollectionView BookingView { get; }

    public ICommand ToggleSeatCommand { get; }
    public ICommand ConfirmBookingCommand { get; }
    public ICommand RefreshMyBookingsCommand { get; }
    public ICommand LookUpBookingCommand { get; }
    public ICommand CancelBookingCommand { get; }

    // Chỉ tài khoản Customer mới được đặt vé
    public bool IsCustomer => _role == "Customer";

    public Showtime? SelectedShowtime
    {
        get => _selectedShowtime;
        set
        {
            _selectedShowtime = value;
            OnPropertyChanged(nameof(SelectedShowtime));
            LoadSeats();
        }
    }

    public BookingListItem? SelectedBooking
    {
        get => _selectedBooking;
        set
        {
            _selectedBooking = value;
            OnPropertyChanged(nameof(SelectedBooking));
            OnPropertyChanged(nameof(CancellationNotice));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public int SelectedSeatCount => Seats.Count(s => s.IsSelected);
    public int AvailableSeatCount => Seats.Count(s => !s.IsBooked);

    // Explain why the disabled Cancel Booking button cannot be used.
    public string CancellationNotice => SelectedBooking?.PaymentStatus == "Paid"
        ? "Đơn đã thanh toán nên không thể hủy."
        : SelectedBooking?.Status == "Cancelled"
            ? "Đơn này đã được hủy."
            : SelectedBooking != null && SelectedBooking.ShowtimeStart <= DateTime.Now
                ? "Suất chiếu đã bắt đầu nên không thể hủy."
                : string.Empty;

    public decimal TotalPrice => (SelectedShowtime?.TicketPrice ?? 0) * SelectedSeatCount;

    public string BookingCodeSearchText
    {
        get => _bookingCodeSearchText;
        set
        {
            _bookingCodeSearchText = value;
            OnPropertyChanged(nameof(BookingCodeSearchText));
        }
    }

    public string Message
    {
        get => _message;
        private set
        {
            _message = value;
            OnPropertyChanged(nameof(Message));
        }
    }

    public string BookingsMessage
    {
        get => _bookingsMessage;
        private set
        {
            _bookingsMessage = value;
            OnPropertyChanged(nameof(BookingsMessage));
        }
    }

    public BookingViewModel(string username, string role)
    {
        Username = username;
        _role = role;

        BookingView = CollectionViewSource.GetDefaultView(MyBookings);
        BookingView.Filter = FilterBooking;

        ToggleSeatCommand = new RelayCommand(ToggleSeat);

        ConfirmBookingCommand = new RelayCommand(
            _ => ConfirmBooking(),
            _ => IsCustomer && SelectedShowtime != null && SelectedSeatCount > 0);

        RefreshMyBookingsCommand = new RelayCommand(_ => LoadMyBookings(), _ => IsCustomer);

        LookUpBookingCommand = new RelayCommand(_ => BookingView.Refresh());

        CancelBookingCommand = new RelayCommand(
            _ => CancelSelectedBooking(),
            _ => IsCustomer && SelectedBooking != null && SelectedBooking.CanCancel);

        LoadMyBookings();
    }

    // Bring the current preview selection into the Booking tab without saving anything.
    public void BeginBooking(Showtime showtime, IEnumerable<int>? previewSeats = null)
    {
        try
        {
            using var db = new CinemaManagementDbContext();
            SelectedShowtime = db.Showtimes.AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .FirstOrDefault(s => s.ShowtimeId == showtime.ShowtimeId) ?? showtime;
        }
        catch (Exception)
        {
            SelectedShowtime = showtime;
        }

        if (previewSeats != null)
        {
            var ids = previewSeats.ToHashSet();
            foreach (var seat in Seats.Where(s => !s.IsBooked && ids.Contains(s.SeatId)))
                seat.IsSelected = true;
            UpdateSeatInfo();
        }
    }

    // Báo cho giao diện cập nhật số ghế và tổng tiền
    private void UpdateSeatInfo()
    {
        OnPropertyChanged(nameof(SelectedSeatCount));
        OnPropertyChanged(nameof(AvailableSeatCount));
        OnPropertyChanged(nameof(TotalPrice));
        CommandManager.InvalidateRequerySuggested();
    }

    // F28: Chọn hoặc bỏ chọn ghế (ghế đã đặt thì bỏ qua)
    private void ToggleSeat(object? parameter)
    {
        if (parameter is BookingSeatOption { IsBooked: false } seat)
        {
            seat.IsSelected = !seat.IsSelected;
            UpdateSeatInfo();
        }
    }

    // F28: Tải ghế và trạng thái ghế của suất chiếu đang chọn
    private void LoadSeats()
    {
        Seats.Clear();

        if (SelectedShowtime == null)
        {
            Message = "Select a showtime to start booking.";
            UpdateSeatInfo();
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();
            int showtimeId = SelectedShowtime.ShowtimeId;
            int roomId = SelectedShowtime.RoomId;

            var bookedSeatIds = db.BookingDetails
                .Where(d => d.ShowtimeId == showtimeId && !d.IsCancelled && !d.Booking.IsCancelled && d.Booking.Status != "Cancelled")
                .Select(d => d.SeatId)
                .ToList();

            var roomSeats = db.Seats
                .AsNoTracking()
                .Where(s => s.RoomId == roomId && s.IsActive)
                .OrderBy(s => s.SeatRow).ThenBy(s => s.SeatNumber)
                .ToList();

            foreach (Seat seat in roomSeats)
            {
                Seats.Add(new BookingSeatOption
                {
                    SeatId = seat.SeatId,
                    SeatCode = seat.SeatRow + seat.SeatNumber,
                    SeatType = seat.SeatType,
                    IsBooked = bookedSeatIds.Contains(seat.SeatId)
                });
            }

            Message = roomSeats.Count == 0
                ? "There are no active seats in this room."
                : "Select available seats, then confirm your booking.";
        }
        catch (Exception ex)
        {
            Message = "Cannot load seats: " + ex.GetBaseException().Message;
        }

        UpdateSeatInfo();
    }


    // F28: Create both the booking and tickets in a single protected transaction.
    private void ConfirmBooking()
    {
        var selectedSeatIds = Seats
            .Where(s => s.IsSelected && !s.IsBooked)
            .Select(s => s.SeatId).Distinct().ToList();

        if (!IsCustomer || SelectedShowtime == null || selectedSeatIds.Count == 0)
        {
            Message = "Please select a showtime and at least one seat.";
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();
            using var tx = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            int showtimeId = SelectedShowtime.ShowtimeId;

            var customer = db.Users.SingleOrDefault(u =>
                u.Username == Username && u.IsActive && u.Role == "Customer");
            var showtime = db.Showtimes.SingleOrDefault(s =>
                s.ShowtimeId == showtimeId && s.IsActive && s.StartTime > DateTime.Now);

            if (customer == null || showtime == null || showtime.TicketPrice <= 0)
            {
                Message = "The customer account or showtime is no longer available.";
                return;
            }

            var validSeatIds = db.Seats
                .Where(s => selectedSeatIds.Contains(s.SeatId)
                    && s.RoomId == showtime.RoomId && s.IsActive)
                .Select(s => s.SeatId).ToList();

            if (validSeatIds.Count != selectedSeatIds.Count)
            {
                Message = "One or more selected seats are no longer valid.";
                return;
            }

            bool alreadyBooked = db.BookingDetails.Any(d =>
                d.ShowtimeId == showtimeId && selectedSeatIds.Contains(d.SeatId)
                && !d.IsCancelled && !d.Booking.IsCancelled
                && d.Booking.Status != "Cancelled");

            if (alreadyBooked)
            {
                Message = "A seat was booked by another customer. Refresh and choose again.";
                return;
            }

            var booking = new Booking
            {
                BookingCode = "BK" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
                CustomerId = customer.UserId,
                ShowtimeId = showtimeId,
                BookingTime = DateTime.Now,
                Status = "Pending",
                PaymentStatus = "Unpaid",
                IsCancelled = false
            };

            foreach (int seatId in selectedSeatIds)
                booking.BookingDetails.Add(new BookingDetail
                {
                    ShowtimeId = showtimeId,
                    SeatId = seatId,
                    PriceAtBooking = showtime.TicketPrice,
                    IsCancelled = false,
                    CheckedIn = false
                });

            db.Bookings.Add(booking);
            db.SaveChanges();
            tx.Commit();

            LoadSeats();
            LoadMyBookings(booking.BookingId);
            Message = "Booking created successfully. Code: " + booking.BookingCode;
        }
        catch (DbUpdateException)
        {
            LoadSeats();
            Message = "A selected seat may have been booked by another customer. Please select again.";
        }
        catch (Exception ex)
        {
            Message = "Cannot create booking: " + ex.GetBaseException().Message;
        }
    }

    // F31: Hủy booking chưa thanh toán, chưa bị hủy, chưa chiếu và chưa Completed
    private static bool IsCancellable(Booking b) =>
        !b.IsCancelled
        && b.Status != "Cancelled"
        && b.PaymentStatus == "Unpaid"
        && b.Showtime.StartTime > DateTime.Now
        && b.Status != "Completed";

    // F29: Tải danh sách booking của khách hàng; selectedBookingId = booking cần chọn lại sau khi tải
    public void LoadMyBookings(int selectedBookingId = 0)
    {
        MyBookings.Clear();
        SelectedBooking = null;

        if (!IsCustomer)
        {
            BookingsMessage = "Only customer accounts can view bookings.";
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            var bookings = db.Bookings
                .Include("Showtime.Movie")
                .Include("Showtime.Room")
                .Include("BookingDetails.Seat")
                .AsNoTracking()
                .Where(b => b.Customer.Username == Username)
                .OrderByDescending(b => b.BookingTime)
                .ToList();

            foreach (Booking b in bookings)
            {
                var seatDetails = b.BookingDetails.Select(d => new BookingSeatDetailItem
                {
                    SeatCode = d.Seat.SeatRow + d.Seat.SeatNumber,
                    SeatType = d.Seat.SeatType,
                    PriceAtBooking = d.PriceAtBooking,
                    IsCancelled = d.IsCancelled
                }).ToList();

                MyBookings.Add(new BookingListItem
                {
                    BookingId = b.BookingId,
                    BookingCode = b.BookingCode,
                    MovieTitle = b.Showtime.Movie.Title,
                    RoomName = b.Showtime.Room.RoomName,
                    ShowtimeStart = b.Showtime.StartTime,
                    BookingTime = b.BookingTime,
                    SeatCodes = seatDetails.Count == 0 ? "-" : string.Join(", ", seatDetails.Select(s => s.SeatCode)),
                    TotalAmount = seatDetails.Sum(s => s.PriceAtBooking),
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus,
                    CanCancel = IsCancellable(b),
                    SeatDetails = seatDetails
                });
            }

            SelectedBooking = MyBookings.FirstOrDefault(b => b.BookingId == selectedBookingId);
            BookingsMessage = "";
        }
        catch (Exception ex)
        {
            BookingsMessage = "Cannot load bookings: " + ex.GetBaseException().Message;
        }

        CommandManager.InvalidateRequerySuggested();
    }

    // F32: dòng nào trả về true thì được hiển thị
    private bool FilterBooking(object item) =>
        item is BookingListItem b
        && b.BookingCode.Contains(BookingCodeSearchText.Trim(), StringComparison.OrdinalIgnoreCase);

    // F31: Hủy booking chưa thanh toán và trả ghế
    private void CancelSelectedBooking()
    {
        if (SelectedBooking == null || !SelectedBooking.CanCancel)
        {
            return;
        }

        int bookingId = SelectedBooking.BookingId;

        try
        {
            using var db = new CinemaManagementDbContext();
            using var tx = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            // Chỉ lấy booking của khách hàng đang đăng nhập
            var booking = db.Bookings
                .Include("Showtime")
                .Include("BookingDetails")
                .FirstOrDefault(b => b.BookingId == bookingId && b.Customer.Username == Username);

            if (booking == null || !IsCancellable(booking))
            {
                BookingsMessage = "This booking is not eligible for cancellation.";
                return;
            }

            booking.IsCancelled = true;
            booking.Status = "Cancelled";

            foreach (BookingDetail detail in booking.BookingDetails)
            {
                detail.IsCancelled = true;
            }

            db.SaveChanges();
            tx.Commit();

            LoadMyBookings(bookingId);
            BookingsMessage = "Booking cancelled successfully. Seats have been released.";

            // Đang xem đúng suất chiếu này thì cập nhật lại ghế
            if (SelectedShowtime?.ShowtimeId == booking.ShowtimeId)
            {
                LoadSeats();
            }
        }
        catch (Exception ex)
        {
            BookingsMessage = "Cannot cancel booking: " + ex.GetBaseException().Message;
        }
    }
}