using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;


namespace CinemaManagement.ViewModels;

public class BrowseMoviesViewModel : BaseViewModel
{
    public ObservableCollection<Movie> Movies { get; } = new();

    private List<Movie> _allMovies = new();

    private string _searchtext = string.Empty;

    public string searchtext
    {
        get { return _searchtext; }
        set
        {
            _searchtext = value;
            OnPropertyChanged(nameof(searchtext));
            SearchMovies();
        }
    }

    public void Load()
    {
        try
        {
            using var db = new CinemaManagementDbContext();

            // Lay danh sach phim dang hoat dong tu database
            var data = (
                from m in db.Movies.AsNoTracking()
                where m.IsActive
                orderby m.Title
                select m
            ).ToList();

            _allMovies.Clear();

            foreach (Movie movie in data)
            {
                _allMovies.Add(movie);
            }

            SearchMovies();
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

    public void SearchMovies()
    {
        Movies.Clear();

        string keyword = searchtext.Trim();

        CompareInfo compare = CultureInfo.GetCultureInfo("vi-VN").CompareInfo;

        foreach (Movie movie in _allMovies)
        {
            if (compare.IndexOf(movie.Title, keyword, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)
            {
                Movies.Add(movie);
            }
        }
    }

    public BrowseMoviesViewModel()
    {
        Load();
    }
}