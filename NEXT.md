# Next

1. Confirm GitHub Actions Windows publish and release archive status for v0.2.1.
2. On Windows, test interactive responsiveness compared with 0.1: manual pointer, 8 visual styles, color switching, 5 autonomous games and shockwaves.
3. Inspect any WPF frame skips/mixed-DPI/dual-monitor issues, energy consumption on Eco mode, input pass-through and cursor recovery.
4. Exercise session rest and timeout, click catch, preset persistence/reload and every global shortcut.
5. If approved in Windows smoke test, graduate prerelease to stable without another code deployment.

6. On Polish Windows verify default Polish on fresh profile, PL/EN toggle, restart persistence, non-Polish fallback, tray menu labels, and navigation sections.
7. Check at 100%/125%/150% text scaling for clipping and ensure switching language while laser is on never resets the cursor or session.

## Landing page next steps
- Enable GitHub Pages in Settings → Pages, source `Deploy from a branch`, branch `main`, folder `/docs` (if not already active).
- Verify actual live URL, mobile layout, both language versions, laser preview and download CTA in a real browser.
- Prepare 9:16 cat-loving short video concept, Gemini / Google Flow storyboard and prompt(s); link the published landing page in social bio.

## v0.3 test checklist

- On Windows: check all four visual illusions, spiral/eight/orbit, selected monitor and area, saved playlist reload and startup, rest, tray submenu, custom hotkey conflicts and rollback, fullscreen pause and cursor restore.
- On Windows: take actual screenshots of CatLaser UI and effects, put under `docs/screenshots/`, replace pending screenshot placeholder.
- On landing: test PL/EN URLs, 8 interactive effects, glow/trail/color inputs, motion previews, sticky download and press SVG links. Check both 390px mobile and desktop.
- Watch for main/release race on combined deploy. Ensure tag `v0.3.0` and ZIP are present before advertising download.

- Verify on Windows that adaptive 12 FPS starts only after the laser is idle, and moving the pointer wakes it immediately.
