using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using CheckBox = System.Windows.Controls.CheckBox;
using TextBox = System.Windows.Controls.TextBox;
using Forms = System.Windows.Forms;

namespace CatLaser;

/// <summary>
/// Compact, bilingual settings dashboard. Rendering/animation stays in LaserOverlay;
/// changing language rebuilds UI chrome without resetting the active laser session.
/// </summary>
internal sealed class SettingsWindow : Window
{
    private static readonly Brush Back = Solid("#10131B");
    private static readonly Brush Sidebar = Solid("#171B25");
    private static readonly Brush CardFill = Solid("#1D2330");
    private static readonly Brush BorderFill = Solid("#343B4A");
    private static readonly Brush Fore = Solid("#F5F6FA");
    private static readonly Brush Muted = Solid("#B1B8C7");
    private static readonly Brush Accent = Solid("#FF465E");
    private static readonly Brush Inactive = Solid("#323B4E");

    private readonly LaserController _controller;
    private readonly Action _exit;
    private readonly HotkeyWindow _hotkeys;
    private readonly PlaylistStore _playlists = new();
    private readonly PresetStore _presets = new();
    private readonly Dictionary<string, Slider> _sliders = new();
    private readonly Dictionary<string, ComboBox> _choices = new();
    private readonly Dictionary<string, CheckBox> _toggles = new();
    private readonly List<(Button Button, int Index, string Group)> _options = new();
    private readonly List<Button> _navigation = new();

    private ScrollViewer _pageScroll = null!;
    private TextBlock _status = null!;
    private TextBlock _stats = null!;
    private TextBlock _feedback = null!;
    private Button _power = null!;
    private TextBox? _presetName;
    private ComboBox? _presetList;
    private Ellipse? _previewGlow;
    private Ellipse? _previewCore;
    private bool _syncing;
    private int _page;

    public SettingsWindow(LaserController controller, Action exit, HotkeyWindow hotkeys)
    {
        _controller = controller;
        _exit = exit;
        _hotkeys = hotkeys;
        Width = 875;
        Height = 710;
        MinWidth = 695;
        MinHeight = 570;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        FontFamily = new FontFamily("Segoe UI");
        Background = Back;
        Foreground = Fore;

        _controller.Changed += RefreshState;
        UiText.LanguageChanged += LanguageChanged;
        BuildShell();
    }

    protected override void OnClosed(EventArgs e)
    {
        _controller.Changed -= RefreshState;
        UiText.LanguageChanged -= LanguageChanged;
        base.OnClosed(e);
    }

    private void LanguageChanged()
    {
        // Keep the current page and live laser untouched.
        BuildShell();
    }

    private void BuildShell()
    {
        Title = UiText.T("window_title");
        _navigation.Clear();
        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(82) });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(105) });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(33) });
        Content = root;

        // Header: brand on the left, an explicit language switch on the right.
        var header = new Grid { Background = Back, Margin = new Thickness(25, 11, 25, 5) };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var brand = new Grid { Width = 49, Height = 49, Margin = new Thickness(0, 0, 13, 0) };
        brand.Children.Add(new Ellipse { Width = 39, Height = 39, Fill = Halo(Color.FromRgb(255, 49, 71)) });
        brand.Children.Add(new Ellipse { Width = 9, Height = 9, Fill = Brushes.White, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
        header.Children.Add(brand);
        var brandCopy = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        brandCopy.Children.Add(Label("CATLASER", Fore, 20, FontWeights.Bold));
        brandCopy.Children.Add(Label(UiText.T("tagline"), Muted, 12, FontWeights.Normal));
        Grid.SetColumn(brandCopy, 1);
        header.Children.Add(brandCopy);

        var language = new Button
        {
            Content = UiText.Current == "pl" ? "PL  /  EN" : "EN  /  PL",
            Width = 95, Height = 35, Background = CardFill, Foreground = Fore,
            BorderBrush = BorderFill, BorderThickness = new Thickness(1),
            FontWeight = FontWeights.SemiBold, FontSize = 12,
            Cursor = System.Windows.Input.Cursors.Hand,
            ToolTip = UiText.T("language"),
            VerticalAlignment = VerticalAlignment.Center
        };
        language.Click += (_, _) => UiText.SetLanguage(UiText.Current == "pl" ? "en" : "pl");
        Grid.SetColumn(language, 2);
        header.Children.Add(language);
        root.Children.Add(header);

        // Compact status bar is always visible, regardless of active page.
        var hero = new Grid { Margin = new Thickness(24, 2, 24, 12) };
        hero.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        hero.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var heroBorder = new Border
        {
            Background = CardFill, BorderBrush = BorderFill, BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(13), Child = hero, Margin = new Thickness(23, 0, 23, 0)
        };
        var statusPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(20, 10, 10, 10) };
        _status = Label("", Fore, 14, FontWeights.SemiBold);
        _stats = Label("", Muted, 12, FontWeights.Normal, 4);
        statusPanel.Children.Add(_status);
        statusPanel.Children.Add(_stats);
        hero.Children.Add(statusPanel);
        _power = ActionButton("", () => _controller.Toggle(), true);
        _power.Width = 175;
        _power.Height = 43;
        _power.Margin = new Thickness(10, 0, 16, 0);
        _power.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(_power, 1);
        hero.Children.Add(_power);
        Grid.SetRow(heroBorder, 1);
        root.Children.Add(heroBorder);

        var workspace = new Grid { Margin = new Thickness(24, 0, 24, 0) };
        workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var sidebarBorder = new Border
        {
            Background = Sidebar, BorderBrush = BorderFill, BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(11),
            Margin = new Thickness(0, 0, 11, 0)
        };
        var navStack = new StackPanel { Margin = new Thickness(9, 13, 9, 9) };
        string[] navKeys =
        {
            "side_appearance", "side_cat", "side_session",
            "side_performance", "side_presets", "side_help"
        };
        for (int i = 0; i < navKeys.Length; i++)
        {
            int index = i;
            var navButton = new Button
            {
                Content = UiText.T(navKeys[i]), Height = 43,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
                Padding = new Thickness(13, 0, 5, 0),
                Margin = new Thickness(0, 2, 0, 2),
                Background = Brushes.Transparent, Foreground = Muted,
                BorderThickness = new Thickness(0),
                FontSize = 13, FontWeight = FontWeights.SemiBold,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            navButton.Click += (_, _) => Navigate(index);
            _navigation.Add(navButton);
            navStack.Children.Add(navButton);
        }
        sidebarBorder.Child = navStack;
        workspace.Children.Add(sidebarBorder);

        _pageScroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = Brushes.Transparent
        };
        var pageBorder = new Border
        {
            Background = Sidebar, BorderBrush = BorderFill,
            BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(11),
            Child = _pageScroll, Margin = new Thickness(0)
        };
        Grid.SetColumn(pageBorder, 1);
        workspace.Children.Add(pageBorder);
        Grid.SetRow(workspace, 2);
        root.Children.Add(workspace);

        var footer = Label(UiText.T("footer"), Muted, 10, FontWeights.Normal);
        footer.TextAlignment = TextAlignment.Right;
        footer.Margin = new Thickness(0, 7, 28, 0);
        Grid.SetRow(footer, 3);
        root.Children.Add(footer);

        Navigate(_page);
        RefreshState();
    }

    private void Navigate(int target)
    {
        _page = Math.Clamp(target, 0, 5);
        _sliders.Clear();
        _choices.Clear();
        _toggles.Clear();
        _options.Clear();
        _presetName = null;
        _presetList = null;
        _previewGlow = null;
        _previewCore = null;
        for (int i = 0; i < _navigation.Count; i++)
        {
            bool selected = i == _page;
            _navigation[i].Background = selected ? Solid("#36313D") : Brushes.Transparent;
            _navigation[i].Foreground = selected ? Fore : Muted;
            _navigation[i].BorderBrush = selected ? Accent : Brushes.Transparent;
            _navigation[i].BorderThickness = selected ? new Thickness(3, 0, 0, 0) : new Thickness(0);
        }

        var body = new StackPanel { Margin = new Thickness(25, 23, 25, 25) };
        switch (_page)
        {
            case 0: BuildAppearance(body); break;
            case 1: BuildCat(body); break;
            case 2: BuildSession(body); break;
            case 3: BuildPerformance(body); break;
            case 4: BuildPresets(body); break;
            default: BuildHelp(body); break;
        }
        _pageScroll.Content = body;
        _pageScroll.ScrollToTop();
        RefreshState();
    }

    private void PageTitle(StackPanel body, string title, string description)
    {
        body.Children.Add(Label(UiText.T(title), Fore, 23, FontWeights.Bold, 0, 5));
        body.Children.Add(Label(UiText.T(description), Muted, 12, FontWeights.Normal, 0, 17));
    }

    private void BuildAppearance(StackPanel body)
    {
        PageTitle(body, "head_appearance", "desc_appearance");
        // A compact nonanimated preview (does not create another frame loop).
        var preview = new Grid { Height = 105 };
        _previewGlow = new Ellipse { Width = 105, Height = 105, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        _previewCore = new Ellipse { Width = 9, Height = 9, Fill = Brushes.White, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        preview.Children.Add(_previewGlow);
        preview.Children.Add(_previewCore);
        var previewCard = Card(preview);
        previewCard.Margin = new Thickness(0, 0, 0, 16);
        body.Children.Add(previewCard);
        body.Children.Add(Label(UiText.T("label_style"), Fore, 13, FontWeights.SemiBold, 0, 9));
        body.Children.Add(OptionButtons("style", new[]
        {
            "style_real", "style_neon", "style_cyber", "style_photoreal",
            "style_comet", "style_electric", "style_prism", "style_plasma",
            "style_halopulse", "style_twinspot", "style_lighttunnel", "style_softstar"
        }, (int)_controller.Options.Style, i => _controller.ChangeStyle((LaserStyle)i)));
        body.Children.Add(Choice("label_color", "color",
            new[] { "color_red", "color_green", "color_blue", "color_violet", "color_amber", "color_cyan" },
            _controller.Options.ColorIndex, _controller.SetColor));
        body.Children.Add(SliderGrid(
            SliderRow("label_glow", "glow", _controller.Options.Glow, v => Update(o => o.Glow = v)),
            SliderRow("label_size", "size", _controller.Options.Size, v => Update(o => o.Size = v)),
            SliderRow("label_trail", "trailLength", _controller.Options.TrailLength, v => Update(o => o.TrailLength = v)),
            SliderRow("label_jitter", "jitter", _controller.Options.Jitter, v => Update(o => o.Jitter = v))));
        body.Children.Add(Toggle("toggle_trail", "trail", _controller.Options.TrailEnabled, v => Update(o => o.TrailEnabled = v)));
        body.Children.Add(Toggle("toggle_wave", "wave", _controller.Options.ShockwaveEnabled, v => Update(o => o.ShockwaveEnabled = v)));
    }

    private void BuildCat(StackPanel body)
    {
        PageTitle(body, "head_cat", "desc_cat");
        body.Children.Add(Label(UiText.T("label_mode"), Fore, 13, FontWeights.SemiBold, 0, 10));
        body.Children.Add(OptionButtons("mode",
            new[] { "mode_manual", "mode_ricochet", "mode_smartprey", "mode_mousehunt", "mode_hideseek", "mode_catchme",
                "mode_spiral", "mode_figureeight", "mode_orbit" },
            (int)_controller.Options.Mode, i => _controller.SetMode((PlayMode)i)));
        body.Children.Add(SliderRow("label_speed", "speed", _controller.Options.Speed, v => Update(o => o.Speed = v)));
        body.Children.Add(SliderRow("label_orbit", "orbit", _controller.Options.OrbitRadius, v => Update(o => o.OrbitRadius = v)));
        body.Children.Add(Label(UiText.T("label_monitor"), Fore, 13, FontWeights.SemiBold, 16, 8));
        var screens=Forms.Screen.AllScreens;
        var monitor=new ComboBox {Height=36,Background=CardFill,Foreground=Brushes.Black};
        monitor.Items.Add(UiText.T("monitor_follow"));
        foreach(var screen in screens)monitor.Items.Add(screen.DeviceName);
        int screenIndex=Array.FindIndex(screens,s=>s.DeviceName==_controller.Options.MonitorDeviceName);
        monitor.SelectedIndex=screenIndex+1;
        monitor.SelectionChanged+=(_,_)=>{
            if(monitor.SelectedIndex<0)return;
            _controller.SetMonitor(monitor.SelectedIndex==0?"":screens[monitor.SelectedIndex-1].DeviceName);
        };
        body.Children.Add(monitor);
        body.Children.Add(Label(UiText.T("label_area"),Fore,13,FontWeights.SemiBold,18,4));
        body.Children.Add(SliderGrid(
            SliderRow("area_x","areaX",_controller.Options.AreaLeft,v=>{Update(o=>o.AreaLeft=v);_controller.ResetPlayArea();}),
            SliderRow("area_y","areaY",_controller.Options.AreaTop,v=>{Update(o=>o.AreaTop=v);_controller.ResetPlayArea();}),
            SliderRow("area_width","areaW",_controller.Options.AreaWidth,v=>{Update(o=>o.AreaWidth=v);_controller.ResetPlayArea();}),
            SliderRow("area_height","areaH",_controller.Options.AreaHeight,v=>{Update(o=>o.AreaHeight=v);_controller.ResetPlayArea();})
        ));
        body.Children.Add(Info(UiText.T("hint_cat")));
    }

    private void BuildSession(StackPanel body)
    {
        PageTitle(body, "head_session", "desc_session");
        body.Children.Add(Choice("label_session", "session", new[] { "seconds_30", "minutes_2", "minutes_5" },
            _controller.Options.SessionSeconds switch { 30 => 0, 300 => 2, _ => 1 },
            i => Update(o => o.SessionSeconds = new[] { 30, 120, 300 }[i])));
        body.Children.Add(Choice("label_rest_every", "restEvery",
            new[] { "seconds_20", "seconds_30", "seconds_60" },
            _controller.Options.RestEverySeconds switch { 20 => 0, 60 => 2, _ => 1 },
            i => Update(o => o.RestEverySeconds = new[] { 20, 30, 60 }[i])));
        body.Children.Add(Choice("label_rest_duration", "rest",
            new[] { "none", "seconds_5", "seconds_8", "seconds_15" },
            _controller.Options.RestSeconds switch { 0 => 0, 5 => 1, 15 => 3, _ => 2 },
            i => Update(o => o.RestSeconds = new[] { 0, 5, 8, 15 }[i])));
        body.Children.Add(Info(UiText.T("session_hint")));
        body.Children.Add(Label(UiText.T("playlist_title"),Fore,16,FontWeights.Bold,26,9));
        body.Children.Add(Label(UiText.T("playlist_desc"),Muted,12,FontWeights.Normal,0,12));
        var draft=new List<PlayStep>();
        var mode=new ComboBox {Height=35,Background=CardFill,Foreground=Brushes.Black,
            ItemsSource=new[]{"mode_smartprey","mode_mousehunt","mode_ricochet",
                "mode_spiral","mode_figureeight","mode_orbit","playlist_rest"}
                .Select(UiText.T).ToArray(),SelectedIndex=0};
        var minutes=new ComboBox {Height=35,Background=CardFill,Foreground=Brushes.Black,
            ItemsSource=new[]{"10 s","20 s","30 s","60 s"},SelectedIndex=1};
        body.Children.Add(Label(UiText.T("playlist_step"),Fore,12,FontWeights.SemiBold,0,5));
        body.Children.Add(mode);
        body.Children.Add(minutes);
        var draftList=Label("",Muted,12,FontWeights.Normal,9,7);
        body.Children.Add(draftList);
        void ShowDraft()=>draftList.Text=draft.Count==0?UiText.T("playlist_empty"):
            string.Join(" → ",draft.Select((s,i)=>
                (s.Mode==PlayMode.Manual?UiText.T("playlist_rest"):UiText.T("mode_"+s.Mode.ToString().ToLowerInvariant()))
                +" "+s.Seconds+"s"));
        ShowDraft();
        body.Children.Add(ActionButton(UiText.T("playlist_add"),()=>{
            PlayMode[] values={PlayMode.SmartPrey,PlayMode.MouseHunt,PlayMode.Ricochet,
                PlayMode.Spiral,PlayMode.FigureEight,PlayMode.Orbit,PlayMode.Manual};
            int[] durations={10,20,30,60};
            if(draft.Count<20)draft.Add(new PlayStep {Mode=values[Math.Max(0,mode.SelectedIndex)],
                Seconds=durations[Math.Max(0,minutes.SelectedIndex)]});
            ShowDraft();
        }));
        body.Children.Add(ActionButton(UiText.T("playlist_clear"),()=>{draft.Clear();ShowDraft();}));
        var name=new TextBox {Text=UiText.T("playlist_default"),MaxLength=40,Height=36,
            Background=CardFill,Foreground=Fore,BorderBrush=BorderFill,
            Padding=new Thickness(10,6,8,6),Margin=new Thickness(0,10,0,5)};
        body.Children.Add(name);
        var feedback=Label("",Muted,12,FontWeights.Normal,8,8);
        body.Children.Add(ActionButton(UiText.T("playlist_save"),()=>{
            bool saved=_playlists.Save(new PlaySequence {Name=name.Text.Trim(),Steps=draft.ToList()});
            feedback.Text=UiText.T(saved?"playlist_saved":"playlist_error");
        }));
        body.Children.Add(feedback);
        var savedList=new ComboBox {Height=36,Background=CardFill,Foreground=Brushes.Black,
            ItemsSource=_playlists.Playlists.Select(p=>p.Name).ToArray(),SelectedIndex=0};
        body.Children.Add(savedList);
        body.Children.Add(ActionButton(UiText.T("playlist_start"),()=>{
            var selected=_playlists.Playlists.FirstOrDefault(p=>p.Name==(savedList.SelectedItem as string));
            if(selected!=null)_controller.StartPlaylist(selected);
        },true));
        body.Children.Add(ActionButton(UiText.T("stop_playlist"),()=>_controller.StopPlaylist()));
    }

    private void BuildPerformance(StackPanel body)
    {
        PageTitle(body, "head_performance", "desc_performance");
        body.Children.Add(Choice("label_performance", "profile",
            new[] { "profile_eco", "profile_balanced", "profile_ultra" },
            (int)_controller.Options.Performance, i =>
            {
                _controller.Options.Performance = (PerformanceProfile)i;
                _controller.Options.CustomFps = null;
                _controller.RefreshSettings();
            }));
        body.Children.Add(Choice("label_fps", "fps",
            new[] { "auto", "fps_30", "fps_60", "fps_120", "fps_144" },
            CurrentFpsSelection(), i =>
                Update(o => o.CustomFps = i switch { 1 => 30, 2 => 60, 3 => 120, 4 => 144, _ => null })));
        body.Children.Add(Toggle("pause_fullscreen","fullscreen",_controller.Options.PauseOnFullscreen,
            v=>Update(o=>o.PauseOnFullscreen=v)));
        body.Children.Add(Info(UiText.T("perf_hint")));
    }

    private void BuildPresets(StackPanel body)
    {
        PageTitle(body, "head_presets", "desc_presets");
        body.Children.Add(Label(UiText.T("label_preset_name"), Fore, 13, FontWeights.SemiBold, 0, 8));
        _presetName = new TextBox
        {
            Text = UiText.T("preset_default"), MaxLength = 40, Height = 37,
            Background = CardFill, Foreground = Fore, BorderBrush = BorderFill,
            Padding = new Thickness(10, 7, 8, 6)
        };
        body.Children.Add(_presetName);
        var save = ActionButton(UiText.T("save_preset"), SavePreset, true);
        save.Margin = new Thickness(0, 10, 0, 0);
        body.Children.Add(save);
        body.Children.Add(Label(UiText.T("label_saved"), Fore, 13, FontWeights.SemiBold, 22, 8));
        _presetList = new ComboBox { Height = 36, Background = CardFill, Foreground = Brushes.Black };
        body.Children.Add(_presetList);
        var actions = new Grid { Margin = new Thickness(0, 10, 0, 0) };
        actions.ColumnDefinitions.Add(new ColumnDefinition());
        actions.ColumnDefinitions.Add(new ColumnDefinition());
        var load = ActionButton(UiText.T("load_preset"), LoadPreset);
        var delete = ActionButton(UiText.T("delete_preset"), DeletePreset);
        load.Margin = new Thickness(0, 0, 6, 0);
        delete.Margin = new Thickness(6, 0, 0, 0);
        Grid.SetColumn(delete, 1);
        actions.Children.Add(load);
        actions.Children.Add(delete);
        body.Children.Add(actions);
        _feedback = Label("", Muted, 12, FontWeights.Normal, 12);
        body.Children.Add(_feedback);
        RefreshPresetList();
    }

    private void BuildHelp(StackPanel body)
    {
        PageTitle(body,"head_help","desc_help");
        body.Children.Add(Label(UiText.T("hotkey_edit"),Fore,15,FontWeights.Bold,0,9));
        var current=HotkeyConfig.Defaults().ToDictionary(b=>b.Id);
        foreach(var active in _hotkeys.Current)
            current[active.Id]=new HotkeyBinding{Id=active.Id,Modifiers=active.Modifiers,VirtualKey=active.VirtualKey};
        var editor=new StackPanel();
        string[] translations={"hotkey_toggle","hotkey_effect","hotkey_color",
            "hotkey_faster","hotkey_slower","hotkey_mode"};
        for(int id=1;id<=6;id++)
        {
            int actionId=id;
            var row=new Grid{Margin=new Thickness(0,0,0,11)};
            row.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)});
            row.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(125)});
            row.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(98)});
            row.Children.Add(Label(UiText.T(translations[id-1]),Fore,12,FontWeights.SemiBold));
            var modifiers=new ComboBox{Height=33,Background=CardFill,Foreground=Brushes.Black,
                ItemsSource=HotkeyConfig.Modifiers.Select(v=>v.Label).ToArray(),
                SelectedIndex=Array.FindIndex(HotkeyConfig.Modifiers,v=>v.Value==current[id].Modifiers)};
            modifiers.SelectionChanged+=(_,_)=>{
                if(modifiers.SelectedIndex>=0)
                    current[actionId].Modifiers=HotkeyConfig.Modifiers[modifiers.SelectedIndex].Value;
            };
            Grid.SetColumn(modifiers,1);row.Children.Add(modifiers);
            var keys=new ComboBox{Height=33,Background=CardFill,Foreground=Brushes.Black,
                ItemsSource=HotkeyConfig.Keys.Select(v=>v.Label).ToArray(),
                SelectedIndex=Array.FindIndex(HotkeyConfig.Keys,v=>v.Value==current[id].VirtualKey)};
            keys.SelectionChanged+=(_,_)=>{
                if(keys.SelectedIndex>=0)
                    current[actionId].VirtualKey=HotkeyConfig.Keys[keys.SelectedIndex].Value;
            };
            Grid.SetColumn(keys,2);row.Children.Add(keys);
            editor.Children.Add(row);
        }
        body.Children.Add(editor);
        var result=Label("",Muted,12,FontWeights.Normal,10,10);
        body.Children.Add(ActionButton(UiText.T("hotkey_apply"),()=>{
            var bindings=current.Values.OrderBy(b=>b.Id).ToArray();
            if(!HotkeyConfig.Valid(bindings)){result.Text=UiText.T("hotkey_duplicate");return;}
            if(!_hotkeys.Rebind(bindings)){result.Text=UiText.T("hotkey_conflict");return;}
            result.Text=UiText.T(HotkeyConfig.Save(bindings)?"hotkey_saved":"hotkey_not_saved");
        },true));
        body.Children.Add(result);
        body.Children.Add(Label(UiText.T("safety_title"),Fore,15,FontWeights.Bold,22,9));
        body.Children.Add(Info(UiText.T("safety_info")));
        var exitButton=ActionButton(UiText.T("exit"),_exit);
        exitButton.Margin=new Thickness(0,18,0,0);
        body.Children.Add(exitButton);
    }

    private static void Hotkey(StackPanel panel, string combination, string key)
    {
        var row = new Grid { Margin = new Thickness(5, 8, 5, 8) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(159) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.Children.Add(Label(combination, Accent, 12, FontWeights.Bold));
        var description = Label(UiText.T(key), Fore, 12, FontWeights.Normal);
        Grid.SetColumn(description, 1);
        row.Children.Add(description);
        panel.Children.Add(row);
    }

    private void SavePreset()
    {
        if (_presetName is null) return;
        string result = _presets.Save(_presetName.Text, _controller.Options) ? "preset_saved" : "preset_invalid";
        _feedback.Text = UiText.T(result);
        RefreshPresetList();
    }

    private void LoadPreset()
    {
        var name = _presetList?.SelectedItem as string;
        var preset = _presets.Presets.FirstOrDefault(x => x.Name == name);
        if (preset is null)
        {
            _feedback.Text = UiText.T("preset_select");
            return;
        }
        _controller.ApplyPreset(preset.Options);
        _feedback.Text = UiText.F("preset_loaded", preset.Name);
    }

    private void DeletePreset()
    {
        if (_presetList?.SelectedItem is not string name) return;
        _presets.Delete(name);
        RefreshPresetList();
        _feedback.Text = UiText.F("preset_deleted", name);
    }

    private void RefreshPresetList()
    {
        if (_presetList is null) return;
        string? previous = _presetList.SelectedItem as string;
        _presetList.ItemsSource = _presets.Presets.Select(p => p.Name).ToArray();
        _presetList.SelectedItem = _presets.Presets.Any(p => p.Name == previous)
            ? previous : _presets.Presets.LastOrDefault()?.Name;
        if (_presets.Presets.Count == 0 && _feedback is not null)
            _feedback.Text = UiText.T("empty_presets");
    }

    private static Border Info(string info)
    {
        return new Border
        {
            Background = CardFill, CornerRadius = new CornerRadius(9),
            BorderThickness = new Thickness(1), BorderBrush = BorderFill,
            Margin = new Thickness(0, 19, 0, 0),
            Padding = new Thickness(13),
            Child = Label(info, Muted, 12, FontWeights.Normal)
        };
    }

    private static Border Card(UIElement inner) => new()
    {
        Background = CardFill, BorderBrush = BorderFill,
        BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10),
        Padding = new Thickness(13), Child = inner
    };

    private UIElement OptionButtons(string group, string[] keys, int selected, Action<int> changed)
    {
        var wrap = new WrapPanel { Margin = new Thickness(0, 0, 0, 11) };
        for (int i = 0; i < keys.Length; i++)
        {
            int index = i;
            var button = ActionButton(UiText.T(keys[i]), () => changed(index));
            button.Padding = new Thickness(13, 4, 13, 4);
            button.MinWidth = 106;
            button.Margin = new Thickness(0, 0, 8, 8);
            button.Height = 36;
            button.BorderThickness = new Thickness(1);
            _options.Add((button, index, group));
            wrap.Children.Add(button);
        }
        RefreshButtons(group, selected);
        return wrap;
    }

    private void RefreshButtons(string group, int selected)
    {
        foreach (var entry in _options.Where(x => x.Group == group))
        {
            bool active = entry.Index == selected;
            entry.Button.Background = active ? Accent : CardFill;
            entry.Button.Foreground = Fore;
            entry.Button.BorderBrush = active ? Accent : BorderFill;
        }
    }

    private UIElement Choice(string labelKey, string key, string[] optionKeys, int selected, Action<int> changed)
    {
        var stack = new StackPanel { Margin = new Thickness(0, 10, 0, 3) };
        stack.Children.Add(Label(UiText.T(labelKey), Fore, 13, FontWeights.SemiBold, 0, 7));
        var combo = new ComboBox
        {
            Height = 36,
            ItemsSource = optionKeys.Select(FormatChoice).ToArray(),
            SelectedIndex = selected,
            Background = CardFill, Foreground = Brushes.Black
        };
        combo.SelectionChanged += (_, _) =>
        {
            if (!_syncing && combo.SelectedIndex >= 0) changed(combo.SelectedIndex);
        };
        _choices[key] = combo;
        stack.Children.Add(combo);
        return stack;
    }

    private static string FormatChoice(string key) => key switch
    {
        "seconds_5" => UiText.F("seconds", 5),
        "seconds_8" => UiText.F("seconds", 8),
        "seconds_15" => UiText.F("seconds", 15),
        "seconds_20" => UiText.F("seconds", 20),
        "seconds_30" => UiText.F("seconds", 30),
        "seconds_60" => UiText.F("seconds", 60),
        "minutes_2" => UiText.F("minutes", 2),
        "minutes_5" => UiText.T("minutes_five"),
        "fps_30" => "30 FPS",
        "fps_60" => "60 FPS",
        "fps_120" => "120 FPS",
        "fps_144" => "144 FPS",
        _ => UiText.T(key)
    };

    private UIElement SliderGrid(params UIElement[] controls)
    {
        var grid = new Grid { Margin = new Thickness(0, 4, 0, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        for (int i = 0; i < controls.Length; i++)
        {
            if (i % 2 == 0) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetColumn(controls[i], i % 2);
            Grid.SetRow(controls[i], i / 2);
            if (controls[i] is FrameworkElement fe)
                fe.Margin = i % 2 == 0
                    ? new Thickness(0, 9, 11, 7) : new Thickness(11, 9, 0, 7);
            grid.Children.Add(controls[i]);
        }
        return grid;
    }

    private UIElement SliderRow(string labelKey, string key, double value, Action<double> changed)
    {
        var stack = new StackPanel { Margin = new Thickness(0, 15, 0, 4) };
        var row = new DockPanel();
        var number = Label($"{value:0}%", Muted, 12, FontWeights.Normal);
        DockPanel.SetDock(number, Dock.Right);
        row.Children.Add(number);
        row.Children.Add(Label(UiText.T(labelKey), Fore, 12, FontWeights.SemiBold));
        stack.Children.Add(row);
        var slider = new Slider
        {
            Minimum = 0, Maximum = 100, Value = value,
            TickFrequency = 5, Margin = new Thickness(0, 7, 0, 0)
        };
        slider.ValueChanged += (_, _) =>
        {
            number.Text = $"{slider.Value:0}%";
            if (!_syncing) changed(slider.Value);
        };
        _sliders[key] = slider;
        stack.Children.Add(slider);
        return stack;
    }

    private UIElement Toggle(string textKey, string key, bool selected, Action<bool> changed)
    {
        var checkbox = new CheckBox
        {
            Content = UiText.T(textKey), IsChecked = selected,
            Foreground = Fore, FontSize = 12,
            Margin = new Thickness(0, 11, 0, 0)
        };
        checkbox.Checked += (_, _) => { if (!_syncing) changed(true); };
        checkbox.Unchecked += (_, _) => { if (!_syncing) changed(false); };
        _toggles[key] = checkbox;
        return checkbox;
    }

    private void Update(Action<LaserOptions> change)
    {
        change(_controller.Options);
        _controller.RefreshSettings();
    }

    private void RefreshState()
    {
        var o = _controller.Options;
        if (_power is null || _status is null || _stats is null) return;
        _power.Content = UiText.T(_controller.Enabled ? "turn_off" : "turn_on");
        _power.Background = _controller.Enabled ? Accent : Inactive;
        _status.Text = !_controller.Enabled ? UiText.T("status_off") :
            o.Mode == PlayMode.Manual
                ? UiText.T(_controller.CursorApplied ? "status_manual" : "status_no_cursor")
                : _controller.IsResting
                    ? UiText.T("status_rest")
                    : UiText.F("status_session", _controller.SessionRemaining);
        _stats.Text = UiText.F("fps_measure", _controller.ActualFps, o.TargetFps);

        _syncing = true;
        try
        {
            RefreshButtons("style", (int)o.Style);
            RefreshButtons("mode", (int)o.Mode);
            ChoiceValue("color", o.ColorIndex);
            ChoiceValue("session", o.SessionSeconds switch { 30 => 0, 300 => 2, _ => 1 });
            ChoiceValue("restEvery", o.RestEverySeconds switch { 20 => 0, 60 => 2, _ => 1 });
            ChoiceValue("rest", o.RestSeconds switch { 0 => 0, 5 => 1, 15 => 3, _ => 2 });
            ChoiceValue("profile", (int)o.Performance);
            ChoiceValue("fps", o.CustomFps switch { 30 => 1, 60 => 2, 120 => 3, 144 => 4, _ => 0 });
            SliderValue("glow", o.Glow);
            SliderValue("size", o.Size);
            SliderValue("trailLength", o.TrailLength);
            SliderValue("jitter", o.Jitter);
            SliderValue("speed", o.Speed);
            SliderValue("orbit", o.OrbitRadius);
            SliderValue("areaX",o.AreaLeft);SliderValue("areaY",o.AreaTop);
            SliderValue("areaW",o.AreaWidth);SliderValue("areaH",o.AreaHeight);
            ToggleValue("trail", o.TrailEnabled);
            ToggleValue("wave", o.ShockwaveEnabled);
            ToggleValue("fullscreen",o.PauseOnFullscreen);
            if (_previewGlow is not null)
                _previewGlow.Fill = Halo(o.PrimaryColor);
            if (_previewCore is not null)
                _previewCore.Fill = Brushes.White;
        }
        finally { _syncing = false; }
    }

    private int CurrentFpsSelection() => _controller.Options.CustomFps switch
    {
        30 => 1,
        60 => 2,
        120 => 3,
        144 => 4,
        _ => 0
    };

    private void ChoiceValue(string key, int index)
    {
        if (_choices.TryGetValue(key, out var choice) && choice.SelectedIndex != index)
            choice.SelectedIndex = index;
    }

    private void SliderValue(string key, double value)
    {
        if (_sliders.TryGetValue(key, out var slider) && Math.Abs(slider.Value - value) > .4)
            slider.Value = value;
    }

    private void ToggleValue(string key, bool value)
    {
        if (_toggles.TryGetValue(key, out var toggle) && toggle.IsChecked != value)
            toggle.IsChecked = value;
    }

    private static Brush Halo(Color color) => new RadialGradientBrush(
        Color.FromArgb(210, color.R, color.G, color.B),
        Color.FromArgb(0, color.R, color.G, color.B));

    private static Button ActionButton(string label, Action action, bool important = false)
    {
        var button = new Button
        {
            Content = label, Background = important ? Accent : CardFill,
            Foreground = Fore, BorderBrush = BorderFill, BorderThickness = new Thickness(1),
            FontWeight = FontWeights.SemiBold, FontSize = 12, Height = 38,
            Cursor = System.Windows.Input.Cursors.Hand
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static TextBlock Label(string content, Brush color, double size, FontWeight weight,
        double marginTop = 0, double marginBottom = 0) => new()
    {
        Text = content, TextWrapping = TextWrapping.Wrap,
        Foreground = color, FontSize = size, FontWeight = weight,
        Margin = new Thickness(0, marginTop, 0, marginBottom)
    };

    private static Brush Solid(string hex) => (Brush)new BrushConverter().ConvertFromString(hex)!;
}
