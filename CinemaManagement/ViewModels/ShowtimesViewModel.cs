
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
        public ObservableCollection<Showtime> Showtimes { get; set; }
        public ObservableCollection<string> Times { get; set; }

        // Phan quyen: chi Admin duoc quan ly suat chieu
        private bool _canManageShowtimes = false;

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

        public string EndTimePreview
        {
            get
            {
                if (TryCalculateTimes(out DateTime start,
                                      out DateTime end))
                {
                    return end.ToString(
                        "dd/MM/yyyy HH:mm",
                        CultureInfo.InvariantCulture);
                }

                return "Enter a valid duration";
            }
        }

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

        public ICommand ShowCreateFormCommand { get; }
        public ICommand CreateShowtimeCommand { get; }

        public ShowtimesViewModel()
        {
            Showtimes = new ObservableCollection<Showtime>();
            Times = new ObservableCollection<string>();

            for (int hour = 8; hour <= 23; hour++)
            {
                Times.Add($"{hour:D2}:00");
                Times.Add($"{hour:D2}:30");
            }

            // Chi Admin moi duoc mo form
            ShowCreateFormCommand = new RelayCommand(
                _ => IsCreateFormVisible = !IsCreateFormVisible,
                _ => CanManageShowtimes
            );

            // Chi Admin moi duoc tao suat chieu
            CreateShowtimeCommand = new RelayCommand(
                _ => CreateShowtime(),
                _ => CanManageShowtimes
            );

            LoadSampleData();
        }

        // F24 - Du lieu mau
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

        // Tinh thoi gian ket thuc
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

        // F23 - Tao suat chieu
        private void CreateShowtime()
        {
            // Kiem tra quyen truoc khi xu ly
            if (!CanManageShowtimes)
            {
                MessageBox.Show(
                    "Only Admin can create showtimes!",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(MovieName) ||
                string.IsNullOrWhiteSpace(RoomName))
            {
                MessageBox.Show(
                    "Please enter movie name and room name!",
                    "Validation Error");
                return;
            }

            if (!TryCalculateTimes(
                out DateTime startTime,
                out DateTime endTime))
            {
                MessageBox.Show(
                    "Please enter a valid date, time and duration!",
                    "Validation Error");
                return;
            }

            if (!decimal.TryParse(
                PriceText,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal price) || price <= 0)
            {
                MessageBox.Show(
                    "Please enter a valid ticket price!",
                    "Validation Error");
                return;
            }

            if (startTime <= DateTime.Now)
            {
                MessageBox.Show(
                    "Showtime must be in the future!",
                    "Validation Error");
                return;
            }

            string room = RoomName.Trim();

            // Kiem tra trung lich, kem 15 phut don phong
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
                    "This room already has a conflicting showtime!",
                    "Schedule Conflict",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            int newId = Showtimes.Count == 0
                ? 1
                : Showtimes.Max(s => s.ShowtimeId) + 1;

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

            Showtimes.Add(newShowtime);

            MessageBox.Show(
                "Showtime created successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            MovieName = "";
            RoomName = "";
            DurationText = "";
            PriceText = "";
            IsCreateFormVisible = false;
        }
    }
}
