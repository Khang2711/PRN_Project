
using CinemaManagement.Helpers;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CinemaManagement.ViewModels;

public class ManageMoviesViewModel : BaseViewModel
{
    // ============================================
    // F11 - F14: DỮ LIỆU QUẢN LÝ PHIM
    // ============================================

    // Danh sách phim hiển thị bên phải
    public ObservableCollection<Movie> Movies { get; } = new();

    public int TotalMovies => Movies.Count;

    // Phim đang được chọn trong bảng
    private Movie? _selectedMovie;

    public Movie? SelectedMovie
    {
        get => _selectedMovie;
        set
        {
            if (ReferenceEquals(_selectedMovie, value))
                return;

            _selectedMovie = value;
            OnPropertyChanged();

            // Chọn phim -> hiện thông tin bên trái
            Form = value == null
                ? new MovieForm()
                : MovieForm.FromMovie(value);

            // Cập nhật trạng thái nút Sửa và Xóa
            CommandManager.InvalidateRequerySuggested();
        }
    }

    // Form nhập thông tin phim
    private MovieForm _form = new();

    public MovieForm Form
    {
        get => _form;
        private set
        {
            _form = value;
            OnPropertyChanged();
        }
    }

    // Các nút chức năng
    public ICommand AddMovieCommand { get; }
    public ICommand EditMovieCommand { get; }
    public ICommand DeleteMovieCommand { get; }
    public ICommand ClearFormCommand { get; }
    public ICommand RefreshMoviesCommand { get; }


    // ============================================
    // F39: DỮ LIỆU BÁO CÁO DOANH THU
    // ============================================

    // Danh sách đơn đã thanh toán
    public ObservableCollection<PaidBookingReportItem>
        PaidBookings
    { get; } = new();

    // Doanh thu theo phim và ngày
    public ObservableCollection<SalesSummaryReportItem>
        SalesSummary
    { get; } = new();

    // Ngày bắt đầu
    private DateTime? _fromDate = new DateTime(
        DateTime.Today.Year,
        DateTime.Today.Month,
        1);

    public DateTime? FromDate
    {
        get => _fromDate;
        set
        {
            _fromDate = value;
            OnPropertyChanged();
        }
    }

    // Ngày kết thúc
    private DateTime? _toDate = DateTime.Today;

    public DateTime? ToDate
    {
        get => _toDate;
        set
        {
            _toDate = value;
            OnPropertyChanged();
        }
    }

    // Tổng số đơn đã thanh toán
    private int _totalPaidBookings;

    public int TotalPaidBookings
    {
        get => _totalPaidBookings;
        private set
        {
            _totalPaidBookings = value;
            OnPropertyChanged();
        }
    }

    // Tổng vé bán
    private int _totalTickets;

    public int TotalTickets
    {
        get => _totalTickets;
        private set
        {
            _totalTickets = value;
            OnPropertyChanged();
        }
    }

    // Tổng doanh thu
    private decimal _totalRevenue;

    public decimal TotalRevenue
    {
        get => _totalRevenue;
        private set
        {
            _totalRevenue = value;
            OnPropertyChanged();
        }
    }

    // Thông báo báo cáo
    private string _reportMessage =
        "Chọn khoảng ngày và bấm Xem báo cáo.";

    public string ReportMessage
    {
        get => _reportMessage;
        private set
        {
            _reportMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadSalesCommand { get; }


    // ============================================
    // CONSTRUCTOR - KHỞI TẠO CHỨC NĂNG
    // ============================================

    public ManageMoviesViewModel()
    {
        // F11: Thêm phim
        AddMovieCommand = new RelayCommand(
            _ => AddMovie());

        // F13: Sửa phim
        EditMovieCommand = new RelayCommand(
            _ => EditMovie(),
            _ => SelectedMovie != null);

        // F14: Ngừng hoạt động phim
        DeleteMovieCommand = new RelayCommand(
            _ => DeactivateMovie(),
            _ => SelectedMovie != null);

        // Xóa dữ liệu form
        ClearFormCommand = new RelayCommand(
            _ => ClearForm());

        // F12: Làm mới danh sách
        RefreshMoviesCommand = new RelayCommand(
            _ => LoadMovies());

        // F39: Xem doanh thu
        LoadSalesCommand = new RelayCommand(
            _ => LoadSales());

        // Tải dữ liệu ban đầu
        LoadMovies();
        LoadSales();
    }


    // ============================================
    // F12 - XEM DANH SÁCH PHIM
    // ============================================

    private void LoadMovies(int? selectMovieId = null)
    {
        try
        {
            using var db = new CinemaManagementDbContext();

            // Đọc tất cả phim từ database
            var movies = db.Movies
                .AsNoTracking()
                .OrderBy(m => m.MovieId)
                .ToList();

            // Bỏ chọn phim cũ
            SelectedMovie = null;

            // Cập nhật bảng phim
            Movies.Clear();

            foreach (var movie in movies)
            {
                Movies.Add(movie);
            }

            // Cập nhật tổng số phim
            OnPropertyChanged(nameof(TotalMovies));

            // Sau khi sửa, chọn lại phim vừa sửa
            if (selectMovieId.HasValue)
            {
                SelectedMovie = Movies.FirstOrDefault(
                    m => m.MovieId == selectMovieId.Value);
            }
        }
        catch (Exception ex)
        {
            ShowError("Không thể tải danh sách phim", ex);
        }
    }


    // ============================================
    // KIỂM TRA DỮ LIỆU FORM
    // ============================================

    private bool ValidateForm(out short duration)
    {
        duration = 0;

        string title = Form.Title?.Trim() ?? "";
        string genre = Form.Genre?.Trim() ?? "";

        // Không được để trống
        if (title.Length == 0 || genre.Length == 0)
        {
            MessageBox.Show(
                "Vui lòng nhập tên phim và thể loại.");
            return false;
        }

        // Kiểm tra thời lượng
        if (!short.TryParse(Form.DurationText, out duration)
            || duration <= 0)
        {
            MessageBox.Show(
                "Thời lượng phải là số nguyên dương.");
            return false;
        }

        // Kiểm tra độ dài
        if (title.Length > 200 ||
            genre.Length > 100 ||
            (Form.AgeRating?.Length ?? 0) > 20 ||
            (Form.Language?.Length ?? 0) > 50 ||
            (Form.PosterUrl?.Length ?? 0) > 500)
        {
            MessageBox.Show(
                "Một số thông tin vượt quá độ dài cho phép.");
            return false;
        }

        return true;
    }


    // ============================================
    // CHUYỂN THÔNG TIN TỪ FORM SANG MOVIE
    // ============================================

    private void CopyFormToMovie(Movie movie, short duration)
    {
        movie.Title = Form.Title.Trim();
        movie.Genre = Form.Genre.Trim();
        movie.DurationMinutes = duration;

        movie.Description = Form.Description?.Trim();
        movie.AgeRating = Form.AgeRating?.Trim();
        movie.Language = Form.Language?.Trim();
        movie.PosterUrl = Form.PosterUrl?.Trim();

        movie.ReleaseDate = Form.ReleaseDate.HasValue
            ? DateOnly.FromDateTime(Form.ReleaseDate.Value)
            : null;
    }


    // ============================================
    // F11 - THÊM PHIM MỚI
    // ============================================

    private void AddMovie()
    {
        // Không thêm khi đang chọn phim để sửa
        if (SelectedMovie != null)
        {
            MessageBox.Show(
                "Hãy bấm Làm mới form trước khi thêm phim.");
            return;
        }

        // Kiểm tra dữ liệu
        if (!ValidateForm(out short duration))
            return;

        try
        {
            using var db = new CinemaManagementDbContext();

            // Tạo đối tượng phim mới
            var movie = new Movie
            {
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            // Lấy dữ liệu từ form
            CopyFormToMovie(movie, duration);

            // Lưu vào database
            db.Movies.Add(movie);
            db.SaveChanges();

            // Làm mới giao diện
            LoadMovies();
            ClearForm();

            MessageBox.Show(
                "Thêm phim thành công!",
                "Thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowError("Không thể thêm phim", ex);
        }
    }


    // ============================================
    // F13 - CHỈNH SỬA THÔNG TIN PHIM
    // ============================================

    private void EditMovie()
    {
        // Bắt buộc chọn phim
        if (SelectedMovie == null)
        {
            MessageBox.Show(
                "Hãy chọn phim cần sửa ở bảng bên phải.");
            return;
        }

        // Kiểm tra dữ liệu nhập
        if (!ValidateForm(out short duration))
            return;

        // Lấy ID phim đang chọn
        int movieId = SelectedMovie.MovieId;

        try
        {
            using var db = new CinemaManagementDbContext();

            // Tìm phim theo ID
            var movie = db.Movies.FirstOrDefault(
                m => m.MovieId == movieId);

            if (movie == null)
            {
                MessageBox.Show(
                    "Không tìm thấy phim trong database.");

                LoadMovies();
                return;
            }

            // Kiểm tra thay đổi thời lượng
            if (movie.DurationMinutes != duration)
            {
                // Có suất chiếu chưa kết thúc hay không?
                bool hasUpcomingShowtime = db.Showtimes.Any(s =>
                    s.MovieId == movieId &&
                    s.IsActive &&
                    s.EndTime > DateTime.Now);

                if (hasUpcomingShowtime)
                {
                    MessageBox.Show(
                        "Không thể đổi thời lượng vì phim " +
                        "còn suất chiếu chưa kết thúc.");

                    return;
                }
            }

            // Cập nhật thông tin phim
            CopyFormToMovie(movie, duration);

            // Lưu xuống SQL Server
            db.SaveChanges();

            // Tải lại danh sách và chọn lại phim
            LoadMovies(movieId);

            MessageBox.Show(
                "Cập nhật phim thành công!",
                "Thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowError("Không thể sửa phim", ex);
        }
    }


    // ============================================
    // F14 - XÓA MỀM / NGỪNG HOẠT ĐỘNG PHIM
    // ============================================

    private void DeactivateMovie()
    {
        // Kiểm tra đã chọn phim chưa
        if (SelectedMovie == null)
        {
            MessageBox.Show(
                "Hãy chọn phim cần ngừng hoạt động.");
            return;
        }

        int movieId = SelectedMovie.MovieId;
        string title = SelectedMovie.Title;

        // Hỏi xác nhận
        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn ngừng hoạt động phim '{title}'?",
            "Xác nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
            return;

        try
        {
            using var db = new CinemaManagementDbContext();

            // Tìm phim trong database
            var movie = db.Movies.FirstOrDefault(
                m => m.MovieId == movieId);

            if (movie == null)
            {
                MessageBox.Show(
                    "Không tìm thấy phim trong database.");

                LoadMovies();
                return;
            }

            // Kiểm tra trạng thái hiện tại
            if (!movie.IsActive)
            {
                MessageBox.Show(
                    "Phim này đã ngừng hoạt động.");
                return;
            }

            // Không ngừng hoạt động khi còn suất chiếu
            bool hasUpcomingShowtime = db.Showtimes.Any(s =>
                s.MovieId == movieId &&
                s.IsActive &&
                s.EndTime > DateTime.Now);

            if (hasUpcomingShowtime)
            {
                MessageBox.Show(
                    "Không thể ngừng hoạt động phim " +
                    "vì vẫn còn suất chiếu chưa kết thúc.");

                return;
            }

            // XÓA MỀM
            // Không xóa bản ghi, chỉ đổi trạng thái
            movie.IsActive = false;

            db.SaveChanges();

            // Cập nhật giao diện
            LoadMovies();

            MessageBox.Show(
                "Đã ngừng hoạt động phim!",
                "Thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowError("Không thể ngừng hoạt động phim", ex);
        }
    }


    // ============================================
    // LÀM MỚI FORM
    // ============================================

    private void ClearForm()
    {
        // Bỏ chọn phim
        SelectedMovie = null;

        // Tạo form rỗng
        Form = new MovieForm();
    }


    // ============================================
    // F39 - BÁO CÁO DOANH THU
    // ============================================

    private void LoadSales()
    {
        // Kiểm tra ngày
        if (!FromDate.HasValue || !ToDate.HasValue)
        {
            ReportMessage = "Vui lòng chọn đủ hai ngày.";
            ClearSales();
            return;
        }

        DateTime from = FromDate.Value.Date;
        DateTime to = ToDate.Value.Date;

        if (from > to)
        {
            ReportMessage =
                "Ngày bắt đầu không được lớn hơn ngày kết thúc.";

            ClearSales();
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            // Bao gồm toàn bộ ngày kết thúc
            DateTime endExclusive = to.AddDays(1);

            // Chỉ lấy các đơn đã thanh toán
            var bookings = db.Bookings
                .AsNoTracking()
                .Include(b => b.BookingDetails)
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Movie)
                .Where(b =>
                    b.PaymentStatus == "Paid" &&
                    !b.IsCancelled &&
                    b.Status != "Cancelled" &&
                    b.PaidAt.HasValue &&
                    b.PaidAt.Value >= from &&
                    b.PaidAt.Value < endExclusive)
                .OrderByDescending(b => b.PaidAt)
                .ToList();

            // Danh sách đơn hợp lệ
            var paidRows = new List<PaidBookingReportItem>();

            foreach (var booking in bookings)
            {
                // Loại bỏ vé đã hủy
                var activeTickets = booking.BookingDetails
                    .Where(d => !d.IsCancelled)
                    .ToList();

                if (activeTickets.Count == 0)
                    continue;

                // Một dòng ứng với một đơn thanh toán
                paidRows.Add(new PaidBookingReportItem
                {
                    BookingCode = booking.BookingCode,
                    MovieId = booking.Showtime.MovieId,
                    MovieTitle = booking.Showtime.Movie.Title,
                    PaymentDate = booking.PaidAt!.Value,
                    TicketCount = activeTickets.Count,
                    Revenue = activeTickets.Sum(
                        d => d.PriceAtBooking)
                });
            }

            // Cập nhật bảng chi tiết đơn thanh toán
            PaidBookings.Clear();

            foreach (var item in paidRows)
            {
                PaidBookings.Add(item);
            }

            // Nhóm doanh thu theo phim và ngày
            var summaries = paidRows
                .GroupBy(x => new
                {
                    Date = x.PaymentDate.Date,
                    x.MovieId,
                    x.MovieTitle
                })
                .Select(g => new SalesSummaryReportItem
                {
                    Date = g.Key.Date,
                    MovieId = g.Key.MovieId,
                    MovieTitle = g.Key.MovieTitle,
                    BookingCount = g.Count(),
                    TicketCount = g.Sum(
                        x => x.TicketCount),
                    Revenue = g.Sum(
                        x => x.Revenue)
                })
                .OrderByDescending(x => x.Date)
                .ThenBy(x => x.MovieTitle)
                .ToList();

            // Cập nhật bảng tổng hợp
            SalesSummary.Clear();

            foreach (var item in summaries)
            {
                SalesSummary.Add(item);
            }

            // Cập nhật các ô thống kê
            TotalPaidBookings = paidRows.Count;

            TotalTickets = paidRows.Sum(
                x => x.TicketCount);

            TotalRevenue = paidRows.Sum(
                x => x.Revenue);

            // Hiển thị thông báo
            ReportMessage = paidRows.Count == 0
                ? $"Không có đơn đã thanh toán từ " +
                  $"{from:dd/MM/yyyy} đến {to:dd/MM/yyyy}."
                : $"Đã tải {paidRows.Count} đơn từ " +
                  $"{from:dd/MM/yyyy} đến {to:dd/MM/yyyy}.";
        }
        catch (Exception ex)
        {
            ClearSales();

            ReportMessage =
                "Không thể tải báo cáo: " +
                ex.GetBaseException().Message;

            ShowError("Lỗi báo cáo doanh thu", ex);
        }
    }


    // ============================================
    // XÓA KẾT QUẢ BÁO CÁO CŨ
    // ============================================

    private void ClearSales()
    {
        PaidBookings.Clear();
        SalesSummary.Clear();

        TotalPaidBookings = 0;
        TotalTickets = 0;
        TotalRevenue = 0m;
    }


    // ============================================
    // HÀM HIỂN THỊ LỖI DÙNG CHUNG
    // ============================================

    private static void ShowError(string message, Exception ex)
    {
        MessageBox.Show(
            message + ":\n" +
            ex.GetBaseException().Message,
            "Lỗi",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}


// ============================================
// MODEL FORM - DỮ LIỆU NHẬP PHIM
// ============================================

public class MovieForm
{
    public string Title { get; set; } = "";
    public string Genre { get; set; } = "";
    public string DurationText { get; set; } = "";
    public string Description { get; set; } = "";
    public string AgeRating { get; set; } = "";

    public DateTime? ReleaseDate { get; set; }

    public string Language { get; set; } = "";
    public string PosterUrl { get; set; } = "";

    // Đưa thông tin phim được chọn vào form
    public static MovieForm FromMovie(Movie movie)
    {
        return new MovieForm
        {
            Title = movie.Title,
            Genre = movie.Genre ?? "",
            DurationText = movie.DurationMinutes.ToString(),
            Description = movie.Description ?? "",
            AgeRating = movie.AgeRating ?? "",
            ReleaseDate = movie.ReleaseDate?.ToDateTime(
                TimeOnly.MinValue),
            Language = movie.Language ?? "",
            PosterUrl = movie.PosterUrl ?? ""
        };
    }
}


// ============================================
// MODEL F39 - CHI TIẾT ĐƠN ĐÃ THANH TOÁN
// ============================================

public class PaidBookingReportItem
{
    public string BookingCode { get; set; } = "";
    public string MovieTitle { get; set; } = "";
    public int MovieId { get; set; }

    public DateTime PaymentDate { get; set; }

    public int TicketCount { get; set; }
    public decimal Revenue { get; set; }
}


// ============================================
// MODEL F39 - TỔNG HỢP THEO PHIM / NGÀY
// ============================================

public class SalesSummaryReportItem
{
    public DateTime Date { get; set; }
    public string MovieTitle { get; set; } = "";
    public int MovieId { get; set; }
    public int BookingCount { get; set; }
    public int TicketCount { get; set; }
    public decimal Revenue { get; set; }
}
