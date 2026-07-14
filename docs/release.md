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

Z katalogu `src/Aplication`:

```
dotnet publish -f net10.0-android -c Release ^
  -p:AndroidPackageFormat=apk ^
  -p:AndroidKeyStore=true ^
  -p:AndroidSigningKeyStore=ttr-legacy.keystore ^
  -p:AndroidSigningKeyAlias=ttrlegacy ^
  -p:AndroidSigningKeyPass=<hasło> ^
  -p:AndroidSigningStorePass=<hasło>
```

Wynik: `bin/Release/net10.0-android/publish/pl.wiktor.szczeszek.tickettoridelegacymap-Signed.apk`.
Plik wysyłasz testerowi dowolnym kanałem; instalacja wymaga włączenia „Zainstaluj z nieznanych
źródeł" dla aplikacji, przez którą apk trafia na telefon (przeglądarka/menedżer plików).

### 1.3 Numer wersji

Każde wydanie do testerów podbija `ApplicationDisplayVersion` (widoczna nazwa, np. `1.1`) i
`ApplicationVersion` (wewnętrzny int, musi **rosnąć** między instalkami, inaczej Android
odrzuci aktualizację jako starszą) w `Aplication.csproj`.

---

## 2. iOS — odłożone

Budowa i podpis iOS wymagają Xcode, czyli fizycznego dostępu do **Maca** — bez niego nie da się
nawet skompilować `.ipa`, niezależnie od typu konta Apple czy sposobu dystrybucji. Publikacja na
iOS jest odłożona do czasu, aż dostępny będzie sprzęt Apple do builda i testów.

---

## 3. Checklist przed wysyłką do testerów

- [ ] Podbity `ApplicationDisplayVersion` / `ApplicationVersion` w `Aplication.csproj`.
- [ ] `dotnet build` na docelowym frameworku bez błędów (sanity-build z [CLAUDE.md](../CLAUDE.md)).
- [ ] Android: apk podpisany tym samym keystore co poprzednie wydania.
