# CatLaser 0.3.0 · Laser Playground 🔴

**Turn your mouse into a desktop laser.** Free, local, open-source Windows 10/11 software, built with .NET 10 + WPF + Win32. No accounts, ads or API keys.

[![Windows build](https://github.com/toniaczlog/catlaser/actions/workflows/windows-build.yml/badge.svg)](https://github.com/toniaczlog/catlaser/actions/workflows/windows-build.yml)

## Download

**[CatLaser v0.3.0 — Windows x64 ZIP](https://github.com/toniaczlog/catlaser/releases/tag/v0.3.0)**

Download `CatLaser-0.3-win-x64.zip`, extract the archive and start `CatLaser.exe`. Self-contained portable application; no separate .NET installation is required. The executable is not code-signed; Windows SmartScreen may warn. The release is marked **prerelease** until an interactive Windows test is performed.

## Język polski / English

- When Windows UI language is Polish (`pl-PL`), CatLaser automatically starts in Polish; other Windows display languages default to English.
- Click **PL / EN** in the top-right corner to switch immediately. This choice is saved to `%LOCALAPPDATA%\\CatLaser\\language.txt` and overrides the system language on later launches.
- The Windows notification-area menu, statuses, explanations, all game modes, presets and settings are translated.
- The new dashboard separates **Laser**, **Cat games**, **Sessions**, **Performance**, **My presets** and **Shortcuts** for less scrolling.
- No changes to the animation engine or system pointer recovery behavior.

## Laser visuals

- Real, Neon and Cyber (original styles)
- Photoreal: warm bright core, tiny coherent-looking grain and asymmetrical halo
- Comet: tapered, fading luminous streak
- Electric: arcing micro-lightning and tiny sparks
- Prism: three rotating colored dots plus a selectable base color
- Plasma: living halo with animated rings
- Shockwaves on clicks and sudden stops, adjustable organic hand-held jitter
- Adjustable glow, size, trail and six laser colors

## Games (without moving the system mouse)

- **Manual** — red (or selected-color) pointer follows mouse
- **Ricochet** — laser reflects from screen edges
- **SmartPrey** — irregular movement, rests, surprise bursts
- **MouseHunt** — fast zigzags, short hesitations
- **HideSeek** — virtual dark hiding blocks, occasional hide points
- **CatchMe** — click/touch-generated click near laser creates catch wave and short freeze

Games always have an automatic end: 30 seconds / 2 minutes / 5 minutes of active play, with optional rests every 20/30/60 seconds.

## Performance

The original WPF overlay used dozens of individual shape controls updated on every frame. CatLaser 0.2 replaces them with **one custom drawing surface per monitor**. Non-active screens skip invalidation. Trail samples are time bounded and draw complexity scales with the performance profile.

Eco (~30 FPS), Balanced (~60 FPS), Ultra (~120 FPS), or manual FPS caps 30/60/120/144. Actual frame rate also depends on Windows and your display.

## Controls

| Hotkey | Action |
|---|---|
| Ctrl + Alt + L | Toggle on/off |
| Ctrl + Alt + E | Next visual style |
| Ctrl + Alt + C | Next color |
| Ctrl + Alt + M | Next game mode |
| Ctrl + Alt + ↑ | Faster |
| Ctrl + Alt + ↓ | Slower |

Close the settings window to keep CatLaser in the notification area; use the tray icon to reopen or exit.

## Custom presets

Use **My Presets** to store up to 25 combinations, each with style, color, speed, effects, play mode and performance settings. Presets are local JSON data in `%LOCALAPPDATA%\CatLaser\presets.json`.

## Safety and cursor recovery

CatLaser draws through a click-through desktop overlay. In **Manual** mode the normal Windows cursor is replaced by a small **visible** dot using `SetSystemCursor`; it is restored on disable, normal exit and startup via `SPI_SETCURSORS`. This global cursor override can affect all Windows programs. If the app crashes before restoration, restart it or run `CatLaser.exe --restore-cursors`. Reapplying your Windows cursor scheme will also restore it. Native replacement cursors are never hidden.

Automatic games do **not** move your actual mouse. Monitor play sessions to prevent cats from scratching or damaging the screen, and finish with a physical toy/treat they can catch.

## Build

On Windows with .NET 10 SDK:

```powershell
dotnet publish CatLaser/CatLaser.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist
```

Automated release builds run on GitHub-hosted Windows runners. CI checks the build artifact but cannot verify live mouse interaction. Do a local GUI test before distributing to others.

## License

MIT.

## Website · GitHub Pages

The bilingual PL/EN landing page lives in [`docs/`](docs/). It is plain HTML, CSS and JavaScript — zero npm dependencies, paid hosting or telemetry. It includes a live laser demo and a direct Windows download link.

**One-time GitHub Pages setup** (requires repository admin in GitHub UI):
1. Open [Settings → Pages](https://github.com/toniaczlog/catlaser/settings/pages).
2. Under **Build and deployment**, select **Deploy from a branch**.
3. Branch: `main`, folder: `/docs`, then **Save**.
4. When Pages finishes publishing, the expected site address is `https://toniaczlog.github.io/catlaser/`. Confirm the actual URL in Pages settings before sharing publicly.

Subsequent edits to `docs/` publish from the same repository automatically. The Windows build workflow is path-filtered so docs-only commits do not rebuild the EXE.

To validate a site edit: `node --check docs/main.js && node scripts/check-site.mjs`.
The language selector defaults to the visitor's preferred browser language (PL or EN), and persists the selected language in browser localStorage.

## CatLaser 0.3.0 features

- Select the game monitor and a percentage-based safe play rectangle (left/top/width/height); movement is clamped to it.
- Playlists: compose up to 20 steps, each 10/20/30/60 seconds. A rest step turns off the virtual dot without moving the physical pointer. Up to 15 named sequences saved locally.
- New Spiral, FigureEight and Orbit motions (with adjustable orbit radius).
- HaloPulse, TwinSpot, LightTunnel and SoftStar light effects; slow alpha transitions, avoiding strobing.
- Pause overlay and restore native cursor when a borderless fullscreen foreground application is detected; resume afterwards. Maximized normal windows are excluded.
- Configurable global shortcuts (six actions). Combo editor validates duplicates and rolls back registrations on OS conflict; bindings are persisted locally.
- Tray submenus: colors, games, presets, playlists and speed +/-10%; localized PL and EN.

**Manual Windows GUI verification is still required** for fullscreen detection, per-monitor DPI and global hotkeys. Source is built and tested on GitHub Actions.

## Website refresh

Landing hosted by GitHub Pages at [CatLaser](https://toniaczlog.github.io/catlaser/), with indexable [/pl/](https://toniaczlog.github.io/catlaser/pl/) and [/en/](https://toniaczlog.github.io/catlaser/en/) routes, an eight-effect browser demo, color/glow/trail controls, four motion previews, mobile sticky download, privacy, release notes and an [SVG press kit](docs/press/).

**App screenshots:** the website has an explicitly marked pending section; real screenshots were not supplied. Add verified Windows 0.3 captures to `docs/screenshots/` and replace that placeholder. Do not pass off generated mockups as real captures.

To avoid unnecessary builds: docs changes trigger the small site-check workflow but not the EXE build. A joint 0.3 code update triggers one Windows Release CI and one Pages deploy.

### Adaptive performance (0.3 maintenance)

In manual Real/Neon/Comet mode, CatLaser reduces its dispatcher work to 12 FPS after the cursor and trail are still, then returns to the selected frame rate when motion resumes. The **Adaptive power** toggle in Performance Center enables/disables this behavior. The displayed millisecond value is WPF drawing submission cost, not a GPU benchmark.
