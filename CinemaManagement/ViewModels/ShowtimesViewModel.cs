
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
        // F24: Danh sách suất chiếu
        public ObservableCollection<Showtime> Showtimes { get; set; }

        // Danh sách phim và phòng mẫu
        public ObservableCollection<string> Movies { get; set; }
        public ObservableCollection<string> Rooms { get; set; }
        public ObservableCollection<string> Times { get; set; }

        // Thông tin tạo suất chiếu
        private string _selectedMovie = "Avatar 3";
        public string SelectedMovie
        {
            get => _selectedMovie;
            set
            {
                _selectedMovie = value;
                OnPropertyChanged();
            }
        }

        private string _selectedRoom = "Room 01";
        public string SelectedRoom
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
            }
        }

        // Hiển thị form tạo suất chiếu
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

            Movies = new ObservableCollection<string>
            {
                "Avatar 3",
                "Doraemon",
                "Conan Movie",
                "Avengers"
            };

            Rooms = new ObservableCollection<string>
            {
                "Room 01",
                "Room 02",
                "Room 03"
            };

            Times = new ObservableCollection<string>();

            // Tạo các mốc giờ cách nhau 30 phút
            for (int hour = 8; hour <= 23; hour++)
            {
                Times.Add($"{hour:D2}:00");
                Times.Add($"{hour:D2}:30");
            }

            // Tạo Command
            ShowCreateFormCommand = new RelayCommand(
                _ => IsCreateFormVisible = !IsCreateFormVisible
            );

            CreateShowtimeCommand = new RelayCommand(
                _ => CreateShowtime()
            );

            // Dữ liệu mẫu
            LoadSampleData();
        }

        // F24: Hiển thị dữ liệu cứng
        private void LoadSampleData()
        {
            DateTime tomorrow = DateTime.Today.AddDays(1);

            Showtimes.Add(new Showtime
            {
                ShowtimeId = 1,
                MovieTitle = "Avatar 3",
                RoomName = "Room 01",
                StartTime = tomorrow.AddHours(9),
                EndTime = tomorrow.AddHours(12),
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

        // F23: Tạo suất chiếu
        private void CreateShowtime()
        {
            // Kiểm tra dữ liệu
            if (string.IsNullOrWhiteSpace(SelectedMovie) ||
                string.IsNullOrWhiteSpace(SelectedRoom) ||
                SelectedDate == null ||
                string.IsNullOrWhiteSpace(SelectedTime))
            {
                MessageBox.Show(
                    "Please fill in all fields!",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            // Chuyển giờ dạng HH:mm thành TimeSpan
            if (!DateTime.TryParseExact(
                    SelectedTime,
                    "HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsedTime))
            {
                MessageBox.Show("Invalid time!");
                return;
            }

            DateTime startTime =
                SelectedDate.Value.Date.Add(parsedTime.TimeOfDay);

            if (startTime <= DateTime.Now)
            {
                MessageBox.Show(
                    "Showtime must be in the future!",
                    "Validation Error"
                );
                return;
            }

            // Thời lượng và giá vé mẫu
            int duration;
            decimal price;

            switch (SelectedMovie)
            {
                case "Avatar 3":
                    duration = 180;
                    price = 90000;
                    break;

                case "Doraemon":
                    duration = 120;
                    price = 70000;
                    break;

                case "Conan Movie":
                    duration = 120;
                    price = 80000;
                    break;

                case "Avengers":
                    duration = 180;
                    price = 100000;
                    break;

                default:
                    MessageBox.Show("Invalid movie!");
                    return;
            }

            DateTime endTime = startTime.AddMinutes(duration);

            // Kiểm tra trùng lịch trong cùng phòng
            // Cần 15 phút giữa hai suất để dọn phòng
            bool isConflict = Showtimes.Any(s =>
                s.RoomName.Equals(
                    SelectedRoom,
                    StringComparison.OrdinalIgnoreCase) &&
                startTime < s.EndTime.AddMinutes(15) &&
                endTime.AddMinutes(15) > s.StartTime
            );

            if (isConflict)
            {
                MessageBox.Show(
                    "This room already has a conflicting showtime!" +
                    "\nPlease choose another room or time.",
                    "Schedule Conflict",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            // Tự tăng mã suất chiếu
            int newId = Showtimes.Count == 0
                ? 1
                : Showtimes.Max(s => s.ShowtimeId) + 1;

            Showtime newShowtime = new Showtime
            {
                ShowtimeId = newId,
                MovieTitle = SelectedMovie,
                RoomName = SelectedRoom,
                StartTime = startTime,
                EndTime = endTime,
                TicketPrice = price,
                Status = "Upcoming"
            };

            // Thêm vào danh sách
            Showtimes.Add(newShowtime);

            MessageBox.Show(
                "Showtime created successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // Đóng form
            IsCreateFormVisible = false;
        }
    }
}
