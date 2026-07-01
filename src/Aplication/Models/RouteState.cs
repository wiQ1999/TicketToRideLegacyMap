namespace Aplication.Models;

/// <summary>
/// Stan pojedynczej trasy w cyklu 3-klikowym. Jest to stan rozgrywki trzymany
/// osobno od niemutowalnej mapy bazowej — patrz serwis stanu interakcji.
/// </summary>
public enum RouteState
{
    None,
    Selected,
    Done
}
