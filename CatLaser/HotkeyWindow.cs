using System;
using System.Runtime.InteropServices;
using Forms = System.Windows.Forms;

namespace CatLaser;

internal sealed class HotkeyWindow : Forms.NativeWindow, IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModNoRepeat = 0x4000;
    private const int HotkeyId = 1;
    private readonly Action _toggle;
    private readonly bool _registered;

    public HotkeyWindow(Action toggle)
    {
        _toggle = toggle;
        CreateHandle(new Forms.CreateParams { Caption = "CatLaser Hotkey", Parent = new IntPtr(-3) });
        _registered = RegisterHotKey(Handle, HotkeyId, ModAlt | ModControl | ModNoRepeat, (uint)Forms.Keys.L);
    }

    protected override void WndProc(ref Forms.Message m)
    {
        if (m.Msg == WmHotkey && m.WParam == new IntPtr(HotkeyId)) _toggle();
        base.WndProc(ref m);
    }

    public void Dispose()
    {
        if (_registered) UnregisterHotKey(Handle, HotkeyId);
        DestroyHandle();
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr window, int id, uint modifiers, uint key);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr window, int id);
}
