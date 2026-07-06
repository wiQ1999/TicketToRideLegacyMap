namespace Aplication.Pages;

public partial class MainMenuPage : ContentPage
{
    private readonly MainMenuPageModel _pageModel;

    public MainMenuPage(MainMenuPageModel pageModel)
    {
        InitializeComponent();
        _pageModel = pageModel;
        BindingContext = pageModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _pageModel.Refresh();
    }
}
