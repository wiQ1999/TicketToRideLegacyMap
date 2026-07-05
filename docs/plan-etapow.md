# Plan etapów pracy nad projektem

Aplikacja-towarzysz do gry "Wsiąść do pociągu: Legacy - Legendy zachodu".
Technologia bazowa: **.NET MAUI** (projekt `src/Aplication`), platformy docelowe Android i iOS.

Źródła: [docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md),
[docs/architektura.md](architektura.md), [docs/renderowanie-mapy.md](renderowanie-mapy.md).

Legenda trybu agenta: **plan** = analiza/projektowanie bez zmian w kodzie, **edit** = implementacja.

---

## Etap 1 — Decyzje techniczne i architektura aplikacji
**Tryb:** plan · **Zrealizowano.**

## Etap 2 — Projekt sposobu renderowania mapy, tras i miast
**Tryb:** plan · **Zrealizowano.**

## Etap 3 — Usunięcie przykładowego szablonu
**Tryb:** edit · **Zrealizowano.**

---

## Etap 4 — Model danych mapy i wczytywanie
**Tryb:** edit

**Prompt:**
Zaimplementuj model danych mapy zgodnie z [docs/architektura.md](architektura.md) (3.1) i
[docs/renderowanie-mapy.md](renderowanie-mapy.md) (§6-7): `City` (Id, `Name`, współrzędne),
`Route` (miasta końcowe, łamana punktów, `WagonCount` z jej długości), `MapData` (rozmiar planszy +
listy). Dodaj `IMapDataProvider`/`MapDataProvider` wczytujący i walidujący `Resources/Raw/mapa.json`
(unikalność identyfikatorów, istnienie miast końcowych, zakres współrzędnych). Uwzględnij pole
`Name` miasta, potrzebne później do wyszukiwania (2.7) i trybu deweloperskiego (2.8).

---

## Etap 5 — Serwis stanu rozgrywki (w pamięci)
**Tryb:** edit

**Prompt:**
Zaimplementuj `GameStateService` (singleton, wyłącznie w pamięci — 3.2) zgodnie z
[docs/architektura.md](architektura.md) (3.2): oznaczone miasta (toggle — 2.3), stan każdej trasy
(cykl `None → Selected → Done → None` — 2.3), wybrany `WagonColor` (2.2, 2.5 — paleta: czarny,
czerwony, niebieski, zielony, żółty), wyliczane liczniki wagonów tras wykonanych/zaznaczonych (2.4).
Operacje: `ToggleCity`, `CycleRoute`, `SetWagonColor`, `ResetGame`. Konstruktor nie ładuje żadnego
zapisanego stanu. Zarejestruj w DI.

---

## Etap 6 — Widok mapy: renderowanie, zoom i przesuwanie
**Tryb:** edit

**Prompt:**
Zaimplementuj kontrolkę mapy (`MapBoardView` + `MapDrawable` + `MapViewport`) renderującą planszę,
miasta i trasy zgodnie z [docs/renderowanie-mapy.md](renderowanie-mapy.md) (§1-4): jeden `IDrawable`
w `GraphicsView`, przestrzeń mapy niezależna od ekranu, fit-to-screen jako widok domyślny, gesty
**pinch-to-zoom** i **pan** (2.1, bez przycisku resetu — powrót przez pinch-out). Renderer ma być
bezstanowy — stan wizualny (2.3) odpytywany z serwisu w Etapie 7. Na tym etapie widok jest tylko do
wyświetlania.

---

## Etap 7 — Serwis stanu interakcji i oznaczanie miast/tras
**Tryb:** edit

**Prompt:**
Zaimplementuj `IMapInteractionState`/`MapInteractionState` (oznaczenia miast — toggle, stany tras —
cykl 3-klikowy) oraz podepnij hit-testing (`MapHitTester`, [renderowanie-mapy.md](renderowanie-mapy.md)
§5) do gestów `MapBoardView`, zgodnie z [docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md)
(2.3): kliknięcie miasta = toggle; kliknięcie trasy = krok w cyklu zaznaczona → wykonana → reset;
niezależne oznaczanie wielu miast i tras. Zapewnij wizualne rozróżnienie stanów trasy (obrys vs
wypełnienie, patrz renderowanie-mapy.md §4) z użyciem koloru wagonów gracza. Brak funkcji undo (3.5).

---

## Etap 8 — Podgląd liczników wagonów
**Tryb:** edit

**Prompt:**
Dodaj nakładkę z licznikami nad widokiem mapy zgodnie z
[docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md) (2.4): liczba wagonów z tras wykonanych
oraz zaznaczonych, w prostej formie liczbowej (np. "12 / 45"), bez paska postępu. Liczniki
aktualizują się na bieżąco na podstawie zdarzeń `GameStateService` (Etap 5).

---

## Etap 9 — Główne menu i nawigacja trybów
**Tryb:** edit

**Prompt:**
Wprowadź `MainMenuPage`/`MainMenuPageModel` jako ekran startowy (root) zgodnie z
[docs/architektura.md](architektura.md) (§5): wybór trybu **mapa gry** / **tryb deweloperski**
(spec. 2.8). Zaktualizuj `AppShell` o trasy `menu` (root), `map`, `developer`, `settings` (ten
ostatni dostępny wyłącznie z widoku mapy). Przenieś dotychczasową zawartość ekranu startowego do
`MapPage`.

---

## Etap 10 — Widok ustawień / działań
**Tryb:** edit

**Prompt:**
Zaimplementuj `SettingsPage`/`SettingsPageModel`, dostępny wyłącznie przez nawigację z widoku mapy
(nie z akcji na mapie), zgodnie z [docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md) (2.5):
**Reset mapy** ("Nowa rozgrywka") — czyści wszystkie oznaczenia, bez potwierdzenia; **Zmiana koloru
wagonów** — wybór z palety (czarny, czerwony, niebieski, zielony, żółty). Obie akcje operują na
`GameStateService` (Etap 5) i natychmiast odzwierciedlają się na mapie.

---

## Etap 11 — Wyszukiwanie miasta
**Tryb:** edit

**Prompt:**
Dodaj do widoku mapy pole wyszukiwania miasta zgodnie z
[docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md) (2.7): podpowiedzi najbardziej pasujących
nazw podczas wpisywania (źródło nazw — `ICityNameCatalog`/dane z `MapData`, patrz
[docs/architektura.md](architektura.md) §3.2); po wyborze miasta z listy — przybliżenie i wycentrowanie
widoku mapy na tym mieście (rozszerzenie `MapViewport` o programowy zoom-to-point) oraz oznaczenie
miasta w `GameStateService`, jeśli nie było wcześniej oznaczone (bez odznaczania, gdy już oznaczone).
Funkcja niedostępna w trybie deweloperskim.

---

## Etap 12 — Tryb deweloperski: szkielet i dodawanie miast
**Tryb:** edit

**Prompt:**
Zaimplementuj `DeveloperPage`/`DeveloperPageModel` oraz `IDeveloperMapEditor` zgodnie z
[docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md) (2.8) i
[docs/architektura.md](architektura.md) (§3.2-3.4): przy wejściu w tryb wczytaj istniejące dane mapy
(`IMapDataProvider`) do roboczych list miast i tras. Widok mapy w tym trybie służy wyłącznie jako
podkład (bez oznaczania miast/tras ze standardowego trybu). Zaimplementuj dodawanie miasta:
wskazanie dokładnego położenia na mapie + formularz z nazwą wybieraną z ustalonej, stałej listy
nazw (`ICityNameCatalog`, z podpowiedziami) oraz edycję i usuwanie pozycji z listy miast.

---

## Etap 13 — Tryb deweloperski: dodawanie i edycja tras
**Tryb:** edit

**Prompt:**
Rozszerz tryb deweloperski (Etap 12) o zarządzanie trasami zgodnie z
[docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md) (2.8): dodanie trasy przez wskazanie na
mapie kolejnych punktów wyznaczających jej przebieg pomiędzy dwoma wybranymi miastami z roboczej
listy oraz uzupełnienie pozostałych danych (m.in. liczby wagonów, wynikającej z liczby odcinków —
[docs/renderowanie-mapy.md](renderowanie-mapy.md) §3). Dodaj edycję i usuwanie pozycji z roboczej
listy tras.

---

## Etap 14 — Tryb deweloperski: eksport danych do JSON
**Tryb:** edit

**Prompt:**
Zaimplementuj `IMapDataExporter` i podepnij go do `DeveloperPageModel` zgodnie z
[docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md) (2.8): serializacja roboczych list miast
i tras z `IDeveloperMapEditor` do formatu zgodnego ze schematem `mapa.json`
([docs/renderowanie-mapy.md](renderowanie-mapy.md) §6) oraz skopiowanie wyniku do schowka
systemowego (`Clipboard`, MAUI Essentials) po akcji dewelopera.

---

## Etap 15 — Konfiguracja platformowa: landscape, offline, język
**Tryb:** edit

**Prompt:**
Skonfiguruj wymagania niefunkcjonalne z [docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md):
wymuszenie wyłącznie orientacji poziomej (landscape) na Androidzie i iOS (3.3), pełne działanie
offline bez zależności sieciowych (3.1) oraz interfejs wyłącznie w języku polskim (2.6). Zweryfikuj
manifesty/pliki konfiguracyjne platform (`Platforms/Android`, `Platforms/iOS`). Usuń ewentualne
pozostałości po onboardingu/pomocy (3.4).

---

## Etap 16 — Integracja rzeczywistych danych mapy
**Tryb:** edit

**Prompt:**
Po dostarczeniu przez zleceniodawcę rzeczywistych danych mapy (lista miast z nazwami, lista tras z
liczbą wagonów — [docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md), sekcje 2.1 i 4) zastąp
dane placeholder z Etapu 4 pełnym układem planszy i dostrój współrzędne miast/tras względem grafiki
tła. Uzupełnij `ICityNameCatalog` o pełną, docelową listę nazw miast. Zweryfikuj poprawność
hit-testingu, wyszukiwania i liczników na pełnych danych.
*(Etap zależny od dostarczenia danych — realizowany, gdy będą dostępne.)*

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
