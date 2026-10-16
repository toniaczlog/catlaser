using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using ComboBox = System.Windows.Controls.ComboBox;
using FontFamily = System.Windows.Media.FontFamily;
using TextBox = System.Windows.Controls.TextBox;

namespace CatLaser;

internal sealed class SettingsWindow : Window
{
    private static readonly Brush Back = Hex("#11131B");
    private static readonly Brush Panel = Hex("#1D222F");
    private static readonly Brush Fore = Hex("#F4F3F7");
    private static readonly Brush Muted = Hex("#ADB4C2");
    private static readonly Brush Accent = Hex("#FE425B");
    private readonly LaserController _controller;
    private readonly Action _exit;
    private readonly PresetStore _presets = new();
    private readonly Dictionary<string, Slider> _sliders = new();
    private readonly Dictionary<string, CheckBox> _toggles = new();
    private readonly Dictionary<string, ComboBox> _choices = new();
    private readonly TextBlock _status;
    private readonly TextBlock _stats;
    private readonly TextBlock _feedback;
    private readonly Button _power;
    private readonly ComboBox _presetList;
    private readonly TextBox _presetName;
    private bool _syncing;

    public SettingsWindow(LaserController controller, Action exit)
    {
        _controller = controller; _exit = exit;
        Title = "CatLaser 0.2 · Laser Playground";
        Width = 535; Height = 840;
        MinWidth = 410; MinHeight = 590;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Back; Foreground = Fore;
        FontFamily = new FontFamily("Segoe UI");

        var root = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        Content = root;
        var body = new StackPanel { Margin = new Thickness(28, 23, 28, 28) };
        root.Content = body;

        body.Children.Add(Text("CATLASER / VERSION 0.2", Accent, 12, FontWeights.Bold));
        body.Children.Add(Text("Laser Playground", Fore, 30, FontWeights.Bold, 5, 5));
        body.Children.Add(Text("A smoother pointer. More magic. More cat games.", Muted, 13, FontWeights.Normal, 0, 12));
        _status = Text("", Muted, 12, FontWeights.Normal, 0, 7);
        body.Children.Add(_status);
        _stats = Text("", Accent, 12, FontWeights.SemiBold, 0, 12);
        body.Children.Add(_stats);

        _power = ActionButton("TOGGLE LASER", () => _controller.Toggle(), true);
        body.Children.Add(_power);

        Section(body, "LIGHT ENGINE", "Choose the glow, shape, texture and color.");
        body.Children.Add(Choice("Style", "style", Enum.GetNames<LaserStyle>(),
            (int)_controller.Options.Style, i => _controller.ChangeStyle((LaserStyle)i)));
        body.Children.Add(Choice("Color", "color", new[] { "Red", "Green", "Blue", "Violet", "Amber", "Cyan" },
            _controller.Options.ColorIndex, _controller.SetColor));
        body.Children.Add(Toggle("Light trail", "trail", _controller.Options.TrailEnabled, v =>
        {
            _controller.Options.TrailEnabled = v; _controller.RefreshSettings();
        }));
        body.Children.Add(Toggle("Shockwave on click or sudden stop", "wave",
            _controller.Options.ShockwaveEnabled, v =>
        {
            _controller.Options.ShockwaveEnabled = v; _controller.RefreshSettings();
        }));
        body.Children.Add(SliderRow("Glow intensity", "glow", _controller.Options.Glow,
            v => { _controller.Options.Glow = v; _controller.RefreshSettings(); }));
        body.Children.Add(SliderRow("Dot size", "size", _controller.Options.Size,
            v => { _controller.Options.Size = v; _controller.RefreshSettings(); }));
        body.Children.Add(SliderRow("Trail length", "trailLength", _controller.Options.TrailLength,
            v => { _controller.Options.TrailLength = v; _controller.RefreshSettings(); }));
        body.Children.Add(SliderRow("Organic jitter", "jitter", _controller.Options.Jitter,
            v => { _controller.Options.Jitter = v; _controller.RefreshSettings(); }));

        Section(body, "CAT PLAYGROUND", "Autonomous laser modes never move the actual mouse.");
        body.Children.Add(Choice("Play behavior", "mode", Enum.GetNames<PlayMode>(),
            (int)_controller.Options.Mode, i => _controller.SetMode((PlayMode)i)));
        body.Children.Add(SliderRow("Movement speed", "speed", _controller.Options.Speed,
            v => { _controller.Options.Speed = v; _controller.RefreshSettings(); }));
        body.Children.Add(Text("HideSeek draws two virtual hiding blocks. CatchMe reacts to a mouse click or touch-generated click near the laser.", Muted, 11, FontWeights.Normal, 4, 4));

        Section(body, "SESSION STUDIO", "Applies to automated modes. All sessions stop themselves.");
        body.Children.Add(Choice("Active play duration", "session",
            new[] { "30 seconds", "2 minutes", "5 minutes" },
            _controller.Options.SessionSeconds switch { 30 => 0, 300 => 2, _ => 1 },
            i =>
            {
                _controller.Options.SessionSeconds = new[] { 30, 120, 300 }[i];
                _controller.RefreshSettings();
            }));
        body.Children.Add(Choice("Pause every", "restEvery",
            new[] { "20 seconds", "30 seconds", "60 seconds" },
            _controller.Options.RestEverySeconds switch { 20 => 0, 60 => 2, _ => 1 },
            i =>
            {
                _controller.Options.RestEverySeconds = new[] { 20, 30, 60 }[i];
                _controller.RefreshSettings();
            }));
        body.Children.Add(Choice("Pause duration", "rest",
            new[] { "None", "5 seconds", "8 seconds", "15 seconds" },
            _controller.Options.RestSeconds switch { 0 => 0, 5 => 1, 15 => 3, _ => 2 },
            i =>
            {
                _controller.Options.RestSeconds = new[] { 0, 5, 8, 15 }[i];
                _controller.RefreshSettings();
            }));
        body.Children.Add(Text("Session and pause changes apply to the active session. Switching behavior restarts the timer.", Muted, 11, FontWeights.Normal, 4, 0));

        Section(body, "PERFORMANCE CENTER", "Adjust the frame budget for your computer.");
        body.Children.Add(Choice("Performance profile", "profile", Enum.GetNames<PerformanceProfile>(),
            (int)_controller.Options.Performance, i =>
            {
                _controller.Options.Performance = (PerformanceProfile)i;
                _controller.Options.CustomFps = null;
                _controller.RefreshSettings();
            }));
        body.Children.Add(Choice("FPS cap", "fps", new[] { "Automatic", "30", "60", "120", "144" },
            CurrentFpsSelection(), i =>
            {
                _controller.Options.CustomFps = i switch { 1 => 30, 2 => 60, 3 => 120, 4 => 144, _ => null };
                _controller.RefreshSettings();
            }));
        body.Children.Add(Text("Eco = 30 FPS with fewer particles; Balanced = 60; Ultra = 120. Actual frame rate depends on Windows composition and your display.", Muted, 11, FontWeights.Normal, 5));

        Section(body, "MY PRESETS", "Save custom combinations locally. No account or cloud.");
        _presetName = new TextBox
        {
            Text = "My Laser", MaxLength = 40, Height = 33,
            Background = Panel, Foreground = Fore, BorderBrush = Hex("#515766"),
            Padding = new Thickness(8, 5, 8, 5)
        };
        body.Children.Add(_presetName);
        body.Children.Add(ActionButton("SAVE / UPDATE PRESET", SavePreset, false, 8));
        _presetList = new ComboBox
        {
            Height = 33, Margin = new Thickness(0, 9, 0, 6),
            Background = Panel, Foreground = Brushes.Black
        };
        body.Children.Add(_presetList);
        var actions = new Grid();
        actions.ColumnDefinitions.Add(new ColumnDefinition());
        actions.ColumnDefinitions.Add(new ColumnDefinition());
        var load = ActionButton("LOAD PRESET", LoadPreset);
        var delete = ActionButton("DELETE PRESET", DeletePreset);
        load.Margin = new Thickness(0, 0, 5, 0);
        delete.Margin = new Thickness(5, 0, 0, 0);
        Grid.SetColumn(delete, 1);
        actions.Children.Add(load); actions.Children.Add(delete);
        body.Children.Add(actions);
        _feedback = Text("", Muted, 11, FontWeights.Normal, 7);
        body.Children.Add(_feedback);
        RefreshPresetList();

        Section(body, "QUICK CONTROL", "All combinations use Ctrl + Alt.");
        body.Children.Add(Text("L  Toggle laser       E  Cycle effect       C  Cycle color", Fore, 12, FontWeights.Normal, 0, 5));
        body.Children.Add(Text("M  Cycle game       ↑ / ↓  Increase / decrease speed", Fore, 12, FontWeights.Normal));
        body.Children.Add(Text("Close this window to keep running in the tray. Use Exit to restore the original Windows cursor scheme.", Muted, 11, FontWeights.Normal, 13, 12));
        body.Children.Add(ActionButton("EXIT CATLASER", _exit));
        _controller.Changed += RefreshState;
        RefreshState();
    }

    private void SavePreset()
    {
        _feedback.Text = _presets.Save(_presetName.Text, _controller.Options)
            ? "Preset saved to your local Windows profile." : "Use a name up to 40 characters (25 preset maximum).";
        RefreshPresetList();
    }

    private void LoadPreset()
    {
        var name = _presetList.SelectedItem as string;
        var preset = _presets.Presets.FirstOrDefault(x => x.Name == name);
        if (preset == null) { _feedback.Text = "Select a preset first."; return; }
        _controller.ApplyPreset(preset.Options);
        _feedback.Text = $"Loaded: {preset.Name}";
    }

    private void DeletePreset()
    {
        if (_presetList.SelectedItem is not string name) return;
        _presets.Delete(name);
        RefreshPresetList();
        _feedback.Text = $"Deleted: {name}";
    }

    private void RefreshPresetList()
    {
        string? prior = _presetList.SelectedItem as string;
        _presetList.ItemsSource = _presets.Presets.Select(p => p.Name).ToArray();
        _presetList.SelectedItem = _presets.Presets.Any(p => p.Name == prior)
            ? prior : _presets.Presets.LastOrDefault()?.Name;
    }

    private void RefreshState()
    {
        var o = _controller.Options;
        _power.Content = _controller.Enabled ? "TURN LASER OFF" : "TURN LASER ON";
        _power.Background = _controller.Enabled ? Accent : Hex("#526077");
        _status.Text = !_controller.Enabled ? "Laser off · Windows cursor restored" :
            o.Mode == PlayMode.Manual ?
                (_controller.CursorApplied ? "Manual laser on · red cursor replaced" : "Laser on · cursor replacement unavailable") :
                (_controller.IsResting ? "Cat game paused · rest break" : $"Cat game active · { _controller.SessionRemaining }s remaining");
        _stats.Text = $"Performance: {_controller.ActualFps} FPS measured · cap {o.TargetFps} FPS · {o.Performance}";
        _syncing = true;
        try
        {
            ChoiceValue("style", (int)o.Style);
            ChoiceValue("mode", (int)o.Mode);
            ChoiceValue("color", o.ColorIndex);
            ChoiceValue("profile", (int)o.Performance);
            ChoiceValue("session", o.SessionSeconds switch { 30 => 0, 300 => 2, _ => 1 });
            ChoiceValue("restEvery", o.RestEverySeconds switch { 20 => 0, 60 => 2, _ => 1 });
            ChoiceValue("rest", o.RestSeconds switch { 0 => 0, 5 => 1, 15 => 3, _ => 2 });
            ChoiceValue("fps", CurrentFpsSelection());
            SliderValue("glow", o.Glow); SliderValue("size", o.Size);
            SliderValue("trailLength", o.TrailLength); SliderValue("speed", o.Speed);
            SliderValue("jitter", o.Jitter);
            ToggleValue("trail", o.TrailEnabled);
            ToggleValue("wave", o.ShockwaveEnabled);
        }
        finally { _syncing = false; }
    }

    private int CurrentFpsSelection() => _controller.Options.CustomFps switch
    { 30 => 1, 60 => 2, 120 => 3, 144 => 4, _ => 0 };
    private void ChoiceValue(string key, int value)
    {
        if (_choices.TryGetValue(key, out var control) && control.SelectedIndex != value)
            control.SelectedIndex = value;
    }
    private void SliderValue(string key, double value)
    {
        if (_sliders.TryGetValue(key, out var control) && Math.Abs(control.Value - value) > .4)
            control.Value = value;
    }
    private void ToggleValue(string key, bool value)
    {
        if (_toggles.TryGetValue(key, out var control) && control.IsChecked != value)
            control.IsChecked = value;
    }

    private UIElement Choice(string label, string key, string[] values, int index, Action<int> changed)
    {
        var block = new StackPanel { Margin = new Thickness(0, 9, 0, 0) };
        block.Children.Add(Text(label, Muted, 12, FontWeights.SemiBold, 0, 4));
        var combo = new ComboBox
        {
            ItemsSource = values, SelectedIndex = index, Height = 34,
            Background = Panel, Foreground = Brushes.Black
        };
        combo.SelectionChanged += (_, _) =>
        {
            if (!_syncing && combo.SelectedIndex >= 0) changed(combo.SelectedIndex);
        };
        _choices[key] = combo;
        block.Children.Add(combo);
        return block;
    }

    private UIElement Toggle(string label, string key, bool value, Action<bool> changed)
    {
        var toggle = new CheckBox
        {
            Content = label, IsChecked = value, Foreground = Fore,
            FontSize = 13, Margin = new Thickness(0, 12, 0, 0)
        };
        toggle.Checked += (_, _) => { if (!_syncing) changed(true); };
        toggle.Unchecked += (_, _) => { if (!_syncing) changed(false); };
        _toggles[key] = toggle;
        return toggle;
    }

    private UIElement SliderRow(string label, string key, double value, Action<double> changed)
    {
        var block = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };
        var row = new DockPanel();
        var percent = Text($"{value:0}%", Muted, 12, FontWeights.Normal);
        DockPanel.SetDock(percent, Dock.Right);
        row.Children.Add(percent);
        row.Children.Add(Text(label, Fore, 12, FontWeights.SemiBold));
        block.Children.Add(row);
        var slider = new Slider { Minimum = 0, Maximum = 100, Value = value, Margin = new Thickness(0, 3, 0, 0) };
        slider.ValueChanged += (_, _) =>
        {
            percent.Text = $"{slider.Value:0}%";
            if (!_syncing) changed(slider.Value);
        };
        _sliders[key] = slider;
        block.Children.Add(slider);
        return block;
    }

    private static void Section(StackPanel stack, string title, string subtitle)
    {
        var separator = new Border
        {
            Height = 1, Background = Hex("#394052"),
            Margin = new Thickness(0, 25, 0, 12)
        };
        stack.Children.Add(separator);
        stack.Children.Add(Text(title, Accent, 12, FontWeights.Bold, 0, 4));
        stack.Children.Add(Text(subtitle, Muted, 12, FontWeights.Normal));
    }

    private static Button ActionButton(string label, Action action, bool primary = false, double top = 0)
    {
        var button = new Button
        {
            Content = label, Height = 38, Margin = new Thickness(0, top, 0, 0),
            Background = primary ? Accent : Panel, Foreground = Fore,
            BorderThickness = new Thickness(0),
            FontSize = 12, FontWeight = FontWeights.Bold,
            Cursor = System.Windows.Input.Cursors.Hand
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static TextBlock Text(string content, Brush color, double size, FontWeight weight,
        double top = 0, double bottom = 0) =>
        new()
        {
            Text = content, TextWrapping = TextWrapping.Wrap,
            Foreground = color, FontSize = size, FontWeight = weight,
            Margin = new Thickness(0, top, 0, bottom)
        };

    private static Brush Hex(string color) =>
        (Brush)new BrushConverter().ConvertFromString(color)!;
}
