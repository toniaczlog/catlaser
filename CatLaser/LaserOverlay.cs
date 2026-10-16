using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;
using Brushes = System.Windows.Media.Brushes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using Forms = System.Windows.Forms;

namespace CatLaser;

/// <summary>One click-through, alpha-transparent topmost surface per monitor.</summary>
internal sealed class LaserOverlay : Window
{
    private const int GwlExStyle = -20;
    private const long WsExTransparent = 0x20;
    private const long WsExToolWindow = 0x80;
    private const long WsExNoActivate = 0x08000000;
    private const int SwpNoActivate = 0x0010;
    private const int SwpNoZOrder = 0x0004;

    private readonly Forms.Screen _screen;
    private readonly Canvas _canvas;
    private readonly Ellipse _glow;
    private readonly Ellipse _dot;
    private readonly Ellipse _ring;
    private readonly Line[] _segments = new Line[36];

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

        _canvas = new Canvas { IsHitTestVisible = false, ClipToBounds = true };
        Content = _canvas;

        for (var i = 0; i < _segments.Length; i++)
        {
            _segments[i] = new Line
            {
                Stroke = new SolidColorBrush(Color.FromRgb(255, 40, 55)),
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = false
            };
            _canvas.Children.Add(_segments[i]);
        }

        var brush = new RadialGradientBrush();
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(235, 255, 44, 54), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(130, 255, 0, 20), 0.22));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(48, 240, 0, 30), 0.55));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 240, 0, 30), 1));

        _glow = new Ellipse { Fill = brush, IsHitTestVisible = false };
        _dot = new Ellipse { Fill = Brushes.White, IsHitTestVisible = false };
        _ring = new Ellipse
        {
            Stroke = new SolidColorBrush(Color.FromArgb(155, 255, 38, 54)),
            StrokeThickness = 1.7,
            Fill = Brushes.Transparent,
            IsHitTestVisible = false
        };

        _canvas.Children.Add(_glow);
        _canvas.Children.Add(_ring);
        _canvas.Children.Add(_dot);
        SourceInitialized += (_, _) => ConfigureWindow();
    }

    private void ConfigureWindow()
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        long styles = GetWindowLongPtr(handle, GwlExStyle).ToInt64();
        SetWindowLongPtr(handle, GwlExStyle, new IntPtr(styles | WsExTransparent | WsExToolWindow | WsExNoActivate));

        // Screen.Bounds is expressed in physical pixels; Win32 ensures precise placement
        // even at mixed display DPI settings.
        var r = _screen.Bounds;
        SetWindowPos(handle, IntPtr.Zero, r.Left, r.Top, r.Width, r.Height, SwpNoActivate | SwpNoZOrder);
    }

    public void Draw(Point screenPoint, IReadOnlyList<TrailSample> trail, LaserOptions options, double seconds, DateTime now)
    {
        Point local = PointFromScreen(screenPoint);
        var visible = _screen.Bounds.Contains((int)screenPoint.X, (int)screenPoint.Y);
        _glow.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        _dot.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        _ring.Visibility = visible && options.Style == LaserStyle.Cyber ? Visibility.Visible : Visibility.Collapsed;

        if (visible)
        {
            double radius = (22 + options.Glow * .55) * (0.68 + options.Size / 150);
            if (options.Style == LaserStyle.Neon) radius *= 1.2;
            _glow.Width = _glow.Height = radius * 2;
            _glow.Opacity = Math.Clamp((options.Glow + 20) / 110, .1, 1);
            Place(_glow, local, radius);
            double dotRadius = 1.5 + options.Size / 68;
            _dot.Width = _dot.Height = dotRadius * 2;
            _dot.Fill = options.Style == LaserStyle.Cyber ? Brushes.LightPink : Brushes.White;
            Place(_dot, local, dotRadius);

            if (options.Style == LaserStyle.Cyber)
            {
                double ring = 18 + 9 * (1 + Math.Sin(seconds * 5));
                _ring.Width = _ring.Height = ring * 2;
                _ring.Opacity = .35 + .35 * (1 + Math.Sin(seconds * 5)) / 2;
                Place(_ring, local, ring);
            }
        }

        double maxAge = .08 + options.TrailLength / 160.0;
        int count = 0;
        if (options.TrailEnabled && options.TrailLength > 0)
        {
            for (int i = trail.Count - 1; i >= 1 && count < _segments.Length; i--)
            {
                double age = (now - trail[i].Timestamp).TotalSeconds;
                if (age > maxAge) break;
                Point a = PointFromScreen(trail[i].Point);
                Point b = PointFromScreen(trail[i - 1].Point);
                double fade = Math.Clamp(1 - age / maxAge, 0, 1);
                var line = _segments[count++];
                line.X1 = a.X; line.Y1 = a.Y; line.X2 = b.X; line.Y2 = b.Y;
                line.Opacity = fade * (options.Style == LaserStyle.Real ? .20 : .78);
                line.StrokeThickness = options.Style == LaserStyle.Cyber ? 4.3 * fade : 2.4 * fade;
                line.Visibility = Visibility.Visible;
            }
        }

        for (int i = count; i < _segments.Length; i++) _segments[i].Visibility = Visibility.Collapsed;
    }

    private static void Place(FrameworkElement element, Point position, double radius)
    {
        Canvas.SetLeft(element, position.X - radius);
        Canvas.SetTop(element, position.Y - radius);
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr(IntPtr window, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr window, int index, IntPtr newValue);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int width, int height, int flags);
}
