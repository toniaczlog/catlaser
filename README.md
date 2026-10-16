# CatLaser 0.2 · Laser Playground 🔴

**Turn your mouse into a desktop laser.** Free, local, open-source Windows 10/11 software, built with .NET 10 + WPF + Win32. No accounts, ads or API keys.

[![Windows build](https://github.com/toniaczlog/catlaser/actions/workflows/windows-build.yml/badge.svg)](https://github.com/toniaczlog/catlaser/actions/workflows/windows-build.yml)

## Download

**[CatLaser v0.2.0 — Windows x64 ZIP](https://github.com/toniaczlog/catlaser/releases/tag/v0.2.0)**

Download `CatLaser-0.2-win-x64.zip`, extract the archive and start `CatLaser.exe`. Self-contained portable application; no separate .NET installation is required. The executable is not code-signed; Windows SmartScreen may warn. The release is marked **prerelease** until an interactive Windows test is performed.

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
