using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Aplication.PageModels;

/// <summary>Pojedynczy kafelek koloru wagonów w menu głównym: wartość, etykieta, próbka i stan wyboru.</summary>
public sealed partial class ColorChoice(WagonColor value, string label, Color swatch, ICommand selectCommand)
    : ObservableObject
{
    public WagonColor Value { get; } = value;

    public string Label { get; } = label;

    public Color Swatch { get; } = swatch;

    public ICommand SelectCommand { get; } = selectCommand;

    [ObservableProperty]
    private bool _isSelected;
}
