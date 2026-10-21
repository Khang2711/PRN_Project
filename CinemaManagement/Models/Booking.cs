namespace CinemaManagement.Models;

public class Booking
{
    public string BookingCode { get; set; } = "";

    public string CustomerUsername { get; set; } = "";

    public int ShowtimeId { get; set; }

    public string MovieTitle { get; set; } = "";

    public string RoomName { get; set; } = "";

    public DateTime StartTime { get; set; }

    public decimal TicketPrice { get; set; }

    public DateTime CreatedAt { get; set; }

    public string SeatLabel { get; set; } = "Not selected";

    public string Status { get; set; } = "Unpaid";
}