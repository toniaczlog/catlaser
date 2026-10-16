# Decisions

- 2026-10-16: Choose native C#/.NET 10 WPF instead of web/Electron to produce a click-through desktop overlay without hosting.
- 2026-10-16: Use visible replacement cursors instead of hiding the Windows cursor, so an unexpected exit cannot leave an invisible pointer.
- 2026-10-16: Cat Mode does not control the real mouse and ends automatically after five minutes.
- 2026-10-16: Build in GitHub Actions, one portable Windows x64 archive under nightly prerelease; avoid Vercel deployments and third-party dependencies.
