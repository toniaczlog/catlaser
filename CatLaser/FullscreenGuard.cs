using System;
using System.Runtime.InteropServices;
using Forms=System.Windows.Forms;

namespace CatLaser;

/// <summary>Foreground borderless fullscreen detector. Ignores shell and minimized windows.</summary>
internal static class FullscreenGuard
{
    public static bool IsFullscreen()
    {
        IntPtr window=GetForegroundWindow();
        if(window==IntPtr.Zero||window==GetShellWindow()||IsIconic(window))return false;
        // Maximized normal windows still have a caption and are NOT exclusive fullscreen.
        const long WsCaption=0x00C00000;
        if((GetWindowLongPtr(window,-16).ToInt64()&WsCaption)!=0)return false;
        if(!GetWindowRect(window,out var r))return false;
        // Require a true monitor-covering foreground window, with small DPI tolerance.
        var center=new System.Drawing.Point(r.Left+(r.Right-r.Left)/2,r.Top+(r.Bottom-r.Top)/2);
        var screen=Forms.Screen.FromPoint(center).Bounds;
        return r.Left<=screen.Left+2 && r.Top<=screen.Top+2 &&
            r.Right>=screen.Right-2 && r.Bottom>=screen.Bottom-2;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct WinRect{public int Left,Top,Right,Bottom;}
    [DllImport("user32.dll",EntryPoint="GetWindowLongPtrW")]private static extern IntPtr GetWindowLongPtr(IntPtr h,int index);
    [DllImport("user32.dll")]private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]private static extern IntPtr GetShellWindow();
    [DllImport("user32.dll")]private static extern bool IsIconic(IntPtr h);
    [DllImport("user32.dll")]private static extern bool GetWindowRect(IntPtr h,out WinRect rect);
}
