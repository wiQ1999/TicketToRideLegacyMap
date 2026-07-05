using Aplication.Pages;

namespace Aplication;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("map", typeof(MapPage));
        Routing.RegisterRoute("developer", typeof(DeveloperPage));
        Routing.RegisterRoute("settings", typeof(SettingsPage));
    }
}
