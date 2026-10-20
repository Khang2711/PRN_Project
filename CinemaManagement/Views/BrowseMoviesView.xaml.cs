using System.Windows.Controls;

namespace CinemaManagement.Views;

public partial class BrowseMoviesView : UserControl
{
    public BrowseMoviesView()
    {
        InitializeComponent();
        DataContext = new ViewModels.BrowseMoviesViewModel();
    }
}
