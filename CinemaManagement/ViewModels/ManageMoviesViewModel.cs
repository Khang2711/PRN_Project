
using CinemaManagement.Helpers;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CinemaManagement.ViewModels;

public class ManageMoviesViewModel : BaseViewModel
{
    // Danh sách phim
    public ObservableCollection<Movie> Movies { get; set; } = new();

    // F12 - Tổng số phim hiện tại
    public int TotalMovies
    {
        get { return Movies.Count; }
    }
    // ID tự tăng
    private int nextId = 1;

    // Tên phim nhập từ giao diện
    private string titleInput = "";
    public string TitleInput
    {
        get { return titleInput; }
        set
        {
            titleInput = value;
            OnPropertyChanged();
        }
    }

    // Thể loại phim
    private string genreInput = "";
    public string GenreInput
    {
        get { return genreInput; }
        set
        {
            genreInput = value;
            OnPropertyChanged();
        }
    }

    // Thời lượng phim
    private string durationInput = "";
    public string DurationInput
    {
        get { return durationInput; }
        set
        {
            durationInput = value;
            OnPropertyChanged();
        }
    }

    // Lệnh cho nút Add Movie
    public ICommand AddMovieCommand { get; set; }

    // F39 - Revenue report
    public ObservableCollection<PaidBookingReportItem> PaidBookings { get; } = new();
    public ObservableCollection<SalesSummaryReportItem> SalesSummary { get; } = new();

    private DateTime? fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    public DateTime? FromDate
    {
        get => fromDate;
        set { fromDate = value; OnPropertyChanged(); }
    }

    private DateTime? toDate = DateTime.Today;
    public DateTime? ToDate
    {
        get => toDate;
        set { toDate = value; OnPropertyChanged(); }
    }

    private int totalPaidBookings;
    public int TotalPaidBookings
    {
        get => totalPaidBookings;
        private set { totalPaidBookings = value; OnPropertyChanged(); }
    }

    private int totalTickets;
    public int TotalTickets
    {
        get => totalTickets;
        private set { totalTickets = value; OnPropertyChanged(); }
    }

    private decimal totalRevenue;
    public decimal TotalRevenue
    {
        get => totalRevenue;
        private set { totalRevenue = value; OnPropertyChanged(); }
    }

    private string reportMessage = "Chọn khoảng thời gian và bấm Xem báo cáo.";
    public string ReportMessage
    {
        get => reportMessage;
        private set { reportMessage = value; OnPropertyChanged(); }
    }

    public ICommand LoadSalesCommand { get; }

    public ManageMoviesViewModel()
    {
        // F11 - Lệnh thêm phim
        AddMovieCommand = new RelayCommand(_ => AddMovie());

        // F12 - Hiển thị danh sách phim có sẵn
        LoadMovies();

        // F39 - Load revenue report for the current month
        LoadSalesCommand = new RelayCommand(_ => LoadSales());
        LoadSales();
    }


    // F12 - View Movies
    private void LoadMovies()
    {
        try
        {
            using var db = new CinemaManagementDbContext();

            // Lay danh sach phim tu database
            var data = (
                from m in db.Movies.AsNoTracking()
                orderby m.MovieId
                select m
            ).ToList();

            Movies.Clear();

            foreach (Movie movie in data)
            {
                Movies.Add(movie);
            }

            // Cập nhật số lượng phim trên giao diện
            OnPropertyChanged(nameof(TotalMovies));
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Cannot load movies from database!\n\n"
                + ex.GetBaseException().Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }



    // F39 - Load paid bookings and calculate ticket revenue.
    private void LoadSales()
    {
        if (!FromDate.HasValue || !ToDate.HasValue)
        {
            ReportMessage = "Vui lòng chọn đầy đủ ngày bắt đầu và ngày kết thúc.";
            return;
        }

        DateTime startDate = FromDate.Value.Date;
        DateTime endDate = ToDate.Value.Date;

        if (startDate > endDate)
        {
            ReportMessage = "Ngày bắt đầu không được lớn hơn ngày kết thúc.";
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            DateTime endExclusive = endDate.AddDays(1);

            // Chỉ lấy đơn đã thanh toán trong khoảng ngày được chọn.
            // PaidAt là thời điểm Staff xác nhận thanh toán.
            var bookings = db.Bookings
                .AsNoTracking()
                .Include(b => b.BookingDetails)
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Movie)
                .Where(b => b.PaymentStatus == "Paid"
                    && !b.IsCancelled
                    && b.PaidAt.HasValue
                    && b.PaidAt.Value >= startDate
                    && b.PaidAt.Value < endExclusive)
                .OrderByDescending(b => b.PaidAt)
                .ToList();

            var paidRows = new List<PaidBookingReportItem>();

            foreach (var booking in bookings)
            {
                var activeTickets = booking.BookingDetails
                    .Where(detail => !detail.IsCancelled)
                    .ToList();

                if (activeTickets.Count == 0)
                    continue;

                paidRows.Add(new PaidBookingReportItem
                {
                    BookingCode = booking.BookingCode,
                    MovieTitle = booking.Showtime.Movie.Title,
                    PaymentDate = booking.PaidAt!.Value,
                    TicketCount = activeTickets.Count,
                    Revenue = activeTickets.Sum(detail => detail.PriceAtBooking)
                });
            }

            PaidBookings.Clear();
            foreach (var item in paidRows)
                PaidBookings.Add(item);

            var summaries = (
                from item in paidRows
                group item by new
                {
                    Date = item.PaymentDate.Date,
                    item.MovieTitle,
                    item.MovieId
                } into grouped
                orderby grouped.Key.Date descending, grouped.Key.MovieTitle
                select new SalesSummaryReportItem
                {
                    Date = grouped.Key.Date,
                    MovieTitle = grouped.Key.MovieTitle,
                    MovieId = grouped.Key.MovieId,
                    BookingCount = grouped.Count(),
                    TicketCount = grouped.Sum(item => item.TicketCount),
                    Revenue = grouped.Sum(item => item.Revenue)
                }
            ).ToList();

            // Gán MovieId từ Showtime của booking tương ứng.
            foreach (var summary in summaries)
            {
                var matchingBooking = bookings.FirstOrDefault(b =>
                    b.Showtime.Movie.Title == summary.MovieTitle);
                if (matchingBooking != null)
                    summary.MovieId = matchingBooking.Showtime.MovieId;
            }

            SalesSummary.Clear();
            foreach (var item in summaries)
                SalesSummary.Add(item);

            TotalPaidBookings = paidRows.Count;
            TotalTickets = paidRows.Sum(item => item.TicketCount);
            TotalRevenue = paidRows.Sum(item => item.Revenue);

            ReportMessage = paidRows.Count == 0
                ? $"Không có đơn thanh toán trong khoảng {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}."
                : $"Đã tải {paidRows.Count} đơn thanh toán từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}.";
        }
        catch (Exception ex)
        {
            ReportMessage = "Không thể tải báo cáo doanh thu: "
                + ex.GetBaseException().Message;

            MessageBox.Show(
                ReportMessage,
                "Revenue Report Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    // F11 - Create Movie
    private void AddMovie()
    {
        // Kiểm tra tên và thể loại
        if (string.IsNullOrWhiteSpace(TitleInput) ||
            string.IsNullOrWhiteSpace(GenreInput))
        {
            MessageBox.Show("Please enter movie title and genre.");
            return;
        }

        // Kiểm tra thời lượng phải là số dương
        if (!short.TryParse(DurationInput, out short duration) ||
            duration <= 0)
        {
            MessageBox.Show("Duration must be a positive number.");
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            // Tạo phim mới
            Movie movie = new Movie
            {
                Title = TitleInput.Trim(),
                Genre = GenreInput.Trim(),
                DurationMinutes = duration,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            // Thêm phim vào database
            db.Movies.Add(movie);
            db.SaveChanges();

            // Cập nhật danh sách phim trên giao diện
            Movies.Add(movie);

            OnPropertyChanged(nameof(TotalMovies));

            MessageBox.Show("Movie added successfully!");

            // Xóa dữ liệu ở các ô nhập
            TitleInput = "";
            GenreInput = "";
            DurationInput = "";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Cannot add movie!\n\n"
                + ex.GetBaseException().Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }
}


public class PaidBookingReportItem
{
    public string BookingCode { get; set; } = "";
    public string MovieTitle { get; set; } = "";
    public DateTime PaymentDate { get; set; }
    public int TicketCount { get; set; }
    public decimal Revenue { get; set; }
}

public class SalesSummaryReportItem
{
    public DateTime Date { get; set; }
    public string MovieTitle { get; set; } = "";
    public int MovieId { get; set; }
    public int BookingCount { get; set; }
    public int TicketCount { get; set; }
    public decimal Revenue { get; set; }
}
