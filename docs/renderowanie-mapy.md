# Renderowanie mapy, tras i miast

Warstwa graficzna aplikacji-towarzysza „Wsiąść do pociągu: Legacy — Legendy zachodu".
Uzupełnia [architekturę](architektura.md) o sposób rysowania planszy; wymagania mapy i interakcji —
[specyfikacja](specyfikacja-aplikacji.md) (2.1, 2.3, 2.4). Konkretne wartości wyglądu (kolory, grubości,
wzory) żyją w kodzie renderera i nie są tu utrwalane.

---

## 1. Technologia: `GraphicsView` + `Microsoft.Maui.Graphics`

Cała plansza to **jeden `IDrawable` (`MapDrawable`)** rysowany w pojedynczej kontrolce **`GraphicsView`**,
opakowanej we własną kontrolkę `MapBoardView` (podpina gesty, nasłuchuje zmian stanu, woła `Invalidate()`).
Bez `SkiaSharp` i bez `AbsoluteLayout` z setkami `View`. Powody:

- **Wydajność gestów.** Pinch/pan zmieniają tylko parametry transformacji i wołają `Invalidate()` — jeden
  `Draw` przerysowuje płaską scenę, bez layoutu setek `View`.
- **Ostrość przy zoomie.** Rysunek wektorowy skaluje się bez utraty jakości.
- **Zero zależności zewnętrznych.** `Microsoft.Maui.Graphics` jest częścią MAUI; działa offline (3.1).
- **Spójny hit-testing.** Ten sam `MapViewport` liczy rysowanie i trafienia (§5) — co widać, to jest klikalne.

---

## 2. Model współrzędnych — `MapViewport`

Geometria miast i tras jest zdefiniowana w **przestrzeni mapy** (logicznej, `0..MapWidth × 0..MapHeight`,
proporcje poziome — landscape), niezależnej od ekranu i zoomu. `MapViewport` trzyma `Scale`, `OffsetX`,
`OffsetY` i mapuje przestrzeń mapy na piksele ekranu (oraz odwrotnie, dla hit-testingu):

```
screenX = mapX * Scale + OffsetX          mapX = (screenX - OffsetX) / Scale
screenY = mapY * Scale + OffsetY          mapY = (screenY - OffsetY) / Scale
```

**Widok „z lotu ptaka" (fit-to-screen)** — liczony na starcie i przy każdej zmianie rozmiaru kontrolki:

```
fitScale = min(viewW / MapWidth, viewH / MapHeight)
OffsetX  = (viewW - MapWidth  * fitScale) / 2
OffsetY  = (viewH - MapHeight * fitScale) / 2
Scale    = fitScale
```

**Gesty** modyfikują `MapViewport` lub stan, po czym wołają `Invalidate()` (jeden przebieg `Draw`):

- **Zoom:** `Scale` ograniczony do `[fitScale, fitScale * MaxZoom]` — dolny limit to widok całej planszy
  (brak przycisku resetu, 2.1), górny to sensowny detal.
- **Pinch:** skalowanie wokół punktu między palcami (punkt mapy pod gestem pozostaje nieruchomy).
- **Pan:** przesuwa `Offset` z clampem, tak by plansza nie zniknęła całkowicie z kadru.

---

## 3. Warstwy i geometria elementów

`Draw` rysuje w kolejności: **1) tło → 2) trasy → 3) miasta**. Tło to opcjonalna **bitmapa podkładu**
(`canvas.DrawImage` z tą samą transformacją) albo jednolity kolor, gdy podkładu brak. Liczniki i legenda (2.4)
są poza kanwą — jako elementy XAML w nakładce nad `GraphicsView`, niezależne od zoomu.

- **Miasto** — punkt `(X, Y)` w przestrzeni mapy. Nazwa miasta **nie jest rysowana na mapie** — służy
  wyłącznie wyszukiwaniu (2.7) i trybowi deweloperskiemu (2.8), poza kanwą.
- **Trasa** — uporządkowana lista **wagoników**; każdy wagonik to niezależny, osiowo zorientowany
  prostokąt zdefiniowany dwoma punktami (przekątna) w przestrzeni mapy. `WagonCount` to liczba
  wagoników wprost z danych — nie jest wyliczana z geometrii. Renderer buduje kształt każdego wagonika
  z osobna; ta sama geometria (prostokąty) służy do hit-testingu (§5) — jedno źródło kształtu.

---

## 4. Renderowanie zależne od stanu

Renderer jest **bezstanowy**: przy każdym `Draw` odpytuje **serwis stanu interakcji**
(`IMapInteractionState`) o stan trasy i oznaczenie miasta. Zgodnie z 2.3 stany rozróżniamy osobnymi
**kanałami wizualnymi**, stosowanymi do **każdego wagonika trasy z osobna**:

| Element / stan | Kanał renderowania |
|---|---|
| Trasa `None` | niewidoczna (podkład prześwituje) |
| Trasa `Selected` | **obrys** każdego wagonika, wnętrze przezroczyste |
| Trasa `Done` | **wypełnienie** każdego wagonika |
| Miasto nieoznaczone | niewidoczne |
| Miasto oznaczone (toggle) | widoczny punkt |

`Selected` (obrys) i `Done` (wypełnienie) używają **różnych kanałów**, więc pozostają rozróżnialne także przy
zaburzeniach widzenia barw (2.3). Trasa przechodzi cykl `None → Selected → Done → None`. Zmiana stanu w
serwisie → zdarzenie → `Invalidate()` → przerysowanie; dane bazowe mapy są niemutowalne, a „reset" czyści
wyłącznie stan w serwisie.

---

## 5. Hit-testing

Punkt dotyku (piksele ekranu) przeliczamy odwrotną transformacją (`MapViewport`) do przestrzeni mapy
i tam testujemy — geometria jest stała i niezależna od zoomu:

- **Miasto:** trafienie, gdy punkt mieści się w stałym promieniu od pozycji miasta — bez dodatkowego marginesu tolerancji; klikalny obszar odpowiada dokładnie temu, co narysowane (§4).
- **Trasa:** trafienie, gdy punkt mieści się w prostokącie (bbox) **dowolnego wagonika** trasy —
  bez dodatkowego marginesu tolerancji; klikalny obszar odpowiada dokładnie temu, co narysowane (§4).

**Miasta testujemy przed trasami** (leżą na końcach tras), a przy kilku kandydatach wygrywa **najbliższy**.
Przy ~40 miastach i ~100 trasach wystarcza liniowy przegląd — bez struktur przestrzennych.

---

## 6. Dane wejściowe

`mapa.json` w `Resources/Raw/` (`MauiAsset`) wczytywany przez `MapDataProvider` i deserializowany do modeli
(`City`/`Route`/`MapData`); współrzędne w przestrzeni mapy, `canvasSize` definiuje jej zakres. Plik jest
zarazem formatem eksportu z trybu deweloperskiego (2.8) — dla trasy zapisuje pełną listę wagoników (oba
punkty przekątnej każdego), bez redukcji, żeby ponowne wczytanie do edycji nie traciło precyzji. Przy
ładowaniu walidowane są m.in. unikalność identyfikatorów, istnienie miast końcowych tras i zakres
współrzędnych — błąd danych to wyjątek przy starcie (dane wbudowane, więc to błąd buildu). Nachodzenie się
lub niestykanie sąsiednich wagoników **nie jest walidowane** — geometrię dobiera deweloper wizualnie względem
podkładu. Aktualny schemat pól definiuje `MapDataProvider`.

---

## 7. Modele i komponenty renderera

- **`City`** — `Id`, `Name`, `X`, `Y` (przestrzeń mapy).
- **`Route`** — `Id`, `CityFromId`, `CityToId`, lista wagoników (`WagonRectangle`: dwa punkty przekątnej
  każdy); `WagonCount` = liczba wagoników. Ten sam model służy trybowi standardowemu i deweloperskiemu.
- **`MapData`** — `CanvasSize` + listy miast i tras.
- **`MapViewport`** — transformacja mapa↔ekran, fit-to-screen, zoom/pan.
- **`MapDrawable : IDrawable`** — rysuje warstwy, odpytuje serwis stanu.
- **`MapHitTester`** — trafienia w przestrzeni mapy.
- **`MapBoardView`** — kontrolka hostująca `GraphicsView`, gesty i spinająca powyższe komponenty.
