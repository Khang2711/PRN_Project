using System.Windows;
using System.Windows.Controls;
using CinemaManagement.ViewModels;

namespace CinemaManagement.Views;

public partial class ViewBookingView : UserControl
{
    public ViewBookingView()
    {
        InitializeComponent();
    }

    // F30: Chọn một dòng thì mở trang Booking Details
    private void BookingGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        BookingListItem? booking = BookingGrid.SelectedItem as BookingListItem;
        BookingViewModel? vm = DataContext as BookingViewModel;

        if (booking == null || vm == null)
        {
            return;
        }

        vm.SelectedBooking = booking;

        // Tạo trang chi tiết và dùng chung ViewModel
        BookingDetailsView detailsView = new BookingDetailsView();
        detailsView.DataContext = vm;
        detailsView.BackRequested += DetailsView_BackRequested;

        DetailsHost.Content = detailsView;
        DetailsHost.Visibility = Visibility.Visible;
        ListPage.Visibility = Visibility.Collapsed;
    }

    // Quay lại danh sách booking
    private void DetailsView_BackRequested(object? sender, EventArgs e)
    {
        DetailsHost.Content = null;
        DetailsHost.Visibility = Visibility.Collapsed;
        ListPage.Visibility = Visibility.Visible;

        // Bỏ chọn dòng để lần sau chọn lại vẫn mở được
        BookingGrid.SelectedItem = null;
    }
}