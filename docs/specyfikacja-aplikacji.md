# Specyfikacja biznesowa aplikacji mobilnej

## 1. Nazwa i kontekst

**Nazwa robocza:** Aplikacja-towarzysz do gry "Wsiąść do pociągu: Legacy - Legendy zachodu" (Ticket to Ride Legacy: Legends of the West)

**Opis ogólny:**
Aplikacja mobilna służąca jako cyfrowy pomocnik podczas rozgrywki w planszową grę "Wsiąść do pociągu: Legacy - Legendy zachodu". Wyświetla interaktywną planszę gry z miastami i połączeniami (trasami pociągów) między nimi.

**Platformy docelowe:** Android, iOS

## 2. Zakres funkcjonalny

### 2.1 Mapa gry
- Aplikacja wyświetla całą planszę gry zawierającą:
  - miasta,
  - trasy (połączenia) pociągów między miastami.
- Mapa przedstawia **jeden, stały stan planszy** - widok ze wszystkimi częściami mapy widocznymi jednocześnie (mimo że gra "Legendy zachodu" jest grą typu Legacy, w której plansza zmienia się trwale między partiami, aplikacja nie odwzorowuje tych etapów/wersji - prezentuje jeden bazowy układ mapy).
- Mapa wspiera standardowe gesty mapowe:
  - **pinch-to-zoom** (oddalanie/przybliżanie),
  - **przesuwanie** (pan) widoku mapy.
- Uzupełnieniem gestów są dwa przyciski nakładki mapy: **przybliżanie** (lupka z „+") i **oddalanie** (lupka z „−"), skalujące widok wokół środka kadru.
- W trybie planowania mapa wypełnia **cały ekran urządzenia** (bez paska nawigacji); elementy sterujące są nakładką nad planszą.
- **Domyślny widok** po otwarciu mapy to **cała plansza w pomniejszeniu** ("z lotu ptaka"), z możliwością przybliżenia do szczegółów (miast, tras) gestem lub przyciskiem.
- Aplikacja **nie posiada** dedykowanego przycisku "resetuj widok / wycentruj mapę" - powrót do widoku ogólnego odbywa się przez oddalenie (gest pinch-out lub przycisk „−") przez użytkownika.
- Dane mapy (lista miast, lista tras z przypisaną liczbą wagonów oraz kolorem trasy) zostaną dostarczone przez użytkownika/zleceniodawcę na podstawie fizycznej gry, w dalszym etapie prac.
- Trasy na mapie posiadają przypisany **kolor zgodny z oryginalną grą** (część tras ma określony kolor wymagający konkretnych kart wagonów, część jest "szara"/neutralna - dowolny kolor). Kolor trasy jest elementem wizualnym mapy bazowej (niezależnym od stanu zaznaczenia/wykonania).

### 2.2 Model użytkownika
- Aplikacja obsługuje **jednego gracza** (brak rozróżniania wielu graczy w jednej instancji aplikacji).
- W realnej rozgrywce każdy gracz przy stole otwiera aplikację indywidualnie na swoim urządzeniu, by planować własne trasy.
- Użytkownik ma możliwość **wyboru koloru wagonów** wyświetlanych na mapie (reprezentujących jego kolor w grze planszowej).
- Wagony w aplikacji mogą być wizualnie reprezentowane jako proste prostokąty w wybranym kolorze (nie muszą być realistyczną grafiką wagonu).

### 2.3 Interakcje użytkownika

**Oznaczanie miast:**
- Użytkownik może oznaczyć wiele miast na mapie poprzez kliknięcie.
- Oznaczenie miasta działa jako **toggle** - ponowne kliknięcie na już oznaczone miasto usuwa to oznaczenie.
- Oznaczenie miasta ma charakter **wyłącznie wizualny / notatkowy** - nie jest powiązane z żadną logiką punktową lub regułami gry (np. biletami/celami). Służy jedynie jako pomoc wizualna dla gracza.

**Oznaczanie tras pociągów (cykl kliknięć):**
- 1. kliknięcie trasy → trasa zostaje **zaznaczona** (np. jako "planowana"/"wybrana").
- 2. kliknięcie tej samej trasy → trasa zostaje oznaczona jako **wykonana**.
- 3. kliknięcie (kolejne) → reset zaznaczenia trasy (powrót do stanu domyślnego).
- Możliwość zaznaczania/oznaczania wielu tras jednocześnie (niezależnie od siebie).
- Stan **zaznaczonej** i **wykonanej** trasy są od siebie **wizualnie rozróżnione** na mapie (różny styl/wygląd, np. zaznaczona - obrys/podświetlenie, wykonana - wypełnienie kolorem wagonów gracza). Dokładny styl wizualny do ustalenia na etapie projektowania UI.

### 2.4 Podgląd liczników
- Użytkownik ma bieżący podgląd:
  - liczby pociągów (wagonów) z **wykonanych** tras,
  - liczby pociągów (wagonów) z **zaznaczonych** (planowanych) tras.
- Liczba pociągów (wagonów) przypisana do każdej trasy jest **wartością stałą, wbudowaną w mapę**, zgodną z oryginalną grą planszową (nie jest wpisywana ręcznie przez użytkownika).
- Liczniki prezentowane są w **prostej formie liczbowej** (np. "12 / 45" - liczba zaznaczonych wagonów tras / liczba wykonanych wagonów wykonanych tras gracza), bez dodatkowych elementów graficznych typu pasek postępu.

### 2.5 Menu główne — kolor wagonów i nowa rozgrywka
- Akcje, które nie powinny być dostępne bezpośrednio z widoku mapy (aby uniknąć przypadkowego ich wywołania), umieszczone są w **menu głównym**, poza samą planszą:
  - **Nowa rozgrywka** ("Nowy plan") - czyści wszystkie oznaczenia miast oraz tras (zaznaczone i wykonane) i rozpoczyna planowanie od stanu domyślnego. Samo wejście z menu głównego jest wystarczającą ochroną przed przypadkowym wywołaniem - **bez dodatkowego potwierdzenia** (np. dialogu "czy na pewno").
  - **Wybór koloru wagonów** - wybór koloru wagonów wyświetlanych na mapie dla danego gracza, z predefiniowanej palety: **czarny, czerwony, niebieski, zielony, żółty**.
- Z menu głównego użytkownik przechodzi do widoku mapy jako **"Nowy plan"** (od czystej mapy) albo **"Kontynuuj"** (z zachowaniem bieżących oznaczeń). Widok mapy **nie zawiera** osobnego ekranu ustawień - powyższe akcje są dostępne wyłącznie z menu głównego.

### 2.6 Język interfejsu
- Aplikacja dostępna jest w **jednym języku: polskim**. Nie jest wymagana wielojęzyczność / lokalizacja na inne języki.

### 2.7 Wyszukiwanie miasta
- W standardowym trybie mapy dostępne jest wyszukiwanie miasta. Domyślnie widoczna jest wyłącznie **ikona lupy**; pole tekstowe rozwija się dopiero po jej kliknięciu i zwija z powrotem do samej lupy po wybraniu miasta lub po utracie focusu (kliknięciu poza elementem wyszukiwania).
- Podczas wpisywania fragmentu nazwy wyświetlana jest lista podpowiedzi z najbardziej pasującymi nazwami miast dostępnych na planszy.
- Po wybraniu miasta z listy podpowiedzi:
  - widok mapy przybliża się (zoom) i centruje na wybranym mieście,
  - miasto zostaje oznaczone (analogicznie do ręcznego oznaczania miast, patrz 2.3), jeśli nie było ono wcześniej oznaczone. Jeśli miasto było już oznaczone, jego stan oznaczenia nie ulega zmianie.
- Wyszukiwanie miasta jest funkcją standardowego trybu mapy - nie jest dostępne w trybie deweloperskim (patrz 2.8).

### 2.8 Tryb deweloperski
- Tryb deweloperski to dodatkowy tryb działania aplikacji, dostępny z menu głównego na równi ze standardowym trybem mapy.
- Przeznaczenie: ręczne przygotowanie i uzupełnianie danych mapy (miast i tras), które docelowo zostają na stałe wbudowane w aplikację. Nie jest to funkcja przeznaczona dla gracza korzystającego z aplikacji podczas rozgrywki.
- Po wejściu w tryb deweloperski wczytywane są dane mapy aktualnie dostępne w aplikacji, tworząc dwie listy robocze: listę miast oraz listę tras.
- W trybie deweloperskim mapa służy wyłącznie jako podkład do wskazywania położenia elementów - funkcje oznaczania miast i tras znane ze standardowego trybu (2.3) są w nim niedostępne.
- **Zarządzanie miastami:**
  - Dodanie miasta: deweloper wskazuje na mapie dokładne położenie, a następnie uzupełnia pozostałe dane miasta, wybierając jego nazwę z ustalonej, stałej listy nazw miast (z podpowiedziami podczas wpisywania).
  - Deweloper może również edytować (np. poprawić położenie lub dane) oraz usuwać pozycje znajdujące się już na liście miast.
- **Zarządzanie trasami:**
  - Dodanie trasy: deweloper wskazuje na mapie kolejne punkty wyznaczające przebieg trasy pomiędzy dwoma wybranymi miastami, a następnie uzupełnia pozostałe dane trasy wymagane do zapisania jej modelu (m.in. liczbę wagonów).
  - Deweloper może również edytować oraz usuwać pozycje znajdujące się już na liście tras.
- Efektem pracy w trybie deweloperskim jest komplet danych (lista miast i lista tras), który można skopiować w formacie JSON do schowka systemowego, do dalszego ręcznego wykorzystania (np. wklejenia do plików danych aplikacji).

## 3. Wymagania niefunkcjonalne

### 3.1 Działanie offline
- Aplikacja działa **w pełni offline** - nie wymaga połączenia z internetem do swojego działania.

### 3.2 Trwałość danych / stan gry
- Oznaczenia na mapie (miasta, zaznaczone trasy, wykonane trasy) **nie są zapisywane trwale**.
- Stan resetuje się po zamknięciu aplikacji (każde uruchomienie aplikacji zaczyna od czystej, nieoznaczonej mapy).

### 3.3 Orientacja ekranu
- Aplikacja wspiera **tylko orientację poziomą (landscape)**, ze względu na szeroki, poziomy układ mapy gry. Orientacja pionowa (portrait) nie jest wspierana.

### 3.4 Pomoc / onboarding
- Aplikacja **nie zawiera** ekranu pomocy, instrukcji ani onboardingu. Obsługa ma być intuicyjna na podstawie samego interfejsu (menu główne + widok mapy).

### 3.5 Granice zakresu funkcjonalnego
- Aplikacja **nie posiada** funkcji "cofnij" (undo) dla akcji na mapie - przypadkowe kliknięcie wymaga ręcznej korekty (np. dodatkowych kliknięć w cyklu trasy).
- Aplikacja **nie odwzorowuje** dodatkowych, unikalnych mechanik gry "Legendy zachodu" względem podstawowej wersji "Wsiąść do pociągu" (np. role, wydarzenia, kontrakty). Aplikacja ogranicza się wyłącznie do:
  - wyświetlania mapy,
  - oznaczania miast i tras,
  - śledzenia liczników wagonów.

### 3.6 Technologia
- Brak sprecyzowanej preferencji co do podejścia technologicznego (natywne aplikacje per platforma vs. podejście multiplatformowe, np. Flutter/React Native) - decyzja pozostaje do podjęcia na etapie technicznym projektu.

## 4. Status specyfikacji

Specyfikacja w obecnej formie pokrywa funkcjonalny i biznesowy zakres aplikacji. Pozostałe elementy do uzupełnienia w kolejnych etapach prac:

- **Dokładne dane mapy** (lista miast, lista tras wraz z przypisaną liczbą wagonów) - zostaną przygotowane przez zleceniodawcę w odrębnym kroku/rozmowie, na podstawie fizycznej gry "Wsiąść do pociągu: Legacy - Legendy zachodu".
- **Szczegóły wizualne UI** (dokładny styl rozróżnienia zaznaczonej/wykonanej trasy, layout menu głównego i widoku mapy) - do ustalenia na etapie projektowania UI/UX.
- **Wybór technologii** (natywne aplikacje vs. multiplatformowe) - do podjęcia na etapie technicznym projektu.

