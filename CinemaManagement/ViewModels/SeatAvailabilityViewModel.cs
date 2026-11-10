using CinemaManagement.Models;
using CinemaManagement.Helpers;
using System.Collections.Generic;
using System.Windows.Input;
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
        public ICommand ToggleSeatCommand { get; }
        public ICommand RefreshSeatsCommand { get; }
        public ICommand ContinueBookingCommand { get; }
        public event Action<IReadOnlyList<int>>? ContinueBookingRequested;

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
            ToggleSeatCommand = new RelayCommand(item => ToggleSeat(item as SeatAvailabilityItem));
            RefreshSeatsCommand = new RelayCommand(_ =>
            {
                LoadSeats();
                Message = "Đã cập nhật trạng thái ghế.";
            });
            ContinueBookingCommand = new RelayCommand(_ => ContinueBooking());
            LoadSeats();
        }

        public void ToggleSeat(SeatAvailabilityItem? seat)
        {
            if (seat == null || seat.IsBooked) return;
            seat.IsSelected = !seat.IsSelected;
            OnPropertyChanged(nameof(SelectedSeats));
            OnPropertyChanged(nameof(TotalPrice));
        }

        private void ContinueBooking()
        {
            // No booking is saved in this window; the Booking tab confirms it.
            var seatIds = Seats.Where(s => s.IsSelected && !s.IsBooked)
                .Select(s => s.SeatId).ToArray();
            if (seatIds.Length == 0)
            {
                Message = "Vui lòng chọn ít nhất một ghế trước khi tiếp tục.";
                return;
            }
            ContinueBookingRequested?.Invoke(seatIds);
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