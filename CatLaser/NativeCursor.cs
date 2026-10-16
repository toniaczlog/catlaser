using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CatLaser;

/// <summary>
/// SetSystemCursor changes the user's desktop cursor globally (not just this process).
/// Reinstall cursors from the active Windows scheme on every disable and normal exit.
/// The replacement always stays VISIBLE even if the app crashes.
/// </summary>
internal static class NativeCursor
{
    private static readonly uint[] CursorIds =
    {
        32512, 32513, 32514, 32515, 32516, 32642, 32643, 32644,
        32645, 32646, 32648, 32649, 32650, 32651, 32671, 32672
    };
    private const uint SpiSetCursors = 0x0057;

    public static bool Apply()
    {
        foreach (uint id in CursorIds)
        {
            IntPtr cursor = CreateLaserCursor();
            if (cursor == IntPtr.Zero || !SetSystemCursor(cursor, id))
            {
                if (cursor != IntPtr.Zero) DestroyIcon(cursor);
                Restore();
                return false;
            }
            // SetSystemCursor takes ownership and destroys each passed cursor.
        }
        return true;
    }

    public static void Restore() =>
        SystemParametersInfo(SpiSetCursors, 0, IntPtr.Zero, 0);

    public static void DestroyIconHandle(IntPtr handle) => DestroyIcon(handle);

    private static IntPtr CreateLaserCursor()
    {
        using var bitmap = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);
            using var faint = new SolidBrush(Color.FromArgb(45, 255, 0, 22));
            using var pink = new SolidBrush(Color.FromArgb(210, 255, 40, 52));
            using var white = new SolidBrush(Color.FromArgb(255, 255, 238, 230));
            graphics.FillEllipse(faint, 7, 7, 18, 18);
            graphics.FillEllipse(pink, 12, 12, 8, 8);
            graphics.FillEllipse(white, 14, 14, 4, 4);
        }

        IntPtr icon = bitmap.GetHicon();
        if (icon == IntPtr.Zero) return IntPtr.Zero;
        try
        {
            if (!GetIconInfo(icon, out var info)) return IntPtr.Zero;
            try
            {
                info.fIcon = false;
                info.xHotspot = 16;
                info.yHotspot = 16;
                return CreateIconIndirect(ref info);
            }
            finally
            {
                if (info.hbmColor != IntPtr.Zero) DeleteObject(info.hbmColor);
                if (info.hbmMask != IntPtr.Zero) DeleteObject(info.hbmMask);
            }
        }
        finally { DestroyIcon(icon); }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IconInfo
    {
        [MarshalAs(UnmanagedType.Bool)] public bool fIcon;
        public uint xHotspot;
        public uint yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetSystemCursor(IntPtr cursor, uint id);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(uint action, uint param, IntPtr value, uint flags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetIconInfo(IntPtr icon, out IconInfo info);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CreateIconIndirect(ref IconInfo info);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr icon);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteObject(IntPtr handle);
}

internal static class TrayIconFactory
{
    public static IntPtr Create()
    {
        using var bitmap = new Bitmap(32, 32);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.FromArgb(24, 26, 34));
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var glow = new SolidBrush(Color.FromArgb(255, 70, 78));
        using var center = new SolidBrush(Color.White);
        graphics.FillEllipse(glow, 6, 6, 20, 20);
        graphics.FillEllipse(center, 13, 13, 6, 6);
        return bitmap.GetHicon();
    }
}
