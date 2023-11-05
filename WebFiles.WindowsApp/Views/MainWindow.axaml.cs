using Avalonia.Controls;
using WebFiles.AppModel;

namespace WebFiles.WindowsApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        var login = new LoginModel() { UserName = "string", Password = "string" };
    }
}