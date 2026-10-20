
using System;
using System.Collections.ObjectModel;
using CinemaManagement.Models;

namespace CinemaManagement.ViewModels
{
    public class ShowtimesViewModel : BaseViewModel
    {
        // Danh sách suất chiếu
        public ObservableCollection<Showtime> Showtimes
        { get; set; }

        public ShowtimesViewModel()
        {
            Showtimes = new ObservableCollection<Showtime>();

            // Dữ liệu cứng, chưa sử dụng database
            DateTime tomorrow = DateTime.Today.AddDays(1);

            Showtimes.Add(new Showtime
            {
                ShowtimeId = 1,
                MovieTitle = "Avatar 3",
                RoomName = "Room 01",
                StartTime = tomorrow.AddHours(9),
                EndTime = tomorrow.AddHours(12),
                TicketPrice = 90000,
                Status = "Upcoming"
            });

            Showtimes.Add(new Showtime
            {
                ShowtimeId = 2,
                MovieTitle = "Doraemon",
                RoomName = "Room 02",
                StartTime = tomorrow.AddHours(10),
                EndTime = tomorrow.AddHours(12),
                TicketPrice = 70000,
                Status = "Upcoming"
            });

            Showtimes.Add(new Showtime
            {
                ShowtimeId = 3,
                MovieTitle = "Conan Movie",
                RoomName = "Room 03",
                StartTime = tomorrow.AddHours(13),
                EndTime = tomorrow.AddHours(15),
                TicketPrice = 80000,
                Status = "Upcoming"
            });

            Showtimes.Add(new Showtime
            {
                ShowtimeId = 4,
                MovieTitle = "Avengers",
                RoomName = "Room 01",
                StartTime = tomorrow.AddHours(15),
                EndTime = tomorrow.AddHours(18),
                TicketPrice = 100000,
                Status = "Upcoming"
            });
        }
    }
}
