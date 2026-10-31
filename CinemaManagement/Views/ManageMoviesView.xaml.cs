<<<<<<< Updated upstream
=======

using CinemaManagement.ViewModels;
using System.ComponentModel;
>>>>>>> Stashed changes
using System.Windows.Controls;

namespace CinemaManagement.Views;

public partial class ManageMoviesView : UserControl
{
    public ManageMoviesView()
    {
        InitializeComponent();
<<<<<<< Updated upstream
=======

        if (!DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = new ViewModels.ManageMoviesViewModel();
        }
>>>>>>> Stashed changes
    }
}
