# Plan etapów pracy nad projektem

Aplikacja-towarzysz do gry "Wsiąść do pociągu: Legacy - Legendy zachodu".
Technologia bazowa: **.NET MAUI** (projekt `src/Aplication`), platformy docelowe Android i iOS.

Specyfikacja źródłowa: [docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md).

Stan wyjściowy: projekt zawiera przykładowy szablon MAUI (aplikacja do zarządzania
projektami/zadaniami — `MainPage`, `ProjectListPage`, `TaskDetailPage`, repozytoria
w `Data/`, modele `Project`/`Task`/`Category`/`Tag`, `SeedData.json`). Szablon trzeba
usunąć i zastąpić aplikacją z dokumentacji.

Legenda trybu agenta: **plan** = analiza/projektowanie bez zmian w kodzie, **edit** = implementacja.

---

## Etap 1 — Decyzje techniczne i architektura aplikacji
**Tryb:** plan

**Prompt:**
Na podstawie [docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md) (sekcje 2 i 3)
przeanalizuj projekt `src/Aplication` (.NET MAUI) i zaproponuj docelową architekturę
aplikacji-towarzysza. Ustal: strukturę katalogów po usunięciu szablonu, podział na warstwy
(modele danych mapy, serwis stanu rozgrywki w pamięci, widoki + page-modele wg wzorca MVVM
obecnego w szablonie), listę ekranów (widok mapy, widok ustawień/działań — sekcja 2.5) oraz
sposób nawigacji między nimi. Uwzględnij wymagania niefunkcjonalne: brak trwałości danych
(3.2 — stan tylko w pamięci, czyszczony przy starcie), działanie offline (3.1), wyłącznie
orientacja landscape (3.3), interfejs tylko po polsku (2.6), brak undo/onboardingu (3.4, 3.5).
Nie modyfikuj jeszcze kodu — przygotuj opis architektury i listę elementów szablonu do usunięcia.

---

## Etap 2 — Projekt sposobu renderowania mapy, tras i miast (etap dodatkowy)
**Tryb:** plan

**Prompt:**
Zaprojektuj techniczny sposób wyświetlania grafiki planszy oraz prezentacji tras i miast,
zgodnie z wymaganiami mapy z [docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md)
(sekcja 2.1) oraz interakcji (2.3). Rozstrzygnij i uzasadnij wybór:
- technologii renderowania mapy w .NET MAUI (np. `GraphicsView`/Microsoft.Maui.Graphics
  z rysowaniem wektorowym, SkiaSharp, czy warstwa elementów `AbsoluteLayout` nad obrazem tła) —
  z uwzględnieniem wydajności przy gestach pinch-to-zoom i pan (2.1);
- modelu współrzędnych mapy i mapowania ich na ekran przy zoomie/przesunięciu;
- sposobu reprezentacji miast (klikalne punkty, toggle oznaczenia — 2.3) i tras
  (segmenty/odcinki wagonów jako prostokąty — 2.2, kolor trasy zgodny z grą — 2.1);
- sposobu rysowania i wizualnego rozróżnienia stanów trasy: domyślny / zaznaczona / wykonana
  (2.3) oraz oznaczenia miasta;
- mechanizmu trafień (hit-testing) kliknięć w miasta i trasy przy dowolnym poziomie zoomu;
- formatu danych wejściowych mapy (lista miast ze współrzędnymi, lista tras z liczbą wagonów
  i kolorem) tak, by dało się je później wypełnić rzeczywistymi danymi z gry (2.1, sekcja 4).
Wynikiem jest dokument decyzji projektowych — bez zmian w kodzie.

---

## Etap 3 — Usunięcie przykładowego szablonu
**Tryb:** edit

**Prompt:**
Usuń z projektu `src/Aplication` wszystkie elementy przykładowego szablonu zarządzania
projektami/zadaniami, zachowując szkielet aplikacji MAUI zdolny do uruchomienia. Do usunięcia:
strony i page-modele w `Pages/` i `PageModels/` powiązane z projektami/zadaniami, modele
domenowe szablonu w `Models/` (Project, Task, Category, Tag, ProjectsTags, IconData,
CategoryChartData), repozytoria i serwisy w `Data/` (repozytoria, JsonContext, SeedDataService,
Constants), kontrolki w `Pages/Controls/` specyficzne dla szablonu oraz `Resources/Raw/SeedData.json`.
Wyczyść rejestracje DI w `MauiProgram.cs`, trasy/elementy w `AppShell.xaml(.cs)` oraz nieaktualne
`GlobalUsings.cs`. Zachowaj konfigurację platform, czcionki/style bazowe i ikonę. Po usunięciu
aplikacja musi się kompilować i uruchamiać z pustą stroną startową. Postępuj zgodnie z architekturą
ustaloną w Etapie 1.

---

## Etap 4 — Model danych mapy i dane bazowe (placeholder)
**Tryb:** edit

**Prompt:**
Zaimplementuj model danych mapy zgodnie z formatem ustalonym w Etapie 2 i wymaganiami
z [docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md) (2.1, 2.4): miasta (identyfikator,
nazwa, współrzędne) oraz trasy (miasta końcowe, liczba wagonów jako wartość stała wbudowana w mapę,
kolor trasy). Dodaj sposób ładowania danych mapy (np. zasób osadzony) wraz z **tymczasowymi
danymi placeholder** (kilka miast i tras), ponieważ rzeczywiste dane mapy zostaną dostarczone
przez zleceniodawcę w późniejszym kroku (sekcja 4). Zadbaj, by struktura umożliwiała późniejsze
podmienienie placeholdera na pełne dane bez zmian w kodzie renderującym.

---

## Etap 5 — Serwis stanu rozgrywki (w pamięci)
**Tryb:** edit

**Prompt:**
Zaimplementuj serwis stanu rozgrywki przechowywany wyłącznie w pamięci, zgodnie z
[docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md) (3.2 — brak trwałości, czysty stan
po każdym uruchomieniu). Serwis ma przechowywać: zbiór oznaczonych miast, stan każdej trasy
(domyślny / zaznaczona / wykonana — 2.3) oraz wybrany kolor wagonów gracza (2.2, 2.5 — paleta:
czarny, czerwony, niebieski, zielony, żółty). Udostępnij operacje: toggle oznaczenia miasta,
cykl stanu trasy (zaznaczona → wykonana → reset), reset całej mapy oraz zmianę koloru wagonów.
Dodaj wyliczane liczniki wagonów dla tras wykonanych i zaznaczonych (2.4). Zarejestruj serwis w DI.

---

## Etap 6 — Widok mapy: renderowanie, zoom i przesuwanie
**Tryb:** edit

**Prompt:**
Zaimplementuj główny widok mapy renderujący planszę, miasta i trasy według decyzji z Etapu 2
i danych z Etapu 4. Zrealizuj gesty mapowe wymagane w
[docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md) (2.1): pinch-to-zoom oraz pan,
domyślny widok = cała plansza w pomniejszeniu, bez przycisku resetu/wycentrowania (powrót przez
gest oddalenia). Trasy renderuj z kolorem zgodnym z grą (2.1, 2.2 — wagony jako prostokąty).
Na tym etapie widok jest tylko do wyświetlania (interakcje w Etapie 7).

---

## Etap 7 — Interakcje: oznaczanie miast i tras
**Tryb:** edit

**Prompt:**
Podepnij interakcje użytkownika do widoku mapy zgodnie z
[docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md) (2.3) i serwisem stanu z Etapu 5,
używając mechanizmu hit-testingu z Etapu 2. Zaimplementuj: oznaczanie miast jako toggle;
cykl kliknięć trasy (1. zaznaczona → 2. wykonana → 3. reset); możliwość niezależnego
oznaczania wielu miast i tras jednocześnie. Zapewnij wizualne rozróżnienie stanów trasy
(zaznaczona vs wykonana) oraz oznaczenia miasta, korzystając z wybranego koloru wagonów gracza.
Brak funkcji undo (3.5).

---

## Etap 8 — Podgląd liczników wagonów
**Tryb:** edit

**Prompt:**
Dodaj do widoku mapy podgląd liczników zgodnie z
[docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md) (2.4): liczba wagonów z tras
wykonanych oraz z tras zaznaczonych. Prezentacja w prostej formie liczbowej (np. "12 / 45" —
wykorzystane / limit gracza), bez paska postępu czy dodatkowej grafiki. Liczniki aktualizują się
na bieżąco wraz ze zmianami stanu tras (dane z serwisu stanu z Etapu 5).

---

## Etap 9 — Widok ustawień / działań
**Tryb:** edit

**Prompt:**
Zaimplementuj osobny widok ustawień/działań dostępny przez nawigację (nie z poziomu akcji na
mapie), zgodnie z [docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md) (2.5). Zawartość:
**Reset mapy / "Nowa rozgrywka"** — czyści wszystkie oznaczenia miast i tras oraz przywraca stan
domyślny, bez dodatkowego potwierdzenia; **Zmiana koloru wagonów** — wybór z palety: czarny,
czerwony, niebieski, zielony, żółty. Obie akcje operują na serwisie stanu z Etapu 5 i natychmiast
odzwierciedlają się na mapie.

---

## Etap 10 — Konfiguracja platformowa: landscape, offline, język
**Tryb:** edit

**Prompt:**
Skonfiguruj wymagania niefunkcjonalne z [docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md):
wymuszenie wyłącznie orientacji poziomej (landscape) na Androidzie i iOS (3.3), zapewnienie pełnego
działania offline bez zależności sieciowych (3.1) oraz interfejs wyłącznie w języku polskim (2.6).
Zweryfikuj manifesty/pliki konfiguracyjne platform (`Platforms/Android`, `Platforms/iOS`).
Usuń ewentualne pozostałości po onboardingu/pomocy (3.4).

---

## Etap 11 — Integracja rzeczywistych danych mapy
**Tryb:** edit

**Prompt:**
Po dostarczeniu przez zleceniodawcę rzeczywistych danych mapy (lista miast, lista tras z liczbą
wagonów i kolorem — [docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md), sekcje 2.1 i 4)
zastąp dane placeholder z Etapu 4 pełnym układem planszy i dostrój współrzędne miast/tras względem
grafiki tła. Zweryfikuj poprawność hit-testingu, kolorów tras i liczników na pełnych danych.
*(Etap zależny od dostarczenia danych — realizowany, gdy będą dostępne.)*

---

## Etap 12 — Testy końcowe i dopracowanie
**Tryb:** edit

**Prompt:**
Przeprowadź weryfikację całej aplikacji względem [docs/specyfikacja-aplikacji.md](specyfikacja-aplikacji.md):
scenariusze oznaczania miast (toggle), pełny cykl stanów tras, poprawność liczników (2.4),
działanie gestów i domyślnego widoku (2.1), reset mapy i zmianę koloru (2.5), zachowanie braku
trwałości stanu po restarcie (3.2) oraz wymuszenie landscape (3.3). Popraw znalezione błędy,
uporządkuj kod i style. Uruchom aplikację na docelowych platformach i potwierdź zgodność z zakresem
(z pominięciem mechanik wykluczonych w 3.5).
