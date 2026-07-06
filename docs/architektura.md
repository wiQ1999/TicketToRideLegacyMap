# Decyzje techniczne i architektura aplikacji

Dokument projektowy dla aplikacji-towarzysza do gry **"Wsiąść do pociągu: Legacy — Legendy zachodu"**.
Bazuje na [specyfikacji](specyfikacja-aplikacji.md) (sekcje 2 i 3) oraz na
[renderowaniu mapy](renderowanie-mapy.md). Warstwy i ekrany są zaimplementowane; dokument opisuje
architekturę docelową i miejscami używa nazw innych niż kod — serwis stanu rozgrywki to
`IMapInteractionState`/`MapInteractionState` (dalej: „`GameStateService`"), a `MapPage` nie ma
osobnego `MapPageModel` (logika w code-behind). Weryfikuj założenia względem `src/Aplication`.

---

## 1. Punkt wyjścia

Projekt `src/Aplication` powstał z szablonu „Project Manager" (.NET MAUI + CommunityToolkit).
Szablon zarządzania projektami/zadaniami został usunięty; zachowano wzorzec **MVVM**
(`CommunityToolkit.Mvvm`: `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`), DI w
`MauiProgram.cs` oraz nawigację Shell — jako wzorce do zastosowania w kolejnych page-modelach.

---

## 2. Decyzje techniczne

| Obszar | Decyzja |
|---|---|
| **Technologia** | .NET MAUI (Android/iOS — spec. 1, 3.6). |
| **Wzorzec architektoniczny** | MVVM (CommunityToolkit.Mvvm), warstwowy: Models → Services → PageModels → Pages. |
| **Stan rozgrywki** | `GameStateService` w pamięci, singleton w DI. Brak persystencji (3.2). |
| **Dane robocze trybu deweloperskiego** | Osobny stan w pamięci (nie mieszany z `GameStateService`); brak zapisu trwałego — jedynym wynikiem pracy jest eksport JSON do schowka (2.8). |
| **Dane mapy** | Statyczna definicja (miasta + trasy) wczytywana z zasobu `Resources/Raw/mapa.json` (2.1, 4). |
| **Offline** | Naturalnie spełnione — brak komunikacji sieciowej (3.1). |
| **Orientacja** | Wymuszony **landscape** na poziomie platformy (3.3). |
| **Język** | Teksty UI wyłącznie po polsku, „na sztywno" w kodzie/XAML — bez `.resx` (2.6). |
| **Undo / onboarding / pomoc** | Brak — nieobecne w architekturze świadomie (3.4, 3.5). |

---

## 3. Podział na warstwy

```
┌───────────────────────────────────────────────────────────────┐
│  Pages — widoki, gesty, rendering                              │
│  ── MainMenuPage, MapPage, SettingsPage, DeveloperPage         │
├───────────────────────────────────────────────────────────────┤
│  PageModels (MVVM) — stan widoku, komendy                      │
│  ── MainMenuPageModel, MapPageModel, SettingsPageModel,        │
│     DeveloperPageModel                                         │
├───────────────────────────────────────────────────────────────┤
│  Services — logika rozgrywki i edycji danych w pamięci         │
│  ── GameStateService, IMapDataProvider, ICityNameCatalog,      │
│     IDeveloperMapEditor, IMapDataExporter                      │
├───────────────────────────────────────────────────────────────┤
│  Models — model danych mapy + stan oznaczeń                    │
│  ── City, Route, MapData, RouteState,                          │
│     CityMarkState, WagonColor                                  │
└───────────────────────────────────────────────────────────────┘
```

### 3.1 Warstwa modeli (`Models/`)

- **`MapData`** — kontener: `IReadOnlyList<City>`, `IReadOnlyList<Route>`, `CanvasSize`. Jeden, stały układ planszy (2.1).
- **`City`** — `Id`, `Name` (nazwa wyświetlana, **nie renderowana na mapie** — wyszukiwanie 2.7, tryb
  deweloperski 2.8), pozycja `X`/`Y`.
- **`Route`** — `Id`, `CityFromId`, `CityToId`, lista wagoników (`WagonRectangle`: dwa punkty przekątnej
  każdy — patrz [renderowanie-mapy.md](renderowanie-mapy.md) §3, §7); `WagonCount` = liczba wagoników,
  wprost z danych. Ten sam model służy trybowi standardowemu i trybowi deweloperskiemu.
- **Stan oznaczeń** (osobno od danych bazowych, bo resetowalny): `RouteState` (`None → Selected → Done`,
  cykl 3-klikowy, 2.3), `CityMarkState` (toggle, 2.3).
- **`WagonColor`** (enum) — paleta gracza: `Czarny, Czerwony, Niebieski, Zielony, Żółty` (2.5).

> Stan oznaczeń trzymany jest w `GameStateService`, **nie** w `Route`/`City`, aby dane bazowe mapy
> pozostały niemutowalne. Robocze listy trybu deweloperskiego (2.8) to osobne, mutowalne kopie
> `City`/`Route` — nie wpływają na `MapData` używane w standardowym trybie mapy.

### 3.2 Warstwa serwisów (`Services/`)

- **`IMapDataProvider`** — dostarcza statyczną `MapData` z `mapa.json` (singleton).
- **`ICityNameCatalog`** — stała, wbudowana lista nazw miast z fizycznej gry; źródło podpowiedzi
  zarówno dla wyszukiwania (2.7), jak i dodawania miast w trybie deweloperskim (2.8).
- **`GameStateService`** (singleton) — w pamięci: oznaczone miasta, stany tras, `WagonColor`,
  wyliczane liczniki wagonów (2.4). Metody: `ToggleCity`, `CycleRoute`, `SetWagonColor`,
  `StartNewPlan` (nowa rozgrywka z menu: czyści oznaczenia + ustawia kolor + zaznacza aktywny plan),
  `ResetGame`; właściwość `HasActivePlan` (czy w sesji rozpoczęto rozgrywkę — steruje opcją
  „Kontynuuj" w menu). `WagonColor` jest losowany przy starcie serwisu. Konstruktor nie ładuje
  zapisanego stanu (3.2).
- **`IDeveloperMapEditor`** — robocze listy miast/tras trybu deweloperskiego; przy starcie
  inicjalizowane danymi z `IMapDataProvider`. Operacje: dodaj/edytuj/usuń miasto, dodaj/edytuj/usuń
  trasę (2.8).
- **`IMapDataExporter`** — serializuje robocze listy `IDeveloperMapEditor` do formatu zgodnego z
  `mapa.json` i kopiuje wynik do schowka (`Clipboard`, MAUI Essentials) (2.8).

### 3.3 Warstwa page-modeli (`PageModels/`)

- **`MainMenuPageModel`** — wybór koloru wagonów (kafelki `ColorChoice`), rozpoczęcie nowej
  rozgrywki (`NewPlanCommand` → `StartNewPlan`, aktywne dopiero po wyborze koloru) lub kontynuacja
  trwającej (`ContinueCommand`, widoczne przy `HasActivePlan`), przejście do trybu deweloperskiego.
  Domyślnie zaznaczony jest bieżący `WagonColor`.
- **`MapPageModel`** — projekcja `GameStateService`; `ToggleCityCommand`, `CycleRouteCommand`;
  liczniki (2.4); pole i podpowiedzi wyszukiwania miasta z `ICityNameCatalog`, komenda
  `SelectSearchResultCommand` (zoom-to-city + oznaczenie, 2.7); nawigacja do ustawień.
- **`SettingsPageModel`** — `ResetGameCommand` (bez potwierdzenia — 2.5), wybór koloru wagonów.
- **`DeveloperPageModel`** — robocze listy miast/tras z `IDeveloperMapEditor`; komendy dodawania
  (wskazanie punktu na mapie + formularz danych), edycji, usuwania oraz `ExportToClipboardCommand`.

### 3.4 Warstwa widoków (`Pages/`)

- **`MainMenuPage`** — menu główne (landscape): branding, wybór koloru wagonów, „Nowy plan"
  (nowa rozgrywka) / „Kontynuuj" (gdy trwa rozgrywka), dyskretne wejście w tryb deweloperski.
  Odświeża stan w `OnAppearing` (po powrocie z mapy pojawia się „Kontynuuj").
- **`MapPage`** — plansza z gestami **pinch-to-zoom** i **pan** (2.1), pole wyszukiwania miasta
  z listą podpowiedzi (2.7), nakładka z licznikami, przejście do ustawień. Rendering: `GraphicsView`
  jak w [renderowanie-mapy.md](renderowanie-mapy.md).
- **`SettingsPage`** — „Nowa rozgrywka" i wybór koloru wagonów (2.5).
- **`DeveloperPage`** — ta sama kontrolka mapy jako podkład do wskazywania punktów (bez oznaczania
  miast/tras ze standardowego trybu), listy robocze miast/tras z formularzami dodania/edycji,
  przycisk kopiowania danych do schowka (2.8).

---

## 4. Lista ekranów

| Ekran | Page / PageModel | Zawartość | Źródło wymagań |
|---|---|---|---|
| **Główne menu** | `MainMenuPage` / `MainMenuPageModel` | Wybór koloru wagonów, „Nowy plan" (nowa rozgrywka) / „Kontynuuj" (gdy trwa) → mapa, tryb deweloperski. | 2.2, 2.5, 2.8 |
| **Widok mapy** | `MapPage` / `MapPageModel` | Plansza, gesty zoom/pan, oznaczanie miast/tras, liczniki, wyszukiwanie miasta, wejście do ustawień. | 2.1, 2.3, 2.4, 2.7 |
| **Widok ustawień / działań** | `SettingsPage` / `SettingsPageModel` | „Nowa rozgrywka", zmiana koloru wagonów. | 2.5 |
| **Tryb deweloperski** | `DeveloperPage` / `DeveloperPageModel` | Robocze listy miast/tras, dodawanie przez wskazanie na mapie + formularz, edycja/usuwanie, eksport JSON. | 2.8 |

Brak ekranu pomocy/onboardingu (3.4).

---

## 5. Nawigacja

- **Mechanizm:** .NET MAUI Shell, `Shell.FlyoutBehavior="Disabled"`.
- **Model nawigacji:** `MainMenuPage` jest ekranem głównym (root): wybór koloru i rozpoczęcie/
  kontynuacja rozgrywki (→ `MapPage`) albo wejście w tryb deweloperski. `SettingsPage` dostępny
  wyłącznie z `MapPage` przez jawną akcję nawigacyjną (2.5 — akcje destrukcyjne nie są dostępne
  bezpośrednio z mapy).

```
MainMenuPage (root, "//menu")
   │  „Nowy plan" / „Kontynuuj"    │  „Tryb deweloperski"
   │  GoToAsync("map")             │  GoToAsync("developer")
   ▼                               ▼
MapPage ("map")                DeveloperPage ("developer")
   │  GoToAsync("settings")
   ▼
SettingsPage ("settings")  ──(wstecz)──►  MapPage
```

---

## 6. Realizacja wymagań niefunkcjonalnych

| Wymaganie | Realizacja w architekturze |
|---|---|
| **3.1 Offline** | Brak warstwy sieciowej; wszystkie dane (mapa, katalog nazw miast) wbudowane / w pamięci. |
| **3.2 Brak trwałości** | `GameStateService` i `IDeveloperMapEditor` jako stan ulotny w pamięci; eksport danych trybu deweloperskiego jest jedyną formą „zapisu" (ręczna, przez schowek). |
| **3.3 Tylko landscape** | Wymuszenie na platformach: Android — `MainActivity`; iOS — `Info.plist`. |
| **2.6 Język polski** | Teksty UI po polsku, bez lokalizacji/`.resx`. |
| **3.4 Brak pomocy/onboardingu** | Brak ekranu pomocy w liście ekranów i w nawigacji. |
| **3.5 Brak undo** | Brak historii akcji w `GameStateService`; w trybie deweloperskim korekta przez edycję/usunięcie pozycji z listy. |

---

## 7. Struktura katalogów

```
src/Aplication/
├── App.xaml / .cs, AppShell.xaml / .cs, MauiProgram.cs, GlobalUsings.cs
├── Models/       MapData, City, Route, WagonRectangle, MapPoint, RouteState, WagonColor
├── Services/     IMapDataProvider/MapDataProvider, MapDataException,
│                 IMapInteractionState/MapInteractionState (stan rozgrywki, dalej „GameStateService"),
│                 ICityNameCatalog/CityNameCatalog, IDeveloperMapEditor/DeveloperMapEditor,
│                 IMapDataExporter/MapDataExporter, IErrorHandler/ModalErrorHandler
├── PageModels/   MainMenuPageModel (+ ColorChoice), SettingsPageModel, DeveloperPageModel  (MapPage → code-behind)
├── Pages/        MainMenuPage, MapPage, SettingsPage, DeveloperPage  (.xaml / .cs)
├── Controls/     MapBoardView  (host GraphicsView + gesty)
├── Rendering/    MapDrawable, MapViewport, MapHitTester, MapMetrics, RouteColorPalette
├── Resources/    Styles (design system: Colors, Typography, Styles, AppStyles), Fonts (Cinzel/
│                 Cinzel Decorative/Bitter + Licenses), AppIcon, Splash; Raw/ (mapa.json + podkład)
└── Platforms/    Android, iOS, MacCatalyst, Windows  (wymuszenie landscape)
```

Katalog nazw miast (`ICityNameCatalog`) jest wpisany na stałe w kodzie (`CityNameCatalog.cs`), nie w `Raw/`.
