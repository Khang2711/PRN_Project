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

    // F30: Chọn một booking để mở trang chi tiết
    private void BookingGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BookingGrid.SelectedItem is not BookingListItem ||
            DataContext is not BookingViewModel vm)
        {
            return;
        }

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
