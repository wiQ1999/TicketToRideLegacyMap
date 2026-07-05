# Plan etapów pracy nad projektem

Aplikacja-towarzysz do gry "Wsiąść do pociągu: Legacy - Legendy zachodu".
Technologia bazowa: **.NET MAUI** (projekt `src/Aplication`), platformy docelowe Android i iOS.

Źródła: [docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md),
[docs/architektura.md](architektura.md), [docs/renderowanie-mapy.md](renderowanie-mapy.md).

Legenda trybu agenta: **plan** = analiza/projektowanie bez zmian w kodzie, **edit** = implementacja.

---

## Etapy zrealizowane

- **Etap 1 — Decyzje techniczne i architektura** (plan).
- **Etap 2 — Projekt renderowania mapy, tras i miast** (plan).
- **Etap 3 — Usunięcie przykładowego szablonu**.
- **Etap 4 — Model danych mapy i wczytywanie**: `City`/`Route`/`WagonRectangle`/`MapData`,
  `IMapDataProvider` z walidacją `Resources/Raw/mapa.json`.
- **Etap 5 — Serwis stanu rozgrywki (w pamięci)**: zaimplementowany jako
  `IMapInteractionState`/`MapInteractionState` (oznaczone miasta, cykl stanów tras, `WagonColor`, liczniki).
- **Etap 6 — Widok mapy: renderowanie, zoom i przesuwanie**: `MapBoardView` + `MapDrawable` + `MapViewport`.
- **Etap 7 — Stan interakcji i oznaczanie miast/tras**: hit-testing (`MapHitTester`) podpięty do gestów.
- **Etap 8 — Podgląd liczników wagonów**: nakładka nad mapą (wykonane / zaznaczone).
- **Etap 9 — Główne menu i nawigacja trybów**: `MainMenuPage`, trasy Shell (`menu`/`map`/`developer`/`settings`).
- **Etap 10 — Widok ustawień / działań**: `SettingsPage` (reset mapy, wybór koloru wagonów).
- **Etap 11 — Wyszukiwanie miasta**: pole i podpowiedzi na `MapPage` (zoom-to-city + oznaczenie); logika w code-behind.
- **Etap 12 — Tryb deweloperski: szkielet i miasta**: `DeveloperPage`/`DeveloperPageModel`,
  `IDeveloperMapEditor`, `ICityNameCatalog`; mapa jako podkład, dodawanie/edycja/usuwanie miast.
- **Etap 13 — Tryb deweloperski: trasy**: dodawanie tras wagonik po wagoniku, edycja/usuwanie wagoników i tras.
- **Etap 14 — Eksport danych do JSON**: `IMapDataExporter` kopiujący robocze dane do schowka w formacie `mapa.json`.
- **Etap 15 — Konfiguracja platformowa**: wymuszony landscape (Android/iOS), usunięte zbędne uprawnienia sieciowe.
- **Etap 16 — Integracja rzeczywistych danych mapy**: pełny podkład planszy + **61 miast** w `mapa.json`.
  *Trasy pozostają puste — do prześledzenia ręcznie w trybie deweloperskim; pozycje miast to przybliżenie.*

---

## Etap 17 — Testy końcowe i dopracowanie
**Tryb:** edit

**Prompt:**
Przeprowadź weryfikację całej aplikacji względem [docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md):
oznaczanie miast (toggle), pełny cykl stanów tras, liczniki (2.4), gesty i domyślny widok (2.1),
wyszukiwanie miasta (2.7), reset mapy i zmianę koloru (2.5), tryb deweloperski wraz z eksportem
JSON (2.8), brak trwałości stanu po restarcie (3.2) oraz wymuszenie landscape (3.3). Popraw
znalezione błędy, uporządkuj kod i style. Uruchom aplikację na docelowych platformach i potwierdź
zgodność z zakresem (z pominięciem mechanik wykluczonych w 3.5).
