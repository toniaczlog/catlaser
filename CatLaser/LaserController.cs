using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using Forms = System.Windows.Forms;

namespace CatLaser;

internal sealed class LaserController : IDisposable
{
    private readonly DispatcherTimer _timer = new(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(16) };
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly List<LaserOverlay> _overlays = new();
    private readonly List<TrailSample> _trail = new();
    private Point _autoPoint;
    private Point _autoTarget;
    private DateTime _autoStarted;
    private DateTime _restUntil = DateTime.MinValue;
    private double _lastSeconds;
    private bool _cursorApplied;
    private bool _disposed;

    public LaserOptions Options { get; } = new();
    public bool Enabled { get; private set; }
    public bool CursorApplied => _cursorApplied;
    public event Action? Changed;

    public void Start()
    {
        RebuildOverlays();
        SystemEvents.DisplaySettingsChanged += DisplayChanged;
        _timer.Tick += Tick;
        _timer.Start();
        SetEnabled(true);
    }

    private void DisplayChanged(object? sender, EventArgs e) => RebuildOverlays();

    private void RebuildOverlays()
    {
        foreach (var overlay in _overlays) overlay.Close();
        _overlays.Clear();
        foreach (var screen in Forms.Screen.AllScreens)
        {
            var overlay = new LaserOverlay(screen);
            _overlays.Add(overlay);
            if (Enabled) overlay.Show();
        }
    }

    public void Toggle() => SetEnabled(!Enabled);

    public void SetEnabled(bool enabled)
    {
        if (Enabled == enabled) return;
        Enabled = enabled;
        if (enabled)
        {
            foreach (var overlay in _overlays) overlay.Show();
        }
        else
        {
            foreach (var overlay in _overlays) overlay.Hide();
            _trail.Clear();
        }
        UpdateNativeCursor();
        Changed?.Invoke();
    }

    public void SetCatMode(bool enabled)
    {
        if (Options.CatMode == enabled) return;
        Options.CatMode = enabled;
        if (enabled)
        {
            var mouse = Forms.Cursor.Position;
            _autoPoint = new Point(mouse.X, mouse.Y);
            _autoTarget = _autoPoint;
            _autoStarted = DateTime.UtcNow;
            _restUntil = DateTime.MinValue;
        }
        _trail.Clear();
        UpdateNativeCursor();
        Changed?.Invoke();
    }

    public void RefreshSettings() => Changed?.Invoke();

    private void UpdateNativeCursor()
    {
        if (_cursorApplied)
        {
            NativeCursor.Restore();
            _cursorApplied = false;
        }
        if (Enabled && !Options.CatMode) _cursorApplied = NativeCursor.Apply();
    }

    private void Tick(object? sender, EventArgs e)
    {
        if (!Enabled || _disposed) return;
        var now = DateTime.UtcNow;
        double seconds = _clock.Elapsed.TotalSeconds;
        double dt = Math.Clamp(seconds - _lastSeconds, 0, .08);
        _lastSeconds = seconds;

        Point location;
        if (Options.CatMode)
        {
            // Five-minute sessions avoid leaving a cat chasing a screen indefinitely.
            if ((now - _autoStarted).TotalMinutes >= 5)
            {
                SetCatMode(false);
                return;
            }
            location = MoveAuto(now, dt);
        }
        else
        {
            var mouse = Forms.Cursor.Position;
            location = new Point(mouse.X, mouse.Y);
        }

        if (_trail.Count == 0 || (location - _trail[^1].Point).Length > 1)
            _trail.Add(new TrailSample(location, now));

        double maxAge = .08 + Options.TrailLength / 160.0;
        while (_trail.Count > 0 && (now - _trail[0].Timestamp).TotalSeconds > maxAge)
            _trail.RemoveAt(0);
        if (_trail.Count > 50) _trail.RemoveRange(0, _trail.Count - 50);

        foreach (var overlay in _overlays)
            overlay.Draw(location, _trail, Options, seconds, now);
    }

    private Point MoveAuto(DateTime now, double dt)
    {
        var screen = Forms.Screen.FromPoint(new System.Drawing.Point((int)_autoPoint.X, (int)_autoPoint.Y));
        var bounds = screen.Bounds;
        if ((now < _restUntil)) return _autoPoint;

        Vector toTarget = _autoTarget - _autoPoint;
        if (toTarget.Length < 14)
        {
            if (Random.Shared.NextDouble() < .20)
                _restUntil = now.AddMilliseconds(120 + Random.Shared.Next(500));

            int marginX = Math.Min(bounds.Width / 5, 160);
            int marginY = Math.Min(bounds.Height / 5, 150);
            _autoTarget = new Point(
                Random.Shared.Next(bounds.Left + marginX, Math.Max(bounds.Left + marginX + 1, bounds.Right - marginX)),
                Random.Shared.Next(bounds.Top + marginY, Math.Max(bounds.Top + marginY + 1, bounds.Bottom - marginY)));
            toTarget = _autoTarget - _autoPoint;
        }
        double speed = 320 + 300 * (1 + Math.Sin(_clock.Elapsed.TotalSeconds * 2.2));
        double step = Math.Min(toTarget.Length, speed * dt);
        if (toTarget.Length > 0) _autoPoint += toTarget / toTarget.Length * step;
        return _autoPoint;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        SystemEvents.DisplaySettingsChanged -= DisplayChanged;
        _timer.Stop();
        _timer.Tick -= Tick;
        foreach (var overlay in _overlays) overlay.Close();
        _overlays.Clear();
        NativeCursor.Restore();
        _cursorApplied = false;
    }
}
