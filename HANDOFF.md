# CatLaser · Handoff (v0.2)

## Repo and runtime
- `toniaczlog/catlaser`, main, public, MIT.
- Native WPF/.NET 10 Windows x64 app, self-contained portable zip through GitHub Actions.
- No Vercel, Netlify, Coolify, Supabase, or secrets.
- Existing global mouse pointer replacement must always restore on exit/start/disable.

## v0.2 architecture
- `LaserController`: dispatcher timer, fixed FPS caps, interaction, sessions, rest, tray mode and transitions.
- `LaserOverlay`: one drawing surface per monitor instead of many individually moving WPF elements; suppress invalidations on inactive monitors.
- `MotionEngine`: six play behaviors; physical mouse unaffected in automated play.
- `LaserOptions`: visual style, color, intensity, mode, runtime performance and session settings.
- `PresetStore`: 25 local JSON presets, sanitize on load, no telemetry.
- `HotkeyWindow`: L/E/C/M/Up/Down global shortcuts with Ctrl+Alt.
- `SettingsWindow`: controls, session settings and measured FPS counter.

## CI / release
- GitHub Actions uses windows-latest, .NET 10; builds `CatLaser-0.2-win-x64.zip`.
- Publishes `v0.2.0` as prerelease once build succeeds. This does not constitute GUI smoke testing.

## Verification
- Inspect exact SHA and CI logs after push.
- Smoke test on Windows: manual cursor restore, click-through, overlays, all styles, FPS, rest/automatic stop, multi-monitor and mixed DPI, preset reload, hotkeys, native recovery.

## v0.2.1 localization and UI
- `UiText` contains PL/EN translations and checks Windows `CurrentUICulture` on first launch.
- Overrides persist at `%LOCALAPPDATA%\\CatLaser\\language.txt`, distinct from laser presets.
- Rebuilt `SettingsWindow`: compact fixed status header and six navigable tabs; language switch rebuilds only window controls, not laser overlay.
- `Program.cs`: tray menu titles and tooltip follow current language.
- `tests/LocalizationSmoke`: no-NuGet assertions for pl-PL default, override, translated strings and change event.
- Release `v0.2.1` remains prerelease until a human tests Polish Windows UI and manual switching.
