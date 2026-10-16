using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using Forms = System.Windows.Forms;
using Point = System.Windows.Point;

namespace CatLaser;

internal sealed class LaserController : IDisposable
{
    private readonly DispatcherTimer _timer = new(DispatcherPriority.Render);
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly List<LaserOverlay> _overlays = new();
    private readonly List<TrailSample> _trail = new();
    private readonly List<Shockwave> _waves = new();
    private readonly MotionEngine _motion = new();
    private double _lastSeconds;
    private double _activeSeconds;
    private double _lastBreakAt;
    private DateTime _breakUntil;
    private Point _previous;
    private bool _previousValid;
    private bool _wasFast;
    private bool _mouseDown;
    private bool _cursorApplied;
    private bool _disposed;
    private double _fpsMeasureStart;
    private int _frames;
    private int _actualFps;
    private PlaySequence? _playlist;
    private int _playlistIndex;
    private double _playlistElapsed;
    private bool _suspended;
    private double _lastFullscreenCheck;
    private double _stillSeconds;
    private int _configuredFps;
    public double LastSubmissionMilliseconds { get; private set; }
    public bool Suspended => _suspended;
    public string? PlaylistName => _playlist?.Name;

    public LaserOptions Options { get; private set; } = new();
    public bool Enabled { get; private set; }
    public bool CursorApplied => _cursorApplied;
    public int ActualFps => _actualFps;
    public bool IsResting => Enabled && Options.Mode != PlayMode.Manual && DateTime.UtcNow < _breakUntil;
    public int SessionRemaining => Math.Max(0, Options.SessionSeconds - (int)_activeSeconds);
    public event Action? Changed;

    public void Start()
    {
        RebuildOverlays();
        SystemEvents.DisplaySettingsChanged += DisplayChanged;
        _timer.Tick += Tick;
        ConfigureFrameRate();
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
            if (Enabled && !_suspended) overlay.Show();
        }
    }

    public void Toggle() => SetEnabled(!Enabled);

    public void StartPlaylist(PlaySequence playlist)
    {
        if(playlist.Steps.Count==0)return;
        _playlist=new PlaySequence {Name=playlist.Name,
            Steps=playlist.Steps.Select(s=>new PlayStep{Mode=s.Mode,Seconds=s.Seconds}).ToList()};
        _playlistIndex=0;_playlistElapsed=0;
        Options.Mode=_playlist.Steps[0].Mode;
        BeginSession();
        if(!Enabled)SetEnabled(true);
        Changed?.Invoke();
    }

    public void StopPlaylist()
    {
        if(_playlist is null)return;
        _playlist=null;
        Options.Mode=PlayMode.Manual;
        BeginSession();
        UpdateNativeCursor();
        Changed?.Invoke();
    }

    public void SetMonitor(string deviceName)
    {
        Options.MonitorDeviceName=deviceName;
        Options.Sanitize();
        BeginSession();
        Changed?.Invoke();
    }

    public void ResetPlayArea(){BeginSession();Changed?.Invoke();}

    public void SetEnabled(bool enabled)
    {
        if (_disposed || Enabled == enabled) return;
        Enabled = enabled;
        if (enabled)
        {
            BeginSession();
            foreach (var overlay in _overlays) { if (!_suspended) overlay.Show(); }
        }
        else
        {
            foreach (var overlay in _overlays) overlay.Hide();
            _trail.Clear(); _waves.Clear();
            _playlist=null;_suspended=false;
        }
        UpdateNativeCursor();
        Changed?.Invoke();
    }

    public void SetMode(PlayMode mode)
    {
        if (Options.Mode == mode) return;
        _playlist=null;
        Options.Mode = mode;
        BeginSession();
        UpdateNativeCursor();
        Changed?.Invoke();
    }

    public void ChangeStyle(LaserStyle style)
    {
        Options.Style = style; Changed?.Invoke();
    }

    public void CycleStyle()
    {
        var styles = Enum.GetValues<LaserStyle>();
        ChangeStyle(styles[(Array.IndexOf(styles, Options.Style) + 1) % styles.Length]);
    }

    public void SetColor(int index)
    {
        index = Math.Clamp(index, 0, 5);
        if (Options.ColorIndex == index) return;
        Options.ColorIndex = index;
        UpdateNativeCursor();
        Changed?.Invoke();
    }

    public void CycleColor()
    {
        Options.ColorIndex = (Options.ColorIndex + 1) % 6;
        UpdateNativeCursor();
        Changed?.Invoke();
    }

    public void CycleMode()
    {
        var modes = Enum.GetValues<PlayMode>();
        SetMode(modes[(Array.IndexOf(modes, Options.Mode) + 1) % modes.Length]);
    }

    public void AdjustSpeed(double difference)
    {
        Options.Speed = Math.Clamp(Options.Speed + difference, 0, 100);
        Changed?.Invoke();
    }

    public void ApplyPreset(LaserOptions options)
    {
        Options = options.Clone();
        Options.Sanitize();
        BeginSession();
        ConfigureFrameRate();
        UpdateNativeCursor();
        Changed?.Invoke();
    }

    public void RefreshSettings()
    {
        Options.Sanitize();
        ConfigureFrameRate();
        Changed?.Invoke();
    }

    private void ConfigureFrameRate()
    {
        _configuredFps = Options.TargetFps;
        _timer.Interval = TimeSpan.FromMilliseconds(1000.0 / _configuredFps);
    }

    private void UpdateDynamicRate(bool lowPower)
    {
        int fps = lowPower && Options.AdaptivePower ? 12 : Options.TargetFps;
        if (_configuredFps == fps) return;
        _configuredFps = fps;
        _timer.Interval = TimeSpan.FromMilliseconds(1000.0 / fps);
    }

    private void BeginSession()
    {
        _activeSeconds = 0;
        _lastBreakAt = 0;
        _breakUntil = DateTime.MinValue;
        _trail.Clear(); _waves.Clear();
        _previousValid = false; _wasFast = false;
        _stillSeconds = 0;
        UpdateDynamicRate(false);
        _mouseDown = false;
        var p = Forms.Cursor.Position;
        _motion.Reset(Options.Mode, new Point(p.X, p.Y), Options);
        _lastSeconds = _clock.Elapsed.TotalSeconds;
    }

    private void UpdateNativeCursor()
    {
        if (_cursorApplied)
        {
            NativeCursor.Restore();
            _cursorApplied = false;
        }
        if (Enabled && !_suspended && _playlist is null && Options.Mode == PlayMode.Manual)
            _cursorApplied = NativeCursor.Apply(Options.ColorIndex);
    }

    private void Tick(object? sender, EventArgs e)
    {
        if (_disposed || !Enabled) return;
        DateTime now = DateTime.UtcNow;
        double seconds = _clock.Elapsed.TotalSeconds;
        double dt = Math.Clamp(seconds - _lastSeconds, 0, .075);
        _lastSeconds = seconds;

        if(seconds-_lastFullscreenCheck>.5)
        {
            _lastFullscreenCheck=seconds;
            bool suspend=Options.PauseOnFullscreen&&FullscreenGuard.IsFullscreen();
            if(suspend!=_suspended)
            {
                _suspended=suspend;
                if(_suspended)
                {
                    foreach(var overlay in _overlays)overlay.Hide();
                    _trail.Clear();_waves.Clear();
                }
                else foreach(var overlay in _overlays)overlay.Show();
                UpdateNativeCursor();
                Changed?.Invoke();
            }
        }
        if(_suspended)
        {
            UpdateDynamicRate(true);
            return;
        }

        if(_playlist is not null)
        {
            var step=_playlist.Steps[_playlistIndex];
            _playlistElapsed+=dt;
            if(_playlistElapsed>=step.Seconds)
            {
                _playlistIndex++;
                _playlistElapsed=0;
                if(_playlistIndex>=_playlist.Steps.Count)
                {
                    _playlist=null;
                    SetEnabled(false);
                    return;
                }
                Options.Mode=_playlist.Steps[_playlistIndex].Mode;
                var p=Forms.Cursor.Position;
                _motion.Reset(Options.Mode,new Point(p.X,p.Y),Options);
                _trail.Clear();
                Changed?.Invoke();
            }
            if(Options.Mode==PlayMode.Manual)
            {
                _trail.Clear();_waves.Clear();Draw(null);
                return; // Manual entries in playlists are intentionally dark rest breaks.
            }
        }

        _frames++;
        if (seconds - _fpsMeasureStart >= 1)
        {
            _actualFps = (int)Math.Round(_frames / Math.Max(.001, seconds - _fpsMeasureStart));
            _fpsMeasureStart = seconds; _frames = 0;
            Changed?.Invoke();
        }

        if (Options.Mode != PlayMode.Manual && _playlist is null)
        {
            if (now < _breakUntil)
            {
                _trail.Clear();
                Draw(null);
                return;
            }

            _activeSeconds += dt;
            if (_activeSeconds >= Options.SessionSeconds)
            {
                SetEnabled(false); // Automatically restores the user's Windows cursor scheme.
                return;
            }

            if (Options.RestSeconds > 0 && _activeSeconds - _lastBreakAt >= Options.RestEverySeconds)
            {
                _lastBreakAt = _activeSeconds;
                _breakUntil = now.AddSeconds(Options.RestSeconds);
                _trail.Clear();
                Draw(null);
                return;
            }
        }

        Point point;
        if (Options.Mode == PlayMode.Manual)
        {
            var mouse = Forms.Cursor.Position;
            point = new Point(mouse.X, mouse.Y);
        }
        else point = _motion.Step(Options, dt, now);

        if (Options.Jitter > 0)
        {
            double amount = Options.Jitter / 25;
            point.Offset(Math.Sin(seconds * 32.3) * amount, Math.Cos(seconds * 29.1) * amount);
        }

        bool down = (GetAsyncKeyState(0x01) & 0x8000) != 0;
        bool clicked = down && !_mouseDown;
        _mouseDown = down;
        if (clicked && Options.ShockwaveEnabled) _waves.Add(new Shockwave(point, now));

        if (Options.Mode == PlayMode.CatchMe && clicked)
        {
            var mouse = Forms.Cursor.Position;
            if ((new Point(mouse.X, mouse.Y) - point).Length < 70)
            {
                _motion.Catch(now);
                _waves.Add(new Shockwave(point, now, true));
            }
        }

        double movement = _previousValid ? (point - _previous).Length : double.MaxValue;
        if (_previousValid && dt > 0)
        {
            double speed = (point - _previous).Length / dt;
            bool fast = speed > 900;
            if (_wasFast && speed < 55 && Options.ShockwaveEnabled)
                _waves.Add(new Shockwave(point, now));
            _wasFast = fast;
        }
        _previous = point; _previousValid = true;

        if (_trail.Count == 0 || (point - _trail[^1].Point).Length >= 1)
            _trail.Add(new TrailSample(point, now));

        double maxAge = .07 + Options.TrailLength / 120.0;
        while (_trail.Count > 0 && (now - _trail[0].Timestamp).TotalSeconds > maxAge)
            _trail.RemoveAt(0);
        if (_trail.Count > 70) _trail.RemoveRange(0, _trail.Count - 70);

        _waves.RemoveAll(w => (now - w.Timestamp).TotalSeconds > .8);
        if (_waves.Count > 6) _waves.RemoveRange(0, _waves.Count - 6);

        // Static manual styles need no WPF invalidation after their last fading trail.
        // Animated styles always keep their selected FPS to preserve visual effects.
        bool staticStyle = Options.Style is LaserStyle.Real or LaserStyle.Neon or LaserStyle.Comet;
        bool still = _playlist is null && Options.Mode == PlayMode.Manual &&
            movement < 1 && Options.Jitter < .001;
        _stillSeconds = still ? _stillSeconds + dt : 0;
        bool lowPower = Options.AdaptivePower && staticStyle && still && _stillSeconds > maxAge + .3 &&
            _waves.Count == 0 && _trail.Count <= 1;
        UpdateDynamicRate(lowPower);
        if (lowPower) return;

        Draw(new LaserFrame
        {
            Position = point, Trail = _trail, Shockwaves = _waves,
            Options = Options, Now = now, Seconds = seconds,
            Hidden = false
        });
    }

    private void Draw(LaserFrame? frame)
    {
        long started = Stopwatch.GetTimestamp();
        foreach (var overlay in _overlays) overlay.Draw(frame);
        LastSubmissionMilliseconds = (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        SystemEvents.DisplaySettingsChanged -= DisplayChanged;
        _timer.Stop(); _timer.Tick -= Tick;
        foreach (var overlay in _overlays) overlay.Close();
        _overlays.Clear();
        NativeCursor.Restore(); _cursorApplied = false;
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int key);
}
