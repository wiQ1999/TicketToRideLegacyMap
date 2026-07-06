using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aplication.PageModels;

/// <summary>
/// Model menu głównego: wybór koloru wagonów, rozpoczęcie nowej rozgrywki lub kontynuacja
/// trwającej, oraz przejście do trybu deweloperskiego. Kolor i stan planu żyją w
/// <see cref="IMapInteractionState"/>.
/// </summary>
public sealed partial class MainMenuPageModel : ObservableObject
{
    private readonly IMapInteractionState _interactionState;
    private readonly IErrorHandler _errorHandler;

    public MainMenuPageModel(IMapInteractionState interactionState, IErrorHandler errorHandler)
    {
        _interactionState = interactionState;
        _errorHandler = errorHandler;
        Colors = BuildColors();
        Refresh();
    }

    public IReadOnlyList<ColorChoice> Colors { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasColor))]
    [NotifyPropertyChangedFor(nameof(ShowNewPlanHint))]
    [NotifyCanExecuteChangedFor(nameof(NewPlanCommand))]
    private ColorChoice? _selectedColor;

    [ObservableProperty]
    private bool _hasActivePlan;

    public bool HasColor => SelectedColor is not null;

    public bool ShowNewPlanHint => !HasColor;

    public void Refresh()
    {
        HasActivePlan = _interactionState.HasActivePlan;
        Select(Colors.FirstOrDefault(c => c.Value == _interactionState.WagonColor));
    }

    [RelayCommand]
    private void SelectColor(ColorChoice choice) => Select(choice);

    [RelayCommand(CanExecute = nameof(HasColor))]
    private async Task NewPlan()
    {
        _interactionState.StartNewPlan(SelectedColor!.Value);
        await NavigateAsync("map");
    }

    [RelayCommand]
    private async Task Continue()
    {
        if (SelectedColor is not null)
        {
            _interactionState.SetWagonColor(SelectedColor.Value);
        }

        await NavigateAsync("map");
    }

    [RelayCommand]
    private async Task GoToDeveloper() => await NavigateAsync("developer");

    private void Select(ColorChoice? choice)
    {
        foreach (var color in Colors)
        {
            color.IsSelected = color == choice;
        }

        SelectedColor = choice;
    }

    private async Task NavigateAsync(string route)
    {
        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError(ex);
        }
    }

    private IReadOnlyList<ColorChoice> BuildColors() =>
    [
        new ColorChoice(WagonColor.Czarny, "Czarny", Swatch("TrainBlack"), SelectColorCommand),
        new ColorChoice(WagonColor.Czerwony, "Czerwony", Swatch("TrainRed"), SelectColorCommand),
        new ColorChoice(WagonColor.Niebieski, "Niebieski", Swatch("TrainBlue"), SelectColorCommand),
        new ColorChoice(WagonColor.Zielony, "Zielony", Swatch("TrainGreen"), SelectColorCommand),
        new ColorChoice(WagonColor.Zolty, "Żółty", Swatch("TrainYellow"), SelectColorCommand)
    ];

    private static Color Swatch(string resourceKey) => (Color)Application.Current!.Resources[resourceKey];
}
