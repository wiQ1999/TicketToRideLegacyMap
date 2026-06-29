# Etap 2 — Projekt sposobu renderowania mapy, tras i miast

Dokument decyzji projektowych dla warstwy graficznej aplikacji-towarzysza do gry
**„Wsiąść do pociągu: Legacy — Legendy zachodu"**. Rozwija [architekturę z Etapu 1](architektura.md)
o szczegóły renderowania planszy. Bazuje na [specyfikacji](specyfikacja-aplikacji.md) — głównie sekcje
**2.1** (mapa, gesty), **2.2** (wagony jako prostokąty), **2.3** (interakcje: miasta toggle, cykl tras),
**2.4** (liczniki) oraz **4** (dane dostarczone później).

> Status: **dokument projektowy (tryb plan)** — nie wprowadza zmian w kodzie. Ustala technologię renderowania,
> model współrzędnych, reprezentację miast i tras, style stanów, hit-testing oraz format danych wejściowych.
> Implementacja: Etapy 4, 6, 7.

---

## 1. Podsumowanie decyzji (TL;DR)

| Zagadnienie | Decyzja |
|---|---|
| **Technologia renderowania** | **Jedna kontrolka `GraphicsView` (Microsoft.Maui.Graphics)** rysująca całą planszę wektorowo (miasta + trasy) w jednym `IDrawable`. Bez `SkiaSharp`, bez `AbsoluteLayout` z setkami elementów. |
| **Model współrzędnych** | **Współrzędne mapy** (logiczne, niezależne od ekranu) → transformacja afiniczna (`scale`, `translate`) → współrzędne ekranu. Jedna macierz widoku (`MapViewport`) wspólna dla rysowania i hit-testingu. |
| **Gesty** | `PinchGestureRecognizer` (zoom) + `PanGestureRecognizer` (przesuwanie) + `TapGestureRecognizer` (klik). Modyfikują `MapViewport`, który wywołuje `Invalidate()` na `GraphicsView`. |
| **Miasta** | Klikalne punkty (okrąg) rysowane wektorowo; oznaczenie jako **toggle** (obrys/pierścień). |
| **Trasy** | Łańcuch **prostokątów-wagonów** (po jednym na wagon) wzdłuż odcinka między miastami; kolor zgodny z grą. |
| **Stany trasy** | `None` / `Selected` / `Done` rozróżnione wizualnie (wypełnienie vs obrys vs kolor gracza). |
| **Hit-testing** | Geometryczny w **przestrzeni mapy** (punkt kliknięcia transformowany odwrotnie przez `MapViewport`), z progiem trafienia skalowanym tak, by cel pozostał klikalny przy każdym zoomie. |
| **Format danych** | `mapa.json` jako **osadzony zasób** (`Resources/Raw/`) — listy `cities` i `routes`; deserializacja do modeli z Etapu 1. Współrzędne w przestrzeni mapy. |

---

## 2. Wybór technologii renderowania

### 2.1 Rozważane warianty

| Wariant | Opis | Wady w naszym kontekście |
|---|---|---|
| **A. `AbsoluteLayout` z elementami** | Każde miasto i każdy wagon to osobny `View` (np. `BoxView`/`Image`) pozycjonowany absolutnie nad obrazem tła. Zoom/pan przez `Scale`/`TranslationX/Y` kontenera. | Plansza „Wsiąść do pociągu" ma ~30–40 miast i ~80–100 tras, a każda trasa to kilka wagonów → **setki–tysiące `View`**. Drzewo wizualne tej wielkości jest kosztowne w mierzeniu/układaniu i **zacina się przy gestach**. Transformacja całego kontenera psuje też ostrość i utrudnia hit-testing przy zoomie. |
| **B. `SkiaSharp` (`SKCanvasView`)** | Rysowanie wektorowe na kanwie Skia, pełna kontrola, wysoka wydajność. | Dodatkowa zależność zewnętrzna i własny model API. **Nadmiarowy** dla statycznej planszy o tej skali — `Microsoft.Maui.Graphics` daje tu wystarczającą wydajność przy mniejszym bagażu. |
| **C. `GraphicsView` + `Microsoft.Maui.Graphics`** ✅ | Jedna kontrolka z `IDrawable`; cała plansza rysowana imperatywnie w `Draw(ICanvas, RectF)`. Wbudowane w MAUI, bez zależności zewnętrznych. | Trzeba samodzielnie zaimplementować transformacje i hit-testing — ale to i tak jest potrzebne dla precyzyjnej obsługi gestów. |

### 2.2 Decyzja: wariant C — `GraphicsView`

**Rysujemy całą planszę jako jeden wektorowy obraz** w pojedynczym `IDrawable` (`MapDrawable`), zamiast budować
drzewo wielu kontrolek. Uzasadnienie:

- **Wydajność gestów (2.1).** Pinch-to-zoom i pan modyfikują tylko parametry transformacji i wywołują
  `Invalidate()` — jeden `Draw` przerysowuje płaską scenę. Brak kosztu layoutu setek `View`. To kluczowe
  dla płynności wymaganej w 2.1.
- **Ostrość przy zoomie.** Rysunek **wektorowy** (okręgi, prostokąty, linie) skaluje się bez utraty jakości —
  przy przybliżeniu do szczegółów (2.1) miasta i wagony pozostają ostre, w przeciwieństwie do skalowanej bitmapy.
- **Zero dodatkowych zależności.** `Microsoft.Maui.Graphics` jest częścią MAUI; nie wprowadzamy `SkiaSharp`
  (zgodnie z duchem Etapu 1 — minimalizacja pakietów). Wspiera w pełni offline (3.1).
- **Spójny hit-testing.** Skoro sami liczymy transformację, ten sam `MapViewport` służy do rysowania
  i do trafień — brak rozjazdu między tym, co widać, a tym, co klikalne (§6).

**Tło planszy.** Specyfikacja (2.1, 4) mówi o grafice planszy dostarczonej później. Architektura zakłada
**rysowanie wektorowe miast i tras na podstawie danych**, a nie odwzorowanie skanu planszy. Dlatego:

- Domyślnie tło rysujemy programowo (jednolity kolor/subtelna tekstura) w tym samym `MapDrawable`.
- Jeśli zleceniodawca dostarczy **bitmapę tła**, rysujemy ją jako pierwszą warstwę w `Draw` przez
  `canvas.DrawImage(...)` z tą samą transformacją `MapViewport` — miasta/trasy nakładają się na nią idealnie,
  bo dzielą układ współrzędnych. To zostawia furtkę bez zmiany architektury.

> **Warstwy renderowania (kolejność w `Draw`):** 1) tło (kolor lub bitmapa) → 2) trasy (wagony) → 3) miasta →
> 4) oznaczenia stanów (obrysy/pierścienie). Liczniki (2.4) **nie** są częścią kanwy — to zwykłe `Label` w
> nakładce XAML nad `GraphicsView`, niezależne od zoomu.

---

## 3. Model współrzędnych i mapowanie na ekran

### 3.1 Dwie przestrzenie

- **Przestrzeń mapy (logiczna).** Stały układ, w którym zdefiniowane są pozycje miast i tras — niezależny od
  rozmiaru ekranu i poziomu zoomu. Proponowany zakres: prostokąt `0..MapWidth × 0..MapHeight` (np. `0..2000 × 0..1200`,
  proporcje szerokiej, poziomej planszy — zgodnie z landscape, 3.3). Dane wejściowe (§7) podają współrzędne **właśnie w tej przestrzeni**.
- **Przestrzeń ekranu (urządzenia).** Piksele niezależne od urządzenia (DIP) wewnątrz `GraphicsView`. To, co faktycznie widać.

### 3.2 Transformacja (`MapViewport`)

Pojedynczy obiekt stanu widoku trzyma:

```
Scale  : float    // współczynnik powiększenia (1 = bazowy "z lotu ptaka")
OffsetX: float    // przesunięcie w pikselach ekranu
OffsetY: float
```

Mapowanie **mapa → ekran**:

```
screenX = mapX * Scale + OffsetX
screenY = mapY * Scale + OffsetY
```

Mapowanie odwrotne **ekran → mapa** (dla hit-testingu, §6):

```
mapX = (screenX - OffsetX) / Scale
mapY = (screenY - OffsetY) / Scale
```

W `Draw` realizujemy to przez `canvas.Translate(OffsetX, OffsetY)` + `canvas.Scale(Scale, Scale)`, po czym rysujemy
wszystko **we współrzędnych mapy** — kanwa sama przelicza. Hit-testing liczymy ręcznie wzorem odwrotnym (kanwa nie
udostępnia odwrotnej transformacji punktu dotyku).

### 3.3 Widok domyślny „z lotu ptaka" (2.1)

Po starcie i przy każdej zmianie rozmiaru kontrolki obliczamy `Scale`/`Offset` metodą **fit-to-screen**:

```
fitScale = min(viewW / MapWidth, viewH / MapHeight)   // cała plansza mieści się w kadrze
OffsetX  = (viewW - MapWidth  * fitScale) / 2          // wyśrodkowanie poziome
OffsetY  = (viewH - MapHeight * fitScale) / 2          // wyśrodkowanie pionowe
Scale    = fitScale
```

To realizuje wymóg „cała plansza w pomniejszeniu" jako stan startowy (2.1). **Brak przycisku resetu/wycentrowania**
(2.1) — powrót do widoku ogólnego następuje wyłącznie gestem pinch-out użytkownika.

### 3.4 Ograniczenia gestów

- **Zoom:** `Scale` ograniczony do `[fitScale, fitScale * MaxZoom]` (np. `MaxZoom = 6`). Dolny limit = widok
  „z lotu ptaka" (nie da się oddalić poniżej całej planszy), górny = sensowny detal.
- **Pinch:** skalowanie **wokół punktu między palcami** — punkt mapy pod gestem pozostaje pod palcami
  (przeliczamy `Offset` tak, by `screenPivot` był nieruchomy).
- **Pan:** aktualizuje `Offset`; **clamp** tak, by plansza nie „uciekła" całkowicie poza kadr (zostaje co najmniej
  jej fragment widoczny).

Każda zmiana `MapViewport` → `graphicsView.Invalidate()` → jeden przebieg `Draw`.

---

## 4. Reprezentacja miast i tras

### 4.1 Miasta (2.3)

- **Geometria:** punkt `(X, Y)` w przestrzeni mapy; rysowany jako **wypełniony okrąg** o stałym promieniu
  w przestrzeni mapy (np. `CityRadius = 14`), opcjonalnie z obrysem i etykietą `Name`.
- **Klikalność:** każdy okrąg jest celem trafienia (§6). Oznaczenie to **toggle** (2.3) — patrz stany §5.2.
- **Etykieta nazwy:** rysowana `canvas.DrawString` obok punktu; może być ukrywana przy małym `Scale` (widok ogólny),
  by uniknąć bałaganu, i pokazywana po przybliżeniu.

### 4.2 Trasy (2.1, 2.2)

Trasa łączy dwa miasta i ma **stałą liczbę wagonów** (`WagonCount`) oraz **kolor** (`RouteColor`).

- **Geometria odcinka:** prosty odcinek od środka miasta `From` do środka miasta `To` (z marginesem, by nie wchodzić
  pod okręgi miast). Trasy szerokie/łukowate z oryginału można odwzorować przez **opcjonalne punkty pośrednie**
  (`Waypoints`, §7) — łamana zamiast prostej; renderer traktuje ją jako sekwencję segmentów.
- **Wagony jako prostokąty (2.2):** odcinek dzielony na `WagonCount` równych pól; w każdym rysujemy **prostokąt**
  (zaokrąglony, `DrawRoundedRectangle`/`FillRoundedRectangle`) **obrócony zgodnie z kierunkiem odcinka**, z małą
  przerwą między wagonami. To wprost realizuje „odcinki wagonów jako prostokąty".
- **Kolor (2.1):** wypełnienie wagonów = kolor trasy z gry. Trasy **szare/neutralne** (`Gray`) renderujemy kolorem
  neutralnym. Kolor trasy to **cecha mapy bazowej**, niezależna od stanu zaznaczenia/wykonania (2.1) — patrz §5.
- **Mapowanie `RouteColor` → kolor RGB:** centralna tablica w rendererze (np. `RouteColorPalette`), by kolory były
  spójne i łatwe do dostrojenia na etapie UI.

> **Rozróżnienie dwóch „kolorów".** `RouteColor` (kolor wymaganych kart, cecha mapy — 2.1) jest czym innym niż
> `WagonColor` gracza (paleta: czarny/czerwony/niebieski/zielony/żółty — 2.2/2.5). Kolor gracza wchodzi do gry
> dopiero przy wizualizacji trasy **wykonanej** (§5.1).

---

## 5. Rysowanie i rozróżnienie stanów

### 5.1 Stany trasy (2.3)

Cykl 3-klikowy `None → Selected → Done → None` (`RouteState` z Etapu 1). Propozycja stylów (ostateczny styl —
etap UI, 2.3/sekcja 4 spec.):

| Stan | Wygląd wagonów | Cel wizualny |
|---|---|---|
| **`None` (domyślny)** | Prostokąty w **kolorze trasy** (`RouteColor`), normalna nieprzezroczystość lub lekko przygaszone. | Mapa bazowa — pokazuje, jakich kart wymaga trasa. |
| **`Selected` (zaznaczona/planowana)** | Kolor trasy + **wyraźny obrys/podświetlenie** całego pasma (np. jasny kontur, pogrubienie, poświata). | „Planuję tę trasę" — wyróżniona, ale nieprzejęta. |
| **`Done` (wykonana)** | Wagony **wypełnione kolorem gracza** (`WagonColor`), pełna nieprzezroczystość, ewentualnie obrys. | „Zbudowane przeze mnie" — jednoznacznie inny kolor niż bazowy i niż zaznaczenie. |

Zaznaczona i wykonana **muszą być wzajemnie rozróżnialne** (2.3): zaznaczona = zmiana **obrysu/poświaty** przy
zachowaniu koloru trasy; wykonana = zmiana **wypełnienia** na kolor gracza. Dwa różne kanały wizualne (kontur vs
wypełnienie) gwarantują czytelny kontrast nawet dla osób z zaburzeniami widzenia barw.

### 5.2 Oznaczenie miasta (2.3)

Toggle bool (`CityMarkState`):

| Stan | Wygląd |
|---|---|
| **Nieoznaczone** | Standardowy okrąg miasta. |
| **Oznaczone** | Dodatkowy **pierścień/obwódka** wokół miasta (np. kontrastowy kolor) lub wypełnienie znacznikiem. Wyłącznie wizualne, bez logiki gry (2.3). |

### 5.3 Źródło stanu

Renderer **nie trzyma** stanu — przy każdym `Draw` odpytuje `GameStateService` (Etap 1) o `RouteState` dla
`Route.Id` i o oznaczenie dla `City.Id`. Reset mapy / zmiana koloru gracza (2.5) zmieniają stan w serwisie →
`Invalidate()` → przerysowanie. Dane bazowe mapy pozostają niemutowalne (zgodnie z §3.1 architektury).

---

## 6. Hit-testing (trafienia kliknięć)

Wymóg: klikalność miast i tras przy **dowolnym poziomie zoomu** (2.3).

### 6.1 Zasada

`TapGestureRecognizer` zwraca punkt dotyku w **pikselach ekranu**. Przeliczamy go odwrotną transformacją
(`MapViewport`, §3.2) do **przestrzeni mapy** i całe testowanie robimy tam, gdzie geometria jest stała i niezależna
od zoomu:

```
tapMap = ScreenToMap(tapScreen)   // (ekran - Offset) / Scale
```

### 6.2 Test miasta

Trafienie, gdy `distance(tapMap, city.Position) <= hitRadius`, gdzie
`hitRadius = max(CityRadius, MinTouchTarget / Scale)`.

Dzielenie progu dotyku przez `Scale` sprawia, że **fizyczny rozmiar celu na ekranie pozostaje stały** (np. ~44 px)
niezależnie od zoomu — przy oddaleniu cel mapowy jest większy, więc mały na ekranie punkt nadal da się trafić palcem.

### 6.3 Test trasy

Dla każdego segmentu trasy: odległość punktu `tapMap` od **odcinka** (`From`–`To` lub kolejnych `Waypoints`).
Trafienie, gdy `distanceToSegment <= routeHitWidth`, gdzie `routeHitWidth = max(WagonHalfWidth, MinTouchTarget / Scale)`.
Próg analogicznie skalowany do zoomu.

### 6.4 Priorytety i kolejność

- **Miasta przed trasami:** miasta leżą na końcach tras; kliknięcie blisko miasta interpretujemy jako miasto.
  Najpierw testujemy miasta, potem trasy.
- **Najbliższy cel:** przy kilku kandydatach wybieramy ten o najmniejszej odległości (a nie pierwszy z listy).
- **Wydajność:** przy ~40 miastach i ~100 trasach liniowy przegląd na każde tapnięcie jest w pełni wystarczający —
  bez potrzeby struktur przestrzennych (quadtree itp.).

> **Spójność z renderem.** Hit-testing i rysowanie używają **tej samej** geometrii w przestrzeni mapy i **tego samego**
> `MapViewport`. Nie ma osobnych „klikalnych prostokątów" do utrzymania — co widać, to jest klikalne.

---

## 7. Format danych wejściowych mapy

### 7.1 Założenia

- Dane dostarczy zleceniodawca później (2.1, sekcja 4) — format ma być **edytowalny ręcznie** i wypełnialny bez
  zmian w kodzie renderującym (Etap 11).
- Plik **`mapa.json`** w `Resources/Raw/` jako **osadzony zasób** (`MauiAsset`), wczytywany przez `MapDataProvider`
  (Etap 1) i deserializowany do modeli `City`/`Route`/`MapData`. Brak sieci → offline (3.1).
- Współrzędne podawane w **przestrzeni mapy** (§3.1); `canvasSize` definiuje jej zakres.

### 7.2 Proponowany schemat

```json
{
  "canvasSize": { "width": 2000, "height": 1200 },

  "cities": [
    { "id": "DEN", "name": "Denver",      "x": 980,  "y": 640 },
    { "id": "SLC", "name": "Salt Lake City", "x": 560, "y": 520 }
  ],

  "routes": [
    {
      "id": "DEN-SLC",
      "from": "DEN",
      "to": "SLC",
      "wagons": 5,
      "color": "Yellow",
      "waypoints": [ { "x": 760, "y": 600 } ]
    },
    {
      "id": "DEN-SLC-2",
      "from": "DEN",
      "to": "SLC",
      "wagons": 5,
      "color": "Red"
    }
  ]
}
```

### 7.3 Znaczenie pól

| Pole | Typ | Opis |
|---|---|---|
| `canvasSize.width/height` | int | Zakres przestrzeni mapy (§3.1). Wszystkie `x`,`y` mieszczą się w tym prostokącie. |
| `cities[].id` | string | Unikatowy identyfikator miasta (klucz dla tras i stanu oznaczeń). |
| `cities[].name` | string | Nazwa wyświetlana (po polsku — 2.6). |
| `cities[].x`,`y` | number | Pozycja środka miasta w przestrzeni mapy. |
| `routes[].id` | string | Unikatowy identyfikator trasy (klucz stanu `RouteState`). |
| `routes[].from`,`to` | string | `id` miast końcowych. |
| `routes[].wagons` | int | **Stała** liczba wagonów = długość trasy (2.4) i liczba prostokątów (2.2). |
| `routes[].color` | enum string | `RouteColor`: kolor wymaganych kart; `Gray` = trasa neutralna (2.1). |
| `routes[].waypoints` | tablica `{x,y}` *(opc.)* | Punkty pośrednie dla tras łukowatych/równoległych — łamana zamiast prostej. Pominięte = prosty odcinek. |

### 7.4 Obsługa tras równoległych (podwójnych)

Część połączeń w grze ma **dwie równoległe trasy** między tymi samymi miastami. Rozwiązanie: dwa osobne wpisy
`routes` o różnych `id` (np. `DEN-SLC`, `DEN-SLC-2`). Aby się nie nakładały, jednej nadajemy `waypoints` z lekkim
odsunięciem **albo** renderer rozsuwa równoległe trasy o stały offset prostopadły (decyzja na etapie implementacji —
preferowane `waypoints` w danych, bo trzyma wygląd w danych, nie w kodzie).

### 7.5 Walidacja przy ładowaniu

`MapDataProvider` przy wczytaniu sprawdza: unikalność `id` miast i tras, istnienie miast `from`/`to`,
`wagons >= 1`, poprawność `color` (parsowalny do `RouteColor`), zakres współrzędnych w `canvasSize`. Błąd danych =
wyjątek przy starcie (dane są wbudowane, więc to błąd buildu, nie runtime'u użytkownika).

---

## 8. Powiązanie z modelami z Etapu 1

Dokument nie zmienia modeli z [architektury](architektura.md), a jedynie je doprecyzowuje:

- **`City`** — `Id`, `Name`, `X`, `Y` (przestrzeń mapy). ✔ zgodne.
- **`Route`** — `Id`, `CityFromId`, `CityToId`, `WagonCount`, `Color (RouteColor)`; **doprecyzowanie:** opcjonalne
  `Waypoints` (lista punktów) dla geometrii łamanej (§7.2/7.4).
- **`RouteColor`** (enum) — wartości zgodne z kartami gry + `Gray`. Renderer mapuje je na RGB przez `RouteColorPalette`.
- **`MapData`** — dochodzi `CanvasSize` (zakres przestrzeni mapy) dla fit-to-screen (§3.3).
- **Nowe elementy renderera (Etap 6):** `MapViewport` (stan transformacji), `MapDrawable : IDrawable`
  (rysowanie warstw), `MapHitTester` (trafienia, §6) — wszystkie operują na danych z `MapDataProvider`
  i stanie z `GameStateService`, bez własnej persystencji.

---

## 9. Realizacja wymagań — mapowanie na specyfikację

| Wymaganie | Realizacja |
|---|---|
| **2.1** cała plansza, jeden stan, zoom/pan, domyślnie „z lotu ptaka", bez resetu widoku | `GraphicsView` + `MapViewport`; fit-to-screen na starcie (§3.3); brak przycisku centrowania, dolny limit zoomu = widok ogólny (§3.4). |
| **2.1** kolor trasy zgodny z grą, niezależny od stanu | `RouteColor` z danych → `RouteColorPalette`; stan zmienia obrys/wypełnienie, nie kolor bazowy (§5.1). |
| **2.2** wagony jako prostokąty | Odcinek dzielony na `WagonCount` obróconych prostokątów (§4.2). |
| **2.3** miasta toggle | Hit-test miasta → `ToggleCity` w serwisie; pierścień oznaczenia (§5.2, §6.2). |
| **2.3** cykl tras, wiele niezależnie, rozróżnienie zaznaczona/wykonana | Hit-test trasy → `CycleRoute`; style dwoma kanałami (obrys vs wypełnienie kolorem gracza) (§5.1, §6.3). |
| **2.4** liczniki | Nakładka `Label` nad kanwą, dane z `GameStateService` — poza warstwą rysowania (§2.2 nota). |
| **dowolny zoom — klikalność** | Hit-testing w przestrzeni mapy z progiem `MinTouchTarget / Scale` (§6). |
| **sekcja 4** dane dostarczone później | `mapa.json` jako osadzony zasób, schemat edytowalny ręcznie, walidacja przy ładowaniu (§7). |
| **3.1 offline / 3.3 landscape** | Brak sieci; przestrzeń mapy w proporcjach poziomych, fit-to-screen pod landscape. |

---

## 10. Następne kroki (poza Etapem 2)

1. **Etap 4** — model `mapa.json` + dane placeholder zgodne ze schematem §7.
2. **Etap 6** — `MapViewport`, `MapDrawable`, gesty (pinch/pan), fit-to-screen.
3. **Etap 7** — `MapHitTester`, podpięcie `Tap` → `ToggleCity`/`CycleRoute`, style stanów.
4. **Etap 11** — podmiana danych placeholder na pełną planszę, dostrojenie współrzędnych i (opcjonalnie) bitmapy tła.
