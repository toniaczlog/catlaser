# Decisions

- 2026-10-16: Use native C#/.NET 10 WPF over web/Electron to support click-through desktop rendering.
- 2026-10-16: Keep a visible Windows cursor replacement instead of hiding the cursor, even on crash.
- 2026-10-16: Automated modes animate a virtual dot, never move real OS pointer.
- 2026-10-16: Cat games automatically stop after limited active time, with configurable rests.
- 2026-10-16: v0.2 is a single bundled feature release; no Vercel deployments.
- 2026-10-16: Replace per-shape render updates with a single custom WPF drawing surface per monitor, adaptive complexity and selectable FPS caps.
- 2026-10-16: User presets stored as local JSON, never synced; limited to 25 to avoid unbounded data.
- 2026-10-16: Distribute `v0.2.0` prerelease with Windows CI compilation; GUI smoke test remains a manual gate for a stable release.

- 2026-10-16: v0.2.1 selects Polish automatically on Polish Windows UI culture, otherwise English. Explicit PL/EN choice overrides system locale on subsequent launches and is persisted separately from presets.
- 2026-10-16: Keep the stable 0.2 animation engine untouched; redesign settings into six sections with static preview and tray translations, minimizing performance risk.
