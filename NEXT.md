# CatLaser — co dalej (po 2026-10-16)

**Dzisiaj kończymy.** Nie uruchamiać nowych buildów ani nie dodawać funkcji bez kolejnej decyzji użytkownika. Ostatni release: **0.3.0 prerelease**. [Stan projektu](HANDOFF.md).

## 1. Ręczny test EXE 0.3 na Windows — priorytet

- [ ] Zamknij wcześniejszy CatLaser w trayu; pobierz [ZIP 0.3](https://github.com/toniaczlog/catlaser/releases/tag/v0.3.0), rozpakuj, uruchom EXE.
- [ ] Polski interfejs na polskim Windows; przełącz PL/EN, uruchom ponownie; sprawdź nawigację i opisy.
- [ ] Efekty Real/Neon/Cyber/Photoreal/Comet/Electric/Prism/Plasma, kolory, ślady, shockwave i jitter.
- [ ] Nowe HaloPulse/TwinSpot/LightTunnel/SoftStar: animacja, brak agresywnych błysków, płynność.
- [ ] Gry Ricochet/SmartPrey/MouseHunt/HideSeek/CatchMe oraz Spiral/FigureEight/Orbit; prędkość, wybrany monitor i ograniczony prostokąt.
- [ ] Playlisty: zapis, uruchomienie, ciemna przerwa (Manual), zakończenie sesji i wyłączenie.
- [ ] Skróty globalne: nowe kombinacje, duplikaty, zajęta kombinacja i rollback bez utraty starych skrótów.
- [ ] Tray: presety, kolory, gry, playlisty, prędkość i wyjście; wyjście przywraca standardowy kursor.
- [ ] Fullscreen pause: bezramkowy pełny ekran, zwykłe zmaksymalizowane okno, wznowienie i przywrócenie kursora.
- [ ] **Krytyczne:** wyłączenie, ponowne uruchomienie i polecenie `CatLaser.exe --restore-cursors` poprawnie przywracają schemat kursorów.
- [ ] Eco/Balanced/Ultra oraz A13: nieruchomy ręczny Real/Neon/Comet ogranicza pracę renderowania; ruch budzi normalny limit. Jeśli możliwe, zmierz obciążenie CPU/GPU.
- [ ] Sprawdź Windows scale 100/125/150% i konfigurację dwóch monitorów/mixed-DPI.

## 2. Kontrola landingu GitHub Pages

- [ ] Obejrzyj desktop i mobile (~390 px): [PL](https://toniaczlog.github.io/catlaser/pl/) oraz [EN](https://toniaczlog.github.io/catlaser/en/).
- [ ] Osiem efektów w demie, zmiana barwy/poświaty/śladu, cztery mini-previews i oczy śledzące kursor.
- [ ] Sprawdź sticky CTA mobile, link ZIP 0.3, sekcję bezpieczeństwa, changelog, FAQ i SVG w `docs/press/`.
- [ ] Obejrzyj realny screenshot WPF w `docs/screenshots/settings-0.3.png`; opcjonalnie nagraj *prawdziwy* desktopowy pokaz nowych efektów na komputerze.

## 3. Stabilizacja i promocja — dopiero po QA

- [ ] Napraw tylko stwierdzone usterki, łącz poprawki w ograniczoną liczbę CI/release.
- [ ] Uporządkuj pochodzenie taga `v0.3.0` względem nowszego nadpisanego assetu ZIP; sprawdź rzeczywisty build/SHA przed stable.
- [ ] Po zatwierdzeniu smoke-testu rozważ zmianę prerelease → stable (bez fikcyjnego potwierdzania testów).
- [ ] Opcjonalnie: 8-sekundowe Gemini, 20–30 sekund Google Flow, publikacja darmowych Shorts/TikTok; użyj realnego wideo z EXE.
- [ ] Opcjonalnie: wybór i zakup taniej domeny dla GitHub Pages. **Na 2026-10-16 domena nie jest kupiona.**

## Przydatne komendy

```powershell
# Awaryjne przywrócenie kursorów w katalogu programu
.\CatLaser.exe --restore-cursors

# Testy na Windows z .NET SDK 10
dotnet run --project tests/LocalizationSmoke/LocalizationSmoke.csproj -c Release
dotnet run --project tests/ConfigSmoke/ConfigSmoke.csproj -c Release
```

```bash
# Walidacja strony (Node.js)
node --check docs/main.js
node scripts/check-site.mjs
```

**Nie zakładać żadnych automatycznych przypomnień związanych z projektem.** Kontynuacja dopiero na życzenie.
