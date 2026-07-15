# Wsiąść do pociągu: Legacy — Legendy Zachodu (aplikacja-towarzysz)

Aplikacja-towarzysz (.NET MAUI) do planszówki **„Wsiąść do pociągu: Legacy — Legendy Zachodu"**.
Wyświetla interaktywną planszę (miasta + trasy), pozwala oznaczać miasta i przełączać stany tras
oraz na bieżąco liczyć wagoniki. Działa **offline**, interfejs jest **po polsku**, wyłącznie w
orientacji **poziomej**.

> **Status:** wersja podstawowa. Wgrany jest pełny podkład planszy i 61 miast (`mapa.json`); sieć
> tras jest uzupełniana ręcznie w trybie deweloperskim. Wygląd (kolory, kształty) wciąż ewoluuje.

## Funkcje

- **Interaktywna plansza** rysowana wektorowo — zoom (przyciski +/− oraz pinch) i przesuwanie.
- **Oznaczanie miast** (toggle) w kolorze gracza z gwiazdką.
- **Cykl stanu trasy** `None → Zaznaczona → Wykonana → None`, z osobnymi kanałami wizualnymi
  (wzór wypełnienia + kłódka na trasie wykonanej).
- **Liczniki wagoników** dla tras zaznaczonych i wykonanych.
- **Wyszukiwanie miasta** z podpowiedziami i wyśrodkowaniem widoku.
- **Wybór koloru wagonów** gracza (czarny, czerwony, niebieski, zielony, żółty).
- **Tryb deweloperski** do ręcznego budowania danych mapy z eksportem do schowka w formacie `mapa.json`.

## Stos technologiczny

- **.NET MAUI** (net10.0), jeden projekt `src/Aplication`.
- Plansza rysowana w jednym `IDrawable` na `GraphicsView` (`Microsoft.Maui.Graphics`), bez SkiaSharp.
- Target frameworki: `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst` oraz
  `net10.0-windows10.0.19041.0` (tylko na Windows).

## Budowanie i uruchamianie

Solution: `src/TicketToRideLegacyMap.slnx` (format `.slnx`).

Szybki sanity-build na Windows:

```bash
dotnet build "src/Aplication/Aplication.csproj" -f net10.0-windows10.0.19041.0 -c Debug
```

Uruchomienie na wybranej platformie — przez `dotnet build -t:Run -f <target>` lub z poziomu IDE
(Visual Studio / Rider). Wydawanie instalek dla testerów opisuje [docs/release.md](docs/release.md).

## Struktura projektu

```
src/Aplication/
  Rendering/     # MapDrawable, MapViewport, MapHitTester, MapMetrics, palety kolorów
  Controls/      # MapBoardView — host GraphicsView + gesty
  Services/      # dane mapy, stan interakcji, edytor trybu deweloperskiego
  Pages/         # MainMenuPage, MapPage, DeveloperPage
  Models/        # City, Route, WagonRectangle, MapData
  Resources/     # Raw/mapa.json + podkład, Styles/ (design system), Fonts/
```

## Dokumentacja

- [docs/specyfikacja-aplikacji.md](docs/specyfikacja-aplikacji.md) — wymagania biznesowe (zakres).
- [docs/architektura.md](docs/architektura.md) — warstwy, modele, nawigacja.
- [docs/renderowanie-mapy.md](docs/renderowanie-mapy.md) — strategia renderowania planszy.
- [docs/release.md](docs/release.md) — wydawanie instalek (podpisany APK + GitHub Releases).
- [CLAUDE.md](CLAUDE.md) — wskazówki dla asystenta AI i konwencje repozytorium.

## Licencje

Czcionki **Cinzel**, **Cinzel Decorative** i **Bitter** są objęte licencją SIL Open Font License
(pliki w `src/Aplication/Resources/Fonts/Licenses/`).
