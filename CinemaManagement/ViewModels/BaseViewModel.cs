using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CinemaManagement.ViewModels;

// Lớp cơ bản giúp giao diện biết khi một thuộc tính thay đổi.
public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
