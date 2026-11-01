using CinemaManagement.Models;
using CinemaManagement.ViewModels;
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
using System.Windows.Shapes;

namespace CinemaManagement.Views
{
    /// <summary>
    /// Interaction logic for SeatAvailabilityWindow.xaml
    /// </summary>
    public partial class SeatAvailabilityWindow : Window
    {
        public SeatAvailabilityWindow(Showtime showtime, string movieTitle)
        {
            InitializeComponent();
            DataContext = new SeatAvailabilityViewModel(showtime, movieTitle);
        }

        private void Close_Click(
            object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
