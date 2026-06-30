# Decyzje techniczne i architektura aplikacji

Dokument projektowy dla aplikacji-towarzysza do gry **"Wsiąść do pociągu: Legacy — Legendy zachodu"**.
Bazuje na [specyfikacji](specyfikacja-aplikacji.md) (sekcje 2 i 3) oraz na analizie istniejącego projektu
.NET MAUI w `src/Aplication`.

> Status: **opis architektury** — niniejszy etap nie wprowadza zmian w kodzie. Zawiera projekt docelowej
> struktury oraz listę elementów szablonu do usunięcia, które zostaną zrealizowane w kolejnym etapie.

---

## 1. Punkt wyjścia — analiza obecnego projektu

Projekt `src/Aplication` to **szablon „Project Manager"** (.NET MAUI + CommunityToolkit). Z perspektywy
aplikacji-towarzysza jest to zestaw przykładowy, którego logika domenowa (projekty, zadania, kategorie, tagi)
jest **w całości do usunięcia**. Wartościowe i do zachowania są natomiast wzorce techniczne szablonu:

| Element szablonu | Decyzja | Uzasadnienie |
|---|---|---|
| **Wzorzec MVVM** (`ObservableObject` + `[ObservableProperty]` + `[RelayCommand]` z `CommunityToolkit.Mvvm`) | **Zachować** | Spójny, nowoczesny wzorzec; podział na `Pages` (widoki XAML) i `PageModels` (logika) jest dokładnie tym, czego potrzebujemy. |
| **DI w `MauiProgram.cs`** (`builder.Services.AddSingleton/AddTransient`) | **Zachować, przebudować rejestracje** | Serwis stanu i page-modele rejestrujemy jako singletony. |
| **Nawigacja Shell** (`AppShell.xaml`) | **Zachować, uprościć** | Z `Flyout` z 3 zakładkami → minimalna nawigacja 2 ekranów. |
| **`CommunityToolkit.Maui`** | **Zachować** | Przydatne behawiory/konwertery; toast nieobowiązkowy. |
| **Warstwa danych** (`Data/`: repozytoria, `JsonContext`, `SeedDataService`, SQLite) | **Usunąć** | Wymaganie 3.2 — brak trwałości; stan tylko w pamięci. |
| **Modele domenowe** (`Models/`: `Project`, `Task`, `Category`, `Tag`, …) | **Usunąć** | Domena „project managera" jest nieadekwatna. |
| **`Syncfusion.Maui.Toolkit`** (wykres kategorii) | **Usunąć** | Brak wykresów w naszej aplikacji; mniej zależności. |
| **`Microsoft.Data.Sqlite` + `SQLitePCLRaw`** | **Usunąć** | Brak bazy danych. |
| **Kontrolki przykładowe** (`Pages/Controls/`: `CategoryChart`, `TaskView`, `ProjectCardView`, …) | **Usunąć** | Zastąpione własnymi kontrolkami mapy. |

### Stosowany wzorzec MVVM (do naśladowania)

Przykład z `MainPageModel`: page-model dziedziczy po `ObservableObject`, właściwości oznaczone
`[ObservableProperty]`, akcje jako `[RelayCommand]`, cykl życia obsługiwany komendami
`Appearing`/`NavigatedTo`/`NavigatedFrom`. Ten sam szkielet zastosujemy w nowych page-modelach.

---

## 2. Decyzje techniczne

| Obszar | Decyzja |
|---|---|
| **Technologia** | .NET MAUI (zgodnie z istniejącym projektem; spełnia wymóg multiplatformowości Android/iOS — spec. 1, 3.6). |
| **Wzorzec architektoniczny** | MVVM (CommunityToolkit.Mvvm), warstwowy: Models → Services → PageModels → Pages. |
| **Stan rozgrywki** | Pojedynczy serwis trzymany w pamięci (`GameStateService`), rejestrowany jako **singleton** w DI. Brak persystencji (3.2). |
| **Trwałość** | **Brak.** Żadnego SQLite, plików, `Preferences` dla stanu gry. Stan inicjowany przy każdym starcie aplikacji „od zera" (3.2). |
| **Dane mapy** | Statyczna definicja (miasta + trasy z liczbą wagonów i kolorem) wczytywana z zasobu w pamięci. Konkretne dane dostarczy zleceniodawca później (spec. 2.1, 4) — na tym etapie tylko model + provider. |
| **Offline** | Naturalnie spełnione — brak jakiejkolwiek komunikacji sieciowej (3.1). |
| **Orientacja** | Wymuszony **landscape** na poziomie platformy (Android `MainActivity`, iOS `Info.plist`) — patrz §6 (3.3). |
| **Język** | Teksty UI wyłącznie po polsku, „na sztywno" w XAML/kodzie — bez plików `.resx`/lokalizacji (2.6). |
| **Undo / onboarding / pomoc** | Brak — nieobecne w architekturze świadomie (3.4, 3.5). |

---

## 3. Podział na warstwy

```
┌──────────────────────────────────────────────────────────┐
│  Pages (XAML + code-behind)  — widoki, gesty, rendering   │
│  ── MapPage, SettingsPage                                 │
├──────────────────────────────────────────────────────────┤
│  PageModels (MVVM)  — stan widoku, komendy                │
│  ── MapPageModel, SettingsPageModel                       │
├──────────────────────────────────────────────────────────┤
│  Services  — logika rozgrywki w pamięci                   │
│  ── GameStateService (singleton), IMapDataProvider        │
├──────────────────────────────────────────────────────────┤
│  Models  — model danych mapy + stan oznaczeń              │
│  ── City, Route, MapData, RouteColor, RouteState,         │
│     CityMarkState, WagonColor                             │
└──────────────────────────────────────────────────────────┘
```

### 3.1 Warstwa modeli (`Models/`)

Czysta definicja danych, bez logiki UI:

- **`MapData`** — kontener: `IReadOnlyList<City>`, `IReadOnlyList<Route>`. Reprezentuje jeden, stały układ planszy (2.1).
- **`City`** — `Id`, `Name`, pozycja na mapie (`X`, `Y` w przestrzeni mapy).
- **`Route`** — `Id`, `CityFromId`, `CityToId`, `WagonCount` (stała liczba wagonów — 2.4/2.1), `Color` (`RouteColor`), geometria do narysowania.
- **`RouteColor`** (enum) — kolory tras z oryginalnej gry, w tym `Gray` (neutralna) (2.1).
- **Stan oznaczeń** (osobno od danych bazowych mapy, bo resetowalny):
  - **`RouteState`** (enum) — `None` → `Selected` (zaznaczona/planowana) → `Done` (wykonana) → `None`; cykl 3-klikowy (2.3).
  - **`CityMarkState`** — oznaczenie miasta jako toggle bool (2.3).
- **`WagonColor`** (enum) — paleta gracza: `Czarny, Czerwony, Niebieski, Zielony, Żółty` (2.5).

> Stan oznaczeń (`RouteState`/`CityMarkState`) trzymany jest w `GameStateService`, a **nie** w obiektach
> `Route`/`City`, aby dane bazowe mapy pozostały niemutowalne i by „reset" sprowadzał się do wyczyszczenia
> słowników stanu.

### 3.2 Warstwa serwisów (`Services/`)

- **`IMapDataProvider` / `MapDataProvider`** — dostarcza statyczną `MapData` (miasta + trasy). Na tym etapie
  może zwracać dane zaślepkowe; docelowo wczyta dane z fizycznej gry (spec. 4). Rejestrowany jako singleton.
- **`GameStateService`** (singleton, **serce aplikacji**) — trzyma **w pamięci** cały zmienny stan rozgrywki:
  - słownik stanów tras (`RouteId → RouteState`),
  - zbiór oznaczonych miast (`HashSet<CityId>`),
  - aktualny `WagonColor` gracza,
  - wyliczane liczniki: suma wagonów z tras **wykonanych** i z tras **zaznaczonych** (2.4).
  - Metody: `ToggleCity(id)`, `CycleRoute(id)` (None→Selected→Done→None), `SetWagonColor(color)`,
    `ResetGame()` (czyści wszystkie oznaczenia — „Nowa rozgrywka", 2.5).
  - Eksponuje zdarzenia/`INotifyPropertyChanged`, by page-modele reagowały na zmiany.
  - **Konstruktor nie ładuje żadnego zapisanego stanu** — gwarancja czystego startu (3.2).

### 3.3 Warstwa page-modeli (`PageModels/`)

- **`MapPageModel`** — projekcja stanu z `GameStateService` na widok mapy; komendy `ToggleCityCommand`,
  `CycleRouteCommand`; właściwości liczników (np. `"12 / 45"` — 2.4); komenda nawigacji do ustawień.
- **`SettingsPageModel`** — komenda `ResetGameCommand` (bez potwierdzenia — 2.5), wybór koloru wagonów
  (`SelectWagonColorCommand`, lista predefiniowanej palety).

### 3.4 Warstwa widoków (`Pages/`)

- **`MapPage`** — wyświetla planszę z obsługą gestów **pinch-to-zoom** i **pan** (2.1); domyślnie cała plansza
  „z lotu ptaka" (2.1); klikalne miasta i trasy; nakładka z licznikami; przycisk/ikona przejścia do ustawień.
  Rendering mapy: kontrolka rysująca (`GraphicsView`/`SKCanvasView`) lub warstwa `AbsoluteLayout` z elementami
  miast i tras — wybór do doprecyzowania na etapie UI.
- **`SettingsPage`** — przyciski „Nowa rozgrywka" i wybór koloru wagonów z palety (2.5).

---

## 4. Lista ekranów

| Ekran | Page / PageModel | Zawartość | Źródło wymagań |
|---|---|---|---|
| **Widok mapy** | `MapPage` / `MapPageModel` | Plansza (miasta + trasy), gesty zoom/pan, oznaczanie miast (toggle) i tras (cykl 3-klikowy), liczniki wagonów (wykonane/zaznaczone), wejście do ustawień. | 2.1, 2.3, 2.4 |
| **Widok ustawień / działań** | `SettingsPage` / `SettingsPageModel` | „Nowa rozgrywka" (reset bez potwierdzenia), zmiana koloru wagonów z palety. | 2.5 |

Dwa ekrany — zgodnie z 3.4 brak ekranu pomocy/onboardingu.

---

## 5. Nawigacja

- **Mechanizm:** .NET MAUI Shell (zachowany z szablonu), ale **uproszczony**.
- Rezygnujemy z `Shell.FlyoutBehavior="Flyout"` i 3 zakładek szablonu.
- **Model nawigacji:** `MapPage` jest ekranem głównym (root). Przejście do `SettingsPage` odbywa się przez
  **jawną akcję nawigacyjną** — ikonę/przycisk „menu/ustawienia" w widoku mapy (`Shell.Current.GoToAsync("settings")`),
  zgodnie z 2.5 (akcje destrukcyjne nie są dostępne bezpośrednio z mapy). Powrót — standardowym „wstecz".
- Rejestracja trasy `settings` w `MauiProgram.cs`; `MapPage` jako `ShellContent` startowy.
- Brak `FlyoutFooter` z przełącznikiem motywu (element szablonu do usunięcia).

```
MapPage (root, "//map")
   │  GoToAsync("settings")
   ▼
SettingsPage ("settings")  ──(wstecz)──►  MapPage
```

---

## 6. Realizacja wymagań niefunkcjonalnych

| Wymaganie | Realizacja w architekturze |
|---|---|
| **3.1 Offline** | Brak warstwy sieciowej; wszystkie dane (mapa) wbudowane / w pamięci. |
| **3.2 Brak trwałości** | Usunięcie SQLite i całej warstwy `Data/`; `GameStateService` jako stan ulotny w pamięci; brak `Preferences` dla stanu gry; czysta mapa przy każdym starcie. |
| **3.3 Tylko landscape** | Wymuszenie na platformach: Android — `[Activity(ScreenOrientation = ScreenOrientation.Landscape)]` w `MainActivity`; iOS — `UISupportedInterfaceOrientations` w `Info.plist` ograniczone do `LandscapeLeft`/`LandscapeRight`. |
| **2.6 Język polski** | Teksty UI po polsku, bez lokalizacji/`.resx`. Nazewnictwo enumów koloru zgodne z polską paletą. |
| **3.4 Brak pomocy/onboardingu** | Brak ekranu pomocy w liście ekranów i w nawigacji. |
| **3.5 Brak undo** | `GameStateService` nie utrzymuje historii akcji; korekta wyłącznie przez ponowne kliknięcia w cyklu. |

---

## 7. Docelowa struktura katalogów (po sprzątnięciu szablonu)

```
src/Aplication/
├── App.xaml / App.xaml.cs              (zachowane, oczyszczone)
├── AppShell.xaml / .cs                 (uproszczone: 2 ekrany, bez flyout/motywu)
├── MauiProgram.cs                      (przebudowane rejestracje DI)
├── GlobalUsings.cs
├── Models/
│   ├── MapData.cs
│   ├── City.cs
│   ├── Route.cs
│   ├── RouteColor.cs
│   ├── RouteState.cs
│   └── WagonColor.cs
├── Services/
│   ├── IMapDataProvider.cs
│   ├── MapDataProvider.cs
│   └── GameStateService.cs
├── PageModels/
│   ├── MapPageModel.cs
│   └── SettingsPageModel.cs
├── Pages/
│   ├── MapPage.xaml / .cs
│   ├── SettingsPage.xaml / .cs
│   └── Controls/                       (własne kontrolki mapy — wg etapu UI)
├── Resources/
│   ├── Styles/  (Colors.xaml, Styles.xaml — oczyszczone)
│   ├── Fonts/   (tylko realnie używane)
│   ├── AppIcon/, Splash/
│   └── Raw/     (ewentualne dane/grafika mapy)
└── Platforms/   (Android, iOS, MacCatalyst, Windows — z wymuszeniem landscape)
```

---

## 8. Lista elementów szablonu do usunięcia

> Do realizacji w **kolejnym** etapie (ten dokument niczego nie usuwa).

### Pliki / katalogi
- `Models/` — `Category.cs`, `CategoryChartData.cs`, `IconData.cs`, `Project.cs`, `ProjectTask.cs`, `ProjectsTags.cs`, `Tag.cs` *(wszystkie)*.
- `PageModels/` — `IProjectTaskPageModel.cs`, `MainPageModel.cs`, `ManageMetaPageModel.cs`, `ProjectDetailPageModel.cs`, `ProjectListPageModel.cs`, `TaskDetailPageModel.cs` *(wszystkie)*.
- `Pages/` — `MainPage.*`, `ManageMetaPage.*`, `ProjectDetailPage.*`, `ProjectListPage.*`, `TaskDetailPage.*` *(wszystkie)*.
- `Pages/Controls/` — `AddButton.*`, `CategoryChart.*`, `ChartDataLabelConverter.cs`, `ChipDataTemplateSelector.cs`, `LegendExt.cs`, `ProjectCardView.*`, `TagView.*`, `TaskView.*` *(wszystkie przykładowe)*.
- `Data/` — `CategoryRepository.cs`, `Constants.cs`, `JsonContext.cs`, `ProjectRepository.cs`, `SeedDataService.cs`, `TagRepository.cs`, `TaskRespository.cs` *(cały katalog)*.
- `Utilities/` — `ProjectExtensions.cs`, `TaskUtilities.cs` *(przegląd; usunąć jeśli zależne od domeny szablonu)*.
- `Resources/Raw/` — przykładowe zasoby seed (np. dane JSON szablonu), jeśli obecne.

### `MauiProgram.cs`
- Usunąć rejestracje: `ProjectRepository`, `TaskRepository`, `CategoryRepository`, `TagRepository`, `SeedDataService`, `MainPageModel`, `ProjectListPageModel`, `ManageMetaPageModel`.
- Usunąć trasy `AddTransientWithShellRoute<ProjectDetailPage,…>("project")` i `…("task")`.
- Usunąć `ConfigureSyncfusionToolkit()` i handler-mapping dla `CategoryChart`.
- Dodać: `GameStateService`, `IMapDataProvider`, `MapPageModel`, `SettingsPageModel`, trasę `settings`.

### `AppShell.xaml`
- Usunąć 3 `ShellContent` (Dashboard/Projects/Manage Meta) i `Shell.FlyoutFooter` z `SfSegmentedControl` (przełącznik motywu).
- Ustawić pojedynczy startowy `ShellContent` dla `MapPage`; usunąć `xmlns:sf` Syncfusion.

### `Aplication.csproj` (pakiety NuGet)
- Usunąć: `Syncfusion.Maui.Toolkit`, `Microsoft.Data.Sqlite.Core`, `SQLitePCLRaw.bundle_green`.
- Zachować: `Microsoft.Maui.Controls`, `CommunityToolkit.Mvvm`, `CommunityToolkit.Maui`, `Microsoft.Extensions.Logging.Debug`.
- Rozważyć zmianę `ApplicationTitle`/`ApplicationId`/`RootNamespace` na docelowe (np. `TicketToRide.Companion`).

### Zasoby
- `Resources/Styles/` — przegląd `Colors.xaml`/`Styles.xaml`, usunięcie kolorów/stylów specyficznych dla szablonu (np. styl wykresu).
- `Resources/Fonts/` — usunięcie nieużywanych fontów (np. `FluentSystemIcons`, `SegoeUI-Semibold`, jeśli zbędne).
- Ikony menu szablonu (`IconDashboard`, `IconProjects`, `IconMeta`, `IconLight`, `IconDark`) — usunąć, jeśli niewykorzystane.
