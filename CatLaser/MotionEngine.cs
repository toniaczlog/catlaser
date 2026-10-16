using System;
using System.Windows;
using Forms = System.Windows.Forms;
using Point = System.Windows.Point;

namespace CatLaser;

/// <summary>Bounded, deterministic-frame-rate-independent motion. Does not move the OS pointer.</summary>
internal sealed class MotionEngine
{
    private Point _position;
    private Point _target;
    private Vector _velocity;
    private DateTime _pauseUntil;
    private PlayMode _mode = PlayMode.Manual;
    private bool _initialized;
    private int _targetCount;

    public void Reset(PlayMode mode, Point position)
    {
        _mode = mode; _position = position; _target = position;
        _initialized = true; _targetCount = 0;
        _velocity = new Vector(1, 0.7);
        _pauseUntil = DateTime.MinValue;
    }

    public Point Step(PlayMode mode, double dt, double speedPercent, DateTime now)
    {
        if (!_initialized || mode != _mode) Reset(mode, CurrentMouse());
        var bounds = Forms.Screen.FromPoint(new System.Drawing.Point((int)_position.X, (int)_position.Y)).Bounds;
        double scale = 0.35 + speedPercent / 60;

        if (mode == PlayMode.Ricochet)
        {
            if (_velocity.LengthSquared < .1) _velocity = new Vector(1, .7);
            _velocity.Normalize();
            _position += _velocity * Math.Min(60, dt * 310 * scale);
            double pad = 38;
            if (_position.X < bounds.Left + pad || _position.X > bounds.Right - pad)
                _velocity.X *= -1;
            if (_position.Y < bounds.Top + pad || _position.Y > bounds.Bottom - pad)
                _velocity.Y *= -1;
            _position.X = Math.Clamp(_position.X, bounds.Left + pad, Math.Max(bounds.Left + pad, bounds.Right - pad));
            _position.Y = Math.Clamp(_position.Y, bounds.Top + pad, Math.Max(bounds.Top + pad, bounds.Bottom - pad));
            return _position;
        }

        if (now < _pauseUntil) return _position;

        Vector direction = _target - _position;
        if (direction.Length < 16)
        {
            _targetCount++;
            int marginX = Math.Min(115, bounds.Width / 5);
            int marginY = Math.Min(105, bounds.Height / 5);
            int lowX = bounds.Left + marginX;
            int highX = Math.Max(lowX + 1, bounds.Right - marginX);
            int lowY = bounds.Top + marginY;
            int highY = Math.Max(lowY + 1, bounds.Bottom - marginY);

            _target = new Point(Random.Shared.Next(lowX, highX), Random.Shared.Next(lowY, highY));
            if (mode == PlayMode.HideSeek && _targetCount % 3 == 0)
                _target = new Point(bounds.Left + bounds.Width * (.33 + Random.Shared.NextDouble() * .34),
                    bounds.Top + bounds.Height * (.36 + Random.Shared.NextDouble() * .28));

            double pauseChance = mode switch
            {
                PlayMode.SmartPrey => .43,
                PlayMode.MouseHunt => .25,
                PlayMode.HideSeek => .48,
                _ => .22
            };
            if (Random.Shared.NextDouble() < pauseChance)
                _pauseUntil = now.AddMilliseconds(Random.Shared.Next(120, mode == PlayMode.MouseHunt ? 560 : 1150));
            direction = _target - _position;
        }
        if (direction.Length < 1) return _position;
        double pace = mode switch
        {
            PlayMode.MouseHunt => 480,
            PlayMode.SmartPrey => _targetCount % 4 == 0 ? 520 : 195,
            PlayMode.HideSeek => 240,
            PlayMode.CatchMe => 350,
            _ => 310
        };
        if (mode == PlayMode.MouseHunt) pace *= (1 + .30 * Math.Sin(now.Ticks / 900000.0));
        _position += direction / direction.Length * Math.Min(direction.Length, Math.Max(0, dt) * pace * scale);
        return _position;
    }

    public void Catch(DateTime now)
    {
        _pauseUntil = now.AddMilliseconds(600);
        _target = _position;
    }

    private static Point CurrentMouse()
    {
        var p = Forms.Cursor.Position;
        return new Point(p.X, p.Y);
    }
}
