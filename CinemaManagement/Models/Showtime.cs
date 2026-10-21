
using System;

namespace CinemaManagement.Models
{
    public class Showtime
    {
        // Mã suất chiếu
        public int ShowtimeId { get; set; }

        // Tên phim
        public string MovieTitle { get; set; } = "";

        // Tên phòng chiếu
        public string RoomName { get; set; } = "";

        // Thời gian bắt đầu
        public DateTime StartTime { get; set; }

        // Thời gian kết thúc
        public DateTime EndTime { get; set; }

        // Thời lượng phim (phút)
        public int DurationMinutes
        {
            get
            {
                return (int)(EndTime - StartTime).TotalMinutes;
            }
        }

        // Giá vé
        public decimal TicketPrice { get; set; }

        // Trạng thái suất chiếu
        public string Status { get; set; } = "";
    }
}
