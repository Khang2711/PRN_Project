using CinemaManagement.ViewModels;
using System.Windows;
using System.Windows.Controls;
namespace CinemaManagement.Views;
public partial class AccountView : UserControl
{
    public AccountView() { InitializeComponent(); }
    private void CreateUser_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AccountViewModel vm && vm.CreateUser(NewPasswordBox.Password))
            NewPasswordBox.Clear();
    }
}
