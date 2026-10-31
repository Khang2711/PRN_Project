using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CinemaManagement.Helpers;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.ViewModels
{
    public class ShowtimesViewModel : BaseViewModel
    {
        // F24: Danh sach suat chieu lay tu SQL
        public ObservableCollection<Showtime> Showtimes { get; }
            = new ObservableCollection<Showtime>();

        public ObservableCollection<string> Times { get; }
            = new ObservableCollection<string>();

        // Phan quyen Admin / User
        private bool _canManageShowtimes;

        public bool CanManageShowtimes
        {
            get => _canManageShowtimes;
            set
            {
                _canManageShowtimes = value;
                OnPropertyChanged();

                if (!value)
                    IsCreateFormVisible = false;

                CommandManager.InvalidateRequerySuggested();
            }
        }

        // Du lieu nhap vao form
        private string _movieName = "";

        public string MovieName
        {
            get => _movieName;
            set
            {
                _movieName = value;
                OnPropertyChanged();
            }
        }

        private string _roomName = "";

        public string RoomName
        {
            get => _roomName;
            set
            {
                _roomName = value;
                OnPropertyChanged();
            }
        }

        private string _durationText = "";

        public string DurationText
        {
            get => _durationText;
            set
            {
                _durationText = value;
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

        // Hien thi gio ket thuc tu dong
        public string EndTimePreview
        {
            get
            {
                if (TryCalculateTimes(
                    out DateTime startTime,
                    out DateTime endTime))
                {
                    return endTime.ToString(
                        "dd/MM/yyyy HH:mm",
                        CultureInfo.InvariantCulture);
                }

                return "Enter a valid duration";
            }
        }

        // An / hien form tao suat chieu
        private bool _isCreateFormVisible;

        public bool IsCreateFormVisible
        {
            get => _isCreateFormVisible;
            set
            {
                _isCreateFormVisible = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand ShowCreateFormCommand { get; }
        public ICommand CreateShowtimeCommand { get; }

        public ShowtimesViewModel()
        {
            // Gio bat dau moi 30 phut
            for (int hour = 8; hour <= 23; hour++)
            {
                Times.Add($"{hour:D2}:00");
                Times.Add($"{hour:D2}:30");
            }

            ShowCreateFormCommand = new RelayCommand(
                _ => IsCreateFormVisible = !IsCreateFormVisible,
                _ => CanManageShowtimes
            );

            CreateShowtimeCommand = new RelayCommand(
                _ => CreateShowtime(),
                _ => CanManageShowtimes
            );

            // F24: Load tu database, khong dung du lieu cung
            LoadShowtimesFromDatabase();
        }

        // F24: Doc du lieu tu SQL Server
        private void LoadShowtimesFromDatabase()
        {
            try
            {
                using var db = new CinemaManagementDbContext();

                // JOIN Showtimes, Movies va Rooms
                var data = (
                    from s in db.Showtimes.AsNoTracking()
                    join m in db.Movies.AsNoTracking()
                        on s.MovieId equals m.MovieId
                    join r in db.Rooms.AsNoTracking()
                        on s.RoomId equals r.RoomId
                    where s.IsActive
                    orderby s.StartTime, s.ShowtimeId
                    select new
                    {
                        Showtime = s,
                        Movie = m,
                        Room = r
                    }
                ).ToList();

                Showtimes.Clear();

                foreach (var item in data)
                {
                    item.Showtime.Movie = item.Movie;
                    item.Showtime.Room = item.Room;

                    Showtimes.Add(item.Showtime);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Cannot load showtimes from database!\n\n"
                    + ex.GetBaseException().Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // Tinh gio bat dau va ket thuc
        private bool TryCalculateTimes(
            out DateTime startTime,
            out DateTime endTime)
        {
            startTime = default;
            endTime = default;

            if (SelectedDate == null)
                return false;

            if (!TimeSpan.TryParseExact(
                SelectedTime,
                @"hh\:mm",
                CultureInfo.InvariantCulture,
                out TimeSpan time))
            {
                return false;
            }

            if (!int.TryParse(
                DurationText,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int duration))
            {
                return false;
            }

            if (duration <= 0 || duration > 600)
                return false;

            startTime = SelectedDate.Value.Date.Add(time);
            endTime = startTime.AddMinutes(duration);

            return true;
        }

        // F23: Tao suat chieu va luu vao SQL Server
        private void CreateShowtime()
        {
            // Kiem tra quyen Admin
            if (!CanManageShowtimes)
            {
                MessageBox.Show(
                    "Only Admin can create showtimes!",
                    "Access Denied");
                return;
            }

            string movieName = MovieName.Trim();
            string roomName = RoomName.Trim();

            if (string.IsNullOrWhiteSpace(movieName) ||
                string.IsNullOrWhiteSpace(roomName))
            {
                MessageBox.Show(
                    "Please enter movie name and room name!");
                return;
            }

            if (!TryCalculateTimes(
                out DateTime startTime,
                out DateTime endTime))
            {
                MessageBox.Show(
                    "Invalid date, time or duration!");
                return;
            }

            // Gia ve phai hop le va trong gioi han DECIMAL(10,2)
            if (!decimal.TryParse(
                    PriceText,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal price) ||
                price <= 0 ||
                price > 99999999.99m)
            {
                MessageBox.Show(
                    "Please enter a valid ticket price!");
                return;
            }

            if (startTime <= DateTime.Now)
            {
                MessageBox.Show(
                    "Showtime must be in the future!");
                return;
            }

            try
            {
                using var db = new CinemaManagementDbContext();

                // Tim phim trong bang Movies
                var movie = db.Movies.FirstOrDefault(
                    m => m.Title == movieName && m.IsActive
                );

                if (movie == null)
                {
                    MessageBox.Show(
                        "Movie does not exist in database!\n" +
                        "Please use a movie from the Movies table.",
                        "Movie Not Found");
                    return;
                }

                // Tim phong trong bang Rooms
                var room = db.Rooms.FirstOrDefault(
                    r => r.RoomName == roomName && r.IsActive
                );

                if (room == null)
                {
                    MessageBox.Show(
                        "Room does not exist in database!\n" +
                        "Please use a room from the Rooms table.",
                        "Room Not Found");
                    return;
                }

                // Thoi luong phai khop voi thong tin phim trong SQL
                int duration =
                    (int)(endTime - startTime).TotalMinutes;

                if (duration != movie.DurationMinutes)
                {
                    MessageBox.Show(
                        $"Duration of {movie.Title} in database " +
                        $"is {movie.DurationMinutes} minutes.\n" +
                        "Please enter the correct duration.",
                        "Duration Mismatch");
                    return;
                }

                // Lay lich hien co cua phong tu SQL
                var roomSchedules = db.Showtimes
                    .AsNoTracking()
                    .Where(s =>
                        s.RoomId == room.RoomId &&
                        s.IsActive)
                    .Select(s => new
                    {
                        s.StartTime,
                        s.EndTime
                    })
                    .ToList();

                // Kiem tra trung lich va 15 phut don phong
                bool isConflict = roomSchedules.Any(s =>
                    startTime < s.EndTime.AddMinutes(15) &&
                    endTime.AddMinutes(15) > s.StartTime
                );

                if (isConflict)
                {
                    MessageBox.Show(
                        "This room already has a conflicting " +
                        "showtime!\nPlease choose another time.",
                        "Schedule Conflict",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Tao suat chieu trong database
                var newShowtime = new Showtime
                {
                    MovieId = movie.MovieId,
                    RoomId = room.RoomId,
                    StartTime = startTime,
                    EndTime = endTime,
                    TicketPrice = price,
                    IsActive = true
                };

                db.Showtimes.Add(newShowtime);

                // INSERT vao SQL Server
                db.SaveChanges();

                // Load lai DataGrid tu database
                LoadShowtimesFromDatabase();

                MessageBox.Show(
                    "Showtime saved to database successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Reset form
                MovieName = "";
                RoomName = "";
                DurationText = "";
                PriceText = "";

                IsCreateFormVisible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Cannot save showtime!\n\n" +
                    ex.GetBaseException().Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}