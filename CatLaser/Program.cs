using System;
using System.Linq;
using System.Windows;
using Forms = System.Windows.Forms;

namespace CatLaser;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Any(x => x.Equals("--restore-cursors", StringComparison.OrdinalIgnoreCase)))
        {
            NativeCursor.Restore();
            return;
        }

        var app = new CatLaserApp { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        app.Run();
    }
}

internal sealed class CatLaserApp : System.Windows.Application
{
    private LaserController? _controller;
    private SettingsWindow? _settingsWindow;
    private Forms.NotifyIcon? _tray;
    private Forms.ContextMenuStrip? _trayMenu;
    private HotkeyWindow? _hotkey;
    private System.Drawing.Icon? _trayIcon;
    private IntPtr _trayIconHandle;
    private bool _exiting;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Also repairs the cursor scheme if a previous CatLaser run ended unexpectedly.
        NativeCursor.Restore();
        _controller = new LaserController();

        _settingsWindow = new SettingsWindow(_controller, ExitApp);
        _settingsWindow.Closing += (_, eventArgs) =>
        {
            if (_exiting) return;
            eventArgs.Cancel = true;
            _settingsWindow.Hide();
        };

        _trayMenu = new Forms.ContextMenuStrip();
        var show = new Forms.ToolStripMenuItem("Open CatLaser", null, (_, _) => ShowSettings());
        var toggle = new Forms.ToolStripMenuItem("Toggle laser (Ctrl+Alt+L)", null, (_, _) => _controller.Toggle());
        var catMode = new Forms.ToolStripMenuItem("Cat mode · Smart Prey", null, (_, _) => _controller.SetMode(_controller.Options.Mode == PlayMode.Manual ? PlayMode.SmartPrey : PlayMode.Manual));
        var restore = new Forms.ToolStripMenuItem("Restore Windows cursors", null, (_, _) => _controller.SetEnabled(false));
        var quit = new Forms.ToolStripMenuItem("Exit", null, (_, _) => ExitApp());

        _trayMenu.Items.AddRange(new Forms.ToolStripItem[]
        {
            show, toggle, catMode, new Forms.ToolStripSeparator(), restore, new Forms.ToolStripSeparator(), quit
        });
        _controller.Changed += () =>
        {
            toggle.Checked = _controller.Enabled;
            catMode.Checked = _controller.Options.Mode != PlayMode.Manual;
        };

        _trayIconHandle = TrayIconFactory.Create();
        _trayIcon = System.Drawing.Icon.FromHandle(_trayIconHandle);
        _tray = new Forms.NotifyIcon
        {
            Icon = _trayIcon,
            Text = "CatLaser — laser pointer for Windows",
            Visible = true,
            ContextMenuStrip = _trayMenu
        };
        _tray.DoubleClick += (_, _) => ShowSettings();

        _hotkey = new HotkeyWindow(
            () => _controller.Toggle(),
            () => _controller.CycleStyle(),
            () => _controller.CycleColor(),
            () => _controller.AdjustSpeed(10),
            () => _controller.AdjustSpeed(-10),
            () => _controller.CycleMode());
        _controller.Start();
        _settingsWindow.Show();
    }

    private void ShowSettings()
    {
        if (_settingsWindow is null) return;
        _settingsWindow.Show();
        _settingsWindow.WindowState = WindowState.Normal;
        _settingsWindow.Activate();
    }

    private void ExitApp()
    {
        if (_exiting) return;
        _exiting = true;
        try
        {
            _controller?.Dispose();
            _hotkey?.Dispose();
            if (_tray is not null) { _tray.Visible = false; _tray.Dispose(); }
            _trayMenu?.Dispose();
            _trayIcon?.Dispose();
            if (_trayIconHandle != IntPtr.Zero) NativeCursor.DestroyIconHandle(_trayIconHandle);
            _settingsWindow?.Close();
        }
        finally
        {
            NativeCursor.Restore();
            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        NativeCursor.Restore();
        base.OnExit(e);
    }
}
