using System.Text.Json;

namespace Aplication.Services;

/// <summary>
/// Wczytuje dane mapy z osadzonego zasobu <c>mapa.json</c> (<c>Resources/Raw/</c>),
/// deserializuje je do modeli domenowych i waliduje. Działa w pełni offline.
/// </summary>
public sealed class MapDataProvider : IMapDataProvider
{
    private const string MapResourceFileName = "mapa.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private MapData? _cached;

    public async Task<MapData> GetMapDataAsync()
    {
        if (_cached is not null)
        {
            return _cached;
        }

        await _loadLock.WaitAsync().ConfigureAwait(false);
        try
        {
            return _cached ??= await LoadAsync().ConfigureAwait(false);
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private static async Task<MapData> LoadAsync()
    {
        MapFileDto? dto;
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync(MapResourceFileName)
                .ConfigureAwait(false);
            dto = await JsonSerializer.DeserializeAsync<MapFileDto>(stream, SerializerOptions)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not MapDataException)
        {
            throw new MapDataException(
                $"Nie udało się wczytać danych mapy z zasobu „{MapResourceFileName}\".", ex);
        }

        if (dto is null)
        {
            throw new MapDataException($"Zasób „{MapResourceFileName}\" jest pusty lub niepoprawny.");
        }

        return Build(dto);
    }

    private static MapData Build(MapFileDto dto)
    {
        if (dto.CanvasSize is not { } canvas || canvas.Width <= 0 || canvas.Height <= 0)
        {
            throw new MapDataException("Dane mapy muszą zawierać dodatni rozmiar planszy (canvasSize).");
        }

        var canvasSize = new MapSize(canvas.Width, canvas.Height);

        var cities = BuildCities(dto.Cities, canvasSize);
        var routes = BuildRoutes(dto.Routes, cities, canvasSize);

        return new MapData(canvasSize, cities.Values.ToList(), routes);
    }

    private static IReadOnlyDictionary<string, City> BuildCities(
        List<CityDto>? cityDtos, MapSize canvas)
    {
        if (cityDtos is null || cityDtos.Count == 0)
        {
            throw new MapDataException("Dane mapy muszą zawierać co najmniej jedno miasto.");
        }

        var cities = new Dictionary<string, City>(cityDtos.Count);
        foreach (var dto in cityDtos)
        {
            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                throw new MapDataException("Każde miasto musi mieć niepuste pole „id\".");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new MapDataException($"Miasto „{dto.Id}\" musi mieć niepuste pole „name\".");
            }

            if (dto.X < 0 || dto.X > canvas.Width || dto.Y < 0 || dto.Y > canvas.Height)
            {
                throw new MapDataException(
                    $"Współrzędne miasta „{dto.Id}\" wykraczają poza rozmiar planszy.");
            }

            if (!cities.TryAdd(dto.Id, new City(dto.Id, dto.Name, dto.X, dto.Y)))
            {
                throw new MapDataException($"Zduplikowany identyfikator miasta: „{dto.Id}\".");
            }
        }

        return cities;
    }

    private static IReadOnlyList<Route> BuildRoutes(
        List<RouteDto>? routeDtos, IReadOnlyDictionary<string, City> cities, MapSize canvas)
    {
        if (routeDtos is null)
        {
            return Array.Empty<Route>();
        }

        var routes = new List<Route>(routeDtos.Count);
        var seenIds = new HashSet<string>(routeDtos.Count);
        foreach (var dto in routeDtos)
        {
            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                throw new MapDataException("Każda trasa musi mieć niepuste pole „id\".");
            }

            if (!seenIds.Add(dto.Id))
            {
                throw new MapDataException($"Zduplikowany identyfikator trasy: „{dto.Id}\".");
            }

            if (string.IsNullOrWhiteSpace(dto.From) || !cities.ContainsKey(dto.From))
            {
                throw new MapDataException(
                    $"Trasa „{dto.Id}\" odwołuje się do nieistniejącego miasta początkowego „{dto.From}\".");
            }

            if (string.IsNullOrWhiteSpace(dto.To) || !cities.ContainsKey(dto.To))
            {
                throw new MapDataException(
                    $"Trasa „{dto.Id}\" odwołuje się do nieistniejącego miasta końcowego „{dto.To}\".");
            }

            if (dto.Wagons is not { Count: >= 1 })
            {
                throw new MapDataException(
                    $"Trasa „{dto.Id}\" musi mieć co najmniej jeden wagonik.");
            }

            var wagons = dto.Wagons.Select(w => BuildWagon(dto.Id, w, canvas)).ToArray();
            routes.Add(new Route(dto.Id, dto.From, dto.To, wagons));
        }

        return routes;
    }

    private static WagonRectangle BuildWagon(string routeId, WagonDto dto, MapSize canvas)
    {
        if (dto.A is not { } a || dto.B is not { } b)
        {
            throw new MapDataException(
                $"Trasa „{routeId}\" ma wagonik bez obu punktów przekątnej („a\"/„b\").");
        }

        var wagon = new WagonRectangle(new MapPoint(a.X, a.Y), new MapPoint(b.X, b.Y));

        // Wagonik może być obrócony pod dowolnym kątem — w zakresie planszy muszą mieścić się
        // wszystkie 4 rogi (dwa podane wprost i dwa wyliczone z założenia kątów prostych), nie
        // tylko podana przekątna.
        foreach (var corner in wagon.Corners)
        {
            ValidateInCanvas(routeId, corner, canvas);
        }

        return wagon;
    }

    private static void ValidateInCanvas(string routeId, MapPoint point, MapSize canvas)
    {
        if (point.X < 0 || point.X > canvas.Width || point.Y < 0 || point.Y > canvas.Height)
        {
            throw new MapDataException(
                $"Trasa „{routeId}\" ma wagonik z punktem poza rozmiarem planszy.");
        }
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
