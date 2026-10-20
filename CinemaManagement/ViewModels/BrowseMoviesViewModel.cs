using CinemaManagement.Models;
using System.Collections.ObjectModel;

namespace CinemaManagement.ViewModels;

public class BrowseMoviesViewModel : BaseViewModel
{
    public ObservableCollection<Movie> Movies { get; } = new();

    public void Load()
    {
        Movies.Add(new Movie { Id = 1, Title = "Hành Trình Ánh Sao", Genre = "Phiêu lưu", DurationMinutes = 120 });
        Movies.Add(new Movie { Id = 2, Title = "Bí Mật Rạp Phim", Genre = "Bí ẩn", DurationMinutes = 105 });
        Movies.Add(new Movie { Id = 3, Title = "Mùa Hè Cuối", Genre = "Tình cảm", DurationMinutes = 98 });
        Movies.Add(new Movie { Id = 4, Title = "Thành Phố Ngủ Quên", Genre = "Hành động", DurationMinutes = 110 });
    }
    public BrowseMoviesViewModel()
    {
        Load();
    }
}
