using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Forms = System.Windows.Forms;

namespace CatLaser;

/// <summary>Global hotkeys: validate every binding, atomically roll back on conflicts.</summary>
internal sealed class HotkeyWindow : Forms.NativeWindow, IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModNoRepeat = 0x4000;
    private readonly Dictionary<int, Action> _actions;
    private readonly Dictionary<int, HotkeyBinding> _active = new();
    private bool _disposed;

    public HotkeyWindow(Action toggle, Action nextStyle, Action nextColor,
        Action faster, Action slower, Action nextMode)
    {
        _actions = new()
        {
            [1]=toggle,[2]=nextStyle,[3]=nextColor,
            [4]=faster,[5]=slower,[6]=nextMode
        };
        CreateHandle(new Forms.CreateParams { Caption = "CatLaser Hotkeys", Parent = new IntPtr(-3) });
        if (!Rebind(HotkeyConfig.Load())) Rebind(HotkeyConfig.Defaults());
    }

    public IReadOnlyList<HotkeyBinding> Current => _active.Values
        .OrderBy(b => b.Id)
        .Select(b => new HotkeyBinding { Id=b.Id, Modifiers=b.Modifiers, VirtualKey=b.VirtualKey })
        .ToArray();

    public bool Rebind(IReadOnlyList<HotkeyBinding> requested)
    {
        if (_disposed || !HotkeyConfig.Valid(requested)) return false;
        var old = Current;
        foreach (int id in _active.Keys) UnregisterHotKey(Handle, id);
        _active.Clear();
        bool ok = true;
        foreach (var b in requested.OrderBy(x=>x.Id))
        {
            if (!RegisterHotKey(Handle, b.Id, b.Modifiers | ModNoRepeat, (uint)b.VirtualKey))
            { ok = false; break; }
            _active[b.Id] = new HotkeyBinding { Id=b.Id,Modifiers=b.Modifiers,VirtualKey=b.VirtualKey };
        }
        if (ok) return true;
        foreach (int id in _active.Keys) UnregisterHotKey(Handle, id);
        _active.Clear();
        foreach (var b in old)
            if (RegisterHotKey(Handle,b.Id,b.Modifiers | ModNoRepeat,(uint)b.VirtualKey))
                _active[b.Id] = b;
        return false;
    }

    protected override void WndProc(ref Forms.Message m)
    {
        if (m.Msg == WmHotkey && _actions.TryGetValue(m.WParam.ToInt32(), out var action))
            action();
        base.WndProc(ref m);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed=true;
        foreach (int id in _active.Keys) UnregisterHotKey(Handle, id);
        _active.Clear();
        DestroyHandle();
    }

    [DllImport("user32.dll", SetLastError=true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr window,int id,uint modifiers,uint key);
    [DllImport("user32.dll", SetLastError=true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr window,int id);
}
