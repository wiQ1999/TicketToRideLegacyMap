using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aplication.PageModels;

/// <summary>
/// Model widoku ustawień: reset mapy ("Nowa rozgrywka", bez potwierdzenia) i wybór koloru wagonów
/// gracza z ustalonej palety. Obie akcje operują na <see cref="IMapInteractionState"/> i od razu
/// odzwierciedlają się na mapie przez jego zdarzenie <see cref="IMapInteractionState.Changed"/>.
/// </summary>
public sealed partial class SettingsPageModel(IMapInteractionState interactionState) : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedColorLabel))]
    private WagonColor _selectedColor = interactionState.WagonColor;

    public string SelectedColorLabel => SelectedColor switch
    {
        WagonColor.Czarny => "czarny",
        WagonColor.Czerwony => "czerwony",
        WagonColor.Niebieski => "niebieski",
        WagonColor.Zielony => "zielony",
        WagonColor.Zolty => "żółty",
        _ => string.Empty
    };

    [RelayCommand]
    private void SelectColor(WagonColor color)
    {
        interactionState.SetWagonColor(color);
        SelectedColor = color;
    }

    [RelayCommand]
    private void ResetGame() => interactionState.ResetMarks();
}
