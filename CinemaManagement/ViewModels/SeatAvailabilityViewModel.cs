using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CinemaManagement.ViewModels
{
    public class SeatAvailabilityItem : BaseViewModel
    {
        public int SeatId { get; set; }
        public string SeatCode { get; set; } = "";
        public string SeatType { get; set; } = "Standard";
        public bool IsBooked { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Status));
            }
        }

        public string Status => IsBooked ? "Đã đặt" : IsSelected ? "Đang chọn" : "Còn trống";
    }

    public class SeatAvailabilityViewModel : BaseViewModel
    {
        private readonly string _username;

        public Showtime SelectedShowtime { get; }
        public string MovieTitle { get; }
        public string RoomName { get; }
        public ObservableCollection<SeatAvailabilityItem> Seats { get; } = new();

        public int TotalSeats => Seats.Count;
        public int BookedSeats => Seats.Count(s => s.IsBooked);
        public int AvailableSeats => Seats.Count(s => !s.IsBooked);
        public int SelectedSeats => Seats.Count(s => s.IsSelected);
        public decimal TotalPrice => SelectedSeats * SelectedShowtime.TicketPrice;
        public bool HasNoSeats => Seats.Count == 0;

        private string _message = "Chọn một hoặc nhiều ghế còn trống.";
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        public SeatAvailabilityViewModel(Showtime showtime, string movieTitle, string username)
        {
            SelectedShowtime = showtime;
            MovieTitle = movieTitle;
            _username = username;
            RoomName = showtime.Room?.RoomName ?? "Phòng " + showtime.RoomId;
            LoadSeats();
        }

        public void ToggleSeat(SeatAvailabilityItem? seat)
        {
            if (seat == null || seat.IsBooked) return;
            seat.IsSelected = !seat.IsSelected;
            OnPropertyChanged(nameof(SelectedSeats));
            OnPropertyChanged(nameof(TotalPrice));
        }

        public bool CreateBooking()
        {
            var selectedIds = Seats.Where(s => s.IsSelected).Select(s => s.SeatId).ToList();
            if (selectedIds.Count == 0)
            {
                Message = "Vui lòng chọn ít nhất một ghế.";
                return false;
            }

            try
            {
                using var db = new CinemaManagementDbContext();
                using var tx = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

                var customer = db.Users.SingleOrDefault(u =>
                    u.Username == _username && u.IsActive && u.Role == "Customer");

                if (customer == null)
                    throw new InvalidOperationException("Tài khoản khách hàng không hợp lệ hoặc đã bị khóa.");

                var showtime = db.Showtimes.SingleOrDefault(s =>
                    s.ShowtimeId == SelectedShowtime.ShowtimeId &&
                    s.RoomId == SelectedShowtime.RoomId &&
                    s.IsActive);

                if (showtime == null || showtime.StartTime <= DateTime.Now)
                    throw new InvalidOperationException("Suất chiếu không còn khả dụng.");

                var validSeats = db.Seats
                    .Where(s => selectedIds.Contains(s.SeatId)
                        && s.RoomId == showtime.RoomId && s.IsActive)
                    .Select(s => s.SeatId)
                    .ToList();

                if (validSeats.Count != selectedIds.Count)
                    throw new InvalidOperationException("Một hoặc nhiều ghế không còn hợp lệ.");

                var alreadyBooked = db.BookingDetails
                    .Where(d => d.ShowtimeId == showtime.ShowtimeId
                        && selectedIds.Contains(d.SeatId)
                        && !d.IsCancelled
                        && !d.Booking.IsCancelled
                        && d.Booking.Status != "Cancelled")
                    .Select(d => d.SeatId)
                    .ToList();

                if (alreadyBooked.Count > 0)
                    throw new InvalidOperationException("Có ghế vừa được người khác đặt. Vui lòng tải lại sơ đồ ghế.");

                var booking = new Booking
                {
                    BookingCode = "BK" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant(),
                    CustomerId = customer.UserId,
                    ShowtimeId = showtime.ShowtimeId,
                    BookingTime = DateTime.Now,
                    Status = "Pending",
                    PaymentStatus = "Unpaid",
                    IsCancelled = false
                };

                db.Bookings.Add(booking);
                db.SaveChanges();

                foreach (int seatId in selectedIds)
                {
                    db.BookingDetails.Add(new BookingDetail
                    {
                        BookingId = booking.BookingId,
                        ShowtimeId = showtime.ShowtimeId,
                        SeatId = seatId,
                        PriceAtBooking = showtime.TicketPrice,
                        IsCancelled = false,
                        CheckedIn = false
                    });
                }

                db.SaveChanges();
                tx.Commit();

                Message = $"Đặt vé thành công! Mã đặt vé: {booking.BookingCode}. Tổng tiền: {selectedIds.Count * showtime.TicketPrice:N0} VND. Vui lòng đến quầy để thanh toán.";
                foreach (var seat in Seats.Where(s => selectedIds.Contains(s.SeatId)))
                {
                    seat.IsSelected = false;
                    seat.IsBooked = true;
                }
                OnPropertyChanged(nameof(SelectedSeats));
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(BookedSeats));
                OnPropertyChanged(nameof(AvailableSeats));
                return true;
            }
            catch (Exception ex)
            {
                Message = "Không thể đặt vé: " + ex.GetBaseException().Message;
                return false;
            }
        }

        public void LoadSeats()
        {
            Seats.Clear();
            try
            {
                using var db = new CinemaManagementDbContext();
                var roomSeats = db.Seats.AsNoTracking()
                    .Where(s => s.RoomId == SelectedShowtime.RoomId && s.IsActive)
                    .OrderBy(s => s.SeatRow).ThenBy(s => s.SeatNumber)
                    .ToList();

                var bookedSeatIds = db.BookingDetails.AsNoTracking()
                    .Where(d => d.ShowtimeId == SelectedShowtime.ShowtimeId
                        && !d.IsCancelled
                        && !d.Booking.IsCancelled
                        && d.Booking.Status != "Cancelled")
                    .Select(d => d.SeatId)
                    .ToHashSet();

                foreach (var seat in roomSeats)
                {
                    Seats.Add(new SeatAvailabilityItem
                    {
                        SeatId = seat.SeatId,
                        SeatCode = seat.SeatRow + seat.SeatNumber,
                        SeatType = seat.SeatType,
                        IsBooked = bookedSeatIds.Contains(seat.SeatId)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải trạng thái ghế!\n" + ex.GetBaseException().Message);
            }

            OnPropertyChanged(nameof(TotalSeats));
            OnPropertyChanged(nameof(BookedSeats));
            OnPropertyChanged(nameof(AvailableSeats));
            OnPropertyChanged(nameof(HasNoSeats));
            OnPropertyChanged(nameof(SelectedSeats));
            OnPropertyChanged(nameof(TotalPrice));
        }
    }
}