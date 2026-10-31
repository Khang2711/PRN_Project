using System.ComponentModel;
using System.Windows.Controls;

namespace CinemaManagement.Views;

public partial class RoomsSeatsView : UserControl
{
    public RoomsSeatsView()
    {
        InitializeComponent();
        if (!DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = new ViewModels.RoomsSeatsViewModel();
        }
    }
}
