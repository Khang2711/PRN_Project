using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class BookingDetail
{
    public int BookingDetailId { get; set; }

    public int BookingId { get; set; }

    public int ShowtimeId { get; set; }

    public int SeatId { get; set; }

    public decimal PriceAtBooking { get; set; }

    public bool IsCancelled { get; set; }

    public bool CheckedIn { get; set; }

    public DateTime? CheckedInAt { get; set; }

    public int? CheckedInByUserId { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual User? CheckedInByUser { get; set; }

    public virtual Seat Seat { get; set; } = null!;
}
