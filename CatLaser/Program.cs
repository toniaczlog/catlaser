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
        _hotkey = new HotkeyWindow(
            () => _controller.Toggle(),
            () => _controller.CycleStyle(),
            () => _controller.CycleColor(),
            () => _controller.AdjustSpeed(10),
            () => _controller.AdjustSpeed(-10),
            () => _controller.CycleMode());
        _settingsWindow = new SettingsWindow(_controller, ExitApp, _hotkey);
        _settingsWindow.Closing += (_, eventArgs) =>
        {
            if (_exiting) return;
            eventArgs.Cancel = true;
            _settingsWindow.Hide();
        };

        _trayMenu = new Forms.ContextMenuStrip();
        var show = new Forms.ToolStripMenuItem(UiText.T("tray_open"), null, (_, _) => ShowSettings());
        var toggle = new Forms.ToolStripMenuItem(UiText.T("tray_toggle"), null, (_, _) => _controller.Toggle());
        var catMode = new Forms.ToolStripMenuItem(UiText.T("tray_cat"), null, (_, _) => _controller.SetMode(_controller.Options.Mode == PlayMode.Manual ? PlayMode.SmartPrey : PlayMode.Manual));
        var restore = new Forms.ToolStripMenuItem(UiText.T("tray_restore"), null, (_, _) => _controller.SetEnabled(false));
        var quit = new Forms.ToolStripMenuItem(UiText.T("tray_exit"), null, (_, _) => ExitApp());

        var modeMenu = new Forms.ToolStripMenuItem();
        var colorMenu = new Forms.ToolStripMenuItem();
        var presetMenu = new Forms.ToolStripMenuItem();
        var playlistMenu = new Forms.ToolStripMenuItem();
        var fasterMenu = new Forms.ToolStripMenuItem();
        var slowerMenu = new Forms.ToolStripMenuItem();
        modeMenu.DropDownOpening+=(_,_)=>{
            modeMenu.DropDownItems.Clear();
            foreach(var mode in Enum.GetValues<PlayMode>())
            {
                var m=mode;
                modeMenu.DropDownItems.Add(new Forms.ToolStripMenuItem(
                    UiText.T("mode_"+mode.ToString().ToLowerInvariant()),
                    null,(_,_)=>_controller.SetMode(m)){Checked=_controller.Options.Mode==m});
            }
        };
        colorMenu.DropDownOpening+=(_,_)=>{
            colorMenu.DropDownItems.Clear();
            string[] keys={"color_red","color_green","color_blue","color_violet","color_amber","color_cyan"};
            for(int i=0;i<keys.Length;i++)
            {
                int index=i;
                colorMenu.DropDownItems.Add(new Forms.ToolStripMenuItem(UiText.T(keys[i]),
                    null,(_,_)=>_controller.SetColor(index)){Checked=_controller.Options.ColorIndex==index});
            }
        };
        presetMenu.DropDownOpening+=(_,_)=>{
            presetMenu.DropDownItems.Clear();
            var presets=new PresetStore().Presets;
            foreach(var p in presets)
                presetMenu.DropDownItems.Add(new Forms.ToolStripMenuItem(p.Name,null,
                    (_,_)=>_controller.ApplyPreset(p.Options)));
            if(presets.Count==0)presetMenu.DropDownItems.Add(new Forms.ToolStripMenuItem("—"){Enabled=false});
        };
        playlistMenu.DropDownOpening+=(_,_)=>{
            playlistMenu.DropDownItems.Clear();
            foreach(var p in new PlaylistStore().Playlists)
                playlistMenu.DropDownItems.Add(new Forms.ToolStripMenuItem(p.Name,null,
                    (_,_)=>_controller.StartPlaylist(p)));
            playlistMenu.DropDownItems.Add(new Forms.ToolStripSeparator());
            playlistMenu.DropDownItems.Add(new Forms.ToolStripMenuItem(UiText.T("stop_playlist"),null,
                (_,_)=>_controller.StopPlaylist()));
        };
        fasterMenu.Click+=(_,_)=>_controller.AdjustSpeed(10);
        slowerMenu.Click+=(_,_)=>_controller.AdjustSpeed(-10);
        void TranslateQuick()
        {
            modeMenu.Text=UiText.T("tray_modes");
            colorMenu.Text=UiText.T("tray_colors");
            presetMenu.Text=UiText.T("tray_presets");
            playlistMenu.Text=UiText.T("tray_playlists");
            fasterMenu.Text=UiText.T("tray_faster");
            slowerMenu.Text=UiText.T("tray_slower");
        }
        TranslateQuick();
        _trayMenu.Items.AddRange(new Forms.ToolStripItem[]
        {
            show,toggle,catMode,new Forms.ToolStripSeparator(),modeMenu,colorMenu,
            presetMenu,playlistMenu,fasterMenu,slowerMenu,new Forms.ToolStripSeparator(),
            restore,new Forms.ToolStripSeparator(),quit
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
            Text = UiText.T("tray_tooltip"),
            Visible = true,
            ContextMenuStrip = _trayMenu
        };
        _tray.DoubleClick += (_, _) => ShowSettings();

        UiText.LanguageChanged += () =>
        {
            show.Text = UiText.T("tray_open");
            toggle.Text = UiText.T("tray_toggle");
            catMode.Text = UiText.T("tray_cat");
            restore.Text = UiText.T("tray_restore");
            quit.Text = UiText.T("tray_exit");
            TranslateQuick();
            if (_tray is not null) _tray.Text = UiText.T("tray_tooltip");
        };


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
