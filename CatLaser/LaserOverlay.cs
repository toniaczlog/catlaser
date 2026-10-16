using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Forms = System.Windows.Forms;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Pen = System.Windows.Media.Pen;
using Rect = System.Windows.Rect;

namespace CatLaser;

/// <summary>
/// Lightweight click-through layer: ONE custom WPF drawing node per monitor,
/// instead of allocating and updating dozens of controls every animation frame.
/// </summary>
internal sealed class LaserOverlay : Window
{
    private const int GwlExStyle = -20;
    private const long WsExTransparent = 0x20;
    private const long WsExToolWindow = 0x80;
    private const long WsExNoActivate = 0x08000000;
    private const int SwpNoActivate = 0x0010;
    private const int SwpNoZOrder = 0x0004;
    private readonly Forms.Screen _screen;
    private readonly LaserSurface _surface;
    private bool _hadInk;

    public LaserOverlay(Forms.Screen screen)
    {
        _screen = screen;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        ShowActivated = false;
        Focusable = false;
        Topmost = true;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        Width = Math.Max(1, screen.Bounds.Width);
        Height = Math.Max(1, screen.Bounds.Height);
        Left = screen.Bounds.Left;
        Top = screen.Bounds.Top;

        _surface = new LaserSurface(this, screen) { IsHitTestVisible = false };
        Content = _surface;
        SourceInitialized += (_, _) => ConfigureWindow();
    }

    private void ConfigureWindow()
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        long styles = GetWindowLongPtr(handle, GwlExStyle).ToInt64();
        SetWindowLongPtr(handle, GwlExStyle,
            new IntPtr(styles | WsExTransparent | WsExToolWindow | WsExNoActivate));
        var r = _screen.Bounds;
        SetWindowPos(handle, IntPtr.Zero, r.Left, r.Top, r.Width, r.Height,
            SwpNoActivate | SwpNoZOrder);
    }

    public void Draw(LaserFrame? frame)
    {
        bool relevant = frame != null &&
            (_screen.Bounds.Contains((int)frame.Position.X, (int)frame.Position.Y) ||
             HasTrailHere(frame.Trail) || HasWavesHere(frame.Shockwaves) ||
             frame.Options.Mode == PlayMode.HideSeek);
        // Skip invalidating completely empty monitors, except once to clear old content.
        if (!relevant && !_hadInk) return;
        _hadInk = relevant;
        _surface.Update(relevant ? frame : null);
    }

    private bool HasTrailHere(IReadOnlyList<TrailSample> trail)
    {
        for (int i = trail.Count - 1; i >= 0 && i > trail.Count - 55; i--)
            if (_screen.Bounds.Contains((int)trail[i].Point.X, (int)trail[i].Point.Y)) return true;
        return false;
    }

    private bool HasWavesHere(IReadOnlyList<Shockwave> waves)
    {
        foreach (var wave in waves)
            if (_screen.Bounds.Contains((int)wave.Point.X, (int)wave.Point.Y)) return true;
        return false;
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr(IntPtr window, int index);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr window, int index, IntPtr value);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y,
        int width, int height, int flags);

    private sealed class LaserSurface : FrameworkElement
    {
        private readonly LaserOverlay _owner;
        private readonly Forms.Screen _screen;
        private readonly Dictionary<int, Brush> _glowCache = new();
        private LaserFrame? _frame;

        public LaserSurface(LaserOverlay owner, Forms.Screen screen)
        {
            _owner = owner; _screen = screen;
            SnapsToDevicePixels = false;
        }

        public void Update(LaserFrame? frame)
        {
            _frame = frame;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            var frame = _frame;
            if (frame == null) return;
            var options = frame.Options;
            var color = options.PrimaryColor;
            Point center = Local(frame.Position);
            bool dotIsHere = _screen.Bounds.Contains((int)frame.Position.X, (int)frame.Position.Y);

            DrawTrail(dc, frame, color);
            DrawWaves(dc, frame, color);

            if (dotIsHere)
            {
                if (options.Style == LaserStyle.Prism) DrawPrism(dc, center, frame);
                else
                {
                    DrawCore(dc, center, frame, color);
                    if (options.Style == LaserStyle.Electric) DrawElectric(dc, center, frame, color);
                    if (options.Style == LaserStyle.Photoreal) DrawSpeckle(dc, center, frame, color);
                    if (options.Style == LaserStyle.Plasma) DrawPlasma(dc, center, frame, color);
                    if (options.Style is LaserStyle.HaloPulse or LaserStyle.TwinSpot or LaserStyle.LightTunnel or LaserStyle.SoftStar)
                        DrawIllusion(dc,center,frame,color);
                    if (options.Style == LaserStyle.Cyber)
                    {
                        var pen = new Pen(new SolidColorBrush(Color.FromArgb(110, color.R, color.G, color.B)), 1.6);
                        double r = 18 + 10 * (1 + Math.Sin(frame.Seconds * 5));
                        dc.DrawEllipse(null, pen, center, r, r);
                    }
                }
            }
            if (options.Mode == PlayMode.HideSeek) DrawObstacles(dc, frame);
        }

        private void DrawCore(DrawingContext dc, Point p, LaserFrame frame, Color color)
        {
            double baseSize = 16 + frame.Options.Size * .48;
            double styleMultiplier = frame.Options.Style switch
            {
                LaserStyle.Neon => 1.2,
                LaserStyle.Plasma => 1.45,
                LaserStyle.Photoreal => 1.15,
                _ => 1.0
            };
            double radius = baseSize * styleMultiplier;
            dc.PushOpacity(.25 + frame.Options.Glow / 130);
            dc.DrawEllipse(GetGlow(frame.Options.ColorIndex, color), null,
                new Point(p.X - radius * .07, p.Y + radius * .035), radius, radius * .94);
            dc.Pop();

            double dotRadius = (1.6 + frame.Options.Size * .022) *
                (frame.Options.Style == LaserStyle.Plasma ? 1.7 : 1);
            Brush bright = new SolidColorBrush(Color.FromArgb(255, 255, 239, 229));
            dc.DrawEllipse(new SolidColorBrush(color), null, p, dotRadius * 1.8, dotRadius * 1.8);
            dc.DrawEllipse(bright, null, p, dotRadius * .65, dotRadius * .65);
        }

        private Brush GetGlow(int index, Color color)
        {
            if (_glowCache.TryGetValue(index, out var existing)) return existing;
            var gradient = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new(Color.FromArgb(245, color.R, color.G, color.B), 0),
                    new(Color.FromArgb(130, color.R, color.G, color.B), .15),
                    new(Color.FromArgb(55, color.R, color.G, color.B), .42),
                    new(Color.FromArgb(0, color.R, color.G, color.B), 1)
                }
            };
            gradient.Freeze();
            _glowCache[index] = gradient;
            return gradient;
        }

        private void DrawTrail(DrawingContext dc, LaserFrame frame, Color color)
        {
            var options = frame.Options;
            if (!options.TrailEnabled || options.TrailLength <= 0) return;
            var trail = frame.Trail;
            double maxAge = .07 + options.TrailLength / 120.0;
            bool comet = options.Style == LaserStyle.Comet;
            int skip = options.Performance == PerformanceProfile.Eco ? 2 : 1;
            int segments = Math.Min(trail.Count - 1, options.Performance == PerformanceProfile.Ultra ? 60 : 36);
            for (int offset = 0; offset < segments; offset += skip)
            {
                int i = trail.Count - 1 - offset;
                if (i <= 0) break;
                double age = (frame.Now - trail[i].Timestamp).TotalSeconds;
                if (age > maxAge) break;
                double fraction = Math.Clamp(1 - age / maxAge, 0, 1);
                double width = comet ? (1.3 + 9 * fraction * fraction) :
                    options.Style == LaserStyle.Neon ? 3.5 * fraction :
                    options.Style == LaserStyle.Electric ? 2.5 * fraction : 2 * fraction;
                byte alpha = (byte)(Math.Clamp((comet ? 235 : 175) * fraction, 0, 255));
                var pen = new Pen(new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B)), Math.Max(.5, width))
                {
                    StartLineCap = PenLineCap.Round,
                    EndLineCap = PenLineCap.Round,
                    LineJoin = PenLineJoin.Round
                };
                int previous = Math.Max(0, i - skip);
                dc.DrawLine(pen, Local(trail[i].Point), Local(trail[previous].Point));
            }
        }

        private void DrawWaves(DrawingContext dc, LaserFrame frame, Color color)
        {
            foreach (var wave in frame.Shockwaves)
            {
                double age = (frame.Now - wave.Timestamp).TotalSeconds;
                if (age < 0 || age > .8) continue;
                double t = age / .8;
                double radius = (wave.Catch ? 84 : 60) * t + 6;
                byte alpha = (byte)(210 * (1 - t));
                var pen = new Pen(new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B)),
                    Math.Max(.5, (wave.Catch ? 4 : 2.3) * (1 - t)));
                dc.DrawEllipse(null, pen, Local(wave.Point), radius, radius);
            }
        }

        private void DrawSpeckle(DrawingContext dc, Point center, LaserFrame frame, Color color)
        {
            int count = frame.Options.Performance switch
            {
                PerformanceProfile.Eco => 4,
                PerformanceProfile.Ultra => 17,
                _ => 9
            };
            int phase = (int)(frame.Seconds * 11);
            for (int i = 0; i < count; i++)
            {
                double angle = (i * 2.39996 + phase * .06);
                double distance = 3 + (i * 17 % 19);
                double jitter = Math.Sin(i * 19.9 + phase * 7.11) * 1.8;
                var spot = new Point(center.X + Math.Cos(angle) * (distance + jitter),
                    center.Y + Math.Sin(angle) * (distance + jitter));
                dc.DrawEllipse(new SolidColorBrush(Color.FromArgb((byte)(45 + (i * 31 % 100)),
                    color.R, color.G, color.B)), null, spot, .6 + (i % 3) * .4, .6 + (i % 3) * .4);
            }
        }

        private void DrawElectric(DrawingContext dc, Point center, LaserFrame frame, Color color)
        {
            int arms = frame.Options.Performance == PerformanceProfile.Eco ? 2 : 4;
            int steps = frame.Options.Performance == PerformanceProfile.Ultra ? 7 : 4;
            double t = Math.Floor(frame.Seconds * 22);
            for (int a = 0; a < arms; a++)
            {
                double angle = a * Math.PI * 2 / arms + t * .07;
                Point prev = center;
                for (int s = 1; s <= steps; s++)
                {
                    double r = s * (23.0 / steps);
                    double drift = Math.Sin(t * 5.21 + a * 17 + s * 4.43) * 7;
                    Point next = new Point(center.X + Math.Cos(angle) * r - Math.Sin(angle) * drift,
                        center.Y + Math.Sin(angle) * r + Math.Cos(angle) * drift);
                    var pen = new Pen(new SolidColorBrush(Color.FromArgb((byte)(180 - s * 20),
                        color.R, color.G, color.B)), Math.Max(.6, 2 - s * .2));
                    dc.DrawLine(pen, prev, next);
                    prev = next;
                }
                dc.DrawEllipse(Brushes.White, null, prev, 1.4, 1.4);
            }
        }

        private void DrawPrism(DrawingContext dc, Point center, LaserFrame frame)
        {
            double t = frame.Seconds;
            Color[] spectrum =
            {
                frame.Options.PrimaryColor,
                Color.FromRgb(45, 221, 240),
                Color.FromRgb(255, 92, 218)
            };
            for (int i = 0; i < spectrum.Length; i++)
            {
                double angle = i * 2 * Math.PI / 3 + t * 1.8;
                Point p = new Point(center.X + Math.Cos(angle) * 11,
                    center.Y + Math.Sin(angle) * 11);
                dc.PushOpacity(.72);
                dc.DrawEllipse(GetGlow(10 + i, spectrum[i]), null, p, 21, 21);
                dc.Pop();
                dc.DrawEllipse(new SolidColorBrush(spectrum[i]), null, p, 3.1, 3.1);
            }
            dc.DrawEllipse(Brushes.White, null, center, 2.1, 2.1);
        }

        private void DrawPlasma(DrawingContext dc, Point center, LaserFrame frame, Color color)
        {
            for (int i = 0; i < 3; i++)
            {
                double r = 13 + i * 11 + (3 + i) * Math.Sin(frame.Seconds * (3 + i) + i);
                var pen = new Pen(new SolidColorBrush(
                    Color.FromArgb((byte)(120 - i * 30), color.R, color.G, color.B)), 1.3 + i * .5);
                dc.DrawEllipse(null, pen, center, r, r * (.8 + .1 * i));
            }
        }

        private void DrawIllusion(DrawingContext dc, Point center, LaserFrame frame, Color color)
        {
            // Avoid sharp flashing: all oscillations are slow and alpha limited.
            var style=frame.Options.Style;
            double t=frame.Seconds;
            var soft=new SolidColorBrush(Color.FromArgb(90,color.R,color.G,color.B));
            var pen=new Pen(soft,1.5);
            if(style==LaserStyle.HaloPulse)
            {
                double r=22+7*Math.Sin(t*2.2);
                dc.DrawEllipse(null,pen,center,r,r);
                dc.DrawEllipse(null,new Pen(soft,.9),center,r+10,r+10);
            }
            else if(style==LaserStyle.TwinSpot)
            {
                double a=t*1.3;
                for(int i=0;i<2;i++)
                {
                    double phase=a+i*Math.PI;
                    var p=new Point(center.X+Math.Cos(phase)*19,center.Y+Math.Sin(phase)*12);
                    dc.DrawEllipse(GetGlow(frame.Options.ColorIndex,color),null,p,13,13);
                    dc.DrawEllipse(Brushes.White,null,p,2.2,2.2);
                }
            }
            else if(style==LaserStyle.LightTunnel)
            {
                int count=frame.Options.Performance==PerformanceProfile.Eco?3:5;
                for(int i=0;i<count;i++)
                {
                    double r=13+i*9+2*Math.Sin(t*1.4-i*.4);
                    byte alpha=(byte)Math.Clamp(85-i*12,18,100);
                    dc.DrawEllipse(null,new Pen(new SolidColorBrush(
                        Color.FromArgb(alpha,color.R,color.G,color.B)),1.3),center,r,r*.83);
                }
            }
            else if(style==LaserStyle.SoftStar)
            {
                double r=16+3*Math.Sin(t*2.1);
                var geo=new StreamGeometry();
                using(var g=geo.Open())
                {
                    for(int i=0;i<8;i++)
                    {
                        double a=-Math.PI/2+i*Math.PI/4;
                        double distance=i%2==0?r:r*.35;
                        var p=new Point(center.X+Math.Cos(a)*distance,center.Y+Math.Sin(a)*distance);
                        if(i==0)g.BeginFigure(p,true,true);
                        else g.LineTo(p,true,false);
                    }
                }
                geo.Freeze();
                dc.DrawGeometry(new SolidColorBrush(Color.FromArgb(55,color.R,color.G,color.B)),pen,geo);
            }
        }

        private void DrawObstacles(DrawingContext dc, LaserFrame frame)
        {
            // Virtual hiding places. They are visual only and NEVER intercept mouse input.
            double w = ActualWidth, h = ActualHeight;
            var fill = new SolidColorBrush(Color.FromArgb(220, 24, 27, 39));
            var edge = new Pen(new SolidColorBrush(Color.FromArgb(210, 255, 80, 97)), 1.3);
            var a = new Rect(w * .31, h * .35, Math.Max(55, w * .08), Math.Max(55, h * .20));
            var b = new Rect(w * .61, h * .42, Math.Max(55, w * .07), Math.Max(55, h * .19));
            dc.DrawRoundedRectangle(fill, edge, a, 10, 10);
            dc.DrawRoundedRectangle(fill, edge, b, 10, 10);
        }

        private Point Local(Point screenPoint) => _owner.PointFromScreen(screenPoint);
    }
}
