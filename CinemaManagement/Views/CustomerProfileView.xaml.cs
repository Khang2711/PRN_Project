using System.Windows.Controls;
using CinemaManagement.ViewModels;

namespace CinemaManagement.Views;

public partial class CustomerProfileView : UserControl
{
    private readonly CustomerProfileViewModel _viewModel;

    public CustomerProfileView()
    {
        InitializeComponent();
        _viewModel = new CustomerProfileViewModel();
        DataContext = _viewModel;
    }

    // MainWindow already calls this method; preserve that integration contract.
    public void LoadProfile(string username)
    {
        _viewModel.LoadProfile(username);
    }
}
