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
    /// Interaction logic for MovieShowtimesWindow.xaml
    /// </summary>
    public partial class MovieShowtimesWindow : Window
    {
        private readonly Movie _movie;
        public MovieShowtimesWindow(Movie movie)
        {
            InitializeComponent();
            
            _movie = movie;
            DataContext = new ViewModels.MovieShowtimesViewModel(movie);
        }

        private void ViewSeats_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            Showtime showtime = (Showtime)button.CommandParameter;

            SeatAvailabilityWindow popup = new SeatAvailabilityWindow(showtime, _movie.Title);

            popup.Owner = this;

            popup.ShowDialog();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MovieShowtimesViewModel vm)
            {
                vm.SelectedDate = null;
            }
        }
    }
}
