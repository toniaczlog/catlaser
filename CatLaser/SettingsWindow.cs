using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CatLaser;

internal sealed class SettingsWindow : Window
{
    private static readonly Brush BackgroundBrush = Hex("#11131B");
    private static readonly Brush PanelBrush = Hex("#1C202C");
    private static readonly Brush TextBrush = Hex("#F4F3F7");
    private static readonly Brush MutedBrush = Hex("#A3A9B8");
    private static readonly Brush AccentBrush = Hex("#FF4357");
    private readonly LaserController _controller;
    private readonly Action _exit;
    private readonly Dictionary<LaserStyle, Button> _styles = new();
    private readonly TextBlock _status;
    private readonly Button _power;

    public SettingsWindow(LaserController controller, Action exit)
    {
        _controller = controller;
        _exit = exit;
        Title = "CatLaser · Windows Laser Pointer";
        Width = 475;
        Height = 735;
        MinWidth = 390;
        MinHeight = 570;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = BackgroundBrush;
        Foreground = TextBrush;
        FontFamily = new FontFamily("Segoe UI");

        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        var content = new StackPanel { Margin = new Thickness(28, 24, 28, 25) };
        scroll.Content = content;
        Content = scroll;

        content.Children.Add(Label("CATLASER  /  WINDOWS", AccentBrush, 12, FontWeights.Bold));
        content.Children.Add(Label("Your mouse. A laser.", TextBrush, 27, FontWeights.Bold, 5, 13));
        content.Children.Add(Label("A playful, system-wide laser cursor with a soft trail and a cat-friendly autopilot.", MutedBrush, 13, FontWeights.Normal, 0, 19));

        var preview = new Grid { Height = 142, Background = PanelBrush, ClipToBounds = true };
        var previewGlow = new Ellipse
        {
            Width = 110, Height = 110,
            Fill = new RadialGradientBrush(Color.FromArgb(225, 255, 26, 40), Color.FromArgb(0, 255, 0, 20)),
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
        };
        preview.Children.Add(previewGlow);
        preview.Children.Add(new Ellipse
        {
            Width = 8, Height = 8, Fill = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
        });
        preview.Children.Add(new TextBlock
        {
            Text = "LIVE DESKTOP EFFECT", FontSize = 10, Foreground = MutedBrush,
            Margin = new Thickness(12), HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom
        });
        content.Children.Add(preview);

        content.Children.Add(Label("LASER STYLE", MutedBrush, 11, FontWeights.Bold, 17, 8));
        var stylesRow = new Grid();
        stylesRow.ColumnDefinitions.Add(new ColumnDefinition());
        stylesRow.ColumnDefinitions.Add(new ColumnDefinition());
        stylesRow.ColumnDefinitions.Add(new ColumnDefinition());
        AddStyle(stylesRow, LaserStyle.Real, 0);
        AddStyle(stylesRow, LaserStyle.Neon, 1);
        AddStyle(stylesRow, LaserStyle.Cyber, 2);
        content.Children.Add(stylesRow);

        content.Children.Add(SliderRow("Glow intensity", _controller.Options.Glow, value =>
        {
            _controller.Options.Glow = value; _controller.RefreshSettings();
        }, 14));
        content.Children.Add(SliderRow("Laser size", _controller.Options.Size, value =>
        {
            _controller.Options.Size = value; _controller.RefreshSettings();
        }, 7));
        content.Children.Add(SliderRow("Trail duration", _controller.Options.TrailLength, value =>
        {
            _controller.Options.TrailLength = value; _controller.RefreshSettings();
        }, 7));

        content.Children.Add(Toggle("Light trail", "Adds a fading streak when the laser moves.", _controller.Options.TrailEnabled,
            selected => { _controller.Options.TrailEnabled = selected; _controller.RefreshSettings(); }, 17));
        content.Children.Add(Toggle("Cat mode", "Moves the light automatically for up to five minutes.", _controller.Options.CatMode,
            selected => _controller.SetCatMode(selected), 10));

        var line = new Border { Height = 1, Background = Hex("#363948"), Margin = new Thickness(0, 21, 0, 16) };
        content.Children.Add(line);
        _status = Label("", MutedBrush, 12, FontWeights.Normal, 0, 10);
        content.Children.Add(_status);

        _power = new Button
        {
            Height = 49, Background = AccentBrush, Foreground = Brushes.White, BorderThickness = new Thickness(0),
            FontWeight = FontWeights.Bold, FontSize = 14, Cursor = System.Windows.Input.Cursors.Hand
        };
        _power.Click += (_, _) => _controller.Toggle();
        content.Children.Add(_power);
        var bottomRow = new DockPanel { Margin = new Thickness(0, 15, 0, 0) };
        var exitButton = new Button
        {
            Content = "Exit app", Foreground = MutedBrush, Background = Brushes.Transparent, BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        exitButton.Click += (_, _) => _exit();
        DockPanel.SetDock(exitButton, Dock.Right);
        bottomRow.Children.Add(exitButton);
        bottomRow.Children.Add(Label("Ctrl + Alt + L   ·   Toggle anywhere", MutedBrush, 11, FontWeights.Normal));
        content.Children.Add(bottomRow);
        content.Children.Add(Label("Closing this window keeps CatLaser in the system tray.", MutedBrush, 11, FontWeights.Normal, 12));

        _controller.Changed += RefreshState;
        RefreshState();
    }

    private void AddStyle(Grid grid, LaserStyle style, int column)
    {
        var button = new Button
        {
            Content = style.ToString(), Height = 43, Margin = new Thickness(column == 0 ? 0 : 4, 0, column == 2 ? 0 : 4, 0),
            BorderThickness = new Thickness(1), Foreground = TextBrush,
            Cursor = System.Windows.Input.Cursors.Hand, FontWeight = FontWeights.SemiBold
        };
        button.Click += (_, _) =>
        {
            _controller.Options.Style = style;
            _controller.RefreshSettings();
        };
        _styles.Add(style, button);
        Grid.SetColumn(button, column);
        grid.Children.Add(button);
    }

    private static StackPanel SliderRow(string name, double value, Action<double> onChange, double marginTop)
    {
        var panel = new StackPanel { Margin = new Thickness(0, marginTop, 0, 0) };
        var header = new DockPanel();
        var current = Label($"{value:0}%", MutedBrush, 12, FontWeights.Normal);
        DockPanel.SetDock(current, Dock.Right);
        header.Children.Add(current);
        header.Children.Add(Label(name, TextBrush, 13, FontWeights.SemiBold));
        panel.Children.Add(header);
        var slider = new Slider { Minimum = 0, Maximum = 100, Value = value, Margin = new Thickness(0, 6, 0, 0) };
        slider.ValueChanged += (_, _) =>
        {
            current.Text = $"{slider.Value:0}%";
            onChange(slider.Value);
        };
        panel.Children.Add(slider);
        return panel;
    }

    private static DockPanel Toggle(string title, string detail, bool selected, Action<bool> onChange, double top)
    {
        var panel = new DockPanel { Margin = new Thickness(0, top, 0, 0) };
        var checkbox = new CheckBox
        {
            IsChecked = selected, VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(14, 0, 0, 0)
        };
        checkbox.Checked += (_, _) => onChange(true);
        checkbox.Unchecked += (_, _) => onChange(false);
        DockPanel.SetDock(checkbox, Dock.Right);
        panel.Children.Add(checkbox);
        var labels = new StackPanel();
        labels.Children.Add(Label(title, TextBrush, 13, FontWeights.SemiBold));
        labels.Children.Add(Label(detail, MutedBrush, 11, FontWeights.Normal, 3));
        panel.Children.Add(labels);
        return panel;
    }

    private void RefreshState()
    {
        foreach (var item in _styles)
        {
            bool active = _controller.Options.Style == item.Key;
            item.Value.Background = active ? AccentBrush : PanelBrush;
            item.Value.BorderBrush = active ? AccentBrush : Hex("#3B3E4B");
        }
        _power.Content = _controller.Enabled ? "TURN LASER OFF" : "TURN LASER ON";
        _power.Background = _controller.Enabled ? AccentBrush : Hex("#4B5367");
        _status.Text = !_controller.Enabled
            ? "Status: laser off · Windows cursor restored"
            : _controller.Options.CatMode
                ? "Status: cat mode · mouse stays available"
                : _controller.CursorApplied
                    ? "Status: desktop laser on · cursor replaced"
                    : "Status: overlay on · cursor replacement unavailable";
    }

    private static TextBlock Label(string value, Brush color, double fontSize, FontWeight weight, double top = 0, double bottom = 0) =>
        new()
        {
            Text = value, TextWrapping = TextWrapping.Wrap, Foreground = color,
            FontSize = fontSize, FontWeight = weight, Margin = new Thickness(0, top, 0, bottom)
        };

    private static Brush Hex(string color) => (Brush)new BrushConverter().ConvertFromString(color)!;
}
