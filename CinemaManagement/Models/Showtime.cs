using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Showtime
{
    public int ShowtimeId { get; set; }

    public int MovieId { get; set; }

    public int RoomId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public decimal TicketPrice { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Movie Movie { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;
}
