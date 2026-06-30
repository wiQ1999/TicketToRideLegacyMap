namespace Aplication.Models;

/// <summary>
/// Stan pojedynczej trasy w cyklu 3-klikowym (2.3). Jest to stan rozgrywki trzymany
/// osobno od niemutowalnej mapy bazowej — patrz serwis stanu interakcji.
/// </summary>
public enum RouteState
{
    /// <summary>Domyślny — trasa nieplanowana, rysowana w kolorze mapy bazowej.</summary>
    None,

    /// <summary>Zaznaczona/planowana — wyróżniona obrysem przy zachowaniu koloru trasy.</summary>
    Selected,

    /// <summary>Wykonana/zbudowana — wypełniona kolorem gracza.</summary>
    Done
}
