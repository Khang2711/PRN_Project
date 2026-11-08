using System.Windows;
using System.Windows.Controls;
using CinemaManagement.ViewModels;

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

    // Nút Pay Now (chưa triển khai thanh toán thật)
    private void PayNow_Click(object sender, RoutedEventArgs e)
    {
        BookingViewModel? vm = DataContext as BookingViewModel;

        if (vm == null || vm.SelectedBooking == null)
        {
            return;
        }

        if (!vm.SelectedBooking.CanPay)
        {
            return;
        }

        MessageBox.Show(
            "Payment feature has not been implemented yet.\n\n"
            + "Booking Code: " + vm.SelectedBooking.BookingCode
            + "\n\nPayment Status: Unpaid",
            "Payment",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }
}