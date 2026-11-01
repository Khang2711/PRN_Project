using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CinemaManagement.ViewModels
{
    public class SeatAvailabilityItem
    {
        public string SeatCode { get; set; } = "";

        public bool IsBooked { get; set; }

        public string Status
        {
            get { return IsBooked ? "Đã đặt" : "Còn trống"; }
        }
    }

    public class SeatAvailabilityViewModel : BaseViewModel
    {
        public Showtime SelectedShowtime { get; }

        public string MovieTitle { get; }

        public string RoomName { get; }

        public ObservableCollection<SeatAvailabilityItem> Seats { get; } = new();

        public int TotalSeats
        {
            get { return Seats.Count; }
        }

        public int BookedSeats
        {
            get { return Seats.Count(s => s.IsBooked); }
        }

        public int AvailableSeats
        {
            get { return Seats.Count(s => !s.IsBooked); }
        }

        public bool HasNoSeats
        {
            get { return Seats.Count == 0; }
        }

        public SeatAvailabilityViewModel(Showtime showtime, string movieTitle)
        {
            SelectedShowtime = showtime;
            MovieTitle = movieTitle;

            RoomName = showtime.Room?.RoomName
                ?? "Phòng " + showtime.RoomId;

            LoadSeats();
        }


        public void LoadSeats()
        {
            Seats.Clear();

            try
            {
                using var db = new CinemaManagementDbContext();

                int roomId = SelectedShowtime.RoomId;
                int showtimeId = SelectedShowtime.ShowtimeId;

                // Lấy tất cả ghế của phòng đang hoạt động
                var seatQuery = from s in db.Seats.AsNoTracking()
                                where s.RoomId == roomId
                                      && s.IsActive
                                orderby s.SeatRow, s.SeatNumber
                                select s;

                var roomSeats = seatQuery.ToList();

                // Lấy những ghế đã được đặt trong suất chiếu
                var bookedQuery = from d in db.BookingDetails.AsNoTracking()
                                  where d.ShowtimeId == showtimeId
                                       && !d.IsCancelled
                                       && !d.Booking.IsCancelled
                                       && d.Booking.Status != "Cancelled"
                                  select d.SeatId;

                var bookedSeatIds = bookedQuery.ToHashSet();

                // Kiểm tra trạng thái từng ghế
                foreach (Seat seat in roomSeats)
                {
                    SeatAvailabilityItem item = new SeatAvailabilityItem
                        {
                            SeatCode = seat.SeatRow + seat.SeatNumber,

                            IsBooked = bookedSeatIds.Contains(seat.SeatId)
                        };

                    Seats.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể tải trạng thái ghế!\n" +
                    ex.GetBaseException().Message);
            }

            OnPropertyChanged(nameof(TotalSeats));
            OnPropertyChanged(nameof(BookedSeats));
            OnPropertyChanged(nameof(AvailableSeats));
            OnPropertyChanged(nameof(HasNoSeats));
        }
    }
}
