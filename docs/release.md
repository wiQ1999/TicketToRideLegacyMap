# Release: Android

Instrukcja przygotowania instalek dla testerów — **bez publikacji w Google Play**. Android
trafia bezpośrednio jako plik `.apk` (sideload). `ApplicationId`, wersje i target frameworki —
`src/Aplication/Aplication.csproj`.

---

## 1. Android — podpisany APK

Wymaga zainstalowanego workloadu (`dotnet workload install android`) i licencji Android SDK —
zwykle już obecne, skoro projekt buduje się na `net10.0-android`.

### 1.1 Keystore (jednorazowo)

```
keytool -genkeypair -v -keystore ttr-legacy.keystore -alias ttrlegacy -keyalg RSA -keysize 2048 -validity 10000
```

Plik `.keystore` i hasła przechowuj poza repo (np. w menedżerze haseł) — bez nich nie da się
wydać aktualizacji podpisanej tym samym kluczem, Android odrzuci apk z innym podpisem jako
niekompatybilny.

### 1.2 Publikacja

Jedna linia (PowerShell nie rozumie `^` jako kontynuacji — to składnia `cmd.exe`; każda kolejna
linia z `^` wykonałaby się jako osobna, niepoprawna komenda). Ścieżka do `.csproj` podana wprost,
więc katalog roboczy nie ma znaczenia:

```
dotnet publish "src/Aplication/Aplication.csproj" -f net10.0-android -c Release -p:AndroidPackageFormat=apk -p:AndroidKeyStore=true -p:AndroidSigningKeyStore="<pełna ścieżka do ttr-legacy.keystore>" -p:AndroidSigningKeyAlias=ttrlegacy -p:AndroidSigningKeyPass=<hasło> -p:AndroidSigningStorePass=<hasło>
```

Jeśli wolisz rozbić na wiele linii w PowerShellu, kontynuacją jest backtick (`` ` ``) na końcu
linii — ale musi to być **ostatni znak w linii**, bez spacji po nim, inaczej łamanie po cichu
nie zadziała.

Wynik: `bin/Release/net10.0-android/publish/pl.wiktor.szczeszek.tickettoridelegacymap-Signed.apk`.
Plik wysyłasz testerowi dowolnym kanałem; instalacja wymaga włączenia „Zainstaluj z nieznanych
źródeł" dla aplikacji, przez którą apk trafia na telefon (przeglądarka/menedżer plików).

### 1.3 Numer wersji

Każde wydanie do testerów podbija `ApplicationDisplayVersion` (widoczna nazwa, np. `1.1`) i
`ApplicationVersion` (wewnętrzny int, musi **rosnąć** między instalkami, inaczej Android
odrzuci aktualizację jako starszą) w `Aplication.csproj`.

---

## 2. Przechowywanie instalatorów — GitHub Releases

Apk **nie trafia do repozytorium** (nie commituj go — `bin/`/`obj*` są już w `.gitignore`, a
binarki w historii gita tylko ją napuchają). Zamiast tego każde wydanie to osobny **GitHub
Release** przypięty do tagu, z apk jako załącznikiem.

### 2.1 Tag zgodny z wersją aplikacji

Tag nazywaj wprost z `ApplicationDisplayVersion` (np. `v1.1`), żeby wersja w repo, w apk i w
release'ie zawsze się zgadzały:

```
git tag v1.1
git push origin v1.1
```

### 2.2 Release z załącznikiem (GitHub CLI)

Wymaga `gh` (zainstalowany i zalogowany — `gh auth login`). Jedna linia (patrz §1.2 — `^` to
składnia `cmd.exe`, nie PowerShella):

```
gh release create v1.1 "bin\Release\net10.0-android\publish\pl.wiktor.szczeszek.tickettoridelegacymap-Signed.apk" --title "v1.1" --notes "Opis zmian w tym wydaniu"
```

Tester pobiera apk bezpośrednio ze strony release'u (`Releases` → wybrana wersja → sekcja
Assets) — link jest stały i nie wymaga logowania do repo, jeśli repozytorium jest publiczne.

### 2.3 Alternatywa bez `gh` — interfejs GitHub

`github.com/<repo>/releases/new` → wybierz/utwórz tag → przeciągnij plik `.apk` w pole
załączników → **Publish release**. Równoważne krokom 2.1–2.2, bez instalowania CLI.

### 2.4 Historia wydań

Kolejne wersje to kolejne tagi/release'y (`v1.2`, `v1.3`, …) — GitHub trzyma je wszystkie
i pozwala testerowi wrócić do starszego apk, gdyby nowy build okazał się wadliwy.

---

## 3. iOS — odłożone

Budowa i podpis iOS wymagają Xcode, czyli fizycznego dostępu do **Maca** — bez niego nie da się
nawet skompilować `.ipa`, niezależnie od typu konta Apple czy sposobu dystrybucji. Publikacja na
iOS jest odłożona do czasu, aż dostępny będzie sprzęt Apple do builda i testów.

---

## 4. Checklist przed wysyłką do testerów

- [ ] Podbity `ApplicationDisplayVersion` / `ApplicationVersion` w `Aplication.csproj`.
- [ ] `dotnet build` na docelowym frameworku bez błędów (sanity-build z [CLAUDE.md](../CLAUDE.md)).
- [ ] Android: apk podpisany tym samym keystore co poprzednie wydania.
- [ ] Tag gita (`vX.Y`) odpowiadający wersji, wypchnięty do zdalnego repo.
- [ ] GitHub Release utworzony, apk załączony jako asset.
