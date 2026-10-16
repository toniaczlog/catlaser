using System;
using System.Linq;
using CatLaser;

static void Assert(bool good,string what)
{
    if(!good)throw new Exception("Config smoke: "+what);
}
var original=HotkeyConfig.Defaults();
Assert(HotkeyConfig.Valid(original),"valid built-ins");
var duplicate=HotkeyConfig.Defaults();
duplicate[1].VirtualKey=duplicate[0].VirtualKey;
Assert(!HotkeyConfig.Valid(duplicate),"duplicate hotkeys");
Assert(HotkeyConfig.Save(original),"store hotkeys");
Assert(HotkeyConfig.Load().Count==6,"reload hotkeys");
var o=new LaserOptions {AreaLeft=75,AreaWidth=95,AreaHeight=210,
    Glow=double.NaN,MonitorDeviceName=new string('a',120)};
o.Sanitize();
Assert(o.AreaWidth<=25,"clamp width inside screen");
Assert(o.AreaHeight<=100,"clamp height");
Assert(o.Glow==65,"sanitize non-finite");
Assert(new LaserOptions().AdaptivePower,"adaptive power defaults on");
Assert(o.MonitorDeviceName=="","sanitize monitor name");
Assert(Enum.IsDefined(PlayMode.Spiral) && Enum.IsDefined(PlayMode.Orbit) &&
       Enum.IsDefined(PlayMode.FigureEight),"new motions");
Assert(Enum.IsDefined(LaserStyle.SoftStar),"new effects");
var sequences=new PlaylistStore();
Assert(sequences.Playlists.Count>0,"starter playlist");
Assert(sequences.Save(new PlaySequence {Name="Smoke Test",
    Steps=new(){new(){Mode=PlayMode.Spiral,Seconds=20},
       new(){Mode=PlayMode.Manual,Seconds=10},
       new(){Mode=PlayMode.Orbit,Seconds=25}}}),"save playlist");
Assert(new PlaylistStore().Playlists.Any(p=>p.Name=="Smoke Test"),"reload playlists");
Console.WriteLine("Config smoke tests passed.");
