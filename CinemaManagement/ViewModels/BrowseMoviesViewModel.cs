using CinemaManagement.Models;
using System.Collections.ObjectModel;
using System.Globalization;


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

    private Movie _selectedMovie;
    public Movie selectedMovie
    {
        get { return _selectedMovie; }
        set
        {
            _selectedMovie = value;
            OnPropertyChanged(nameof(selectedMovie));
        }
    }

    public BrowseMoviesViewModel()
    {
        Load();
    }

    public void Load()
    {
        _allMovies.Clear();

        _allMovies.Add(new Movie { Id = 1, Title = "Hành Trình Ánh Sao", Genre = "Phiêu lưu", DurationMinutes = 120 });
        _allMovies.Add(new Movie { Id = 2, Title = "Bí Mật Rạp Phim", Genre = "Bí ẩn", DurationMinutes = 105 });
        _allMovies.Add(new Movie { Id = 3, Title = "Mùa Hè Cuối", Genre = "Tình cảm", DurationMinutes = 98 });
        _allMovies.Add(new Movie { Id = 4, Title = "Thành Phố Ngủ Quên", Genre = "Hành động", DurationMinutes = 110 });

        SearchMovies();
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

    
}