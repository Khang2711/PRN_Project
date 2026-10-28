
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
