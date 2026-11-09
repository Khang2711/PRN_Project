using System.Windows;
using System.Windows.Controls;

namespace CinemaManagement.Views;

public partial class BookingDetailsView : UserControl
{
    // Sự kiện báo cho ViewBookingView biết cần quay lại danh sách
    public event EventHandler? BackRequested;

    public BookingDetailsView()
    {
        InitializeComponent();
    }

    // Nút quay lại danh sách booking
    private void BackToList_Click(object sender, RoutedEventArgs e)
    {
        if (BackRequested != null)
        {
            BackRequested(this, EventArgs.Empty);
        }
    }

}