# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Przegląd

Aplikacja-towarzysz (.NET MAUI) do gry planszowej **„Wsiąść do pociągu: Legacy — Legendy zachodu"**.
Wyświetla interaktywną planszę (miasta + trasy), pozwala oznaczać miasta i przełączać stany tras.
Jeden projekt: `src/Aplication`. Interfejs po polsku, działanie offline, wyłącznie orientacja landscape.

> **Status: PoC** — bieżąca implementacja (zwłaszcza wygląd i format danych) będzie się jeszcze mocno
> zmieniać. Nie przywiązuj się do szczegółów wyglądu ani do konkretnego schematu `mapa.json`.

## Dokumentacja (czytaj najpierw)

- [docs/specyfikacja-aplikacji.md](docs/specyfikacja-aplikacji.md) — wymagania biznesowe (źródło prawdy o zakresie).
- [docs/architektura.md](docs/architektura.md) — architektura docelowa: warstwy, modele, nawigacja.
- [docs/renderowanie-mapy.md](docs/renderowanie-mapy.md) — technologia UI i strategia renderowania planszy.
- [docs/plan-etapow.md](docs/plan-etapow.md) — roadmapa etapów.

> Dokumenty opisują też elementy **docelowe/planowane**, których nie ma jeszcze w kodzie (m.in. MVVM
> `PageModels`, `SettingsPage`, `GameStateService`, liczniki, kolor gracza). Weryfikuj założenia względem kodu.

## Budowanie i uruchamianie

- Brak pliku `.sln` — buduj projekt bezpośrednio. Brak testów automatycznych w repo.
- Szybki sanity-build (Windows, używany w tej sesji):
  `dotnet build "src/Aplication/Aplication.csproj" -f net10.0-windows10.0.19041.0 -c Debug`
- Target frameworki: `net10.0-android` (+ `-ios`, `-maccatalyst`; `-windows...` tylko na Windows).
- Domyślna powłoka: PowerShell (Windows); dostępny też Bash. Dane mapy i podkład: `src/Aplication/Resources/Raw/`.

## Architektura renderowania (big picture)

Szczegóły w [renderowanie-mapy.md](docs/renderowanie-mapy.md); w skrócie:

- Cała plansza rysowana wektorowo w **jednym `IDrawable`** (`Rendering/MapDrawable`) w kontrolce
  `GraphicsView`, hostowanej przez `Controls/MapBoardView` (gesty pinch/pan/tap → `Invalidate()`).
- Geometria żyje w **przestrzeni mapy**; `Rendering/MapViewport` mapuje mapa↔ekran i jest **wspólny**
  dla rysowania oraz hit-testingu (`Rendering/MapHitTester`) — co widać, to jest klikalne.
- Renderer jest **bezstanowy**: przy każdym `Draw` odpytuje `Services/IMapInteractionState`
  (miasta oznaczone; trasy `None`/`Selected`/`Done`). Zmiana stanu → zdarzenie → `Invalidate()`.
- Dane mapy: `Resources/Raw/mapa.json` → `Services/MapDataProvider` → modele w `Models/`.
  Trasa to **łamana punktów** (`Route.Points`), a `WagonCount` = liczba jej odcinków.

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
