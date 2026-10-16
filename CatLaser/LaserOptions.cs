using System;
using System.Windows;

namespace CatLaser;

public enum LaserStyle { Real, Neon, Cyber }

public sealed class LaserOptions
{
    public LaserStyle Style { get; set; } = LaserStyle.Real;
    public double Glow { get; set; } = 70;
    public double Size { get; set; } = 45;
    public double TrailLength { get; set; } = 55;
    public bool TrailEnabled { get; set; } = true;
    public bool CatMode { get; set; }
}

internal readonly record struct TrailSample(Point Point, DateTime Timestamp);
