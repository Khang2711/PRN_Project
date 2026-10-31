using System.ComponentModel;
using System.Windows.Controls;

namespace CinemaManagement.Views;

public partial class ShowtimesView : UserControl
{
    public ShowtimesView()
    {
        InitializeComponent();
        if (!DesignerProperties.GetIsInDesignMode(this))
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new ViewModels.ShowtimesViewModel();
            }
        }
    }
}
