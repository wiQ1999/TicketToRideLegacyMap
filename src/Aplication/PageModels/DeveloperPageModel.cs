using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aplication.PageModels;

/// <summary>
/// Model trybu deweloperskiego (etap: miasta). Przy wejściu ładuje aktualne dane mapy do roboczej
/// listy przez <see cref="IDeveloperMapEditor"/>. Umożliwia dodawanie miasta (wskazane położenie na
/// mapie + nazwa z <see cref="ICityNameCatalog"/> z podpowiedziami) oraz edycję i usuwanie pozycji.
/// </summary>
public sealed partial class DeveloperPageModel(
    IMapDataProvider mapDataProvider,
    IDeveloperMapEditor editor,
    ICityNameCatalog cityNameCatalog,
    IErrorHandler errorHandler) : ObservableObject
{
    private const int MaxNameSuggestions = 6;

    private bool _suppressSuggestions;

    /// <summary>Zgłaszane, gdy zmienia się nakładka mapy (lista miast lub wskazany punkt).</summary>
    public event EventHandler? OverlayChanged;

    public MapData? Map { get; private set; }

    public ObservableCollection<City> Cities { get; } = [];

    public ObservableCollection<string> NameSuggestions { get; } = [];

    public bool HasSuggestions => NameSuggestions.Count > 0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddOrUpdateCityCommand))]
    private string _cityName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddOrUpdateCityCommand))]
    [NotifyPropertyChangedFor(nameof(PickedPositionText))]
    private MapPoint? _pickedPosition;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing))]
    [NotifyPropertyChangedFor(nameof(SubmitLabel))]
    private City? _editingCity;

    public bool IsEditing => EditingCity is not null;

    public string SubmitLabel => IsEditing ? "Zapisz" : "Dodaj";

    public string PickedPositionText => PickedPosition is { } p
        ? $"Położenie: {p.X:0}, {p.Y:0}"
        : "Wskaż położenie na mapie";

    public async Task InitializeAsync()
    {
        try
        {
            Map = await mapDataProvider.GetMapDataAsync();
            editor.LoadFrom(Map);
            ClearForm();
            RefreshCities();
        }
        catch (Exception ex)
        {
            errorHandler.HandleError(ex);
        }
    }

    /// <summary>Zapisuje wskazane na mapie położenie jako kandydata dla dodawanego/edytowanego miasta.</summary>
    public void SetPickedPosition(MapPoint position)
    {
        PickedPosition = position;
        OnOverlayChanged();
    }

    [RelayCommand]
    private void SelectSuggestion(string name)
    {
        _suppressSuggestions = true;
        CityName = name;
        _suppressSuggestions = false;
        ClearSuggestions();
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private void AddOrUpdateCity()
    {
        if (cityNameCatalog.Resolve(CityName) is not { } canonicalName || PickedPosition is not { } position)
        {
            return;
        }

        if (EditingCity is { } editing)
        {
            editor.UpdateCity(editing.Id, canonicalName, position);
        }
        else
        {
            editor.AddCity(canonicalName, position);
        }

        ClearForm();
        RefreshCities();
    }

    private bool CanSubmit() =>
        PickedPosition is not null && cityNameCatalog.Resolve(CityName) is not null;

    [RelayCommand]
    private void EditCity(City city)
    {
        EditingCity = city;
        _suppressSuggestions = true;
        CityName = city.Name;
        _suppressSuggestions = false;
        ClearSuggestions();
        PickedPosition = city.Position;
        OnOverlayChanged();
    }

    [RelayCommand]
    private void DeleteCity(City city)
    {
        editor.RemoveCity(city.Id);
        if (EditingCity?.Id == city.Id)
        {
            ClearForm();
        }

        RefreshCities();
    }

    [RelayCommand]
    private void CancelEdit()
    {
        ClearForm();
        OnOverlayChanged();
    }

    partial void OnCityNameChanged(string value)
    {
        if (_suppressSuggestions)
        {
            return;
        }

        var matches = cityNameCatalog.Suggest(value, MaxNameSuggestions);
        NameSuggestions.Clear();
        foreach (var match in matches)
        {
            NameSuggestions.Add(match);
        }

        OnPropertyChanged(nameof(HasSuggestions));
    }

    private void RefreshCities()
    {
        Cities.Clear();
        foreach (var city in editor.Cities)
        {
            Cities.Add(city);
        }

        OnOverlayChanged();
    }

    private void ClearForm()
    {
        EditingCity = null;
        _suppressSuggestions = true;
        CityName = string.Empty;
        _suppressSuggestions = false;
        ClearSuggestions();
        PickedPosition = null;
    }

    private void ClearSuggestions()
    {
        NameSuggestions.Clear();
        OnPropertyChanged(nameof(HasSuggestions));
    }

    private void OnOverlayChanged() => OverlayChanged?.Invoke(this, EventArgs.Empty);
}
