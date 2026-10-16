# CatLaser 🔴

**Turn your Windows mouse cursor into a laser pointer.** A tiny, free, native Windows app with realistic red glow, light trails, three styles, and a cat mode that moves the spot on its own.

[![Windows build](https://github.com/toniaczlog/catlaser/actions/workflows/windows-build.yml/badge.svg)](https://github.com/toniaczlog/catlaser/actions/workflows/windows-build.yml)

## Download

**[Download latest Windows x64 portable build](https://github.com/toniaczlog/catlaser/releases/tag/nightly)** (from the automated build workflow).

Download `CatLaser-win-x64.zip`, extract it and run `CatLaser.exe`. The app requires Windows 10/11 (x64); no separate .NET installation is required. The binary is **not code-signed**, so Windows SmartScreen might ask you to confirm the download.

## Features

- Real / Neon / Cyber laser styles
- Adjustable glow, laser size and fading trail
- Click-through glow overlay spanning all connected monitors
- Replaces standard Windows cursor shapes with a **visible** red laser dot while manual mode is on
- Cat Mode: random, bounded motion; auto-stops after five minutes
- Toggle with **Ctrl+Alt+L**; use the tray icon to reopen or quit
- No account, telemetry, ads, network access, or paid APIs

## How it works

CatLaser uses C#/.NET 10 and WPF transparent per-monitor windows for the animated effect. The pointer itself is made into a small red dot with the Windows `SetSystemCursor` API, then restored from the current Windows cursor scheme with `SPI_SETCURSORS` when disabled or upon normal exit.

**Important:** native cursor replacement affects the whole Windows desktop, not only this app. If CatLaser closes unexpectedly, the red pointer might remain. It is intentionally **never made invisible**. Start CatLaser again to restore the cursor scheme, or run `CatLaser.exe --restore-cursors`. Restarting Windows or reapplying your preferred cursor theme in Windows Settings also restores it. Custom cursors changed while CatLaser is running may be reset on exit.

Cat Mode does **not** move the physical mouse pointer. It animates a virtual laser dot, leaving mouse interaction usable.

## Build from source

```powershell
dotnet publish CatLaser/CatLaser.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist
```

Requirements: Windows 10/11 and .NET 10 SDK with Windows Desktop tooling. CI builds on GitHub Actions (Windows runner).

## Cat safety

Supervise your cat near the monitor to avoid scratches or damage. Keep play sessions short (auto mode has a five-minute limit) and finish with a physical toy or treat that the cat can catch. This software does not emit a real laser beam.

## License

MIT — see [LICENSE](LICENSE).
