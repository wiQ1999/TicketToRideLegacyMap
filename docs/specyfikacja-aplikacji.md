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
- **Domyślny widok** po otwarciu aplikacji to **cała plansza w pomniejszeniu** ("z lotu ptaka"), z możliwością przybliżenia do szczegółów (miast, tras) za pomocą gestów.
- Aplikacja **nie posiada** dedykowanego przycisku "resetuj widok / wycentruj mapę" - powrót do widoku ogólnego odbywa się poprzez ręczny gest oddalenia (pinch-out) przez użytkownika.
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
- Liczniki prezentowane są w **prostej formie liczbowej** (np. "12 / 45" - liczba wykorzystanych wagonów / całkowity limit wagonów gracza), bez dodatkowych elementów graficznych typu pasek postępu.

### 2.5 Widok ustawień / działań
- W aplikacji istnieje dodatkowy, osobny widok (ekran) zawierający akcje, które nie powinny być dostępne bezpośrednio z widoku mapy, aby uniknąć przypadkowego ich wywołania:
  - **Reset mapy** ("Nowa rozgrywka") - czyści wszystkie oznaczenia miast oraz tras (zaznaczone i wykonane) i przywraca mapę do stanu domyślnego. Samo przejście do osobnego widoku jest wystarczającą ochroną przed przypadkowym wywołaniem - **bez dodatkowego potwierdzenia** (np. dialogu "czy na pewno").
  - **Zmiana koloru wagonów** - wybór koloru wagonów wyświetlanych na mapie dla danego gracza, z predefiniowanej palety: **czarny, czerwony, niebieski, zielony, żółty**.
- Wejście do tego widoku odbywa się poprzez nawigację (np. osobny przycisk/ikona menu), a nie poprzez akcję bezpośrednio na mapie.

### 2.6 Język interfejsu
- Aplikacja dostępna jest w **jednym języku: polskim**. Nie jest wymagana wielojęzyczność / lokalizacja na inne języki.

## 3. Wymagania niefunkcjonalne

### 3.1 Działanie offline
- Aplikacja działa **w pełni offline** - nie wymaga połączenia z internetem do swojego działania.

### 3.2 Trwałość danych / stan gry
- Oznaczenia na mapie (miasta, zaznaczone trasy, wykonane trasy) **nie są zapisywane trwale**.
- Stan resetuje się po zamknięciu aplikacji (każde uruchomienie aplikacji zaczyna od czystej, nieoznaczonej mapy).

### 3.3 Orientacja ekranu
- Aplikacja wspiera **tylko orientację poziomą (landscape)**, ze względu na szeroki, poziomy układ mapy gry. Orientacja pionowa (portrait) nie jest wspierana.

### 3.4 Pomoc / onboarding
- Aplikacja **nie zawiera** ekranu pomocy, instrukcji ani onboardingu. Obsługa ma być intuicyjna na podstawie samego interfejsu (widok mapy + widok ustawień).

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

- **Dokładne dane mapy** (lista miast, lista tras wraz z przypisaną liczbą wagonów i kolorem trasy) - zostaną przygotowane przez zleceniodawcę w odrębnym kroku/rozmowie, na podstawie fizycznej gry "Wsiąść do pociągu: Legacy - Legendy zachodu".
- **Szczegóły wizualne UI** (dokładny styl rozróżnienia zaznaczonej/wykonanej trasy, layout widoku mapy i widoku ustawień) - do ustalenia na etapie projektowania UI/UX.
- **Wybór technologii** (natywne aplikacje vs. multiplatformowe) - do podjęcia na etapie technicznym projektu.

