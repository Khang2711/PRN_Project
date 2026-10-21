
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CinemaManagement.Models;
using CinemaManagement.Helpers;

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

    public ManageMoviesViewModel()
    {
        // F11 - Lệnh thêm phim
        AddMovieCommand = new RelayCommand(_ => AddMovie());

        // F12 - Hiển thị danh sách phim có sẵn
        LoadMovies();
    }


    // F12 - View Movies
    private void LoadMovies()
    {
        // Thêm dữ liệu mẫu để kiểm tra chức năng F12
        Movies.Add(new Movie
        {
            Id = 1,
            Title = "Hành Trình Ánh Sao",
            Genre = "Phiêu lưu",
            DurationMinutes = 120
        });

        Movies.Add(new Movie
        {
            Id = 2,
            Title = "Bí Mật Rạp Phim",
            Genre = "Bí ẩn",
            DurationMinutes = 105
        });

        Movies.Add(new Movie
        {
            Id = 3,
            Title = "Mùa Hè Cuối",
            Genre = "Tình cảm",
            DurationMinutes = 98
        });

        Movies.Add(new Movie
        {
            Id = 4,
            Title = "Thành Phố Ngủ Quên",
            Genre = "Hành động",
            DurationMinutes = 110
        });

        // Phim mới sẽ bắt đầu từ ID = 5
        nextId = 5;

        // Cập nhật số lượng phim trên giao diện
        OnPropertyChanged(nameof(TotalMovies));
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
        if (!int.TryParse(DurationInput, out int duration) ||
            duration <= 0)
        {
            MessageBox.Show("Duration must be a positive number.");
            return;
        }

        // Tạo phim mới
        Movie movie = new Movie
        {
            Id = nextId,
            Title = TitleInput.Trim(),
            Genre = GenreInput.Trim(),
            DurationMinutes = duration
        };

        // Thêm phim vào danh sách
        Movies.Add(movie);

        OnPropertyChanged(nameof(TotalMovies));

        // Tăng ID cho phim tiếp theo
        nextId++;

        MessageBox.Show("Movie added successfully!");

        // Xóa dữ liệu ở các ô nhập
        TitleInput = "";
        GenreInput = "";
        DurationInput = "";
    }
}
