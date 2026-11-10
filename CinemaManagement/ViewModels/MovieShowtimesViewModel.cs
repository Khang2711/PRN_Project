using CinemaManagement.Models;
using CinemaManagement.Helpers;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CinemaManagement.ViewModels
{
    class MovieShowtimesViewModel : BaseViewModel
    {
        public ICommand ViewSeatsCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public event Action<Showtime>? SeatPreviewRequested;

        public Movie SelectedMovie { get; }

        public ObservableCollection<Showtime> Showtimes { get; } = new();

        private DateTime? _selectedDate = null;

        public DateTime? SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
                LoadShowtimes();
            }
        }

        public bool HasNoShowtimes
        {
            get { return Showtimes.Count == 0; }
        }

        public MovieShowtimesViewModel(Movie movie)
        {
            SelectedMovie = movie;
            ViewSeatsCommand = new RelayCommand(value =>
            {
                if (value is Showtime showtime) SeatPreviewRequested?.Invoke(showtime);
            });
            ClearFilterCommand = new RelayCommand(_ => SelectedDate = null);
            LoadShowtimes();
        }


        public void LoadShowtimes()
        {
            Showtimes.Clear();

            try
            {
                using var db = new CinemaManagementDbContext();

                int movieId = SelectedMovie.MovieId;

                // Lấy tất cả suất chiếu của phim
                var query = from s in db.Showtimes
                                .AsNoTracking()
                                .Include(s => s.Room)
                            where s.MovieId == movieId
                                  && s.IsActive
                            select s;

                // Nếu người dùng chọn ngày thì mới lọc
                if (SelectedDate != null)
                {
                    DateTime start = SelectedDate.Value.Date;
                    DateTime end = start.AddDays(1);

                    query = from s in query
                            where s.StartTime >= start
                                  && s.StartTime < end
                            select s;
                }

                // Sắp xếp theo thời gian
                var data = (from s in query
                            orderby s.StartTime
                            select s).ToList();

                foreach (Showtime showtime in data)
                {
                    Showtimes.Add(showtime);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải lịch chiếu!\n" +
                                ex.GetBaseException().Message);
            }

            OnPropertyChanged(nameof(HasNoShowtimes));
        }
    }
}
