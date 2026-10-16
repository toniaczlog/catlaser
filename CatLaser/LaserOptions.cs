using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CatLaser;

public enum LaserStyle { Real, Neon, Cyber, Photoreal, Comet, Electric, Prism, Plasma, HaloPulse, TwinSpot, LightTunnel, SoftStar }
public enum PlayMode { Manual, Ricochet, SmartPrey, MouseHunt, HideSeek, CatchMe, Spiral, FigureEight, Orbit }
public enum PerformanceProfile { Eco, Balanced, Ultra }

public sealed class LaserOptions
{
    public LaserStyle Style { get; set; } = LaserStyle.Photoreal;
    public PlayMode Mode { get; set; } = PlayMode.Manual;
    public PerformanceProfile Performance { get; set; } = PerformanceProfile.Balanced;
    public double Glow { get; set; } = 65;
    public double Size { get; set; } = 45;
    public double TrailLength { get; set; } = 45;
    public double Speed { get; set; } = 50;
    public double Jitter { get; set; } = 0;
    public bool TrailEnabled { get; set; } = true;
    public bool ShockwaveEnabled { get; set; } = true;
    public int ColorIndex { get; set; } = 0;
    public int SessionSeconds { get; set; } = 120;
    public int RestEverySeconds { get; set; } = 30;
    public int RestSeconds { get; set; } = 8;
    public int? CustomFps { get; set; }
    public string MonitorDeviceName { get; set; } = "";
    public double AreaLeft { get; set; } = 5;
    public double AreaTop { get; set; } = 5;
    public double AreaWidth { get; set; } = 90;
    public double AreaHeight { get; set; } = 90;
    public bool PauseOnFullscreen { get; set; } = true;
    public double OrbitRadius { get; set; } = 30;

    public int TargetFps => Math.Clamp(CustomFps ?? (Performance switch
    {
        PerformanceProfile.Eco => 30,
        PerformanceProfile.Ultra => 120,
        _ => 60
    }), 15, 144);

    public Color PrimaryColor => ColorIndex switch
    {
        1 => Color.FromRgb(40, 255, 102),
        2 => Color.FromRgb(49, 150, 255),
        3 => Color.FromRgb(193, 99, 255),
        4 => Color.FromRgb(255, 184, 45),
        5 => Color.FromRgb(41, 231, 235),
        _ => Color.FromRgb(255, 28, 48)
    };

    public LaserOptions Clone() => (LaserOptions)MemberwiseClone();

    public void Sanitize()
    {
        if (!Enum.IsDefined(Style)) Style = LaserStyle.Photoreal;
        if (!Enum.IsDefined(Mode)) Mode = PlayMode.Manual;
        if (!Enum.IsDefined(Performance)) Performance = PerformanceProfile.Balanced;
        Glow = Safe(Glow, 65); Size = Safe(Size, 45);
        TrailLength = Safe(TrailLength, 45); Speed = Safe(Speed, 50);
        Jitter = Safe(Jitter, 0);
        ColorIndex = Math.Clamp(ColorIndex, 0, 5);
        MonitorDeviceName ??= "";
        if (MonitorDeviceName.Length > 80) MonitorDeviceName = "";
        AreaLeft = Safe(AreaLeft,5); AreaTop = Safe(AreaTop,5);
        AreaWidth = Math.Clamp(double.IsFinite(AreaWidth)?AreaWidth:90, 5, Math.Max(5,100-AreaLeft));
        AreaHeight = Math.Clamp(double.IsFinite(AreaHeight)?AreaHeight:90, 5, Math.Max(5,100-AreaTop));
        OrbitRadius = Safe(OrbitRadius,30);
        SessionSeconds = SessionSeconds is 30 or 120 or 300 ? SessionSeconds : 120;
        RestEverySeconds = Math.Clamp(RestEverySeconds, 10, 120);
        RestSeconds = Math.Clamp(RestSeconds, 0, 40);
        if (CustomFps is not null) CustomFps = Math.Clamp(CustomFps.Value, 15, 144);
    }

    private static double Safe(double v, double defaultValue) =>
        double.IsFinite(v) ? Math.Clamp(v, 0, 100) : defaultValue;
}

internal readonly record struct TrailSample(Point Point, DateTime Timestamp);
internal readonly record struct Shockwave(Point Point, DateTime Timestamp, bool Catch = false);

internal sealed class LaserFrame
{
    public required Point Position { get; init; }
    public required IReadOnlyList<TrailSample> Trail { get; init; }
    public required IReadOnlyList<Shockwave> Shockwaves { get; init; }
    public required LaserOptions Options { get; init; }
    public required DateTime Now { get; init; }
    public required double Seconds { get; init; }
    public required bool Hidden { get; init; }
}
