# CatLaser 0.3.0

**A desktop laser pointer, made for curious cats.** Free, open-source Windows 10/11 (x64) application with interactive light effects and optional automatic play modes. Built with native **C# / .NET 10 / WPF / Win32**.

[![Windows build](https://github.com/toniaczlog/catlaser/actions/workflows/windows-build.yml/badge.svg)](https://github.com/toniaczlog/catlaser/actions/workflows/windows-build.yml)
[![Website tests](https://github.com/toniaczlog/catlaser/actions/workflows/site-check.yml/badge.svg)](https://github.com/toniaczlog/catlaser/actions/workflows/site-check.yml)

**[Website](https://toniaczlog.github.io/catlaser/)** · **[Polski](https://toniaczlog.github.io/catlaser/pl/)** · **[English](https://toniaczlog.github.io/catlaser/en/)** · **[Download Windows v0.3.0](https://github.com/toniaczlog/catlaser/releases/download/v0.3.0/CatLaser-0.3-win-x64.zip)**

## Download / Pobierz

1. Download [CatLaser-0.3-win-x64.zip](https://github.com/toniaczlog/catlaser/releases/download/v0.3.0/CatLaser-0.3-win-x64.zip) (~71.5 MB).
2. Unzip and launch **CatLaser.exe**. No installer or separately installed .NET runtime required.
3. Choose a light effect or automatic game. Use the Windows tray icon to change settings or exit.

**Polski:** Pobierz ZIP, rozpakuj, uruchom CatLaser.exe. Na polskim Windows interfejs domyślnie jest po polsku; przełącznik **PL / EN** zapisuje Twój wybór.

**Release status:** 0.3.0 is a **prerelease**, not yet approved through full hands-on Windows GUI testing. The executable is **unsigned**; SmartScreen can warn. Windows CI compiled and packaged this release successfully.

## Features

### Light effects
- 12 styles: **Real, Neon, Cyber, Photoreal, Comet, Electric, Prism, Plasma, HaloPulse, TwinSpot, LightTunnel, SoftStar**.
- Six base colors, adjustable size, glow, trail and subtle organic jitter.
- Optional shockwave on click or sudden stop. New effects avoid intentional high-contrast strobing.

### Play modes
- **Manual** plus eight automatic modes: **Ricochet, SmartPrey, MouseHunt, HideSeek, CatchMe, Spiral, FigureEight, Orbit**.
- Pick a monitor and a percentage-based safe play rectangle, then adjust speed/orbit radius.
- Sessions of 30 s, 2 min or 5 min with configurable rests.
- Named **Laser Playlists**: up to 15 sequences with up to 20 steps. A Manual step in a playlist deliberately acts as a *dark rest*.
- Automated modes only animate a virtual light: they **never move the real mouse cursor**.

### Performance and controls
- One lightweight click-through WPF drawing surface per monitor.
- **Eco (~30 FPS), Balanced (~60 FPS), Ultra (~120 FPS)**, or custom frame caps at 30/60/120/144 FPS.
- **AdaptivePower**: after inactivity in Manual Real/Neon/Comet, the dispatcher slows toward 12 FPS and skips unnecessary redraws; moving the pointer restores the selected speed. Actual CPU/GPU savings still require benchmarking.
- Best-effort pause when a borderless fullscreen foreground app is detected.
- Expanded tray menus for modes, colors, speed, saved presets and playlists.
- Six customizable global hotkeys with duplicate/conflict checking and rollback.

| Default shortcut | Action |
|---|---|
| Ctrl + Alt + L | Toggle laser |
| Ctrl + Alt + E | Next effect |
| Ctrl + Alt + C | Next color |
| Ctrl + Alt + M | Next play mode |
| Ctrl + Alt + Up | Faster |
| Ctrl + Alt + Down | Slower |

### PL/EN and privacy
- App UI language follows the Windows display language: Polish for PL, English otherwise. Manual selection takes priority thereafter.
- Settings are local. **No account, ads, subscriptions, cloud synchronization or telemetry.**
- User data is under **%LOCALAPPDATA%/CatLaser/**: language.txt, presets.json, playlists.json and hotkeys.json.

## Safety and cursor recovery

**Important:** Manual mode replaces the **system-wide Windows cursor** with a *visible* laser-style cursor. CatLaser restores the Windows cursor scheme on startup, disable and normal exit, including appropriate fullscreen pauses.

If a crash leaves the pointer changed, restart and exit CatLaser normally, or run from the executable directory:

```powershell
.\CatLaser.exe --restore-cursors
```

You can also reapply the original pointer scheme in Windows Mouse settings. **Supervise cats near the screen**, use short sessions and finish with a physical toy they can catch. CatLaser displays light on a monitor — it does **not** emit a physical laser.

## Website (GitHub Pages)

Static bilingual landing published at **https://toniaczlog.github.io/catlaser/** from **main → /docs**. No Vercel, backend, hosting bill, client trackers or JavaScript build framework.

- Switchable PL/EN with indexable **/pl/** and **/en/** routes.
- Browser laser demo with **eight effects**, color/glow/trail controls and tracking eyes; four mini movement previews.
- Mobile sticky download CTA, FAQ, privacy/safety, changelog and download instructions.
- **[Real Windows settings screenshot](docs/screenshots/settings-0.3.png)** produced from the actual WPF app in Windows CI; not an AI-generated mockup. This only shows the settings window, not live laser effects.
- [Press and social SVG assets](docs/press/): laser mark, wordmark, vertical Shorts graphic and horizontal banner.
- A custom domain has **not** been purchased or configured for this project.

## Build and validation

On Windows with .NET SDK 10:

```powershell
dotnet run --project tests/LocalizationSmoke/LocalizationSmoke.csproj -c Release
dotnet run --project tests/ConfigSmoke/ConfigSmoke.csproj -c Release
dotnet publish CatLaser/CatLaser.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist
```

Optional authentic WPF settings screenshot from the published executable:

```powershell
.\dist\CatLaser.exe --capture-settings settings.png
```

Website validation (Node.js, no npm install required):

```bash
node --check docs/main.js
node scripts/check-site.mjs
```

**CI separation:** changes under CatLaser/ or tests/ trigger Windows build/release; docs/ and website tests trigger website validation; GitHub Pages publishes static site. Markdown documentation-only commits do not intentionally produce a new Windows release.

**Verified 2026-10-16:** Windows CI and its localization/config smoke tests succeeded; website CI and GitHub Pages deployment succeeded. Full **hands-on Windows 0.3 GUI testing has NOT been completed** (multi-monitor mixed DPI, cursor recovery, global keys, fullscreen suspension, playlists, FPS).

## Project documentation

- [HANDOFF.md](HANDOFF.md) — current status, release provenance and technical handoff.
- [DECISIONS.md](DECISIONS.md) — product and architectural decisions.
- [NEXT.md](NEXT.md) — manual QA checklist and optional future work.

## License

[MIT](LICENSE).
