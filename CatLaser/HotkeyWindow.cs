using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Forms = System.Windows.Forms;

namespace CatLaser;

internal sealed class HotkeyWindow : Forms.NativeWindow, IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModNoRepeat = 0x4000;
    private readonly Dictionary<int, Action> _registered = new();

    public HotkeyWindow(Action toggle, Action nextStyle, Action nextColor,
        Action faster, Action slower, Action nextMode)
    {
        CreateHandle(new Forms.CreateParams { Caption = "CatLaser Hotkeys", Parent = new IntPtr(-3) });
        Bind(1, Forms.Keys.L, toggle);
        Bind(2, Forms.Keys.E, nextStyle);
        Bind(3, Forms.Keys.C, nextColor);
        Bind(4, Forms.Keys.Up, faster);
        Bind(5, Forms.Keys.Down, slower);
        Bind(6, Forms.Keys.M, nextMode);
    }

    private void Bind(int id, Forms.Keys key, Action action)
    {
        if (RegisterHotKey(Handle, id, ModAlt | ModControl | ModNoRepeat, (uint)key))
            _registered[id] = action;
    }

    protected override void WndProc(ref Forms.Message message)
    {
        if (message.Msg == WmHotkey && _registered.TryGetValue(message.WParam.ToInt32(), out var action))
            action();
        base.WndProc(ref message);
    }

    public void Dispose()
    {
        foreach (int id in _registered.Keys) UnregisterHotKey(Handle, id);
        _registered.Clear();
        DestroyHandle();
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr window, int id, uint modifiers, uint key);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr window, int id);
}
