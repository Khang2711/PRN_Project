using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CinemaManagement.ViewModels;

namespace CinemaManagement.Views
{
    /// <summary>
    /// Interaction logic for ViewBookingView.xaml
    /// </summary>
    public partial class ViewBookingView : UserControl
    {
        public ViewBookingView()
        {
            InitializeComponent();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is BookingViewModel viewModel)
            {
                viewModel.LoadMyBookings();
            }
        }
    }
}