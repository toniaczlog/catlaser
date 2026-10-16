# HANDOFF — CatLaser

## Current scope
Windows 10/11 x64 portable laser pointer MVP. .NET 10 WPF + Win32, no external dependencies.
- Native visible red dot replaces system pointers in manual mode.
- WPF click-through overlay renders Real / Neon / Cyber with glow/trail on all monitors.
- Cat Mode is a virtual moving light, limited to five minutes.
- Tray, Ctrl+Alt+L hotkey, recovery command, CI downloadable nightly ZIP.

## Deployment
GitHub Actions on main builds self-contained exe, attaches ZIP artifact, and creates/updates the prerelease tag `nightly`. No Vercel/Netlify/Coolify integration.

## Verification
Windows compilation and interactive smoke test must be confirmed on GitHub Actions and Windows. Native cursor restoration and mixed-DPI screens warrant manual testing.
