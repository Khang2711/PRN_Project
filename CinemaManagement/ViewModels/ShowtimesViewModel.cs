
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CinemaManagement.Helpers;
using CinemaManagement.Models;

namespace CinemaManagement.ViewModels
{
    public class ShowtimesViewModel : BaseViewModel
    {
        // F24: Danh sach suat chieu
        public ObservableCollection<Showtime> Showtimes
        { get; set; }

        // Danh sach gio bat dau
        public ObservableCollection<string> Times
        { get; set; }

        // F23: Nhap ten phim
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

        // Nhap ten phong
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

        // Nhap thoi luong phim (phut)
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

        // Nhap gia ve
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

        // Chon ngay chieu
        private DateTime? _selectedDate =
            DateTime.Today.AddDays(1);

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

        // Chon gio bat dau
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

        // Tu dong tinh gio ket thuc
        public string EndTimePreview
        {
            get
            {
                if (TryCalculateTimes(
                    out DateTime start,
                    out DateTime end))
                {
                    return end.ToString(
                        "dd/MM/yyyy HH:mm",
                        CultureInfo.InvariantCulture);
                }

                return "Enter a valid duration";
            }
        }

        // Hien thi form them suat chieu
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
            Showtimes = new ObservableCollection<Showtime>();

            Times = new ObservableCollection<string>();

            // Tao gio bat dau cach nhau 30 phut
            for (int hour = 8; hour <= 23; hour++)
            {
                Times.Add($"{hour:D2}:00");
                Times.Add($"{hour:D2}:30");
            }

            ShowCreateFormCommand = new RelayCommand(
                _ => IsCreateFormVisible = !IsCreateFormVisible
            );

            CreateShowtimeCommand = new RelayCommand(
                _ => CreateShowtime()
            );

            LoadSampleData();
        }

        // F24: Du lieu mau
        private void LoadSampleData()
        {
            DateTime tomorrow = DateTime.Today.AddDays(1);

            Showtimes.Add(new Showtime
            {
                ShowtimeId = 1,
                MovieTitle = "Avatar 3",
                RoomName = "Room 01",
                StartTime = tomorrow.AddHours(9),
                EndTime = tomorrow.AddHours(11),
                TicketPrice = 90000,
                Status = "Upcoming"
            });

            Showtimes.Add(new Showtime
            {
                ShowtimeId = 2,
                MovieTitle = "Doraemon",
                RoomName = "Room 02",
                StartTime = tomorrow.AddHours(10),
                EndTime = tomorrow.AddHours(12),
                TicketPrice = 70000,
                Status = "Upcoming"
            });

            Showtimes.Add(new Showtime
            {
                ShowtimeId = 3,
                MovieTitle = "Conan Movie",
                RoomName = "Room 03",
                StartTime = tomorrow.AddHours(13),
                EndTime = tomorrow.AddHours(15),
                TicketPrice = 80000,
                Status = "Upcoming"
            });

            Showtimes.Add(new Showtime
            {
                ShowtimeId = 4,
                MovieTitle = "Avengers",
                RoomName = "Room 01",
                StartTime = tomorrow.AddHours(15),
                EndTime = tomorrow.AddHours(18),
                TicketPrice = 100000,
                Status = "Upcoming"
            });
        }

        // Tinh gio ket thuc tu thoi luong phim
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

            // Cong so phut vao gio bat dau
            endTime = startTime.AddMinutes(duration);

            return true;
        }

        // F23: Tao suat chieu
        private void CreateShowtime()
        {
            // Kiem tra ten phim va phong
            if (string.IsNullOrWhiteSpace(MovieName) ||
                string.IsNullOrWhiteSpace(RoomName))
            {
                MessageBox.Show(
                    "Please enter movie name and room name!",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Kiem tra thoi luong va ngay gio
            if (!TryCalculateTimes(
                out DateTime startTime,
                out DateTime endTime))
            {
                MessageBox.Show(
                    "Please select a valid date/time and " +
                    "enter a duration from 1 to 600 minutes!",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Kiem tra gia ve
            if (!decimal.TryParse(
                PriceText,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal price) || price <= 0)
            {
                MessageBox.Show(
                    "Please enter a valid ticket price!",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Khong cho tao suat chieu trong qua khu
            if (startTime <= DateTime.Now)
            {
                MessageBox.Show(
                    "Showtime must be in the future!",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            string room = RoomName.Trim();

            // Kiem tra trung lich cung phong
            // Them 15 phut de don dep phong
            bool isConflict = Showtimes.Any(s =>
                s.RoomName.Equals(
                    room,
                    StringComparison.OrdinalIgnoreCase) &&
                startTime < s.EndTime.AddMinutes(15) &&
                endTime.AddMinutes(15) > s.StartTime
            );

            if (isConflict)
            {
                MessageBox.Show(
                    "This room already has a conflicting " +
                    "showtime!\nPlease choose another time or room.",
                    "Schedule Conflict",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Tao ID moi
            int newId = Showtimes.Count == 0
                ? 1
                : Showtimes.Max(s => s.ShowtimeId) + 1;

            // Tao doi tuong suat chieu
            Showtime newShowtime = new Showtime
            {
                ShowtimeId = newId,
                MovieTitle = MovieName.Trim(),
                RoomName = room,
                StartTime = startTime,
                EndTime = endTime,
                TicketPrice = price,
                Status = "Upcoming"
            };

            // Them vao danh sach
            Showtimes.Add(newShowtime);

            MessageBox.Show(
                "Showtime created successfully!",
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
    }
}
