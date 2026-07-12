# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Przegląd

Aplikacja-towarzysz (.NET MAUI) do gry planszowej **„Wsiąść do pociągu: Legacy — Legendy zachodu"**.
Wyświetla interaktywną planszę (miasta + trasy), pozwala oznaczać miasta i przełączać stany tras.
Jeden projekt: `src/Aplication`. Interfejs po polsku, działanie offline, wyłącznie orientacja landscape.

> **Status:** przygotowana podstawowa wersja aplikacji. Wgrany jest pełny podkład planszy i **61 miast**
> (`mapa.json`), ale **lista tras jest jeszcze pusta** (uzupełniana ręcznie w trybie deweloperskim),
> a pozycje miast to przybliżenie odczytane z podkładu.
> Wygląd (kolory, kształty) wciąż może się zmieniać — nie przywiązuj się do jego szczegółów.

## Dokumentacja (czytaj najpierw)

- [docs/specyfikacja-aplikacji.md](docs/specyfikacja-aplikacji.md) — wymagania biznesowe (źródło prawdy o zakresie).
- [docs/architektura.md](docs/architektura.md) — architektura docelowa: warstwy, modele, nawigacja.
- [docs/renderowanie-mapy.md](docs/renderowanie-mapy.md) — technologia UI i strategia renderowania planszy.

> Dokumenty opisują architekturę **docelową** i miejscami rozjeżdżają się z nazwami w kodzie — najważniejsze:
> serwis stanu rozgrywki to `IMapInteractionState`/`MapInteractionState` (nie `GameStateService`), a `MapPage`
> nie ma osobnego `MapPageModel` (logika wyszukiwania jest w code-behind). Weryfikuj założenia względem kodu.

## Budowanie i uruchamianie

- Solution: `src/TicketToRideLegacyMap.slnx` (format `.slnx`, nie `.sln`). Brak testów automatycznych w repo.
- Szybki sanity-build (Windows, używany w tej sesji):
  `dotnet build "src/Aplication/Aplication.csproj" -f net10.0-windows10.0.19041.0 -c Debug`
- Target frameworki: `net10.0-android` (+ `-ios`, `-maccatalyst`; `-windows...` tylko na Windows).
- Domyślna powłoka: PowerShell (Windows); dostępny też Bash. Dane mapy i podkład: `src/Aplication/Resources/Raw/`.

## Praca nad UI

- **Nie rób samodzielnie zrzutów ekranu** aplikacji (uruchamianie okna i przechwytywanie) do oceny
  wyglądu. Zamiast tego **poproś użytkownika o przesłanie zrzutu** — do porównania z mockupem i
  wizualnej weryfikacji zmian. Build służy tylko do wykrywania błędów kompilacji/XAML; zgodność
  wizualną potwierdza użytkownik.
- Design system „Legendy Zachodu" jest **współdzielony przez wszystkie widoki**: paleta kolorów w
  `Resources/Styles/Colors.xaml`, tokeny czcionek w `Resources/Styles/Typography.xaml`
  (Cinzel / Cinzel Decorative / Bitter — zarejestrowane w `MauiProgram.cs`, pliki `.ttf` w
  `Resources/Fonts/`, licencje OFL w `Resources/Fonts/Licenses/`). Stosuj tokeny
  (`{StaticResource ...}`) zamiast wpisywać kolory/rodziny czcionek na sztywno.
- Mockupy widoków (PDF) leżą w `docs/mockups/`.

## Architektura renderowania (big picture)

Szczegóły w [renderowanie-mapy.md](docs/renderowanie-mapy.md); w skrócie:

- Cała plansza rysowana wektorowo w **jednym `IDrawable`** (`Rendering/MapDrawable`) w kontrolce
  `GraphicsView`, hostowanej przez `Controls/MapBoardView` (gesty pinch/pan/tap → `Invalidate()`).
  **Gotcha:** multi-touch pinch/pan na `GraphicsView` jest zawodny na Androidzie (ograniczenie MAUI;
  działa na Windows, więc sanity-build tego nie wykryje). Pewne przybliżanie to przyciski **+/−**
  (`MapBoardView.ZoomIn`/`ZoomOut`); docelowa naprawa = natywny dotyk w handlerze. Naprawa świadomie
  odłożona przez użytkownika — nie ruszać bez prośby.
- Geometria żyje w **przestrzeni mapy**; `Rendering/MapViewport` mapuje mapa↔ekran i jest **wspólny**
  dla rysowania oraz hit-testingu (`Rendering/MapHitTester`) — co widać, to jest klikalne.
- Renderer jest **bezstanowy**: przy każdym `Draw` odpytuje `Services/IMapInteractionState`
  (miasta oznaczone; trasy `None`/`Selected`/`Done`). Zmiana stanu → zdarzenie → `Invalidate()`.
- Dane mapy: `Resources/Raw/mapa.json` → `Services/MapDataProvider` → modele w `Models/`.
  Trasa to **lista wagoników** (`Route.Wagons`; każdy `WagonRectangle` = dwa punkty przekątnej prostokąta),
  a `WagonCount` = liczba wagoników.
- Tryb deweloperski (`Pages/DeveloperPage`, `PageModels/DeveloperPageModel`) edytuje robocze kopie miast/tras
  (`Services/IDeveloperMapEditor`), z nazwami z `Services/ICityNameCatalog` i eksportem do schowka
  (`Services/IMapDataExporter`) w formacie `mapa.json`.

## Konwencje kodu

- **Bez odwołań do specyfikacji/dokumentów w komentarzach** (żadnych `(2.1)`, `(§6.3)`, nazw plików `.md`).
- Używaj **primary constructors**.
- `<summary>` **tylko** na publicznych typach i publicznych metodach; usuwaj je z właściwości, pól, stałych,
  zdarzeń, konstruktorów, składowych enum i elementów niepublicznych.
- Zostawiaj **tylko funkcjonalny kod** — usuwaj martwe elementy (nieużywane pola/stałe/pliki) przy okazji zmian.

## Pisanie dokumentacji (`docs/*.md`)

- Utrzymuj pliki MD w okolicach **~150 linii**.
- Bądź **konkretny**: fakty, nazwy klas, wzory — zamiast opisowych ogólników i meta-uwag „poza zakresem".
- **Nie powielaj** treści między dokumentami; nie opisuj niewybranych wariantów.
- Nie utrwalaj zmiennych parametrów wyglądu (kolory, kształty, wzory) ani szczegółowego schematu JSON.
- `specyfikacja-aplikacji.md` to dokument biznesowy — **nie zmieniaj bez wyraźnej prośby**.

## Commity

- **Conventional Commits** (`feat`/`fix`/`refactor`/`docs`/`style`/`chore`…), opis w trybie rozkazującym,
  po angielsku, małą literą, bez kropki. Grupuj zmiany w małe, spójne paczki (dostępny skill `/conventional-commit`).
- **Bez stopki** `Co-Authored-By`/autora w treści commita.
- Pracuj na gałęzi funkcyjnej; nie commituj bezpośrednio na `main`. Commituj/pushuj tylko na prośbę użytkownika.
