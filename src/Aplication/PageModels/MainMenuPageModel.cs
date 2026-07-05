using CommunityToolkit.Mvvm.Input;

namespace Aplication.PageModels;

/// <summary>
/// Model ekranu głównego: komendy nawigacji do trybu mapy i trybu deweloperskiego.
/// </summary>
public sealed partial class MainMenuPageModel(IErrorHandler errorHandler)
{
    [RelayCommand]
    private async Task GoToMap()
    {
        await NavigateAsync("map");
    }

    [RelayCommand]
    private async Task GoToDeveloper()
    {
        await NavigateAsync("developer");
    }

    private async Task NavigateAsync(string route)
    {
        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            errorHandler.HandleError(ex);
        }
    }
}
