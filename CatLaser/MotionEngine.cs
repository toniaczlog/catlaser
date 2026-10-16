using System;
using System.Linq;
using System.Windows;
using Forms = System.Windows.Forms;
using Point = System.Windows.Point;

namespace CatLaser;

/// <summary>Bounded frame-independent play motions; never moves the physical OS cursor.</summary>
internal sealed class MotionEngine
{
    private Point _position;
    private Point _target;
    private Vector _velocity;
    private DateTime _pauseUntil;
    private PlayMode _mode;
    private double _phase;
    private int _targetCount;
    private bool _initialized;

    public static System.Drawing.Rectangle GetPlayArea(LaserOptions options, Point start)
    {
        var screens=Forms.Screen.AllScreens;
        var screen=screens.FirstOrDefault(s=>s.DeviceName==options.MonitorDeviceName)
            ?? Forms.Screen.FromPoint(new System.Drawing.Point((int)start.X,(int)start.Y));
        var b=screen.Bounds;
        // Percentages are normalized to the selected monitor's physical pixel rectangle.
        var left=b.Left+(int)(b.Width*options.AreaLeft/100);
        var top=b.Top+(int)(b.Height*options.AreaTop/100);
        var width=(int)(b.Width*options.AreaWidth/100);
        var height=(int)(b.Height*options.AreaHeight/100);
        return new System.Drawing.Rectangle(left,top,Math.Max(35,width),Math.Max(35,height));
    }

    public void Reset(PlayMode mode, Point start, LaserOptions options)
    {
        _mode=mode;_initialized=true;_phase=0;_targetCount=0;
        _velocity=new Vector(1,.7);_pauseUntil=DateTime.MinValue;
        var b=GetPlayArea(options,start);
        _position=new Point(Math.Clamp(start.X,b.Left+12,b.Right-12),
            Math.Clamp(start.Y,b.Top+12,b.Bottom-12));
        _target=_position;
    }

    public Point Step(LaserOptions options, double dt, DateTime now)
    {
        var mode=options.Mode;
        if(!_initialized||mode!=_mode)
        {
            var mouse=Forms.Cursor.Position;
            Reset(mode,new Point(mouse.X,mouse.Y),options);
        }
        double scale=.35+options.Speed/60;
        var b=GetPlayArea(options,_position);
        double pad=Math.Min(25,Math.Min(b.Width,b.Height)*.18);
        double x0=b.Left+pad,x1=b.Right-pad,y0=b.Top+pad,y1=b.Bottom-pad;
        _position=new Point(Math.Clamp(_position.X,x0,x1),Math.Clamp(_position.Y,y0,y1));
        if(mode is PlayMode.Spiral or PlayMode.FigureEight or PlayMode.Orbit)
        {
            _phase+=Math.Min(.075,dt)*scale*2.25;
            double radius=Math.Min(b.Width,b.Height)*(.10+options.OrbitRadius*.0032);
            double cx=b.Left+b.Width/2.0,cy=b.Top+b.Height/2.0;
            double x,y;
            if(mode==PlayMode.FigureEight)
            {x=cx+Math.Sin(_phase)*radius*1.35;y=cy+Math.Sin(2*_phase)*radius*.54;}
            else if(mode==PlayMode.Spiral)
            {
                double r=radius*(.10+.90*(.5+.5*Math.Sin(_phase*.28)));
                x=cx+Math.Cos(_phase)*r;y=cy+Math.Sin(_phase)*r;
            }
            else {x=cx+Math.Cos(_phase)*radius;y=cy+Math.Sin(_phase)*radius;}
            _position=new Point(Math.Clamp(x,x0,x1),Math.Clamp(y,y0,y1));
            return _position;
        }
        if(mode==PlayMode.Ricochet)
        {
            if(_velocity.LengthSquared<.01)_velocity=new Vector(1,.7);
            _velocity.Normalize();
            _position+=_velocity*Math.Min(70,dt*310*scale);
            if(_position.X<x0||_position.X>x1)_velocity.X*=-1;
            if(_position.Y<y0||_position.Y>y1)_velocity.Y*=-1;
            _position=new Point(Math.Clamp(_position.X,x0,x1),Math.Clamp(_position.Y,y0,y1));
            return _position;
        }
        if(now<_pauseUntil)return _position;
        var direction=_target-_position;
        if(direction.Length<16)
        {
            _targetCount++;
            int lowX=(int)x0, highX=Math.Max(lowX+1,(int)x1);
            int lowY=(int)y0, highY=Math.Max(lowY+1,(int)y1);
            _target=new Point(Random.Shared.Next(lowX,highX),Random.Shared.Next(lowY,highY));
            if(mode==PlayMode.HideSeek&&_targetCount%3==0)
                _target=new Point(b.Left+b.Width*(.33+Random.Shared.NextDouble()*.34),
                    b.Top+b.Height*(.36+Random.Shared.NextDouble()*.28));
            var chance=mode switch
            {
                PlayMode.SmartPrey=>.43,PlayMode.MouseHunt=>.25,
                PlayMode.HideSeek=>.48,_=>.22
            };
            if(Random.Shared.NextDouble()<chance)
                _pauseUntil=now.AddMilliseconds(Random.Shared.Next(120,mode==PlayMode.MouseHunt?560:1150));
            direction=_target-_position;
        }
        if(direction.Length<1)return _position;
        double pace=mode switch
        {
            PlayMode.MouseHunt=>480,PlayMode.SmartPrey=>_targetCount%4==0?520:195,
            PlayMode.HideSeek=>240,PlayMode.CatchMe=>350,_=>310
        };
        if(mode==PlayMode.MouseHunt)pace*=1+.30*Math.Sin(now.Ticks/900000.0);
        _position+=direction/direction.Length*Math.Min(direction.Length,Math.Max(0,dt)*pace*scale);
        return _position;
    }

    public void Catch(DateTime now)
    {
        _pauseUntil=now.AddMilliseconds(600);
        _target=_position;
    }
}
