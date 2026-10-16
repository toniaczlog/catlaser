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

- 2026-10-16: Publish CatLaser marketing page via GitHub Pages using `main /docs` static source, no Vercel/Netlify or runtime dependencies.
- 2026-10-16: PL/EN language selector for website uses browser language on first visit, with a localStorage override; this is separate from Windows app language detection.
- 2026-10-16: Suppress Windows CI builds for website-only changes to conserve build minutes and avoid accidental duplicate application releases.

- 2026-10-16: CatLaser 0.3 adds bounded monitor play region and reusable Playlists; no moving OS cursor, no unbounded runtime.
- 2026-10-16: Fullscreen-pausing is limited to foreground borderless windows to avoid pausing during ordinary maximized browser windows; must test in live Windows.
- 2026-10-16: Custom shortcut registration rolls back when any combination conflicts; persist only valid set.
- 2026-10-16: Landing expands without frameworks or remote tracking. Separate `/pl/`, `/en/` static, indexable pages; press graphics are vector SVG.
- 2026-10-16: Do not fabricate app screenshots. Landing explicitly labels this user-verified capture as pending.
