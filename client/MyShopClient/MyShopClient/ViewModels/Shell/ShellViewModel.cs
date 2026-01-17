using CommunityToolkit.Mvvm.ComponentModel;
using MyShopClient.ViewModels.Base;

namespace MyShopClient.ViewModels;

public partial class ShellViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? _selectedItem;

    public ShellViewModel()
    {
    }

    public bool IsAdmin => App.Current.IsAdmin;
}
