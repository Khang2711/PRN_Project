
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CinemaManagement.Helpers;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.ViewModels;

public class ShowtimesViewModel : BaseViewModel
{
    private const int CleaningMinutes = 15;
    private int? _editingShowtimeId;

    private readonly List<Showtime> _allShowtimes = new();

    public ObservableCollection<Showtime> Showtimes { get; } = new();
    public ObservableCollection<Movie> AvailableMovies { get; } = new();
    public ObservableCollection<Room> AvailableRooms { get; } = new();
    public ObservableCollection<string> Times { get; } = new();

    // SEARCH - TIM NGAY KHI GO
    private string _searchText = "";

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == (value ?? "")) return;

            _searchText = value ?? "";
            OnPropertyChanged();

            ApplySearchFilter();
        }
    }

    public string SearchResultText =>
        $"Showing {Showtimes.Count} / {_allShowtimes.Count} showtimes";

    // PHAN QUYEN
    private bool _canManageShowtimes;

    public bool CanManageShowtimes
    {
        get => _canManageShowtimes;
        set
        {
            _canManageShowtimes = value;
            OnPropertyChanged();

            if (!value)
            {
                IsFormVisible = false;
                IsPriceFormVisible = false;
            }

            CommandManager.InvalidateRequerySuggested();
        }
    }

    // SUAT CHIEU DANG CHON
    private Showtime? _selectedShowtime;

    public Showtime? SelectedShowtime
    {
        get => _selectedShowtime;
        set
        {
            _selectedShowtime = value;
            OnPropertyChanged();

            CommandManager.InvalidateRequerySuggested();
        }
    }

    // HIEN SUAT CHIEU DA NGUNG HOAT DONG
    private bool _showInactive;

    public bool ShowInactive
    {
        get => _showInactive;
        set
        {
            if (_showInactive == value) return;

            _showInactive = value;
            OnPropertyChanged();

            LoadData();
        }
    }

    // FORM ADD / EDIT
    private bool _isFormVisible;

    public bool IsFormVisible
    {
        get => _isFormVisible;
        set
        {
            _isFormVisible = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private bool _isEditing;

    public bool IsEditing
    {
        get => _isEditing;
        private set
        {
            _isEditing = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FormTitle));
            OnPropertyChanged(nameof(SaveButtonText));
        }
    }

    public string FormTitle =>
        IsEditing ? "EDIT SHOWTIME" : "CREATE SHOWTIME";

    public string SaveButtonText =>
        IsEditing ? "Save Changes" : "Save Showtime";

    // FORM THAY DOI GIA VE
    private bool _isPriceFormVisible;

    public bool IsPriceFormVisible
    {
        get => _isPriceFormVisible;
        set
        {
            _isPriceFormVisible = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    // THONG TIN FORM
    private Movie? _selectedMovie;

    public Movie? SelectedMovie
    {
        get => _selectedMovie;
        set
        {
            _selectedMovie = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DurationPreview));
            OnPropertyChanged(nameof(EndTimePreview));
        }
    }

    private Room? _selectedRoom;

    public Room? SelectedRoom
    {
        get => _selectedRoom;
        set
        {
            _selectedRoom = value;
            OnPropertyChanged();
        }
    }

    private DateTime? _selectedDate = DateTime.Today.AddDays(1);

    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            _selectedDate = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EndTimePreview));
        }
    }

    private string _selectedTime = "19:00";

    public string SelectedTime
    {
        get => _selectedTime;
        set
        {
            _selectedTime = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EndTimePreview));
        }
    }

    private string _priceText = "";

    public string PriceText
    {
        get => _priceText;
        set
        {
            _priceText = value;
            OnPropertyChanged();
        }
    }

    private string _newPriceText = "";

    public string NewPriceText
    {
        get => _newPriceText;
        set
        {
            _newPriceText = value;
            OnPropertyChanged();
        }
    }

    public string DurationPreview =>
        SelectedMovie is null
            ? "Choose a movie"
            : $"{SelectedMovie.DurationMinutes} minutes (from database)";

    public string EndTimePreview =>
        TryCalculateTimes(out _, out DateTime end)
            ? end.ToString(
                "dd/MM/yyyy HH:mm",
                CultureInfo.InvariantCulture)
            : "Choose movie, date and start time";

    // COMMANDS
    public ICommand RefreshCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand ShowCreateFormCommand { get; }
    public ICommand EditShowtimeCommand { get; }
    public ICommand SaveShowtimeCommand { get; }
    public ICommand CancelFormCommand { get; }
    public ICommand DeactivateShowtimeCommand { get; }
    public ICommand RestoreShowtimeCommand { get; }
    public ICommand SetTicketPriceCommand { get; }
    public ICommand SavePriceCommand { get; }
    public ICommand CancelPriceCommand { get; }

    public ShowtimesViewModel()
    {
        for (int hour = 0; hour < 24; hour++)
        {
            Times.Add($"{hour:D2}:00");
            Times.Add($"{hour:D2}:30");
        }

        RefreshCommand = new RelayCommand(
            _ => LoadData());

        ClearSearchCommand = new RelayCommand(
            _ => SearchText = "");

        ShowCreateFormCommand = new RelayCommand(
            _ => OpenCreateForm(),
            _ => CanManageShowtimes);

        EditShowtimeCommand = new RelayCommand(
            _ => OpenEditForm(),
            _ => CanChangeSelected());

        SaveShowtimeCommand = new RelayCommand(
            _ => SaveShowtime(),
            _ => CanManageShowtimes && IsFormVisible);

        CancelFormCommand = new RelayCommand(
            _ => IsFormVisible = false);

        DeactivateShowtimeCommand = new RelayCommand(
            _ => DeactivateShowtime(),
            _ => CanChangeSelected());

        RestoreShowtimeCommand = new RelayCommand(
            _ => RestoreShowtime(),
            _ => CanManageShowtimes
                && SelectedShowtime is { IsActive: false }
                && SelectedShowtime.StartTime > DateTime.Now);

        SetTicketPriceCommand = new RelayCommand(
            _ => OpenPriceForm(),
            _ => CanChangeSelected());

        SavePriceCommand = new RelayCommand(
            _ => SaveTicketPrice(),
            _ => CanManageShowtimes && IsPriceFormVisible);

        CancelPriceCommand = new RelayCommand(
            _ => IsPriceFormVisible = false);

        LoadData();
    }

    private bool CanChangeSelected()
    {
        return CanManageShowtimes
            && SelectedShowtime is { IsActive: true }
            && SelectedShowtime.StartTime > DateTime.Now;
    }

    // F24 - TAI DU LIEU SQL SERVER
    private void LoadData()
    {
        try
        {
            using var db = new CinemaManagementDbContext();

            var movies = db.Movies
                .AsNoTracking()
                .Where(m => m.IsActive)
                .OrderBy(m => m.Title)
                .ToList();

            var rooms = db.Rooms
                .AsNoTracking()
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoomName)
                .ToList();

            IQueryable<Showtime> query = db.Showtimes
                .AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.Room);

            if (!ShowInactive)
            {
                query = query.Where(s => s.IsActive);
            }

            var data = query
                .OrderBy(s => s.StartTime)
                .ThenBy(s => s.ShowtimeId)
                .ToList();

            AvailableMovies.Clear();

            foreach (var movie in movies)
            {
                AvailableMovies.Add(movie);
            }

            AvailableRooms.Clear();

            foreach (var room in rooms)
            {
                AvailableRooms.Add(room);
            }

            _allShowtimes.Clear();
            _allShowtimes.AddRange(data);

            // Loc lai theo noi dung Search hien tai
            ApplySearchFilter();
        }
        catch (Exception ex)
        {
            ShowError("Cannot load data from SQL Server", ex);
        }
    }

    // SEARCH - PHIM, PHONG, GIA, NGAY GIO, ID
    private void ApplySearchFilter()
    {
        int? selectedId = SelectedShowtime?.ShowtimeId;

        string[] keywords = SearchText.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries
                | StringSplitOptions.TrimEntries);

        var viCulture = CultureInfo.GetCultureInfo("vi-VN");
        var compare = viCulture.CompareInfo;

        const CompareOptions options =
            CompareOptions.IgnoreCase
            | CompareOptions.IgnoreNonSpace;

        bool Matches(Showtime showtime)
        {
            string[] values =
            {
                showtime.ShowtimeId.ToString(
                    CultureInfo.InvariantCulture),

                showtime.Movie?.Title ?? "",

                showtime.Room?.RoomName ?? "",

                showtime.TicketPrice.ToString(
                    "0.##",
                    CultureInfo.InvariantCulture),

                showtime.TicketPrice.ToString(
                    "N0",
                    CultureInfo.GetCultureInfo("en-US")),

                showtime.TicketPrice.ToString(
                    "N0",
                    viCulture),

                showtime.Movie?.DurationMinutes.ToString(
                    CultureInfo.InvariantCulture) ?? "",

                showtime.StartTime.ToString(
                    "dd/MM/yyyy HH:mm",
                    viCulture),

                showtime.EndTime.ToString(
                    "dd/MM/yyyy HH:mm",
                    viCulture),

                showtime.IsActive ? "active" : "inactive"
            };

            // Vi du: "Mai Cinema 01"
            return keywords.All(keyword =>
                values.Any(value =>
                    compare.IndexOf(
                        value,
                        keyword,
                        options) >= 0));
        }

        var filtered = keywords.Length == 0
            ? _allShowtimes.ToList()
            : _allShowtimes.Where(Matches).ToList();

        Showtimes.Clear();

        foreach (var showtime in filtered)
        {
            Showtimes.Add(showtime);
        }

        SelectedShowtime = selectedId.HasValue
            ? Showtimes.FirstOrDefault(s =>
                s.ShowtimeId == selectedId.Value)
            : null;

        OnPropertyChanged(nameof(SearchResultText));
    }

    // MO FORM THEM SUAT CHIEU
    private void OpenCreateForm()
    {
        if (!CanManageShowtimes) return;

        _editingShowtimeId = null;
        IsEditing = false;

        SelectedMovie = null;
        SelectedRoom = null;
        SelectedDate = DateTime.Today.AddDays(1);
        SelectedTime = "19:00";
        PriceText = "";

        IsPriceFormVisible = false;
        IsFormVisible = true;
    }

    // MO FORM SUA SUAT CHIEU
    private void OpenEditForm()
    {
        if (!CanChangeSelected()) return;

        var showtime = SelectedShowtime!;

        _editingShowtimeId = showtime.ShowtimeId;
        IsEditing = true;

        SelectedMovie = AvailableMovies.FirstOrDefault(m =>
            m.MovieId == showtime.MovieId);

        SelectedRoom = AvailableRooms.FirstOrDefault(r =>
            r.RoomId == showtime.RoomId);

        SelectedDate = showtime.StartTime.Date;

        SelectedTime = showtime.StartTime.ToString(
            "HH:mm",
            CultureInfo.InvariantCulture);

        PriceText = showtime.TicketPrice.ToString(
            "0.##",
            CultureInfo.InvariantCulture);

        IsPriceFormVisible = false;
        IsFormVisible = true;
    }

    // TINH GIO BAT DAU VA KET THUC
    private bool TryCalculateTimes(
        out DateTime start,
        out DateTime end)
    {
        start = default;
        end = default;

        if (SelectedMovie is null
            || SelectedDate is null
            || SelectedMovie.DurationMinutes <= 0
            || !TimeSpan.TryParseExact(
                SelectedTime?.Trim(),
                @"hh\:mm",
                CultureInfo.InvariantCulture,
                out var time)
            || time < TimeSpan.Zero
            || time >= TimeSpan.FromDays(1))
        {
            return false;
        }

        start = SelectedDate.Value.Date.Add(time);
        end = start.AddMinutes(SelectedMovie.DurationMinutes);

        return true;
    }

    // KIEM TRA GIA VE
    private static bool TryParsePrice(
        string? input,
        out decimal price)
    {
        return decimal.TryParse(
                   input,
                   NumberStyles.Number,
                   CultureInfo.InvariantCulture,
                   out price)
               && price > 0m
               && price <= 99999999.99m
               && decimal.Round(price, 2) == price;
    }

    // KIEM TRA TRUNG LICH, CONG 15 PHUT DON PHONG
    private static bool HasRoomConflict(
        CinemaManagementDbContext db,
        int roomId,
        DateTime start,
        DateTime end,
        int? excludeShowtimeId)
    {
        var schedules = db.Showtimes
            .AsNoTracking()
            .Where(s =>
                s.IsActive
                && s.RoomId == roomId
                && (!excludeShowtimeId.HasValue
                    || s.ShowtimeId != excludeShowtimeId.Value))
            .Select(s => new
            {
                s.StartTime,
                s.EndTime
            })
            .ToList();

        return schedules.Any(s =>
            start < s.EndTime.AddMinutes(CleaningMinutes)
            && end.AddMinutes(CleaningMinutes) > s.StartTime);
    }

    // KIEM TRA DA CO BOOKING CHUA
    private static bool HasActiveBookings(
        CinemaManagementDbContext db,
        int showtimeId)
    {
        return db.Bookings.Any(b =>
            b.ShowtimeId == showtimeId
            && !b.IsCancelled);
    }

    // F23 THEM VA F25 SUA
    private void SaveShowtime()
    {
        if (!CanManageShowtimes || !IsFormVisible)
            return;

        if (SelectedMovie is null || SelectedRoom is null)
        {
            MessageBox.Show(
                "Please select a movie and a room.",
                "Validation");
            return;
        }

        if (!TryCalculateTimes(
            out var start,
            out var end))
        {
            MessageBox.Show(
                "Please select a valid date and start time.",
                "Validation");
            return;
        }

        if (start <= DateTime.Now)
        {
            MessageBox.Show(
                "Showtime must start in the future.",
                "Validation");
            return;
        }

        if (!TryParsePrice(PriceText, out var price))
        {
            MessageBox.Show(
                "Enter a valid ticket price.",
                "Validation");
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            var movie = db.Movies.FirstOrDefault(m =>
                m.MovieId == SelectedMovie.MovieId
                && m.IsActive);

            var room = db.Rooms.FirstOrDefault(r =>
                r.RoomId == SelectedRoom.RoomId
                && r.IsActive);

            if (movie is null || room is null)
            {
                MessageBox.Show(
                    "Movie or room is no longer active.",
                    "Validation");
                return;
            }

            if (movie.DurationMinutes <= 0)
            {
                MessageBox.Show(
                    "Movie duration is invalid.",
                    "Validation");
                return;
            }

            end = start.AddMinutes(movie.DurationMinutes);

            Showtime? showtime = null;

            if (_editingShowtimeId.HasValue)
            {
                showtime = db.Showtimes.FirstOrDefault(s =>
                    s.ShowtimeId == _editingShowtimeId.Value
                    && s.IsActive);

                if (showtime is null
                    || showtime.StartTime <= DateTime.Now)
                {
                    MessageBox.Show(
                        "This showtime cannot be edited.",
                        "Validation");
                    return;
                }

                if (HasActiveBookings(
                    db,
                    showtime.ShowtimeId))
                {
                    MessageBox.Show(
                        "Cannot edit: this showtime has active bookings.",
                        "Booking Exists");
                    return;
                }
            }

            if (HasRoomConflict(
                db,
                room.RoomId,
                start,
                end,
                _editingShowtimeId))
            {
                MessageBox.Show(
                    "Room conflict! Keep at least 15 minutes between showtimes.",
                    "Schedule Conflict",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (showtime is null)
            {
                // INSERT
                showtime = new Showtime
                {
                    MovieId = movie.MovieId,
                    RoomId = room.RoomId,
                    StartTime = start,
                    EndTime = end,
                    TicketPrice = price,
                    IsActive = true
                };

                db.Showtimes.Add(showtime);
            }
            else
            {
                // UPDATE
                showtime.MovieId = movie.MovieId;
                showtime.RoomId = room.RoomId;
                showtime.StartTime = start;
                showtime.EndTime = end;
                showtime.TicketPrice = price;
            }

            db.SaveChanges();

            bool wasEditing = _editingShowtimeId.HasValue;
            _editingShowtimeId = null;

            IsFormVisible = false;
            LoadData();

            MessageBox.Show(
                wasEditing
                    ? "Showtime updated successfully."
                    : "Showtime created successfully.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowError("Cannot save showtime", ex);
        }
    }

    // F26 NGUNG AP DUNG SUAT CHIEU
    private void DeactivateShowtime()
    {
        if (!CanChangeSelected()) return;

        int id = SelectedShowtime!.ShowtimeId;

        var result = MessageBox.Show(
            $"Deactivate showtime #{id}?",
            "Confirm Deactivation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            using var db = new CinemaManagementDbContext();

            var entity = db.Showtimes.FirstOrDefault(s =>
                s.ShowtimeId == id && s.IsActive);

            if (entity is null
                || entity.StartTime <= DateTime.Now)
            {
                MessageBox.Show(
                    "Showtime is not available for deactivation.",
                    "Validation");
                return;
            }

            if (HasActiveBookings(db, id))
            {
                MessageBox.Show(
                    "Cannot deactivate a showtime with active bookings.",
                    "Booking Exists");
                return;
            }

            entity.IsActive = false;

            db.SaveChanges();

            IsFormVisible = false;
            IsPriceFormVisible = false;

            LoadData();

            MessageBox.Show(
                "Showtime deactivated successfully.",
                "Success");
        }
        catch (Exception ex)
        {
            ShowError("Cannot deactivate showtime", ex);
        }
    }

    // KHOI PHUC SUAT CHIEU
    private void RestoreShowtime()
    {
        if (!CanManageShowtimes
            || SelectedShowtime is not { IsActive: false })
        {
            return;
        }

        int id = SelectedShowtime.ShowtimeId;

        try
        {
            using var db = new CinemaManagementDbContext();

            var entity = db.Showtimes.FirstOrDefault(s =>
                s.ShowtimeId == id
                && !s.IsActive);

            if (entity is null
                || entity.StartTime <= DateTime.Now)
            {
                MessageBox.Show(
                    "Past showtimes cannot be restored.",
                    "Validation");
                return;
            }

            if (HasRoomConflict(
                db,
                entity.RoomId,
                entity.StartTime,
                entity.EndTime,
                id))
            {
                MessageBox.Show(
                    "Cannot restore: room/time conflict.",
                    "Schedule Conflict");
                return;
            }

            entity.IsActive = true;

            db.SaveChanges();
            LoadData();

            MessageBox.Show(
                "Showtime restored successfully.",
                "Success");
        }
        catch (Exception ex)
        {
            ShowError("Cannot restore showtime", ex);
        }
    }

    // F27 MO FORM GIA VE
    private void OpenPriceForm()
    {
        if (!CanChangeSelected()) return;

        NewPriceText = SelectedShowtime!.TicketPrice.ToString(
            "0.##",
            CultureInfo.InvariantCulture);

        IsFormVisible = false;
        IsPriceFormVisible = true;
    }

    // F27 LUU GIA VE
    private void SaveTicketPrice()
    {
        if (!CanManageShowtimes
            || !IsPriceFormVisible
            || SelectedShowtime is null)
        {
            return;
        }

        if (!TryParsePrice(
            NewPriceText,
            out var price))
        {
            MessageBox.Show(
                "Enter a valid ticket price.",
                "Validation");
            return;
        }

        try
        {
            int id = SelectedShowtime.ShowtimeId;

            using var db = new CinemaManagementDbContext();

            var entity = db.Showtimes.FirstOrDefault(s =>
                s.ShowtimeId == id
                && s.IsActive);

            if (entity is null
                || entity.StartTime <= DateTime.Now)
            {
                MessageBox.Show(
                    "Ticket price can only be changed before the showtime starts.",
                    "Validation");
                return;
            }

            if (HasActiveBookings(db, id))
            {
                MessageBox.Show(
                    "Cannot change price: this showtime has active bookings.",
                    "Booking Exists");
                return;
            }

            entity.TicketPrice = price;

            db.SaveChanges();

            IsPriceFormVisible = false;
            LoadData();

            MessageBox.Show(
                "Ticket price updated successfully.",
                "Success");
        }
        catch (Exception ex)
        {
            ShowError("Cannot change ticket price", ex);
        }
    }

    private static void ShowError(
        string title,
        Exception ex)
    {
        MessageBox.Show(
            $"{title}!\n\n{ex.GetBaseException().Message}",
            "Database Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
