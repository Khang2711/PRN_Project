using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CinemaManagement.Models;

public partial class Booking
{
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = null!;
    public int CustomerId { get; set; }
    public int ShowtimeId { get; set; }
    public DateTime BookingTime { get; set; }
    public string Status { get; set; } = null!;
    public bool IsCancelled { get; set; }
    public string PaymentStatus { get; set; } = null!;
    public decimal? PaidAmount { get; set; }
    public DateTime? PaidAt { get; set; }
    public int? ConfirmedByUserId { get; set; }
    public string? Note { get; set; }

    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
    public virtual User? ConfirmedByUser { get; set; }
    public virtual User Customer { get; set; } = null!;
    public virtual Showtime Showtime { get; set; } = null!;

    [NotMapped]
    public string SeatSummary => string.Join(", ", BookingDetails
        .Where(d => !d.IsCancelled && d.Seat != null)
        .OrderBy(d => d.Seat.SeatRow)
        .ThenBy(d => d.Seat.SeatNumber)
        .Select(d => d.Seat.SeatRow + d.Seat.SeatNumber));

    [NotMapped]
    public decimal TotalAmount => BookingDetails
        .Where(d => !d.IsCancelled)
        .Sum(d => d.PriceAtBooking);
}