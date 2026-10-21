
using System.Windows.Controls;
using CinemaManagement.ViewModels;

namespace CinemaManagement.Views;

public partial class ManageMoviesView : UserControl
{
    public ManageMoviesView()
    {
        InitializeComponent();

        DataContext = new ManageMoviesViewModel();
    }
}
