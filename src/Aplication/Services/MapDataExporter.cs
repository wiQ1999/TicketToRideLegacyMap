using System.Text.Json;

namespace Aplication.Services;

/// <summary>
/// Domyślna implementacja eksportu danych mapy trybu deweloperskiego. Buduje DTO zgodne ze schematem
/// <c>mapa.json</c> (patrz <see cref="MapDataProvider"/>) i serializuje je z nazwami pól w formacie
/// camelCase, tak by wynik można było wprost wkleić jako plik danych mapy.
/// </summary>
public sealed class MapDataExporter : IMapDataExporter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string ExportToJson(MapSize canvasSize, IReadOnlyList<City> cities, IReadOnlyList<Route> routes)
    {
        var dto = new MapFileDto
        {
            CanvasSize = new MapSizeDto { Width = canvasSize.Width, Height = canvasSize.Height },
            Cities = cities.Select(c => new CityDto { Id = c.Id, Name = c.Name, X = c.X, Y = c.Y }).ToList(),
            Routes = routes.Select(r => new RouteDto
            {
                Id = r.Id,
                From = r.CityFromId,
                To = r.CityToId,
                Wagons = r.Wagons.Select(w => new WagonDto
                {
                    A = new PointDto { X = w.A.X, Y = w.A.Y },
                    B = new PointDto { X = w.B.X, Y = w.B.Y }
                }).ToList()
            }).ToList()
        };

        return JsonSerializer.Serialize(dto, SerializerOptions);
    }

    public async Task ExportToClipboardAsync(MapSize canvasSize, IReadOnlyList<City> cities, IReadOnlyList<Route> routes)
    {
        var json = ExportToJson(canvasSize, cities, routes);
        await Clipboard.Default.SetTextAsync(json).ConfigureAwait(false);
    }

    // --- DTO odpowiadające schematowi mapa.json ---

    private sealed class MapFileDto
    {
        public MapSizeDto? CanvasSize { get; set; }
        public List<CityDto>? Cities { get; set; }
        public List<RouteDto>? Routes { get; set; }
    }

    private sealed class MapSizeDto
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }

    private sealed class CityDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }

    private sealed class RouteDto
    {
        public string? Id { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public List<WagonDto>? Wagons { get; set; }
    }

    private sealed class WagonDto
    {
        public PointDto? A { get; set; }
        public PointDto? B { get; set; }
    }

    private sealed class PointDto
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
