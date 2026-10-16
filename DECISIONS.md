# CatLaser — decyzje projektowe

**Aktualizacja: 2026-10-16** · Aplikacja: **v0.3.0 prerelease** · Strona: GitHub Pages.

## Architektura i hosting

- **2026-10-16:** Natywna aplikacja Windows 10/11 x64: C# / .NET 10 WPF + Win32. Nie używać Electron ani dodatkowego serwera tylko do renderowania lasera.
- **2026-10-16:** Publiczne repozytorium MIT; dystrybucja self-contained ZIP przez GitHub Releases po pozytywnej kompilacji Windows.
- **2026-10-16:** Landing jako statyczne HTML/CSS/JS na darmowym GitHub Pages (`main:/docs`), bez npm, płatnego hostingu ani backendu.
- **2026-10-16:** CI rozdzielone: ścieżki kodu/testów EXE uruchamiają Windows build, ścieżki WWW uruchamiają site validation. Nie generować release EXE przy zmianach samego Markdown.

## Bezpieczeństwo i prywatność

- **2026-10-16:** Tryb manualny używa **widocznego** kursora Windows; globalnej zmiany nigdy nie zastępować ukryciem kursora. Przywrócenie na starcie, wyłączeniu, zakończeniu i przez `--restore-cursors`.
- **2026-10-16:** Tryby autonomiczne poruszają tylko wirtualny punkt; nie wykonują ruchu faktyczną myszą.
- **2026-10-16:** Sesje z limitami czasu i przerwami; bez intensywnego stroboskopu. Ostrzeżenie o pilnowaniu kota przy monitorze i kończeniu zabawy fizyczną zabawką.
- **2026-10-16:** Presety, playlisty, hotkeys i język przechowywane lokalnie. Brak kont, reklam, telemetrii, abonamentów i synchronizacji.

## Produkt, wersje i UX

- **2026-10-16:** 0.2 zastąpiła wiele WPF shape-controls **jedną powierzchnią renderowania na ekran**, z FPS Eco/Balanced/Ultra oraz oszczędzaniem pracy bezczynnych monitorów.
- **2026-10-16:** 0.2.1 dodała PL/EN, automatyczny polski na polskim Windows oraz panel z sześcioma uporządkowanymi zakładkami. Ręczny wybór języka zapamiętywany.
- **2026-10-16:** 0.3 dodaje prostokątny obszar zabawy, wybór monitora, playlisty, ruch Spiral/FigureEight/Orbit, efekty HaloPulse/TwinSpot/LightTunnel/SoftStar, mapowanie sześciu globalnych skrótów i szybkie menu tray.
- **2026-10-16:** Dodatkowy best-effort fullscreen pause przy bezramkowym pełnym ekranie, bez wpływania na normalne zmaksymalizowane okna; konieczny test na sprzęcie.
- **2026-10-16:** A13 ogranicza dispatcher do ~12 FPS i zbędne invalidacje wyłącznie dla nieruchomego, ręcznego punktu w statycznych stylach. Nie deklarować poprawy CPU/GPU bez pomiarów.
- **2026-10-16:** Strona udostępnia osobne trasy SEO `/pl/` i `/en/`, osiem efektów w demie, tryby mini-preview, mobilny CTA, changelog, prywatność i prasowe assety SVG.
- **2026-10-16:** Demo landingowe przedstawia tylko subtelne oczy podążające za laserem, bez pełnej kreskówkowej sylwetki kota.
- **2026-10-16:** Na landingu publikować wyłącznie **autentyczne przechwyty programu**. Screenshot `docs/screenshots/settings-0.3.png` powstał z WPF na Windows runnerze, nie z generatora grafik.

## Publikacja i następne sesje

- **2026-10-16:** Wydanie `v0.3.0` pozostaje **prerelease**, bo zielone CI nie zastępuje rzeczywistego smoke-testu Windows.
- **2026-10-16:** Przed zmianami zawsze sprawdzić bieżący HEAD, CI i asset release. Weryfikować pochodzenie assetu ZIP względem taga; wcześniejszy workflow nadpisywał asset po późniejszym buildzie.
- **2026-10-16:** Nigdy nie nadpisywać równoległych zmian na main, nie używać force-push.
- **2026-10-16:** Kolejne prace: najpierw test i ewentualne poprawki regresji 0.3; bez automatycznego rozszerzania funkcji.
- **2026-10-16:** Reklamy Gemini/Google Flow, darmowy YouTube Shorts/TikTok i krótka domena — **opcje na przyszłość**; nie dokonano zakupów domeny ani kampanii.

Techniczny stan zamknięcia: [HANDOFF.md](HANDOFF.md). Lista kontrolna: [NEXT.md](NEXT.md).
