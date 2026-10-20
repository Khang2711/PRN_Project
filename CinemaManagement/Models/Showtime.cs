
using System;

namespace CinemaManagement.Models
{
    public class Showtime
    {
        public int ShowtimeId { get; set; }

        public string MovieTitle { get; set; } = "";

        public string RoomName { get; set; } = "";

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal TicketPrice { get; set; }

        public string Status { get; set; } = "";
    }
}
