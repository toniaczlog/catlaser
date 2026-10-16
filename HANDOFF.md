# CatLaser — HANDOFF, 2026-10-16

**Stan: zamykamy dziś pracę.** Nie uruchamiać nowych wdrożeń bez kolejnego polecenia. Ten dokument opisuje ostatni potwierdzony stan, nie zastępuje odczytu aktualnego HEAD.

## Repozytorium, release i strona

- Repo publiczne MIT: **https://github.com/toniaczlog/catlaser**; branch **main**.
- HEAD sprzed commita dokumentacyjnego: **a6b46d0b407e07632e4ff3bafd720a219b3d7369**. Przy wznowieniu odczytaj aktualny HEAD.
- Produkcja strony: **https://toniaczlog.github.io/catlaser/**; wersje językowe **/pl/** i **/en/**; source **main:/docs**.
- Native EXE: **CatLaser 0.3.0** — C# / .NET 10 WPF + Windows Forms + Win32, portable Windows 10/11 x64.
- Release: **https://github.com/toniaczlog/catlaser/releases/tag/v0.3.0**.
- Asset: **CatLaser-0.3-win-x64.zip**, **71 504 977 bajtów** (ok. 71,5 MB), niepodpisany cyfrowo; **prerelease**, nie stable.
- GitHub Pages jest już aktywne i działa. Bez Vercel/Netlify/Coolify/Supabase, logowania, zakupionej domeny lub płatnego hostingu.

## Co jest zrobione w EXE 0.3

- **A1:** wybór monitora i procentowego prostokąta ruchu dla lasera automatycznego. `MotionEngine.cs` operuje na granicach ekranu; nie porusza kursora Windows.
- **A3:** `PlaylistStore.cs` — do 15 nazwanych sekwencji, do 20 etapów. `Manual` w playliście oznacza **ciemną przerwę**, a nie tryb sterowania myszą.
- **A4:** Spiral, FigureEight i Orbit, z konfigurowalnym promieniem/prędkością.
- **A6:** HaloPulse, TwinSpot, LightTunnel i SoftStar; łącznie **12 stylów**.
- **A9:** `HotkeyConfig.cs` + `HotkeyWindow.cs` — sześć własnych skrótów, wykrywanie duplikatów, nieudanej rejestracji i rollback.
- **A11:** rozszerzone menu tray: gry, kolory, presety, playlisty, prędkość.
- **A13:** `LaserController.cs` / `LaserOptions.cs` — adaptacyjny tryb redukujący pracę renderowania i cykl dispatchera do ~12 FPS dla nieruchomego Manual Real/Neon/Comet; animowane efekty i gry pozostają przy ustawionym FPS. Mierzony parametr **LastSubmissionMilliseconds to czas zgłoszenia rysowania, nie render GPU**.
- Dodatkowo: `FullscreenGuard.cs` — best-effort wykrywanie bezramkowego fullscreen i wstrzymanie overlay.
- PL/EN przez `UiText.cs`, domyślnie polski w Windows z polskim językiem UI; wybór zapamiętywany.
- `PresetStore.cs` — do 25 lokalnych konfiguracji, `playlists.json` i `hotkeys.json` w `%LOCALAPPDATA%/CatLaser/`.

## Co jest zrobione na landingu

Wszystkie wybrane **L1, L3, L5, L6, L7, L8, L9, L10, L11, L13 i L14**:

- Rzeczywisty zrzut okna WPF w `docs/screenshots/settings-0.3.png`, **63 523 bajtów**. Wygenerowany przez uruchomione EXE na runnerze Windows — nie makieta AI. Nie potwierdza jeszcze rzeczywistych efektów lasera ani zachowania GUI na domowym PC.
- Demo Canvas: 8 efektów, regulacja koloru/poświaty/śladu; śledzące punkt minimalistyczne oczy; 4 mini-demonstracje ruchu.
- Sticky CTA na mobile; download 0.3/ZIP, changelog, bezpieczeństwo i prywatność, animacje przewijania z obsługą reduced motion.
- `docs/index.html`, `docs/pl/index.html`, `docs/en/index.html`, SEO robots/sitemap i metadane.
- `docs/press/` zawiera mark/wordmark i formaty 9:16, 16:9 jako SVG.
- Brak dodatkowego płatnego hostingu i brak kupionej/podpiętej domeny.

## Testy i historia wdrożeń

- **Windows**: [Actions 38954570106](https://github.com/toniaczlog/catlaser/actions/runs/38954570106) — **success** (PL/EN smoke, ConfigSmoke, publish x64, upload asset, screenshot).
- **Strona**: [Actions 38955090041](https://github.com/toniaczlog/catlaser/actions/runs/38955090041) — **success**, po poprawie testu nazwy screenshotu.
- **Pages**: [Actions 38955090774](https://github.com/toniaczlog/catlaser/actions/runs/38955090774) — **success** na HEAD sprzed aktualizacji docs.
- Pośrednia nieudana walidacja `38955011028` wynikała z błędnego escapowania regex w `scripts/check-site.mjs`; poprawiona w `a6b46d0`, obecny test zielony.
- **Uwaga provenance wydania:** release `v0.3.0` ma pole `target_commitish` wskazujące `413d565a...`, chociaż aktualny ZIP był ponownie wgrywany przez nowszy build `b1876be...`. Przed stable rozdzielić „do jakiego commita prowadzi tag” od „z jakiego builda pochodzą bajty ZIP” i zweryfikować/reutworzyć prawidłowe wydanie.
- Workflow Windows filtruje `CatLaser/**` i `tests/**`. Zmiany tylko Markdown nie powinny produkować nowej wersji EXE. Nie modyfikować workflow bez potrzeby.
- Zrzut WPF wykonywany przez flagę `--capture-settings`; workflow może stworzyć dodatkowy commit do `docs/screenshots/` podczas builda — sprawdzać HEAD i race publikacji.

## Znane ryzyka / czego nie wolno twierdzić

1. **Brak pełnego ręcznego smoke-testu CatLaser 0.3 na rzeczywistym Windows.** CI potwierdza kompilację i testy konfiguracji, nie jakość animacji ani poprawność wszystkich interakcji.
2. Nieweryfikowane: kilka monitorów/mixed-DPI, click-through, efekty optyczne, playlisty z przerwą, konflikty globalnych klawiszy, powrót z fullscreen i bezpieczne przywracanie kursora.
3. **Brak benchmarku CPU/GPU** dla A13; 12 FPS to ustawienie dispatchera podczas spoczynku, nie pomiar oszczędności.
4. EXE bez podpisu kodu (możliwy SmartScreen).
5. Screenshot WPF prawdziwy, ale nie pokazuje lasera na innych aplikacjach; lepsze materiały promocyjne powinny powstać z rzeczywistych nagrań.

## Krytyczny mechanizm bezpieczeństwa

`NativeCursor.Apply` podmienia **widoczny globalny kursor Windows** w Manual. Niezmienny wymóg: `NativeCursor.Restore` na starcie, wyłączeniu i normalnym wyjściu oraz w odpowiedniej pauzie fullscreen. Nigdy nie ukrywać kursora. Tryby automatyczne nie wysyłają ruchów do myszy.

Awaryjnie, w folderze z aplikacją:

```powershell
.\CatLaser.exe --restore-cursors
```

## Start kolejnej sesji

1. Przeczytaj aktualny `README.md`, `HANDOFF.md`, `DECISIONS.md`, `NEXT.md`, odczytaj HEAD, listę commitów i CI; nie zakładaj stanu z tego SHA po nowych zmianach.
2. Na komputerze użytkownika zamknij stary CatLaser w trayu, pobierz 0.3 i wykonaj **ręczny test według NEXT.md**, zwłaszcza kursora, playlist i pełnego ekranu.
3. Napraw stwierdzone usterki, nie dodawaj od razu kolejnej partii efektów; zminimalizuj liczbę buildów/release.
4. Po QA można wrócić do promowania projektu darmowymi Shorts/TikTok, scenariuszem Gemini/Google Flow, opcjonalnie do kupna krótkiej domeny. **Nie kupiono domeny ani reklam.**

**Zamknięcie prac: 2026-10-16.**
