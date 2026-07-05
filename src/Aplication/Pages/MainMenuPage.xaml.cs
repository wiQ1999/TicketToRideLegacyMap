namespace Aplication.Pages;

public partial class MainMenuPage : ContentPage
{
    public MainMenuPage(MainMenuPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }
}
