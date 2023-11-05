using WebFiles.AppModel;

namespace WebFiles.WindowsApp.ViewModels;

public class MainViewModel : ViewModelBase
{
    private UserAppModel _user;
    public UserAppModel User
    {
        get => _user;
        set => SetField(ref _user, value);
    }
}