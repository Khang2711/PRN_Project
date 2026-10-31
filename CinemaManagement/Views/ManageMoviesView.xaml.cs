
using CinemaManagement.ViewModels;
using System.ComponentModel;

using System.Windows.Controls;
using CinemaManagement.ViewModels;

namespace CinemaManagement.Views;

public partial class ManageMoviesView : UserControl
{
    public ManageMoviesView()
    {
        InitializeComponent();

        if (!DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = new ViewModels.ManageMoviesViewModel();
        }
    }
}
