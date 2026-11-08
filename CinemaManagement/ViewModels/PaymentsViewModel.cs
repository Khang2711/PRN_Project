using CinemaManagement.Helpers;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CinemaManagement.ViewModels;

public class PaymentsViewModel : BaseViewModel
{
    private readonly string _username;
    public ObservableCollection<Booking> Bookings { get; } = new();
    public ObservableCollection<BookingDetail> Tickets { get; } = new();
    private Booking? _selectedBooking;
    public Booking? SelectedBooking { get => _selectedBooking; set { _selectedBooking = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); } }
    private BookingDetail? _selectedTicket;
    public BookingDetail? SelectedTicket { get => _selectedTicket; set { _selectedTicket = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); } }
    private string _bookingCode = "";
    public string BookingCode { get => _bookingCode; set { _bookingCode = value; OnPropertyChanged(); } }
    private string _ticketMessage = "Enter a booking code to find its tickets.";
    public string TicketMessage { get => _ticketMessage; set { _ticketMessage = value; OnPropertyChanged(); } }
    public ICommand RefreshCommand { get; }
    public ICommand ConfirmPaymentCommand { get; }
    public ICommand SearchTicketsCommand { get; }
    public ICommand CheckInCommand { get; }

    public PaymentsViewModel(string username)
    {
        _username = username;
        RefreshCommand = new RelayCommand(_ => LoadBookings());
        ConfirmPaymentCommand = new RelayCommand(_ => ConfirmPayment(), _ => SelectedBooking != null && SelectedBooking.PaymentStatus == "Unpaid" && !SelectedBooking.IsCancelled);
        SearchTicketsCommand = new RelayCommand(_ => SearchTickets());
        CheckInCommand = new RelayCommand(_ => CheckIn(), _ => SelectedTicket != null && !SelectedTicket.CheckedIn && !SelectedTicket.IsCancelled);
        LoadBookings();
    }
    private int GetOperatorId(CinemaManagementDbContext db)
    {
        var user = db.Users.SingleOrDefault(x => x.Username == _username && x.IsActive);
        if (user == null || (user.Role != "Admin" && user.Role != "Staff"))
            throw new InvalidOperationException("Only active Admin/Staff accounts can perform this action.");
        return user.UserId;
    }
    private void LoadBookings()
    {
        try
        {
            using var db = new CinemaManagementDbContext();
            var results = db.Bookings.AsNoTracking().Include(x => x.Customer).Include(x => x.Showtime).ThenInclude(x => x.Movie).OrderByDescending(x => x.BookingTime).ToList();
            Bookings.Clear();
            foreach (var item in results) Bookings.Add(item);
            SelectedBooking = null;
        }
        catch (Exception ex) { Error("Could not load bookings", ex); }
    }
    private void ConfirmPayment()
    {
        if (SelectedBooking == null) return;
        var code = SelectedBooking.BookingCode;
        if (MessageBox.Show($"Confirm payment for {code}?", "Confirm Payment", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try
        {
            using var db = new CinemaManagementDbContext();
            using var tx = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            var operatorId = GetOperatorId(db);
            var booking = db.Bookings.Include(x => x.BookingDetails).SingleOrDefault(x => x.BookingCode == code);
            if (booking == null || booking.IsCancelled || booking.Status == "Cancelled" || booking.PaymentStatus != "Unpaid")
                throw new InvalidOperationException("Booking is not available for payment confirmation.");
            var activeTickets = booking.BookingDetails.Where(x => !x.IsCancelled).ToList();
            if (activeTickets.Count == 0) throw new InvalidOperationException("Booking has no active tickets.");
            booking.PaidAmount = activeTickets.Sum(x => x.PriceAtBooking);
            booking.PaymentStatus = "Paid";
            booking.PaidAt = DateTime.Now;
            booking.ConfirmedByUserId = operatorId;
            booking.Status = "Confirmed";
            db.SaveChanges();
            tx.Commit();
            MessageBox.Show("Payment confirmed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadBookings();
        }
        catch (Exception ex) { Error("Cannot confirm payment", ex); }
    }
    private void SearchTickets()
    {
        Tickets.Clear();
        SelectedTicket = null;
        if (string.IsNullOrWhiteSpace(BookingCode)) { TicketMessage = "Please enter a booking code."; return; }
        try
        {
            using var db = new CinemaManagementDbContext();
            var booking = db.Bookings.AsNoTracking().Include(x => x.BookingDetails).ThenInclude(x => x.Seat).SingleOrDefault(x => x.BookingCode == BookingCode.Trim());
            if (booking == null) { TicketMessage = "Booking not found."; return; }
            if (booking.IsCancelled || booking.Status == "Cancelled") { TicketMessage = "This booking was cancelled."; return; }
            if (booking.PaymentStatus != "Paid") { TicketMessage = "Payment has not been confirmed."; return; }
            foreach (var ticket in booking.BookingDetails.Where(x => !x.IsCancelled).OrderBy(x => x.BookingDetailId)) Tickets.Add(ticket);
            TicketMessage = Tickets.Count == 0 ? "No active tickets found." : $"Found {Tickets.Count} ticket(s) for {booking.BookingCode}. Select a ticket to check in.";
        }
        catch (Exception ex) { Error("Cannot search tickets", ex); }
    }
    private void CheckIn()
    {
        if (SelectedTicket == null) return;
        var ticketId = SelectedTicket.BookingDetailId;
        if (MessageBox.Show($"Check in ticket #{ticketId}?", "Check In Ticket", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try
        {
            using var db = new CinemaManagementDbContext();
            using var tx = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            var operatorId = GetOperatorId(db);
            var ticket = db.BookingDetails.Include(x => x.Booking).SingleOrDefault(x => x.BookingDetailId == ticketId);
            if (ticket == null || ticket.IsCancelled || ticket.CheckedIn || ticket.Booking.IsCancelled || ticket.Booking.Status == "Cancelled" || ticket.Booking.PaymentStatus != "Paid")
                throw new InvalidOperationException("Ticket is invalid, unpaid, cancelled, or already checked in.");
            ticket.CheckedIn = true;
            ticket.CheckedInAt = DateTime.Now;
            ticket.CheckedInByUserId = operatorId;
            db.SaveChanges();
            tx.Commit();
            MessageBox.Show("Ticket checked in successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            SearchTickets();
        }
        catch (Exception ex) { Error("Cannot check in ticket", ex); }
    }
    private static void Error(string title, Exception ex) => MessageBox.Show(ex.GetBaseException().Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
}
